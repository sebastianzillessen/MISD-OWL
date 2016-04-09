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

namespace MISD.Client.Notification
{
    
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        readonly GrowlNotifiactions growlNotifications = new GrowlNotifiactions();

        public MainWindow()
        {
            InitializeComponent();
            growlNotifications.Top = SystemParameters.WorkArea.Top + 20;
            growlNotifications.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - 380;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            growlNotifications.AddNotification(new Notification { Title = "Mesage #2", ImageUrl = "pack://application:,,,/Resources/notification-icon.png", Message = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." });
        }
    }
}
