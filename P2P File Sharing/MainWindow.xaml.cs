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
using System.IO;

namespace P2P_File_Sharing
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainAppInstance;
        public MainWindow()
        {
            InitializeComponent();
            mainAppInstance = this;
            if (!Directory.Exists(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage")))
            {
                Directory.CreateDirectory(String.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\Peer_Storage"));
                tbAppActivity.Text = "Created required directory 'Peer_Storage' and other required directories in the Documents folder.";
            }
            else
            {
                tbAppActivity.Text = "Ready";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Store_File store_File = new Store_File();
            store_File.ShowDialog();
        }
    }
}
