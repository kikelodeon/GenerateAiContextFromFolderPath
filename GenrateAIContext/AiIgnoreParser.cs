using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;

namespace GenrateAIContext
{
    public static class AiIgnoreParser
    {
        public static (HashSet<string> folders, HashSet<string> extensions, Matcher matcher) Parse(string rootPath)
        {
            var aiignorePath = Path.Combine(rootPath, ".aiignore");
            var gitignorePath = Path.Combine(rootPath, ".gitignore");

            try
            {
                // Si .aiignore no existe pero sí .gitignore, lo copiamos
                if (!File.Exists(aiignorePath) && File.Exists(gitignorePath))
                {
                    File.Copy(gitignorePath, aiignorePath);
                    Console.WriteLine($".aiignore creado automáticamente desde .gitignore en {rootPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al copiar .gitignore a .aiignore: {ex.Message}");
            }

            var matcher = new Matcher();
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(aiignorePath))
                return (folders, extensions, matcher);

            try
            {
                var lines = File.ReadAllLines(aiignorePath);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();

                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    trimmed = trimmed.Replace('\\', '/');
                    matcher.AddExclude(trimmed);

                    // Identificar carpetas
                    if (trimmed.EndsWith("/"))
                        folders.Add(trimmed.TrimEnd('/').Split('/').Last());

                    // Identificar extensiones
                    if (Path.HasExtension(trimmed))
                        extensions.Add(Path.GetExtension(trimmed));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al procesar .aiignore: {ex.Message}");
            }

            return (folders, extensions, matcher);
        }
    }
}
