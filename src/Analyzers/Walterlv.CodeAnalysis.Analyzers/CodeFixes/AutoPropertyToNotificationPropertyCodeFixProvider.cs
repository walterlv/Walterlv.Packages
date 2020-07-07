using System.Collections.Immutable;
using System.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

using Walterlv.CodeAnalysis.Properties;

namespace Walterlv.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// 自动属性转可通知属性。
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToNotificationPropertyCodeFixProvider)), Shared]
    public class AutoPropertyToNotificationPropertyCodeFixProvider : CodeFixProvider
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
                    title: Resources.AutoPropertyToNotificationPropertyFix,
                    createChangedDocument: ct => ConvertToNotificationProperty(context.Document, declaration, ct),
                    equivalenceKey: nameof(AutoPropertyToNotificationPropertyCodeFixProvider)),
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
            ChangeAutoPropertyToNotificationProperty(editor, propertyDeclarationSyntax);
            return editor.GetChangedDocument();
        }

        private static void ChangeAutoPropertyToNotificationProperty(DocumentEditor editor, PropertyDeclarationSyntax propertyDeclarationSyntax)
        {
            if (propertyDeclarationSyntax.AccessorList is null)
            {
                return;
            }

            // 生成可通知属性的类型/名称/字段名称。
            var type = propertyDeclarationSyntax.Type;
            var propertyName = propertyDeclarationSyntax.Identifier.ValueText;
            var fieldName = $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";

            // 替换 get/set。
            var accessorList = propertyDeclarationSyntax.AccessorList.Accessors.OfType<AccessorDeclarationSyntax>();
            foreach (var accessor in accessorList.Where(x => x.ExpressionBody is null))
            {
                if (accessor.Keyword.Text == "get")
                {
                    // get => GetValue(_field);
                    editor.ReplaceNode(accessor,
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.GetAccessorDeclaration
                        )
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken),
                                SyntaxFactory.IdentifierName(fieldName)
                            )
                        )
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
                }
                else if (accessor.Keyword.Text == "set")
                {
                    // set => SetValue(ref _field, value);
                    editor.ReplaceNode(accessor,
                        SyntaxFactory.AccessorDeclaration(
                            SyntaxKind.SetAccessorDeclaration
                        )
                        .WithExpressionBody(
                            SyntaxFactory.ArrowExpressionClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsGreaterThanToken),
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.IdentifierName("SetValue"),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                                        SyntaxFactory.SeparatedList(new[]
                                        {
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.IdentifierName(fieldName)
                                                )
                                                .WithRefKindKeyword(
                                                    SyntaxFactory.Token(SyntaxKind.RefKeyword)
                                                ),
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.IdentifierName("value")
                                                ),
                                        }),
                                        SyntaxFactory.Token(SyntaxKind.CloseParenToken)
                                    )
                                )
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
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),
                    SyntaxFactory.VariableDeclaration(
                        type,
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(fieldName)
                            )
                        })
                    ),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            });
        }
    }
}
