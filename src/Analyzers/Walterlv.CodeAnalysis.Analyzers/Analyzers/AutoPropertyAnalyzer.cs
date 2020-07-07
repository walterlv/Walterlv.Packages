using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Walterlv.CodeAnalysis;
using Walterlv.CodeAnalysis.Properties;

namespace Walterlv.CodeAnalysis.Analyzers
{
    /// <summary>
    /// 查找自动属性。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AutoPropertyAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticIds.AutoProperty,
            LocalizableStrings.Get(nameof(Resources.AutoPropertyTitle)),
            LocalizableStrings.Get(nameof(Resources.AutoPropertyMessage)),
            "Walterlv.Usage",
            DiagnosticSeverity.Hidden,
            isEnabledByDefault: true,
            description: LocalizableStrings.Get(nameof(Resources.AutoPropertyDescription)),
            helpLinkUri: DiagnosticUrls.Get(DiagnosticIds.AutoProperty));

        /// <inheritdoc />
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc />
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeAutoProperty, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeAutoProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyNode = (PropertyDeclarationSyntax)context.Node;
            if (propertyNode.AccessorList != null)
            {
                var get = propertyNode.AccessorList.Accessors.OfType<AccessorDeclarationSyntax>().FirstOrDefault(x => x.Keyword.Text == "get");
                var set = propertyNode.AccessorList.Accessors.OfType<AccessorDeclarationSyntax>().FirstOrDefault(x => x.Keyword.Text == "set");
                if (get != null && set != null && get.ExpressionBody is null && set.ExpressionBody is null)
                {
                    var propertyName = propertyNode.Identifier.ValueText;
                    var diagnostic = Diagnostic.Create(Rule, propertyNode.GetLocation(), propertyName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
