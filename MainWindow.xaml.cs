using System;
using System.Diagnostics;
using MahApps.Metro.Controls;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SafeCleaner
{
    public partial class MainWindow : MetroWindow
    {
        private string logFilePath = "cleaner_log.txt";
        private int totalDeleted = 0;
        private int totalSkipped = 0;

        public MainWindow()
        {
            InitializeComponent();
            txtResult.Document.Blocks.Clear();

            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            EnsureAdminRights();
        }

        private void EnsureAdminRights()
        {
            if (!IsRunningAsAdmin())
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = System.Reflection.Assembly.GetExecutingAssembly().Location,
                    UseShellExecute = true,
                    Verb = "runas"
                };

                try
                {
                    Process.Start(processInfo);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Application needs administrative privileges.\n{ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
            }
        }

        private bool IsRunningAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            txtResult.Document.Blocks.Clear();
            progressRing.Visibility = Visibility.Visible;
            progressRing.IsActive = true;

            totalDeleted = 0;
            totalSkipped = 0;
            AppendToLog("Starting cleanup...\n");

            if (chkSoftwareDistribution.IsChecked == true)
                await CleanDirectoryAsync(@"C:\Windows\SoftwareDistribution");

            if (chkWindowsTemp.IsChecked == true)
                await CleanDirectoryAsync(@"C:\Windows\Temp");

            if (chkAppDataTemp.IsChecked == true)
                await CleanDirectoryAsync(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"));

            progressRing.IsActive = false;
            progressRing.Visibility = Visibility.Hidden;

            AppendToLog("\n✅ Cleanup completed.\n");
            AppendToLog($"Total Deleted: {totalDeleted}\nTotal Skipped (Locked): {totalSkipped}\n");
        }

        private async Task CleanDirectoryAsync(string directoryPath)
        {
            AppendToLog($"\n🧹 Cleaning directory: {directoryPath}\n");

            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
                    var directories = Directory.GetDirectories(directoryPath);

                    int totalItems = files.Length + directories.Length;
                    int processedItems = 0;

                    foreach (var dir in directories)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            AppendToLog($"✅ Deleted: {dir}\n");
                            totalDeleted++;
                        }
                        catch
                        {
                            AppendToLog($"❌ Ignored (Locked): {dir}\n");
                            totalSkipped++;
                        }
                        processedItems++;
                    }

                    foreach (var file in files)
                    {
                        try
                        {
                            File.Delete(file);
                            AppendToLog($"✅ Deleted: {file}\n");
                            totalDeleted++;
                        }
                        catch
                        {
                            AppendToLog($"❌ Ignored (Locked): {file}\n");
                            totalSkipped++;
                        }
                        processedItems++;
                    }
                }
                else
                {
                    AppendToLog($"❌ Directory does not exist: {directoryPath}\n");
                }
            }
            catch (Exception ex)
            {
                AppendToLog($"❌ Error accessing directory {directoryPath}: {ex.Message}\n");
            }
        }

        private void AppendToLog(string text)
        {
            Dispatcher.Invoke(() =>
            {
                txtResult.AppendText(text);
                txtResult.ScrollToEnd();
                File.AppendAllText(logFilePath, text);
            });
        }

        private void btnShowLog_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(logFilePath))
                Process.Start("notepad.exe", logFilePath);
            else
                MessageBox.Show("Log file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenWebsite(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://peterfromslovakia.github.io/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the website.\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
