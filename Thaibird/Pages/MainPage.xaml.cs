using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Thaibird.Common;
using Microsoft.Phone.Shell;

namespace Thaibird
{
    public partial class MainPage : PhoneApplicationPage
    {
        //public bool _firstTime = true;
        public MainPage()
        {
            InitializeComponent();

            #region Application Bar
            ApplicationBar = new ApplicationBar { IsMenuEnabled = true, IsVisible = true, Opacity = .9 };

            var tweet = new ApplicationBarIconButton(new Uri("/Images/tweet.png", UriKind.Relative));
            tweet.Text = "tweet";
            tweet.Click += new EventHandler(btnTweetButton_Click);

            var refresh = new ApplicationBarIconButton(new Uri("/Images/refresh.png", UriKind.Relative));
            refresh.Text = "refresh";
            refresh.Click += new EventHandler(btnRefreshButton_Click);

            //var accountSetting = new ApplicationBarMenuItem();
            //accountSetting.Text = "Change account";
            //accountSetting.Click += new EventHandler(btnAccSetting_Click);

            var support = new ApplicationBarMenuItem();
            support.Text = "About";
            support.Click += new EventHandler(support_Click);

            ApplicationBar.Buttons.Add(tweet);
            ApplicationBar.Buttons.Add(refresh);
            //ApplicationBar.MenuItems.Add(accountSetting);
            ApplicationBar.MenuItems.Add(support);

            #endregion

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            Loaded += MainPage_Loaded;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void support_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/Support.xaml", UriKind.Relative));
        }

        //private void btnAccSetting_Click(object sender, EventArgs e)
        //{
        //    NavigationService.Navigate(new Uri("/Pages/TwitterAuthPage.xaml", UriKind.Relative));
        //}


        private void btnTweetButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/TweetPage.xaml", UriKind.Relative));
        }


        private void btnRefreshButton_Click(object sender, EventArgs e)
        {
            App.ViewModel.Refresh();
        }

        private void ListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex == -1)
                return;

            var selectedItem = (ItemViewModel)((ListBox)sender).SelectedItem;
            if (selectedItem == null)
                return;

            var detailPage = new DetailPageData
            {
                UserDisplayName = selectedItem.DisplayUserName,
                UserName = selectedItem.UserName,
                CreatedDate = selectedItem.CreatedDate,
                Text = selectedItem.TweetText,
                Source = selectedItem.Source,
                Id = selectedItem.Id
            };

            // Save the detailpage object which the detailpage will load up
            Helper.SaveSetting(Constants.DetailPageFileName, detailPage);

            NavigationService.Navigate(new Uri("/Pages/DetailPage.xaml", UriKind.Relative));
        }
    }
}