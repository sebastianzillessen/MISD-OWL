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
using System.Windows.Shapes;

namespace MISD.Client.Controls.DialogTextbox
{
    /// <summary>
    /// Interaktionslogik für DialogTextBox.xaml
    /// </summary>
    public partial class DialogTextBox : Window
    {
        public string LayoutName
        {
            get;
            set;
        }
        public DialogTextBox()
        {
            InitializeComponent();
        }


        private void EnterButton_Click_1(object sender, RoutedEventArgs e)
        {
            LayoutName = this.NameText.Text;
            this.Close();
        }

        private void CloseButton_Click_1(object sender, RoutedEventArgs e)
        {
            LayoutName = null;
            this.Close();
        }

        private void NameText_GotFocus_1(object sender, RoutedEventArgs e)
        {
            this.NameText.AutoWordSelection=true;
            this.NameText.Select(0, this.NameText.Text.Length);

        }
    }
}
