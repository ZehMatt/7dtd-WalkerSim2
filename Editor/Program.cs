using System;
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
            WalkerSim.Drawing.Loader = new WalkerSim.Editor.Drawing.ImageLoader();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
