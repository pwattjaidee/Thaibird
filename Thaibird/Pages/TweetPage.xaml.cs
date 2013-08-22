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
using Microsoft.Phone.Shell;
using Thaibird.Common;
using System.Windows.Navigation;

namespace Thaibird.Pages
{
    public partial class TweetPage : PhoneApplicationPage
    {
        private TwitterAccess _twitterSettings; 

        public TweetPage()
        {
            InitializeComponent();

            #region Application Bar
            ApplicationBar = new ApplicationBar { IsMenuEnabled = true, IsVisible = true, Opacity = .9 }; 

            var tweet = new ApplicationBarIconButton(new Uri("/Images/mess.png", UriKind.Relative));
            tweet.Text = "tweet";
            tweet.Click += TweetClick;
            tweet.IsEnabled = false; 

            var cancel = new ApplicationBarIconButton(new Uri("/Images/cancel.png", UriKind.Relative));
            cancel.Text = "cancel";
            cancel.Click += CancelClick;

            ApplicationBar.Buttons.Add(tweet);
            ApplicationBar.Buttons.Add(cancel);

            #endregion

            UpdateRemainingCharacters();
        }


        private void CancelClick(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }


        private void TweetClick(object sender, EventArgs e)
        {
            PostTweet();
        }

        private void PostTweet()
        {
            if (String.IsNullOrEmpty(TweetTextBox.Content))
                return;

            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            var twitter = new TwitterHelper(_twitterSettings);

            // Successful event handler, navigate back if successful
            twitter.TweetCompletedEvent += (sender, e) =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            };

            // Failed event handler, show error
            twitter.ErrorEvent += (sender, e) =>
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                MessageBox.Show("Connection to Twitter was unsuccessful.");
            };

            twitter.NewTweet(TweetTextBox.Content);
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _twitterSettings = Helper.LoadSetting<TwitterAccess>(Constants.TwitterAccess);
            if (_twitterSettings == null) return;

            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = !String.IsNullOrEmpty(_twitterSettings.AccessToken) && !String.IsNullOrEmpty(_twitterSettings.AccessTokenSecret);

            var detailItem = Helper.LoadSetting<TweetPageData>(Constants.TweetPageFileName);
            if (detailItem != null)
            {
                TweetTextBox.Content = detailItem.Tweet;

                var tweetPage = new TweetPageData
                {
                    Tweet = ""
                };

                // Save the detailpage object which the detailpage will load up
                Helper.SaveSetting(Constants.TweetPageFileName, tweetPage);
            }
        }


        private void UpdateRemainingCharacters()
        {
            CharactersCountTextBlock.Text = String.Format("{0}", 140 - TweetTextBox.Content.Length);
            if ((int.Parse(CharactersCountTextBlock.Text) > 50) && (int.Parse(CharactersCountTextBlock.Text) <= 140))
            {
                CharactersCountTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(124, 124, 124, 124));
            }
            else
                if ((int.Parse(CharactersCountTextBlock.Text) <= 50) && (int.Parse(CharactersCountTextBlock.Text) > 25))
                {
                    CharactersCountTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(124, 255, 255, 0));
                }
                else
                    if ((int.Parse(CharactersCountTextBlock.Text) <= 25) && (int.Parse(CharactersCountTextBlock.Text) >= 0))
                    {
                        CharactersCountTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(124, 255, 0, 0));
                    }
        }


        private void TweetTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRemainingCharacters();
        }


        private void MessageTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Focus();
            }
        }

        private void CharactersCountTextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TweetTextBox.Focus();
        }



    }
}