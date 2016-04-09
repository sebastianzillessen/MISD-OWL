using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConsoleLog
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        protected MainWindow()
        {
            InitializeComponent();
        }

        public void WriteLine(String s)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
                               { this.List.Items.Insert(0, s); }));
        }

        #region Singleton

        private static volatile MainWindow instance;
        private static object syncRoot = new Object();



        /// <summary>
        /// Property to access the class.
        /// </summary>
        public static MainWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                               {

                                   instance = new MainWindow();
                                   instance.Show();
                               }));
                        }
                    }
                }

                return instance;
            }
        }
        #endregion
    }
}
