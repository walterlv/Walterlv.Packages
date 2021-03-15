using System.Composition;

using Microsoft.CodeAnalysis;
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
    public class AutoPropertyToDependencyPropertyCodeFixProvider : AutoPropertyToOtherCodeFixProvider
    {
        /// <inheritdoc />
        protected override string CodeActionTitle => Resources.AutoPropertyToDependencyPropertyFix;

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
            var dependencyPropertyName = $"{propertyName}Property";

            // 增加字段。
            editor.InsertBefore(propertySyntax, new SyntaxNode[]
            {
                // public static readonly DependencyProperty XxxProperty;
                SyntaxFactory.FieldDeclaration(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                        SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.ParseTypeName("System.Windows.DependencyProperty"),
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(dependencyPropertyName),
                                null,
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ParseExpression(@$"System.Windows.DependencyProperty.Register(
{"",4}nameof({propertyName}), typeof({propertyType}), typeof({ownerType}),
{"",4}new System.Windows.PropertyMetadata(default({propertyType})))")
                                )
                            )
                        })
                    ),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
            });

            // 替换 get/set。
            editor.ReplaceNode(
                propertySyntax,
                SyntaxFactory.ParseMemberDeclaration(
                    $@"{propertySyntax.AttributeLists.ToFullString()}{propertySyntax.Modifiers.ToFullString()}{propertySyntax.Type.ToFullString()}{propertySyntax.Identifier.ToFullString()}
{{
    get => ({propertyType})GetValue({dependencyPropertyName});
    set => SetValue({dependencyPropertyName}, value);
}}")!
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
                );
        }
    }
}
