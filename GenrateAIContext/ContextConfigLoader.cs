using System.IO;
using Newtonsoft.Json;

namespace GenrateAIContext
{
    public static class ContextConfigLoader
    {
        public static ContextConfig LoadConfig(string baseFolder)
        {
            var cfg = Path.Combine(baseFolder, ".contextconfig.json");
            if (File.Exists(cfg))
                return JsonConvert.DeserializeObject<ContextConfig>(File.ReadAllText(cfg))
                       ?? new ContextConfig();
            return new ContextConfig();
        }
    }
}
