using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Thaibird
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private string _userName;
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                if (value != _userName)
                {
                    _userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        private string _displayUserName;
        public string DisplayUserName
        {
            get
            {
                return _displayUserName;
            }
            set
            {
                if (value != _displayUserName)
                {
                    _displayUserName = value;
                    NotifyPropertyChanged("DisplayUserName");
                }
            }
        }

        private string _tweetText;
        public string TweetText
        {
            get
            {
                return _tweetText;
            }
            set
            {
                if (value != _tweetText)
                {
                    _tweetText = value;
                    NotifyPropertyChanged("TweetText");
                }
            }
        }

        private string _createdDate;
        public string CreatedDate
        {
            get
            {
                return _createdDate;
            }
            set
            {
                if (value != _createdDate)
                {
                    _createdDate = value;
                    NotifyPropertyChanged("CreatedDate");
                }
            }
        }

        private string _image;
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (value != _image)
                {
                    _image = value;
                    NotifyPropertyChanged("Image");
                }
            }
        }

        private string _source;
        public string Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (value != _source)
                {
                    _source = value;
                    NotifyPropertyChanged("Source");
                }
            }
        }

        public long Id { get; set; }
        public bool NewTweet { get; set; }

        public SolidColorBrush TitleColor
        {
            get
            {
                return NewTweet ? (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"]
                               : (SolidColorBrush)Application.Current.Resources["PhoneBackgroundBrush"];
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}