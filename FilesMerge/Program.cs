using FilesMerge.Models;
using FilesMerge.Presenters;
using FilesMerge.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesMerge
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

            IPresenter control = new FilesPresenter(new FileInfo(), new MainForm());
            control.Run();
        }
    }
}
