using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Live_Electrochem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        DateTime expirydatetime = new(2027, 6, 30);


        protected override void OnStartup(StartupEventArgs starte)
        {
            this.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

            base.OnStartup(starte);

            // Piracy checks
            if ((expirydatetime - DateTime.Now).TotalDays < 30)
                MessageBox.Show(string.Format("This application will expiry on {0}\n\nPlease contact Peter Freestone (peter.s.freestone@gmail.com) for continued access", expirydatetime), "Expiry", MessageBoxButton.OK, MessageBoxImage.Warning);


            if (DateTime.Now > expirydatetime)
            {
                MessageBox.Show("This application has expired. Please contact Peter Freestone (peter.s.freestone@gmail.com) for continued access", "Expiry", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }

            //MessageBox.Show("The plot of Charge v Time (bottom plot) is now showing correctly (increasing postive charge does up)", "ATTENTION", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        //private void LogUnhandledException(Exception exception, string @event)
        //{
        //    SimpleLog.WriteLog(exception);
        //    MessageBox.Show(string.Format("Exception occured:\n\n{0}\n\nLogged: {1}", exception.Message, SimpleLog.GetBaseDirectory), "Exception Logged", MessageBoxButton.OK, MessageBoxImage.Error);
        //}

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                Helper.Controls.Dialogs.ExceptionViewer EV = new("An unexpected error occurred in the application", e.Exception, this.MainWindow);
                EV.ShowDialog();
                e.Handled = false;
            }
            catch (InvalidOperationException ioe)
            {
                Debug.Print($"InvalidOperationException occurred '{ioe.Message}' in OnDispatcherUnhandledException() with original exception '{e.Exception.Message}'");
            }
            //string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            //MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //e.Handled = true;
        }

    }
}
