using System.IO;

namespace Http.Gateway.Tests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class Utility
    {
        public static string ReadScript(string scriptName)
        {
            return File.ReadAllText($"./scripts/{scriptName}");
        }
    }
}
