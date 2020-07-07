namespace Walterlv.CodeAnalysis
{
    internal static class DiagnosticUrls
    {
        public static string Get(string diagnosticId)
            => $"https://github.com/dotnet-campus/dotnetCampus.CommandLine/docs/analyzers/{diagnosticId}.md";
    }
}
