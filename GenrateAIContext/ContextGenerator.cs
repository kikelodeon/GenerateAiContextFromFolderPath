using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GenrateAIContext
{
    public static class ContextGenerator
    {
        public static void GenerateContext(
            string baseFolder,
            string[] excludedFolders,
            string[] excludedExtensions,
            string outputFileName)
        {
            // Rutas
            var outPath = Path.Combine(baseFolder, outputFileName);
            var outFullPath = Path.GetFullPath(outPath);

            // 1) Enumerar y filtrar
            var exF = new HashSet<string>(excludedFolders, StringComparer.OrdinalIgnoreCase);
            var exE = new HashSet<string>(excludedExtensions, StringComparer.OrdinalIgnoreCase);

            var files = Directory
                .EnumerateFiles(baseFolder, "*.*", SearchOption.AllDirectories)
                // excluyo carpetas
                .Where(f =>
                    !PathHelpers.GetRelativePath(baseFolder, f)
                                .Split(Path.DirectorySeparatorChar)
                                .Any(seg => exF.Contains(seg)))
                // excluyo extensiones
                .Where(f => !exE.Contains(Path.GetExtension(f)))
                // excluyo *exactamente* el archivo de salida
                .Where(f => !string.Equals(Path.GetFullPath(f), outFullPath, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();

            // 2) Escritura atómica a temp
            var tmpPath = outPath + ".tmp";
            using (var w = new StreamWriter(tmpPath, false, Encoding.UTF8))
            {
                var tree = new List<string>();
                foreach (var f in files)
                {
                    w.WriteLine($"### {f}\n");

                    // Si falla la lectura, simplemente lo anotamos y seguimos
                    try
                    {
                        w.WriteLine(File.ReadAllText(f));
                    }
                    catch (IOException)
                    {
                        w.WriteLine($"// SKIPPED (locked): {f}");
                    }

                    w.WriteLine("\n-----\n");
                    tree.Add(PathHelpers.GetRelativePath(baseFolder, f));
                }

                w.WriteLine("### Árbol:");
                foreach (var line in tree)
                    w.WriteLine(line);
            }

            // 3) Reemplazo seguro
            File.Delete(outPath);
            File.Move(tmpPath, outPath);
        }
    }
}
