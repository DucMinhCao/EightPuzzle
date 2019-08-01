using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace navigation
{
    /// <summary>
    /// Interaction logic for BrowseWindow.xaml
    /// </summary>
    public partial class BrowseWindow : Window, INotifyPropertyChanged
    {
        public BrowseWindow(Window mainWindow)
        {
            InitializeComponent();
            //this.mainWindow = mainWindow;
            fileName = System.AppContext.BaseDirectory + "DefaultImages/defaultimage.jpg";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName, UriKind.Absolute);
            bi.EndInit();
            TheGoal.Source = bi;
        }

        private string fileName;
        private MainWindow mainWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (screen.ShowDialog() == true)
            {
                try
                {
                    fileName = screen.FileName;
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(fileName, UriKind.Absolute);
                    bi.EndInit();
                    TheGoal.Source = bi;

                    if (checkValidSize(bi) == false)
                    {
                        string text = "The selected image's size is out of range. Please choose other one";
                        string caption = "Error";

                        InValidFile(text, caption);
                        OpenFile(sender, e);
                    }
                }
                catch (System.NotSupportedException)
                {
                    string text = "The selected file's type is not being supported. Please choose other one";
                    string caption = "Error";

                    InValidFile(text, caption);
                    OpenFile(sender, e);
                }
            }
        }

        private void InValidFile(string sMessageBoxText, string sCaption)
        {
            MessageBoxButton btnMessageBox = MessageBoxButton.OK;
            MessageBoxImage icnMessageBox = MessageBoxImage.Stop;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

        }

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 wd1 = new Window1(this, fileName);
            wd1.ShowDialog();
        }

        private int N = 3;
        

        private bool checkValidSize(BitmapImage bitmap)
        {
            int i = 0, j = N - 1;
            try
            {
                var cropped = new CroppedBitmap(bitmap, new Int32Rect(
(int)(j * bitmap.Width / N), (int)(i * bitmap.Height / N),
(int)bitmap.Width / N, (int)bitmap.Height / N));
            }
            catch (System.ArgumentException)
            {
                return false;
            }
            return true;
        }
    }
}
