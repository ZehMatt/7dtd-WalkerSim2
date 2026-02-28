using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WalkerSim.Editor
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            WalkerSim.Drawing.Loader = new WalkerSim.Editor.Drawing.ImageLoader();

            //throw new Exception("Ouch");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ShowExceptionDetails(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowExceptionDetails(e.ExceptionObject as Exception);
        }

        static void ShowExceptionDetails(Exception Ex)
        {
            if (Ex == null)
            {
                MessageBox.Show("An unknown error occurred.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string reportUrl = "https://github.com/ZehMatt/7dtd-WalkerSim2/issues/new";
            string version = BuildInfo.Version;
            string commit = BuildInfo.Commit;

            // Detailed message for MessageBox
            string detailedMessage = $"An unhandled exception occurred:\n\n" +
                                   $"Error Message: {Ex.Message}\n\n" +
                                   $"Exception Type: {Ex.GetType().FullName}\n\n" +
                                   $"Stack Trace:\n{Ex.StackTrace}\n\n" +
                                   $"Would you like to report this issue on GitHub?";

            // Pre-filled GitHub issue data
            string title = Uri.EscapeDataString($"Editor Crash in version {version}");
            string body = Uri.EscapeDataString(
                $"**Describe the bug**\nAn unhandled exception occurred:\n\n" +
                $"**Error Message**\n{Ex.Message}\n\n" +
                $"**Exception Type**\n{Ex.GetType().FullName}\n\n" +
                $"**Stack Trace**\n```\n{Ex.StackTrace}\n```\n\n" +
                $"**Version**: {version}\n**Commit**: {commit}\n\n" +
                $"**To Reproduce**\nSteps to reproduce the behavior:\n1. [Please fill in]\n\n" +
                $"**Expected behavior**\n[Please describe what you expected to happen]\n\n" +
                $"**Additional context**\n[Add any other context about the problem here]");

            string fullUrl = $"{reportUrl}?title={title}&body={body}";

            DialogResult result = MessageBox.Show(detailedMessage, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

            if (result == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open browser: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
