using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.ComponentModel.Design;

namespace GenrateAIContext
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Generar Contexto IA", "Dialogo de configuración", "1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid("c27c8469-d722-43f4-8fcf-2d130fb5a0ac")]
    public sealed class GenrateAIContextPackage : AsyncPackage
    {
        protected override async Task InitializeAsync(
            CancellationToken ct, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(ct);
            var mcs = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var cmdId = new CommandID(
                new Guid("c27c8469-d722-43f4-8fcf-2d130fb5a0ac"), 0x0100);
            var menu = new MenuCommand((s, e) => ShowConfig(), cmdId);
            mcs.AddCommand(menu);
        }

        private async void ShowConfig()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            // Obtener owner HWND
            var uiShell = await GetServiceAsync(typeof(SVsUIShell)) as IVsUIShell;
            uiShell.GetDialogOwnerHwnd(out IntPtr hwndOwner);

            // Default desde .contextconfig.json en la raíz de la solución
            var dte = (EnvDTE.DTE)await GetServiceAsync(typeof(EnvDTE.DTE));
            var root = Path.GetDirectoryName(dte.Solution.FullName);

            // Mostrar formulario
            using var form = new ConfigForm(root);
            if (hwndOwner != IntPtr.Zero)
                form.ShowDialog(new WindowWrapper(hwndOwner));
            else
                form.ShowDialog();
        }
    }
}
