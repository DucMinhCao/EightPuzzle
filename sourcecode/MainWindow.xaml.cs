using System;
using System.Collections.Generic;
using System.IO;
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

namespace navigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var path = System.AppContext.BaseDirectory + "DefaultImages/" + "cover.jpg";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.Absolute);
            bi.EndInit();

            imageCover.Source = bi;
        }

        private void changeFrametoNewGame(object sender, RoutedEventArgs e)
        {
            this.Hide();
            //Window1 wd1 = new Window1(this);
            BrowseWindow bw = new BrowseWindow(this);
            bw.ShowDialog();
        }

        private void LoadGame(object sender, RoutedEventArgs e)
        {
            string readfile = System.AppContext.BaseDirectory + "load.dat";
            if (!File.Exists(readfile))//File does not exist
            {
                string sMessageBoxText = "Load file was missing. Please check again.";
                string sCaption = "Error";

                MessageBoxButton btnMessageBox = MessageBoxButton.OK;
                MessageBoxImage icnMessageBox = MessageBoxImage.Error;

                MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);
            }
            else
            {
                this.Hide();
                Window1 wd1 = new Window1(this);
                wd1.ShowDialog();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(1);
        }
    }
}
