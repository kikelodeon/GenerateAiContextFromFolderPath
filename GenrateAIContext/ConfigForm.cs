using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace GenrateAIContext
{
    public class ConfigForm : Form
    {
        private TextBox txtRoot;
        private TextBox txtOutput;
        private Button btnGenerate;
        private Button btnCancel;

        public ConfigForm(string initialRoot)
        {
            // Form básico
            Text = "Configurar Generar Contexto IA";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(600, 200);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Panel principal
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10),
            };
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            main.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

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

            // Fila 1: Archivo de salida
            main.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            main.Controls.Add(new Label { Text = "Archivo salida:", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            txtOutput = new TextBox { Dock = DockStyle.Fill };
            var btnBrowseOut = new Button { Text = "...", AutoSize = true };
            main.Controls.Add(txtOutput, 1, 1);
            main.Controls.Add(btnBrowseOut, 2, 1);
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

            // Cargar valores iniciales
            txtRoot.Text = initialRoot;
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

            var outputFile = txtOutput.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                MessageBox.Show("Especifique un nombre para el archivo de salida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Sin exclusiones manuales, solo usa .aiignore
            ContextGenerator.GenerateContext(root, Array.Empty<string>(), Array.Empty<string>(), outputFile);

            MessageBox.Show($"Contexto generado en:\n{Path.Combine(root, outputFile)}",
                            "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}
