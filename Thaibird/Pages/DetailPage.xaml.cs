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
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Thaibird.Common;
using Microsoft.Phone.Tasks;

namespace Thaibird.Pages
{
    public partial class DetailPage
    {
        private DetailPageData _detailItem;
        public DetailPage()
        {
            InitializeComponent();

            #region Application Bar
            ApplicationBar = new ApplicationBar { IsMenuEnabled = true, IsVisible = true, Opacity = .9 };

            var favorite = new ApplicationBarIconButton(new Uri("/Images/fav.png", UriKind.Relative));
            favorite.Text = "favorite";
            favorite.Click += FavoriteClick;

            var sendEmail = new ApplicationBarIconButton(new Uri("/Images/mail.png", UriKind.Relative));
            sendEmail.Text = "E-mail tweet";
            sendEmail.Click += SendMailClick;

            var retweet = new ApplicationBarIconButton(new Uri("/Images/retweet.png", UriKind.Relative));
            retweet.Text = "retweet";
            retweet.Click += RetweetClick;

            var reply = new ApplicationBarIconButton(new Uri("/Images/reply.png", UriKind.Relative));
            reply.Text = "reply";
            reply.Click += ReplyClick;

            var tweet = new ApplicationBarMenuItem("Send direct message");
            tweet.Click += SendDirectMessageClick;
            ApplicationBar.MenuItems.Add(tweet);

            //var browserItem = new ApplicationBarMenuItem("Open in Internet Explorer");
            //browserItem.Click += OpenInBrowserItemClick;
            //browserItem.IsEnabled = false;


            ApplicationBar.Buttons.Add(favorite);
            ApplicationBar.Buttons.Add(sendEmail);
            ApplicationBar.Buttons.Add(retweet);
            ApplicationBar.Buttons.Add(reply);
            //ApplicationBar.MenuItems.Add(browserItem);
            #endregion

            Loaded += Post_Loaded;
        }

        private void FavoriteClick(object sender, EventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;

            var twitter = new TwitterHelper();
            twitter.FavoriteCompletedEvent += (sender2, e2) =>
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
                MessageBox.Show("Item added to favorites");
            };
            twitter.ErrorEvent += (sender2, e2) =>
            {
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
            };
            twitter.FavoriteItem(_detailItem.Id);
        }



        private void SendMailClick(object sender, EventArgs e)
        {
            var task = new EmailComposeTask
            {
                Body = _detailItem.Text,
                Subject = String.Format("Tweet by {0}", _detailItem.UserDisplayName)
            };
            task.Show();
        }

        private void RetweetClick(object sender, EventArgs e)
        {
            var tweetPage = new TweetPageData
            {
                Tweet = String.Format("RT @{0} {1}", _detailItem.UserName, _detailItem.Text)
            };

            // Save the detailpage object which the detailpage will load up
            Helper.SaveSetting(Constants.TweetPageFileName, tweetPage);

            NavigationService.Navigate(new Uri("/Pages/TweetPage.xaml", UriKind.Relative));
        }

        private void ReplyClick(object sender, EventArgs e)
        {
            var tweetPage = new TweetPageData
            {
                Tweet = String.Format("@{0} ", _detailItem.UserName)
            };

            // Save the detailpage object which the detailpage will load up
            Helper.SaveSetting(Constants.TweetPageFileName, tweetPage);

            NavigationService.Navigate(new Uri("/Pages/TweetPage.xaml", UriKind.Relative));
        }

        private void SendDirectMessageClick(object sender, EventArgs e)
        {
            var tweetPage = new TweetPageData
            {
                Tweet = String.Format("d {0} ", _detailItem.UserName),
            };

            Helper.SaveSetting(Constants.TweetPageFileName, tweetPage);

            NavigationService.Navigate(new Uri("/Pages/TweetPage.xaml", UriKind.Relative));
        }

        //private void OpenInBrowserItemClick(object sender, EventArgs e)
        //{
        //    var task = new WebBrowserTask { URL = WebBrowser.Source.ToString() };
        //    task.Show();
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _detailItem = Helper.LoadSetting<DetailPageData>(Constants.DetailPageFileName);
            if (_detailItem == null)
            {
                MessageBox.Show("Error loading page data");
                return;
            }

