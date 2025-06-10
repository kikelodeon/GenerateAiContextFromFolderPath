using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace GenrateAIContext
{
    public static class ContextGenerator
    {
        public static void GenerateContext(
            string baseFolder,
            string[] manualExcludedFolders,
            string[] manualExcludedExtensions,
            string outputFileName)
        {
            var outPath = Path.Combine(baseFolder, outputFileName);
            var outFullPath = Path.GetFullPath(outPath);

            // Leer .aiignore (o crearlo desde .gitignore si no existe)
            var (aiIgnoreFolders, aiIgnoreExts, aiMatcher) = AiIgnoreParser.Parse(baseFolder);

            // Combinar exclusiones manuales + .aiignore (por si en el futuro se combinan)
            var exF = new HashSet<string>(manualExcludedFolders.Concat(aiIgnoreFolders), StringComparer.OrdinalIgnoreCase);
            var exE = new HashSet<string>(manualExcludedExtensions.Concat(aiIgnoreExts), StringComparer.OrdinalIgnoreCase);

            // Obtener todos los archivos del proyecto
            var files = Directory
                .EnumerateFiles(baseFolder, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var relPath = PathHelpers.GetRelativePath(baseFolder, f).Replace('\\', '/');

                    // Excluir por .aiignore (globs)
                    if (aiMatcher.Match(relPath).HasMatches)
                        return false;

                    // Excluir por carpetas explícitas
                    if (relPath.Split('/').Any(seg => exF.Contains(seg)))
                        return false;

                    // Excluir por extensión
                    if (exE.Contains(Path.GetExtension(f)))
                        return false;

                    // Excluir el propio archivo de salida
                    return !string.Equals(Path.GetFullPath(f), outFullPath, StringComparison.OrdinalIgnoreCase);
                })
                .OrderBy(f => f)
                .ToList();

            // Escribir a archivo temporal
            var tmpPath = outPath + ".tmp";
            using (var w = new StreamWriter(tmpPath, false, Encoding.UTF8))
            {
                var tree = new List<string>();

                foreach (var file in files)
                {
                    w.WriteLine($"### {file}\n");

                    try
                    {
                        w.WriteLine(File.ReadAllText(file));
                    }
                    catch (IOException)
                    {
                        w.WriteLine($"// SKIPPED (locked): {file}");
                    }

                    w.WriteLine("\n-----\n");
                    tree.Add(PathHelpers.GetRelativePath(baseFolder, file));
                }

                // Agregar árbol de archivos al final
                w.WriteLine("### Árbol:");
                foreach (var line in tree)
                    w.WriteLine(line);
            }

            // Reemplazo atómico
            File.Delete(outPath);
            File.Move(tmpPath, outPath);
        }
    }
}
