using System;

namespace GenrateAIContext
{
    public class ContextConfig
    {
        public string[] ExcludedFolders { get; set; } = Array.Empty<string>();
        public string[] ExcludedExtensions { get; set; } = Array.Empty<string>();
        public string OutputFileName { get; set; } = "ProjectContext.txt";
    }
}
