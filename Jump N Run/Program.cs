using Himmels_Spring_Spiel;
using System;
using System.Windows.Forms;

namespace Jump_N_Run
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HauptFenster());
        }
    }
}