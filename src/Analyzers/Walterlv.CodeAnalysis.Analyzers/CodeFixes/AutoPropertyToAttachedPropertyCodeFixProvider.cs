using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToAttachedPropertyCodeFixProvider)), Shared]
    public class AutoPropertyToAttachedPropertyCodeFixProvider : AutoPropertyToOtherCodeFixProvider
    {
        /// <inheritdoc />
        protected override string CodeActionTitle => Resources.AutoPropertyToAttachedPropertyFix;

        /// <inheritdoc />
        protected override void ChangePropertyCore(DocumentEditor editor, PropertyDeclarationSyntax propertySyntax)
        {
            var ownerType = (propertySyntax.Parent as ClassDeclarationSyntax)?.Identifier.ValueText;
            if (propertySyntax.AccessorList is null || ownerType is null)
            {
                return;
            }

            // 生成可通知属性的类型/名称/字段名称。
            var propertyType = propertySyntax.Type;
            propertyType = propertyType is NullableTypeSyntax nullableTypeSyntax ? nullableTypeSyntax.ElementType : propertyType;
            var propertyName = propertySyntax.Identifier.ValueText;
            var attachedPropertyName = $"{propertyName}Property";

            // 增加字段。
            editor.InsertBefore(propertySyntax, new SyntaxNode[]
            {
                // private Type _field;
                SyntaxFactory.FieldDeclaration(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("System.Windows.DependencyProperty"),
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(attachedPropertyName),
                                null,
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ParseExpression(@$"System.Windows.DependencyProperty.RegisterAttached(
{"",4}""{propertyName}"", typeof({propertyType}), typeof({ownerType}),
{"",4}new System.Windows.PropertyMetadata(default({propertyType})))")
                                )
                            )
                        })
                    ),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
            });

            editor.InsertBefore(
                propertySyntax,
                SyntaxFactory.ParseMemberDeclaration(
                    $@"public static {propertySyntax.Type.ToFullString()}Get{propertyName}(System.Windows.DependencyObject element) => ({propertySyntax.Type.ToFullString()})element.GetValue({attachedPropertyName});")!
                .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(Environment.NewLine))
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
                );

            editor.InsertBefore(
                propertySyntax,
                SyntaxFactory.ParseMemberDeclaration(
                    $@"public static void Set{propertyName}(System.Windows.DependencyObject element, {propertySyntax.Type.ToFullString()} value) => element.SetValue({attachedPropertyName}, value);")!
                .WithLeadingTrivia(SyntaxFactory.ParseLeadingTrivia(Environment.NewLine))
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
                );

            editor.RemoveNode(propertySyntax);
        }
    }
}
