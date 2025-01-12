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

namespace Clients
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = IpAddressTextBox.Text;
            int port = Convert.ToInt32(PortTextBox.Text);

            // Create an instance of MainWindow and pass the values
            MainWindow mainWindow = new MainWindow(ipAddress, port);
            mainWindow.Show();

            // Optionally close the current window
            this.Close();
        }
    }
}
