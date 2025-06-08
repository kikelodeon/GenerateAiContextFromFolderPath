using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace GenrateAIContext
{
    public class ConfigForm : Form
    {
        private TextBox txtRoot;
        private ListBox lstExcludedFolders;
        private ListBox lstExcludedExts;
        private TextBox txtOutput;
        private Button btnGenerate;
        private Button btnCancel;

        public ConfigForm(string initialRoot)
        {
            // Form básico
            Text = "Configurar Generar Contexto IA";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(600, 480);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Panel principal con tabla
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 4,
                Padding = new Padding(10),
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // etiqueta
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // control
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // botón

            // Fila 0: Carpeta raíz
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.Controls.Add(new Label { Text = "Carpeta raíz:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            txtRoot = new TextBox { Dock = DockStyle.Fill };
            var btnBrowseRoot = new Button { Text = "...", AutoSize = true };
            main.Controls.Add(txtRoot, 1, 0);
            main.Controls.Add(btnBrowseRoot, 2, 0);
            btnBrowseRoot.Click += (_, __) =>
            {
                using var dlg = new FolderBrowserDialog { SelectedPath = txtRoot.Text };
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtRoot.Text = dlg.SelectedPath;
            };

            // Fila 1: Excluir carpetas
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.Controls.Add(new Label { Text = "Excluir carpetas:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            lstExcludedFolders = new ListBox { Height = 100, Dock = DockStyle.Fill };
            var pnlFolderBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
            var btnAddFolder = new Button { Text = "+", AutoSize = true };
            var btnRemoveFolder = new Button { Text = "–", AutoSize = true };
            pnlFolderBtns.Controls.AddRange(new Control[] { btnAddFolder, btnRemoveFolder });
            main.Controls.Add(lstExcludedFolders, 1, 1);
            main.Controls.Add(pnlFolderBtns, 2, 1);
            btnAddFolder.Click += (_, __) =>
            {
                using var dlg = new FolderBrowserDialog { SelectedPath = txtRoot.Text };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var rel = PathHelpers.GetRelativePath(txtRoot.Text, dlg.SelectedPath);
                    if (!lstExcludedFolders.Items.Contains(rel))
                        lstExcludedFolders.Items.Add(rel);
                }
            };
            btnRemoveFolder.Click += (_, __) =>
            {
                if (lstExcludedFolders.SelectedItem != null)
                    lstExcludedFolders.Items.Remove(lstExcludedFolders.SelectedItem);
            };

            // Fila 2: Excluir extensiones
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.Controls.Add(new Label { Text = "Excluir extensiones:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            lstExcludedExts = new ListBox { Height = 100, Dock = DockStyle.Fill };
            var pnlExtBtns = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
            var btnAddExt = new Button { Text = "+", AutoSize = true };
            var btnRemoveExt = new Button { Text = "–", AutoSize = true };
            pnlExtBtns.Controls.AddRange(new Control[] { btnAddExt, btnRemoveExt });
            main.Controls.Add(lstExcludedExts, 1, 2);
            main.Controls.Add(pnlExtBtns, 2, 2);
            btnAddExt.Click += (_, __) =>
            {
                var ext = Interaction.InputBox(
                    "Extensión (incluye el punto):", "Agregar Extensión", ".log"
                ).Trim();
                if (!string.IsNullOrEmpty(ext))
                {
                    if (!ext.StartsWith(".")) ext = "." + ext;
                    if (!lstExcludedExts.Items.Contains(ext))
                        lstExcludedExts.Items.Add(ext);
                }
            };
            btnRemoveExt.Click += (_, __) =>
            {
                if (lstExcludedExts.SelectedItem != null)
                    lstExcludedExts.Items.Remove(lstExcludedExts.SelectedItem);
            };

            // Fila 3: Archivo de salida
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.Controls.Add(new Label { Text = "Archivo salida:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);
            txtOutput = new TextBox { Dock = DockStyle.Fill };
            var btnBrowseOut = new Button { Text = "...", AutoSize = true };
            main.Controls.Add(txtOutput, 1, 3);
            main.Controls.Add(btnBrowseOut, 2, 3);
            btnBrowseOut.Click += (_, __) =>
            {
                using var dlg = new SaveFileDialog
                {
                    InitialDirectory = txtRoot.Text,
                    FileName = txtOutput.Text,
                    Filter = "Text files|*.txt|All files|*.*"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtOutput.Text = Path.GetFileName(dlg.FileName);
            };

            Controls.Add(main);

            // Panel inferior con botones
            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(10),
                AutoSize = true
            };
            btnCancel = new Button { Text = "Cancelar", AutoSize = true, DialogResult = DialogResult.Cancel };
            btnGenerate = new Button { Text = "Generar", AutoSize = true };
            btnGenerate.Click += Generate_Click;
            bottom.Controls.Add(btnCancel);
            bottom.Controls.Add(btnGenerate);
            Controls.Add(bottom);

            // Cargo configuración inicial (o valores por defecto)
            txtRoot.Text = initialRoot;
            new[] { "bin", "obj", ".git", ".vs", "TestData" }
                .ToList().ForEach(f => lstExcludedFolders.Items.Add(f));
            new[] { ".exe", ".dll", ".pdb", ".png", ".jpg", ".user", ".suo" }
                .ToList().ForEach(e => lstExcludedExts.Items.Add(e));
            txtOutput.Text = "AIContext.txt";
        }

        private void Generate_Click(object sender, EventArgs e)
        {
            var root = txtRoot.Text.Trim();
            if (!Directory.Exists(root))
            {
                MessageBox.Show("La carpeta raíz no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var exFolders = lstExcludedFolders.Items.OfType<string>().ToArray();
            var exExts = lstExcludedExts.Items.OfType<string>().ToArray();
            var outFile = txtOutput.Text.Trim();

            // Genero el contexto
            ContextGenerator.GenerateContext(root, exFolders, exExts, outFile);

            MessageBox.Show($"Contexto generado en:\n{Path.Combine(root, outFile)}",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
