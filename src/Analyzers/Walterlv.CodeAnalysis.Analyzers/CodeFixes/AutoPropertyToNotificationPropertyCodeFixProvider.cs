

using System.Composition;
using System.Globalization;

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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoPropertyToNotificationPropertyCodeFixProvider)), Shared]
    public class AutoPropertyToNotificationPropertyCodeFixProvider : AutoPropertyToOtherCodeFixProvider
    {
        /// <inheritdoc />
        protected override string CodeActionTitle => Resources.AutoPropertyToNotificationPropertyFix;

        /// <inheritdoc />
        protected override void ChangePropertyCore(DocumentEditor editor, PropertyDeclarationSyntax propertySyntax)
        {
            if (propertySyntax.AccessorList is null)
            {
                return;
            }

            // 生成可通知属性的类型/名称/字段名称。
            var propertyType = propertySyntax.Type;
            var propertyName = propertySyntax.Identifier.ValueText;
            var fieldName = $"_{char.ToLower(propertyName[0], CultureInfo.InvariantCulture)}{propertyName.Substring(1)}";

            // 增加字段。
            editor.InsertBefore(propertySyntax, new SyntaxNode[]
            {
                // private Type _field;
                SyntaxFactory.FieldDeclaration(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),
                    SyntaxFactory.VariableDeclaration(
                        propertyType,
                        SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(fieldName)
                            )
                        })
                    ),
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            });

            // 替换 get/set。
            editor.ReplaceNode(
                propertySyntax,
                SyntaxFactory.ParseMemberDeclaration(
                    $@"{propertySyntax.AttributeLists.ToFullString()}{propertySyntax.Modifiers.ToFullString()}{propertySyntax.Type.ToFullString()}{propertySyntax.Identifier.ToFullString()}
{{
    get => GetValue({fieldName});
    set => SetValue(ref {fieldName}, value);
}}")!
                .WithAdditionalAnnotations(new SyntaxAnnotation[] { Simplifier.Annotation, Formatter.Annotation })
                );
        }
    }
}
