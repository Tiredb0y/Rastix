using System;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

[assembly: AssemblyTitle("Rastix Setup")]
[assembly: AssemblyDescription("Rastix portable Windows Terminal installer")]
[assembly: AssemblyCompany("Rastix Project")]
[assembly: AssemblyProduct("Rastix")]
[assembly: AssemblyVersion("0.1.2.0")]
[assembly: AssemblyFileVersion("0.1.2.0")]

internal static class Installer
{
    private const string Version = "0.1.2";
    private const string PayloadResource = "RastixInstaller.Payload.zip";

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && String.Equals(args[0], "/verify", StringComparison.OrdinalIgnoreCase))
            {
                VerifyPayload();
                return 0;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
            return 0;
        }
        catch (Exception ex)
        {
            if (args.Length > 0 && String.Equals(args[0], "/verify", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine(ex.ToString());
                return 1;
            }
            MessageBox.Show(ex.Message, "Rastix Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    private static void VerifyPayload()
    {
        using (Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream(PayloadResource))
        {
            if (payload == null) throw new InvalidDataException("Installer payload is missing.");
            string temp = Path.Combine(Path.GetTempPath(), "Rastix-verify-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temp);
            try
            {
                string zip = Path.Combine(temp, "payload.zip");
                using (FileStream output = File.Create(zip)) payload.CopyTo(output);
                string extracted = Path.Combine(temp, "app");
                SafeExtract(zip, extracted);
                FindAppDirectory(extracted);
            }
            finally { try { Directory.Delete(temp, true); } catch { } }
        }
    }

    private static string FindAppDirectory(string extracted)
    {
        string[] executables = Directory.GetFiles(extracted, "WindowsTerminal.exe", SearchOption.AllDirectories);
        if (executables.Length != 1) throw new InvalidDataException("Expected exactly one WindowsTerminal.exe in the portable payload.");
        string appDirectory = Path.GetDirectoryName(executables[0]);
        foreach (string file in new[] { "resources.pri", ".portable", "Microsoft.UI.Xaml.dll", "rx.exe" })
        {
            if (!File.Exists(Path.Combine(appDirectory, file)))
                throw new InvalidDataException("Portable Rastix payload is missing: " + file);
        }
        return appDirectory;
    }

    private static void ExtractResource(string resourceName, string destination)
    {
        using (Stream input = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
        {
            if (input == null) throw new InvalidDataException("Installer resource is missing: " + resourceName);
            using (FileStream output = File.Create(destination)) input.CopyTo(output);
        }
    }

    private static void SafeExtract(string zipPath, string destination)
    {
        Directory.CreateDirectory(destination);
        string root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string target = Path.GetFullPath(Path.Combine(destination, entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
                if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsafe path in embedded payload.");
                if (entry.FullName.EndsWith("/", StringComparison.Ordinal) || entry.FullName.EndsWith("\\", StringComparison.Ordinal))
                {
                    Directory.CreateDirectory(target);
                    continue;
                }
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                using (Stream input = entry.Open())
                using (FileStream output = File.Create(target)) input.CopyTo(output);
            }
        }
    }

    private sealed class SetupForm : Form
    {
        private readonly Button _install;
        private readonly Label _status;

        public SetupForm()
        {
            Text = "Install Rastix";
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(510, 320);
            MinimumSize = Size;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            Label title = new Label { Text = "Rastix Setup", Font = new Font(Font.FontFamily, 16, FontStyle.Bold) };
            title.SetBounds(22, 18, 440, 35);
            Controls.Add(title);

            Label details = new Label();
            details.Text = "Installs Rastix as a portable, unpackaged desktop app for the current user. No MSIX registration, Developer Mode, administrator access, or certificate is required. Settings will be stored beside the installed app.\r\n\r\nThis setup is unsigned. Windows SmartScreen may display an Unknown publisher warning; no certificate is installed or trusted by this installer.";
            details.SetBounds(24, 64, 445, 135);
            Controls.Add(details);

            _status = new Label { Text = "Version " + Version };
            _status.SetBounds(24, 210, 462, 32);
            _status.AutoEllipsis = true;
            Controls.Add(_status);

            Button cancel = new Button { Text = "Cancel" };
            cancel.SetBounds(381, 266, 105, 32);
            cancel.Click += delegate { Close(); };
            Controls.Add(cancel);

            _install = new Button { Text = "Install" };
            _install.SetBounds(266, 266, 105, 32);
            _install.Click += InstallClicked;
            Controls.Add(_install);
            AcceptButton = _install;
            CancelButton = cancel;
        }

        private void InstallClicked(object sender, EventArgs e)
        {
            _install.Enabled = false;
            _status.Text = "Installing...";
            Refresh();
            try
            {
                string target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Rastix", Version);
                if (Directory.Exists(target)) throw new IOException("This version is already installed at:\r\n" + target + "\r\n\r\nUse its Uninstall Rastix shortcut first if you want to reinstall it.");

                string stage = target + ".installing-" + Guid.NewGuid().ToString("N");
                Directory.CreateDirectory(stage);
                try
                {
                    using (Stream payload = Assembly.GetExecutingAssembly().GetManifestResourceStream(PayloadResource))
                    {
                        if (payload == null) throw new InvalidDataException("Installer payload is missing.");
                        string zip = Path.Combine(stage, "payload.zip");
                        using (FileStream output = File.Create(zip)) payload.CopyTo(output);
                        string contents = Path.Combine(stage, "app");
                        SafeExtract(zip, contents);
                        string appRoot = FindAppDirectory(contents);
                        string stagedApp = Path.Combine(stage, "RastixApp");
                        Directory.Move(appRoot, stagedApp);
                        string uninstallExe = Path.Combine(stagedApp, "Rastix-Uninstall.exe");
                        ExtractResource("RastixInstaller.Uninstaller.exe", uninstallExe);
                        Directory.CreateDirectory(Path.GetDirectoryName(target));
                        Directory.Move(stagedApp, target);
                        uninstallExe = Path.Combine(target, "Rastix-Uninstall.exe");
                        string shortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Rastix.lnk");
                        try { CreateShortcut(shortcut, Path.Combine(target, "WindowsTerminal.exe"), Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)); }
                        catch (Exception shortcutError) { MessageBox.Show("Rastix files were installed, but the Start menu shortcut could not be created. Launch WindowsTerminal.exe from:\r\n" + target + "\r\n\r\n" + shortcutError.Message, "Rastix Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                        try { CreateShortcut(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Uninstall Rastix.lnk"), uninstallExe, target); }
                        catch (Exception shortcutError) { MessageBox.Show("Could not create the uninstall shortcut. Run Rastix-Uninstall.exe from this folder to remove Rastix and its PATH entry:\r\n" + target + "\r\n\r\n" + shortcutError.Message, "Rastix Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                        try { AddToUserPath(target); }
                        catch (Exception pathError) { MessageBox.Show("Rastix was installed, but its folder could not be added to your user PATH. You can still use the Start menu shortcut.\r\n\r\n" + pathError.Message, "Rastix Setup", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
                    }
                }
                finally { try { Directory.Delete(stage, true); } catch { } }

                _status.Text = "Rastix installed successfully.";
                DialogResult launch = MessageBox.Show("Rastix is installed. Use the Start menu shortcut, or open WindowsTerminal.exe from:\r\n" + target + "\r\n\r\nThe setup is unsigned; Windows may show a SmartScreen Unknown publisher warning when you run it.\r\n\r\nLaunch Rastix now?", "Rastix Setup", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (launch == DialogResult.Yes) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Path.Combine(target, "WindowsTerminal.exe")) { WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) });
                Close();
            }
            catch (Exception ex)
            {
                _status.Text = "Installation failed.";
                _install.Enabled = true;
                MessageBox.Show(ex.Message, "Rastix Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void AddToUserPath(string directory)
        {
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? String.Empty;
            string[] entries = path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string entry in entries)
            {
                if (String.Equals(entry.Trim().Trim('"'), directory, StringComparison.OrdinalIgnoreCase)) return;
            }
            string updated = String.IsNullOrWhiteSpace(path) ? directory : path.TrimEnd(';') + ";" + directory;
            Environment.SetEnvironmentVariable("PATH", updated, EnvironmentVariableTarget.User);
            IntPtr result;
            SendMessageTimeout(new IntPtr(0xffff), 0x001A, IntPtr.Zero, "Environment", 0x0002, 5000, out result);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint message, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);

        private static void CreateShortcut(string path, string target, string workingDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null) throw new InvalidOperationException("Windows Script Host is unavailable; the shortcut could not be created.");
            object shell = Activator.CreateInstance(shellType);
            try
            {
                object shortcut = shellType.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { path });
                Type shortcutType = shortcut.GetType();
                shortcutType.InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { target });
                shortcutType.InvokeMember("WorkingDirectory", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { workingDirectory });
                shortcutType.InvokeMember("Description", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { "Rastix Terminal" });
                shortcutType.InvokeMember("IconLocation", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { target + ",0" });
                shortcutType.InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                Marshal.ReleaseComObject(shortcut);
            }
            finally { Marshal.ReleaseComObject(shell); }
        }
    }
}
