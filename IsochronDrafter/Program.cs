using System;
using System.Net;
using System.Windows.Forms;

namespace IsochronDrafter
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            // allow for newer TLS versions
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072/*Tls12*/ | (SecurityProtocolType)768/*Tls11*/ | SecurityProtocolType.Tls;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DraftWindow());
        }
    }
}
