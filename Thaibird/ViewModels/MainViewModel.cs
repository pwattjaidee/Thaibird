using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Thaibird.Common;


namespace Thaibird.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ItemViewModel> Items { get; private set; }
        public ObservableCollection<ItemViewModel> MentionItems { get; private set; }
        public ObservableCollection<ItemViewModel> DirectMessageItems { get; private set; }
        public ObservableCollection<ItemViewModel> FavoriteItems { get; private set; }

        private static readonly Object ThisLock = new Object();
        private int _syncCount;
        private const int SyncItemCount = 4;

        public MainViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>();
            MentionItems = new ObservableCollection<ItemViewModel>();
            DirectMessageItems = new ObservableCollection<ItemViewModel>();
            FavoriteItems = new ObservableCollection<ItemViewModel>();
        }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get
            {
                return _progressBarVisibility;
            }
            set
            {
                if (value != _progressBarVisibility)
                {
                    _progressBarVisibility = value;
                    NotifyPropertyChanged("ProgressBarVisibility");
                }
            }
        }

        private bool _progressBarIsIndeterminate;
        public bool ProgressBarIsIndeterminate
        {
            get
            {
                return _progressBarIsIndeterminate;
            }
            set
            {
                if (value != _progressBarIsIndeterminate)
                {
                    _progressBarIsIndeterminate = value;
                    NotifyPropertyChanged("ProgressBarIsIndeterminate");
                }
            }
        }


        public bool IsDataLoaded
        {
            get;
            set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            LoadData(false);
            IsDataLoaded = true;
        }

        public void Refresh()
        {
            LoadData(true);
        }

        private void LoadData(bool refresh)
        {
            _syncCount = SyncItemCount;
            ProgressBarIsIndeterminate = true;
            ProgressBarVisibility = Visibility.Visible;

            LoadList(TwitterListType.Statuses, refresh);
            LoadList(TwitterListType.Mentions, refresh);
            LoadList(TwitterListType.DirectMessages, refresh);
            LoadList(TwitterListType.Favorites, refresh);

        }

        private void LoadList(TwitterListType listType, bool refresh)
        {
            LoadList(listType, refresh, String.Empty);
        }

        private void LoadList(TwitterListType listType, bool refresh, string searchTerm)
        {
            string fileName = null;
            ObservableCollection<ItemViewModel> parentList = null;

            switch (listType)
            {
                case TwitterListType.Statuses:
                    fileName = Constants.StatusesFileName;
                    parentList = Items;
                    break;
                case TwitterListType.Mentions:
                    fileName = Constants.MentionsFileName;
                    parentList = MentionItems;
                    break;
                case TwitterListType.DirectMessages:
                    fileName = Constants.DirectMessagesFileName;
                    parentList = DirectMessageItems;
                    break;
                case TwitterListType.Favorites:
                    fileName = Constants.FavoritesFileName;
                    parentList = FavoriteItems;
                    break;
            }

            if (String.IsNullOrEmpty(fileName))
                return;

            // If a cached file exists, bind it first then go update unless we are refreshing
            if (!refresh)
            {
                var itemList = Helper.LoadSetting<List<ItemViewModel>>(fileName);
                if (itemList != null)
                {
                    BindList(parentList, itemList);
                }
            }

            var twitterHelper = new TwitterHelper();
            twitterHelper.LoadList(listType, (parentList != null && parentList.Count > 0) ? parentList[0].Id : 0, searchTerm);
            twitterHelper.LoadedCompleteEvent += (sender, e) =>
            {
                var list = Helper.LoadSetting<List<ItemViewModel>>(fileName);
                if (list == null)
                {
                    Helper.ShowMessage("Error Loading Data from Twitter.");
                    return;
                }
                CheckCount();
                Deployment.Current.Dispatcher.BeginInvoke(() => BindList(parentList, list));
            };
        }

        private void CheckCount()
        {
            lock (ThisLock)
            {
                _syncCount--;
                if (_syncCount == 0)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        ProgressBarIsIndeterminate = false;
                        ProgressBarVisibility = Visibility.Collapsed;
                    });
                }
            }
        }



        private static void BindList(ObservableCollection<ItemViewModel> parentList, IEnumerable<ItemViewModel> list)
        {
            parentList.Clear();

            foreach (var item in list)
            {
                parentList.Add(item);
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
    public enum TwitterListType { Statuses, Mentions, DirectMessages, Favorites }
}