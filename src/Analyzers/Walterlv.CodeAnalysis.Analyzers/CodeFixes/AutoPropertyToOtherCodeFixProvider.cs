using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace Walterlv.CodeAnalysis.CodeFixes
{
    /// <summary>
    /// 自动属性转其他种类的属性。
    /// </summary>
    public abstract class AutoPropertyToOtherCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.AutoProperty);

        /// <inheritdoc />
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// 重写以指定转换描述。
        /// </summary>
        protected abstract string CodeActionTitle { get; }

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
                    title: CodeActionTitle,
                    createChangedDocument: ct => ConvertToOther(context.Document, declaration, ct),
                    equivalenceKey: GetType().Name),
                diagnostic);
        }

        private async Task<Document> ConvertToOther(
            Document document,
            PropertyDeclarationSyntax propertyDeclarationSyntax,
            CancellationToken cancellationToken)
        {
            // 获取文档根语法节点。
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null)
            {
                return document;
            }

            // 修改文档。
            var editor = await DocumentEditor.CreateAsync(document).ConfigureAwait(false);
            ChangePropertyCore(editor, propertyDeclarationSyntax);
            return editor.GetChangedDocument();
        }

        /// <summary>
        /// 重写以转换属性。
        /// </summary>
        protected abstract void ChangePropertyCore(DocumentEditor editor, PropertyDeclarationSyntax typeSyntax);
    }
}
