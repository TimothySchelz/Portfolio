using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SpreadsheetGUI
{
    /// <summary>
    /// Keeps track of how many top-level forms are running
    /// </summary>
    class SSApplicationContext : ApplicationContext
    {
        // Number of open forms
        private int formCount = 0;

        // Singleton ApplicationContext
        private static SSApplicationContext appContext;

        /// <summary>
        /// Private constructor for singleton pattern
        /// </summary>
        private SSApplicationContext()
        {
        }

        /// <summary>
        /// Returns the one SSApplicationContext.
        /// </summary>
        public static SSApplicationContext getAppContext()
        {
            if (appContext == null)
            {
                appContext = new SSApplicationContext();
            }
            return appContext;
        }

        /// <summary>
        /// Runs the form
        /// </summary>
        public void RunForm(Form form)
        {
            // One more form is running
            formCount++;

            // When this form closes, we want to find out
            form.FormClosed += (o, e) => { if (--formCount <= 0) ExitThread(); };

            // Run the form
            form.Show();
        }

    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SSApplicationContext appContext = SSApplicationContext.getAppContext();
            string path = null;
            if (args.Length > 0)
            {
                path = args[0];
            }
            // Tell the application context to run the form on the same
            // thread as the other forms.

            //appContext.RunForm(new SplashForm());
            
            appContext.RunForm(new SpreadSheetForm(false, "", "", new SSController(new ClientController())));
              Application.Run(appContext);
        }

    }
}
