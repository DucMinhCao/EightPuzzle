using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace navigation
{
    class Tile : INotifyPropertyChanged
    {
        #region Attributes
        private Point position;
        private Image content;

        #endregion

        #region Properties
        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Position"));
            }
        }

        //public double PositionY
        //{
        //    get
        //    {
        //        return positionY;
        //    }
        //    set
        //    {
        //        positionY = value;
        //    }
        //}

        public Image Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Content"));

            }
        }

        #endregion    }
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }


    }
}
