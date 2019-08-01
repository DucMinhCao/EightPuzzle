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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace navigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class Window1 : Window, INotifyPropertyChanged
    {
        public Window1(Window mainWindow, string filename)
        {
            InitializeComponent();

            fileName = filename;
            newAll();
            CountDown();
            time = time_min * 60 + time_sec;
            try
            {
                TheGoal.Source = new BitmapImage(new Uri(fileName, UriKind.RelativeOrAbsolute));
            }
            catch (ArgumentNullException)
            {

            }
            Init(0);
        }

        private void newAll()
        {
            myItems = new BindingList<Tile>();
            SurroundingItemsIndex = new List<int>();
            TilesIndexOrder = new List<int>();
            this.DataContext = this;
        }

        public Window1(MainWindow mainWindow)// load game
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            newAll();
            load();
            Init(1);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(fileName, UriKind.Absolute);
            bi.EndInit();
            TheGoal.Source = bi;

            if (checkIfWon() == true)
                EndGame(1);//won

            CountDown();
        }

        private void CountDown()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Tick += Timer_Tick;
            dispatcherTimer.Start();

        }

        private void save()
        {
            string writefile = System.AppContext.BaseDirectory + "load.dat";
            var writer = new StreamWriter(writefile);

            writer.WriteLine($"{time}");
            writer.WriteLine($"{fileName}");
            for(int i = 0; i < N * N; i++)
            {
                writer.Write($"{TilesIndexOrder[i]} ");
            }
            writer.WriteLine();
            writer.Write(BlankTileIndex);

            writer.Close();
        }

        private void load()
        {
            string readfile = System.AppContext.BaseDirectory + "load.dat";
            var reader = new StreamReader(readfile);

            var firstLine = reader.ReadLine();// time
            var secondLine = reader.ReadLine();// file path
            var thirdLine = reader.ReadLine();// tiles index
            var fourthline = reader.ReadLine();// Blank tile index

            //Check if tiles index were valid
            trashstring = thirdLine;

            string[] list = trashstring.Split(' ');
            List<int> int_list = new List<int>();
            int temp;

            for (int i = 0; i < N * N; i++)
            {
                int.TryParse(list[i], out temp);
                int_list.Add(temp);
            }

            for(int i = 0; i < N * N; i++)
            {
                if (int_list.Contains(i) == false)
                {
                    string text = "Tiles' index were invalid. Please click 'OK' to start a new game or 'Cancel' to cancel the program.";
                    string caption = "Error";

                    CannotLoad(text, caption);
                }
            }

            //Check if BlankTileIndex was valid
            if (int.TryParse(fourthline, out BlankTileIndex) == false || BlankTileIndex >= N * N || BlankTileIndex < 0)
            {
                string text = "BlankTileIndex was invalid. Please click 'OK' to start a new game or 'Cancel' to cancel the program. ";
                string caption = "Error";

                CannotLoad(text, caption);
            }

            //Check if file path was existing
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(secondLine, UriKind.Absolute);
                bitmap.EndInit();

                fileName = secondLine;
            }
            catch (System.IO.FileNotFoundException)
            {
                string text = "File path was wrong. Please click 'OK' to choose other file or 'Cancel' to cancel the program. ";
                string caption = "Error";

                CannotLoad(text, caption);
            }

            //Check if time was valid
            int.TryParse(firstLine, out time);
            if (time > 60 * 3 || time < 0)
            {
                string text = "Time-set was invalid. Please click 'OK' to start a new game or 'Cancel' to cancel the program.";
                string caption = "Error";

                CannotLoad(text, caption);
            }

            reader.Close();
        }

        private void CannotLoad(string sMessageBoxText, string sCaption)
        {
            MessageBoxButton btnMessageBox = MessageBoxButton.OKCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Error;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Cancel:
                    {
                        Environment.Exit(1);
                    }
                    break;
                case MessageBoxResult.OK:
                    {
                        this.Hide();
                        BrowseWindow browseWindow = new BrowseWindow(this);
                        browseWindow.ShowDialog();
                    }
                    break;
            }
        }

        private string trashstring;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (time > 0)
            {
                time--;
                if (time % 60 >= 10)
                {
                    TBCountdown.Text = string.Format("0{0}:{1}", time / 60, time % 60);
                }
                else TBCountdown.Text = string.Format("0{0}:0{1}", time / 60, time % 60);
            }
            else if (time <= 0)
            {
                dispatcherTimer.Stop();
                if (solved == false)
                {
                    EndGame(0); //lose
                }
                else
                {
                    EndGame(1); //won
                }
            }
        }

        private void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myListView.ItemsSource = myItems;
        }

        #region Key state
        const int LEFT = 0;
        const int UP = 1;
        const int DOWN = 2;
        const int RIGHT = 3;

        #endregion

        #region Game properties
        BindingList<Tile> myItems;// BindingList tiles
        private int N = 3; // N-puzzle
        private double _Width = 100; // width of a tile
        private double _Height = 100; // height of a tile
        private int BlankTileIndex;
        private List<int> SurroundingItemsIndex;//0: left, 1: top, 2: bottom, 3: right
        private List<int> TilesIndexOrder;
        private string fileName;
        private bool solved = false;
        private DispatcherTimer dispatcherTimer;
        private int time_min = 3;
        private int time_sec = 0;
        private int time;
        #endregion

        #region Mouse Control
        private Point MouseDownLocation;
        private Point CurrentPoint;
        private int SelectedItemIndex;
        private bool moving = false;
        public event PropertyChangedEventHandler PropertyChanged;
        private Point BasePoint = new Point(0.0, 0.0);
        private double DeltaX = 0.0;
        private double DeltaY = 0.0;

        #endregion

        #region Mouse Events
        private void MouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            Label l = e.Source as Label;
            if (l != null)
            {
                l.CaptureMouse();
                var item = (sender as FrameworkElement).DataContext;
                SelectedItemIndex = myListView.Items.IndexOf(item);
                if (checkValidSelectedItemIndex())
                {
                    MouseDownLocation = e.GetPosition(l);
                    moving = true;
                    MouseMoveEvent(sender, e);
                }
                else MouseUpEvent(sender, e);
            }
            else
            {
                Image i = e.Source as Image;
                if (i != null)
                {
                    i.CaptureMouse();
                    var item = (sender as FrameworkElement).DataContext;
                    SelectedItemIndex = myListView.Items.IndexOf(item);
                    if (checkValidSelectedItemIndex())
                    {
                        MouseDownLocation = e.GetPosition(i);
                        moving = true;
                        MouseMoveEvent(sender, e);
                        
                    }
                    else MouseUpEvent(sender, e);
                }
            }
        }

        private bool checkValidSelectedItemIndex()
        {
            return SurroundingItemsIndex.Contains(SelectedItemIndex);
        }

        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                Point p = e.GetPosition(null);
                try
                {
                    CurrentPoint = myItems[SelectedItemIndex].Position;//catch exception here

                    int side = SurroundingItemsIndex.IndexOf(SelectedItemIndex);

                    DeltaX = p.X - MouseDownLocation.X;
                    DeltaY = p.Y - MouseDownLocation.Y;

                    CurrentPoint.X = DeltaX - BasePoint.X;
                    CurrentPoint.Y = DeltaY - BasePoint.Y;

                    if (checkValidUpdatedPosition(side))
                    {
                        if (side == 0 || side == 3)//left or right
                        {
                            CurrentPoint.Y = myItems[SelectedItemIndex].Position.Y;
                            myItems[SelectedItemIndex].Position = CurrentPoint;
                        }
                        else //top or bottom
                        {
                            CurrentPoint.X = myItems[SelectedItemIndex].Position.X;
                            myItems[SelectedItemIndex].Position = CurrentPoint;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException u)
                {
                    moving = false;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="side"></param>
        /// 0-->left, 1-->top, 2-->bot, 3-->right
        /// <returns></returns>
        private bool checkValidUpdatedPosition(int side)
        {
            // side == 0 ->left; 1-> top; 2->bot; 3->right
            switch (side)
            {
                case 0://left
                    {
                        return CurrentPoint.X >= (myItems[BlankTileIndex].Position.X - _Width) && (CurrentPoint.X <= myItems[BlankTileIndex].Position.X);
                    }
                case 3://right
                    {
                        return CurrentPoint.X >= myItems[BlankTileIndex].Position.X && CurrentPoint.X <= (myItems[BlankTileIndex].Position.X + _Width);
                    }
                case 1://top
                    {
                        return CurrentPoint.Y >= (myItems[BlankTileIndex].Position.Y - _Height) && (CurrentPoint.Y <= myItems[BlankTileIndex].Position.Y);
                    }
                case 2://bottom
                    {
                        return CurrentPoint.Y >= myItems[BlankTileIndex].Position.Y && CurrentPoint.Y <= (myItems[BlankTileIndex].Position.Y + _Height);
                    }
            }
            return true;
        }

        private void 
            MouseUpEvent(object sender, MouseButtonEventArgs e)
        {
            Label l = e.Source as Label;
            if (l != null)
            {
                l.ReleaseMouseCapture();
            }
            else
            {
                Image i = e.Source as Image;
                if (i != null)
                {
                    i.ReleaseMouseCapture();
                    //DeltaX = 0.0;
                    //DeltaY = 0.0;
                    //moving = false;
                }
                else
                {
                    ListView li = e.Source as ListView;
                    if (li != null)
                    {
                        li.ReleaseMouseCapture();
                    }
                }
            }
            DeltaX = 0.0;
            DeltaY = 0.0;
            moving = false;

            UpdateBlankTileIndex_Mouse();

            if (checkIfWon() == true)
                EndGame(1);//won
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">
        /// 0-->lose, 1-->won
        /// </param>
        private void EndGame(int state)
        {
            switch (state)
            {
                case 0:// lose
                    {
                        Lose();
                    }
                    break;
                case 1:// win
                    {
                        Won();
                    }
                    break;
            }
        }

        private void Lose()
        {
            string sMessageBoxText ="Wanna back to main menu?";
            string sCaption = "Time's up";

            MessageBoxButton btnMessageBox = MessageBoxButton.OKCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Stop;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.OK:
                    {
                        this.Hide();
                        MainWindow mw = new MainWindow();
                        mw.ShowDialog();
                    }
                    break;

                case MessageBoxResult.Cancel:
                    {
                        Environment.Exit(1);
                    }
                    break;
            }
        }

        private void Won()
        {
            string sMessageBoxText = "Back to menu?";
            string sCaption = "You won";

            MessageBoxButton btnMessageBox = MessageBoxButton.OKCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Question;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.OK:
                    {
                        this.Hide();
                        MainWindow mw = new MainWindow();
                        mw.ShowDialog();
                    }
                    break;

                case MessageBoxResult.Cancel:
                    {
                        Environment.Exit(1);
                    }
                    break;
            }
        }

        private bool checkIfWon()
        {
            for (int i = 0; i < TilesIndexOrder.Count; i++)
                if (TilesIndexOrder[i] != i)
                    return false;

            solved = true;
            return true;
        }

        private void UpdateBlankTileIndex_Mouse()
        {
            if (checkIfWasChanged() == true)
            {
                ReloadPosition();
                UpdateBlankTile();
            }
            else
            {
                ReloadPosition();
            }
        }

        private void ReloadPosition()
        {
            CurrentPoint.X = (SelectedItemIndex % N) * _Width;
            CurrentPoint.Y = (SelectedItemIndex / N) * _Height;
            myItems[SelectedItemIndex].Position = CurrentPoint;
        }

        private void UpdateBlankTile()
        {
            #region Update Content
            myItems[BlankTileIndex].Content = myItems[SelectedItemIndex].Content;
            myItems[SelectedItemIndex].Content = null;

            #endregion

            int temp = TilesIndexOrder[BlankTileIndex];
            TilesIndexOrder[BlankTileIndex] = TilesIndexOrder[SelectedItemIndex];
            TilesIndexOrder[SelectedItemIndex] = temp;

            BlankTileIndex = SelectedItemIndex;
            InitSurroundingListOfItems();
        }

        private bool checkIfWasChanged()
        {
            Point centrePointOfSelectedTile = CalcCentrePointOfTile(myItems[SelectedItemIndex].Position, _Width, _Height);
            Point centrePointOfBlankTile = CalcCentrePointOfTile(myItems[BlankTileIndex].Position, _Width, _Height);

            double distance = Point.Subtract(centrePointOfBlankTile, centrePointOfSelectedTile).Length;

            return distance <= (_Height * 0.5) || distance <= (_Width * 0.5);
        }

        private Point CalcCentrePointOfTile(Point UpperLeft, double width, double height)
        {
            return new Point(UpperLeft.X + (double)(width * 0.5), UpperLeft.Y + (double)(height * 0.5));
        }

        //check if filename is an image's name
        private bool isItAnImage(String loadPath)
        {
            return (
               loadPath.EndsWith(".jpg") ||
               loadPath.EndsWith(".jpeg") ||
               loadPath.EndsWith(".png"));
        }

        #region Init Items
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">
        /// 0->new game; 1->load game
        /// </param>
        private void Init(int state) // Init tiles
        {
            Construction.Text =
                "NGÔ THANH TRÚC     1753115\n" +
                "MAI BẢO TRÂN       1753133\n" +
                "CAO MINH ĐỨC       1753141";
            TilesIndexOrder.Clear();
            myItems.Clear();
            BasePoint.X = myGoal.Margin.Left + myGoal.Margin.Right + myGoal.Width + myListView.Margin.Left;
            BasePoint.Y = myListView.Margin.Top;
            string title = fileName;

            int count = 0;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fileName, UriKind.Absolute);
            bitmap.EndInit();

            var imagelist = new List<Image>();
            var fullImageWidth = 300;

            var heightOfImage = (int)(fullImageWidth * bitmap.Height / bitmap.Width);
            if (heightOfImage > myListView.Height)
            {
                _Height = (int)myListView.Height / 3;
                _Width = (int)(fullImageWidth * _Height / heightOfImage);
            }
            else
            {
                _Width = (int)fullImageWidth / 3;
                _Height = (int)(fullImageWidth * bitmap.Height / bitmap.Width) / 3;
            }


            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    TilesIndexOrder.Add(count);
                    CroppedBitmap cropped;
                    cropped = new CroppedBitmap(bitmap, new Int32Rect(
(int)(j * bitmap.Width / N), (int)(i * bitmap.Height / N),
(int)bitmap.Width / N, (int)bitmap.Height / N));

                    var imageView = new Image();
                    imageView.Source = cropped;
                    imageView.Width = _Width;
                    imageView.Height = _Height;

                    imagelist.Add(imageView);
                    myItems.Add(new Tile() { Position = new Point(j * _Width, i * _Height), Content = null });
                    count++;



                    //
                }
            }

            if (state == 0)//new game
            {
                RandomizeArrayIndex();

                //Create blank tile
                BlankTileIndex = TilesIndexOrder.IndexOf(ChooseBlankTile());
            }
            else if (state == 1)//load game
            {
                string[] list = trashstring.Split(' ');

                for (int i = 0; i < N * N; i++)
                {
                    TilesIndexOrder[i] = int.Parse(list[i]);
                }
            }

            for (int i = 0; i < N * N; i++)
            {
                myItems[i].Content = imagelist[TilesIndexOrder[i]];
            }


            myItems[BlankTileIndex].Content = null;

            InitSurroundingListOfItems();
            InitListOfKeys();

            //Init 4 buttons
            uparrow.Source = setButtnImage(uparrow.Name);
            downarrow.Source = setButtnImage(downarrow.Name);
            leftarrow.Source = setButtnImage(leftarrow.Name);
            rightarrow.Source = setButtnImage(rightarrow.Name);
        }


        private void RandomizeArrayIndex()
        {
            Random rnd = new Random();
            while (true)
            {
                TilesIndexOrder = TilesIndexOrder.OrderBy(x => rnd.Next()).ToList();
                if (checkIfSolvablePattern(TilesIndexOrder))
                {
                    break;
                }
                else continue;
            }
        }

        //Check if the random order can reach the goal state
        private bool checkIfSolvablePattern(List<int> p)
        {
            int inversion = 0;

            for (int i = 0; i < N - 1; i++)
            {
                for (int j = i + 1; j < N; j++)
                {
                    if (p.ElementAt(j * N + i) > p.ElementAt(i * N + j))
                    {
                        inversion++;
                    }
                }
            }

            return inversion % 2 == 0;
        }

        private void InitListOfKeys()
        {
            ListOfKeysControl = new List<Key>();
            ListOfKeysControl.Add(Key.Left);//0
            ListOfKeysControl.Add(Key.Up);//1
            ListOfKeysControl.Add(Key.Down);//2
            ListOfKeysControl.Add(Key.Right);//3
        }

        //Create surrounding list of items
        //0: left, 1: top, 2: bottom, 3: right
        // == -1 if it does not exist
        private void InitSurroundingListOfItems()
        {
            SurroundingItemsIndex.Clear();

            #region Base Case
            SurroundingItemsIndex.Add(BlankTileIndex - 1);//left
            SurroundingItemsIndex.Add(BlankTileIndex - N);//top
            SurroundingItemsIndex.Add(BlankTileIndex + N);//bot
            SurroundingItemsIndex.Add(BlankTileIndex + 1);//right 
            #endregion

            if (BlankTileIndex == 0) //upper left
            {
                SurroundingItemsIndex[0] = -1;//left does not have tile
                SurroundingItemsIndex[1] = -1;//top does not have tile
            }
            else if (BlankTileIndex == N - 1)// upper right
            {
                SurroundingItemsIndex[1] = -1;//top
                SurroundingItemsIndex[3] = -1;//right
            }
            else if (BlankTileIndex == myItems.Count - N)// lower left
            {
                SurroundingItemsIndex[0] = -1;//left
                SurroundingItemsIndex[2] = -1;//bot
            }
            else if (BlankTileIndex == myItems.Count - 1)// lower right
            {
                SurroundingItemsIndex[2] = -1;//bot
                SurroundingItemsIndex[3] = -1;//right
            }
            else if (BlankTileIndex % N == 0)// left edge
            {
                SurroundingItemsIndex[0] = -1;
            }
            else if (BlankTileIndex % N == (N - 1)) // right edge
            {
                SurroundingItemsIndex[3] = -1;
            }
            else if (BlankTileIndex < N - 1) //top edge
            {
                SurroundingItemsIndex[1] = -1;
            }
            else if (BlankTileIndex > myItems.Count - N) //bottom edge
            {
                SurroundingItemsIndex[2] = -1;
            }
        }

        private int ChooseBlankTile()
        {
            Random random = new Random();

            if (random.NextDouble() < 0.9)
            {
                if (random.Next(0, 2) == 0)
                {
                    return myItems.Count - N;
                }
                else return myItems.Count - 1;
            }
            return random.Next(0, myItems.Count - 1);
        }


        #endregion

        private List<Key> ListOfKeysControl;
        private bool pressing = false;
        private MainWindow mainWindow;

        private void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if (pressing == false)
            {
                pressing = true;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">
        /// state = 0 -> left; 1->up; 2->down; 3->right
        ///</param>
        private void HandleKeyDown(int state)
        {
            switch (state)
            {
                case LEFT:
                    {
                        if (SurroundingItemsIndex[3] != -1)
                        {
                            SelectedItemIndex = SurroundingItemsIndex[3];
                        }
                        break;
                    }
                case UP:
                    {
                        if (SurroundingItemsIndex[2] != -1)
                        {
                            SelectedItemIndex = SurroundingItemsIndex[2];
                        }
                        break;
                    }
                case DOWN:
                    {
                        if (SurroundingItemsIndex[1] != -1)
                        {
                            SelectedItemIndex = SurroundingItemsIndex[1];
                        }
                        break;
                    }
                case RIGHT:
                    {
                        if (SurroundingItemsIndex[0] != -1)
                        {
                            SelectedItemIndex = SurroundingItemsIndex[0];
                        }
                        break;
                    }
                default: return;
            }
            ReloadPosition();
            UpdateBlankTile();
        }


        private void KeyUpEvent(object sender, KeyEventArgs e)
        {
                        if (ListOfKeysControl.Contains(e.Key))
            {
                int state = ListOfKeysControl.IndexOf(e.Key);
                //state = 0 -> left; 1->up; 2->down; 3->right
            }
            if (ListOfKeysControl.Contains(e.Key))
            {
                int state = ListOfKeysControl.IndexOf(e.Key);
                //state = 0 -> left; 1->up; 2->down; 3->right
                HandleKeyDown(state);
                if (checkIfWon() == true)
                    EndGame(1);

            }

            pressing = false;
        }

        private BitmapImage setButtnImage(string nameOfButton)
        {
            var path = System.AppContext.BaseDirectory + "DefaultImages/" + nameOfButton + ".png";
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.Absolute);
            bi.EndInit();

            return bi;
        }

        private void Label_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = true;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            dispatcherTimer.Stop();
            string sMessageBoxText = "Do you want to save game?";
            string sCaption = "Exit";

            MessageBoxButton btnMessageBox = MessageBoxButton.YesNoCancel;
            MessageBoxImage icnMessageBox = MessageBoxImage.Question;

            MessageBoxResult rsltMessageBox = MessageBox.Show(sMessageBoxText, sCaption, btnMessageBox, icnMessageBox);

            switch (rsltMessageBox)
            {
                case MessageBoxResult.Yes:
                    {
                        save();
                        Environment.Exit(1);
                    }
                    break;

                case MessageBoxResult.No:
                    {
                        Environment.Exit(1);
                    }
                    break;

                case MessageBoxResult.Cancel:
                    {
                        e.Cancel = true;
                        dispatcherTimer.Start();
                    }
                    break;
            }
            //e.Cancel = true;
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            HandleKeyDown(1);
            if (checkIfWon())
            {
                EndGame(1);
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            HandleKeyDown(2);
            if (checkIfWon())
            {
                EndGame(1);
            }
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            HandleKeyDown(0);
            if (checkIfWon())
            {
                EndGame(1);
            }
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            HandleKeyDown(3);
            if (checkIfWon())
            {
                EndGame(1);
            }
        }
    }
}
