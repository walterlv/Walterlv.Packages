using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

using Walterlv.CodeAnalysis.Properties;

namespace Walterlv.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// 自动属性转可通知属性。
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToDependencyPropertyCodeFixProvider)), Shared]
    public class AutoPropertyToDependencyPropertyCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.AutoProperty);

        /// <inheritdoc />
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc />
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return;
            }

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = (PropertyDeclarationSyntax)root.FindNode(diagnostic.Location.SourceSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Resources.AutoPropertyToDependencyPropertyFix,
                    createChangedDocument: ct => ConvertToNotificationProperty(context.Document, declaration, ct),
                    equivalenceKey: nameof(AutoPropertyToDependencyPropertyCodeFixProvider)),
                diagnostic);
        }

        private async Task<Document> ConvertToNotificationProperty(Document document,
            PropertyDeclarationSyntax propertyDeclarationSyntax, CancellationToken cancellationToken)
        {
            // 获取文档根语法节点。
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return document;
            }

            // 修改文档。
            var editor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            ChangeAutoPropertyToDependencyProperty(editor, propertyDeclarationSyntax);
            return editor.GetChangedDocument();
        }

        private static void ChangeAutoPropertyToDependencyProperty(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            var ownerType = (propertyDeclarationSyntax.Parent as ClassDeclarationSyntax)?.Identifier.ValueText;
            if (propertyDeclarationSyntax.AccessorList is null || ownerType is null)
            {
                return;
            }

            // 生成可通知属性的类型/名称/字段名称。
            var type = propertyDeclarationSyntax.Type;
            if (type is NullableTypeSyntax nullableTypeSyntax)
            {
                type = nullableTypeSyntax.ElementType;
            }
            var whitespaceTrivia = propertyDeclarationSyntax.GetLeadingTrivia().LastOrDefault().Span.Length + 4;
            var propertyName = propertyDeclarationSyntax.Identifier.ValueText;
            var dependencyPropertyName = $"{propertyName}Property";

            // 替换 get/set。
            var accessorList = propertyDeclarationSyntax.AccessorList.Accessors.OfType<AccessorDeclarationSyntax>();
            foreach (var accessor in accessorList.Where(x => x.ExpressionBody is null))
            {
                if (accessor.Keyword.Text == "get")
                {
                    // get => (Type)GetValue(DependencyProperty);
                    editor.ReplaceNode(accessor,
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration
                        )
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken),
                                SyntaxFactory.ParseExpression($"({type})GetValue({dependencyPropertyName})")
                            )
                        )
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                else if (accessor.Keyword.Text == "set")
                {
                    // set => SetValue(DependencyProperty, value);
                    editor.ReplaceNode(accessor,
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration
                        )
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken),
                                SyntaxFactory.ParseExpression($"SetValue({dependencyPropertyName}, value)")
                            )
                        )
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
            }

            // 增加字段。
            editor.InsertBefore(propertyDeclarationSyntax, new SyntaxNode[]
            {
                // private Type _field;
                SyntaxFactory.FieldDeclaration(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    SyntaxFactory.VariableDeclaration(
                        type,
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(dependencyPropertyName),
                                null,
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ParseExpression(@$"System.Windows.DependencyProperty.Register(
{"".PadLeft(whitespaceTrivia, ' ')}nameof({propertyName}), typeof({type}), typeof({ownerType}),
{"".PadLeft(whitespaceTrivia, ' ')}new System.Windows.PropertyMetadata(default({type}))")
                                    .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
                                )
                            )
                        })
                    ),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            });
        }
    }
}
