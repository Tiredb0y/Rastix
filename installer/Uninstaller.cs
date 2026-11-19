using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[assembly: AssemblyTitle("Uninstall Rastix")]
[assembly: AssemblyDescription("Rastix uninstaller")]
[assembly: AssemblyCompany("Rastix Project")]
[assembly: AssemblyProduct("Rastix")]
[assembly: AssemblyVersion("0.1.2.0")]
[assembly: AssemblyFileVersion("0.1.2.0")]

internal static class Uninstaller
{
    [STAThread]
    private static int Main()
    {
        try
        {
            string installDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string expectedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Rastix", "0.1.2");
            if (!String.Equals(Path.GetFullPath(installDirectory), Path.GetFullPath(expectedDirectory), StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(Path.Combine(installDirectory, ".portable")) || !File.Exists(Path.Combine(installDirectory, "WindowsTerminal.exe")))
                throw new InvalidOperationException("Run the uninstaller from its original Rastix installation folder: " + expectedDirectory);
            DialogResult answer = MessageBox.Show("This removes Rastix and its portable settings from:\r\n" + installDirectory + "\r\n\r\nBack up the settings folder first if you want to keep your settings. Close all Rastix windows before continuing. Continue?", "Uninstall Rastix", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (answer != DialogResult.Yes) return 0;

            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            foreach (string name in new[] { "Rastix.lnk", "Uninstall Rastix.lnk" })
            {
                string shortcut = Path.Combine(programs, name);
                if (File.Exists(shortcut)) File.Delete(shortcut);
            }
            RemoveFromUserPath(installDirectory);
            string encodedPath = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(installDirectory));
            string script = "$p=[Text.Encoding]::Unicode.GetString([Convert]::FromBase64String('" + encodedPath + "')); " +
                "for($i=0;$i -lt 10;$i++){Start-Sleep -Seconds 2; try {Remove-Item -LiteralPath $p -Recurse -Force -ErrorAction Stop; exit 0} catch {}}; " +
                "Add-Type -AssemblyName System.Windows.Forms; [void][Windows.Forms.MessageBox]::Show('Could not remove all Rastix files. Close Rastix and delete this folder manually: '+$p,'Uninstall Rastix'); exit 1";
            string encodedScript = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script));
            string powershell = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
            Process.Start(new ProcessStartInfo(powershell, "-NoProfile -NonInteractive -WindowStyle Hidden -EncodedCommand " + encodedScript) { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = Path.GetTempPath() });
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not completely uninstall Rastix. Close all Rastix windows and remove this folder manually if needed.\r\n\r\n" + ex.Message, "Uninstall Rastix", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static void RemoveFromUserPath(string directory)
    {
        string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? String.Empty;
        List<string> entries = new List<string>();
        foreach (string entry in path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!String.Equals(entry.Trim().Trim('"'), directory, StringComparison.OrdinalIgnoreCase)) entries.Add(entry);
        }
        Environment.SetEnvironmentVariable("PATH", String.Join(";", entries.ToArray()), EnvironmentVariableTarget.User);
        IntPtr result;
        SendMessageTimeout(new IntPtr(0xffff), 0x001A, IntPtr.Zero, "Environment", 0x0002, 5000, out result);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint message, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);
}