            PageTitle.Text = _detailItem.UserDisplayName;
        }

        private void Post_Loaded(object sender, RoutedEventArgs e)
        {
            //var html = new StringBuilder();
            
            //html.Append(String.Format("{0}<br><br>", _detailItem.CreatedDate));
            //html.Append(MakeLinks(_detailItem.Text));
            //if (!String.IsNullOrEmpty(_detailItem.Source))
            //    html.Append(String.Format("<br><br>Source: {0}", _detailItem.Source));



            twitterTime.Text = _detailItem.CreatedDate;

            if (!String.IsNullOrEmpty(_detailItem.Source))
            {
                twitterClient.Text = "from: " + RemoveHtml(_detailItem.Source);
            }
            
            twitterText.Text = _detailItem.Text;

            string[] links = FindLinks(_detailItem.Text);
            
            addLinks2btn(links.Distinct().ToArray());

            //WebBrowser.NavigateToString(html.ToString());
        }

        private static string RemoveHtml(string txt)
        {
            return Regex.Replace(txt, @"<(.|\n)*?>", string.Empty);
        }

        private static string[] FindLinks(string txt)
        {
            var regx = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            var matches = regx.Matches(txt);

            string[] links = new string[matches.Count];

            int i = 0;
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                links[i] = match.Value;
                i++;
            }

            return links;
        }

        private void addLinks2btn(string[] links)
        {
            switch(links.Count())
            {
                case 0:
                    break;
                case 1:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    break;
                case 2:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    break;
                case 3:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    break;
                case 4:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    hyperlinkButton4.Content = links[3].ToString();
                    hyperlinkButton4.NavigateUri = new Uri(links[3].ToString());
                    hyperlinkButton4.TargetName = "_blank";
                    break;
                case 5:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    hyperlinkButton4.Content = links[3].ToString();
                    hyperlinkButton4.NavigateUri = new Uri(links[3].ToString());
                    hyperlinkButton4.TargetName = "_blank";
                    hyperlinkButton5.Content = links[4].ToString();
                    hyperlinkButton5.NavigateUri = new Uri(links[4].ToString());
                    hyperlinkButton5.TargetName = "_blank";
                    break;
                case 6:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    hyperlinkButton4.Content = links[3].ToString();
                    hyperlinkButton4.NavigateUri = new Uri(links[3].ToString());
                    hyperlinkButton4.TargetName = "_blank";
                    hyperlinkButton5.Content = links[4].ToString();
                    hyperlinkButton5.NavigateUri = new Uri(links[4].ToString());
                    hyperlinkButton5.TargetName = "_blank";
                    hyperlinkButton6.Content = links[5].ToString();
                    hyperlinkButton6.NavigateUri = new Uri(links[5].ToString());
                    hyperlinkButton6.TargetName = "_blank";
                    break;
                case 7:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    hyperlinkButton4.Content = links[3].ToString();
                    hyperlinkButton4.NavigateUri = new Uri(links[3].ToString());
                    hyperlinkButton4.TargetName = "_blank";
                    hyperlinkButton5.Content = links[4].ToString();
                    hyperlinkButton5.NavigateUri = new Uri(links[4].ToString());
                    hyperlinkButton5.TargetName = "_blank";
                    hyperlinkButton6.Content = links[5].ToString();
                    hyperlinkButton6.NavigateUri = new Uri(links[5].ToString());
                    hyperlinkButton6.TargetName = "_blank";
                    hyperlinkButton7.Content = links[6].ToString();
                    hyperlinkButton7.NavigateUri = new Uri(links[6].ToString());
                    hyperlinkButton7.TargetName = "_blank";
                    break;
                case 8:
                    hyperlinkButton1.Content = links[0].ToString();
                    hyperlinkButton1.NavigateUri = new Uri(links[0].ToString());
                    hyperlinkButton1.TargetName = "_blank";
                    hyperlinkButton2.Content = links[1].ToString();
                    hyperlinkButton2.NavigateUri = new Uri(links[1].ToString());
                    hyperlinkButton2.TargetName = "_blank";
                    hyperlinkButton3.Content = links[2].ToString();
                    hyperlinkButton3.NavigateUri = new Uri(links[2].ToString());
                    hyperlinkButton3.TargetName = "_blank";
                    hyperlinkButton4.Content = links[3].ToString();
                    hyperlinkButton4.NavigateUri = new Uri(links[3].ToString());
                    hyperlinkButton4.TargetName = "_blank";
                    hyperlinkButton5.Content = links[4].ToString();
                    hyperlinkButton5.NavigateUri = new Uri(links[4].ToString());
                    hyperlinkButton5.TargetName = "_blank";
                    hyperlinkButton6.Content = links[5].ToString();
                    hyperlinkButton6.NavigateUri = new Uri(links[5].ToString());
                    hyperlinkButton6.TargetName = "_blank";
                    hyperlinkButton7.Content = links[6].ToString();
                    hyperlinkButton7.NavigateUri = new Uri(links[6].ToString());
                    hyperlinkButton7.TargetName = "_blank";
                    hyperlinkButton8.Content = links[7].ToString();
                    hyperlinkButton8.NavigateUri = new Uri(links[7].ToString());
                    hyperlinkButton8.TargetName = "_blank";
                    break;
                default:
                    break;
            }
        }

        private static string MakeLinks(string txt)
        {
            var regx = new Regex(@"http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?", RegexOptions.IgnoreCase);
            var matches = regx.Matches(txt);

            return matches.Cast<Match>().Aggregate(txt, (current, match) => current.Replace(match.Value, "<a href='" + match.Value + "'>" + match.Value + "</a>"));
        }

        private void WebBrowserNavigating(object sender, NavigatingEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[1]).IsEnabled = true;
        }

        private void WebBrowserNavigated(object sender, NavigationEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
        }

    }
}