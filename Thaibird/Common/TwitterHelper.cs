using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Web;
using Thaibird.ViewModels;

namespace Thaibird.Common
{
    public class TwitterHelper
    {
        private const String MaxCount = "50";
        private readonly TwitterAccess _twitterSettings;
        private readonly bool _authorized;
        private readonly OAuthCredentials _credentials;
        private readonly RestClient _client;
        public event EventHandler TweetCompletedEvent;
        public event EventHandler LoadedCompleteEvent;
        public event EventHandler ErrorEvent;

        public TwitterHelper()
        {
            _twitterSettings = Helper.LoadSetting<TwitterAccess>(Constants.TwitterAccess);

            if (_twitterSettings == null || String.IsNullOrEmpty(_twitterSettings.AccessToken) ||
               String.IsNullOrEmpty(_twitterSettings.AccessTokenSecret))
            {
                return;
            }

            _authorized = true;

            _credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = TwitterSettings.ConsumerKey,
                ConsumerSecret = TwitterSettings.ConsumerKeySecret,
                Token = _twitterSettings.AccessToken,
                TokenSecret = _twitterSettings.AccessTokenSecret,
                Version = TwitterSettings.OAuthVersion,
            };

            _client = new RestClient
            {
                Authority = "http://api.twitter.com",
                HasElevatedPermissions = true
            };
        }

        public void LoadList(TwitterListType listType, long sinceId, string searchTerm)
        {
            switch (listType)
            {
                case TwitterListType.Statuses:
                    LoadStatuses(sinceId);
                    break;
                case TwitterListType.Mentions:
                    LoadMentions(sinceId);
                    break;
                case TwitterListType.DirectMessages:
                    LoadDirectMessages(sinceId);
                    break;
                case TwitterListType.Favorites:
                    LoadFavorites();
                    break;
                default:
                    return;
            }
        }

        private void LoadStatuses(long sinceId)
        {
            if (!_authorized)
            {
                if (LoadedCompleteEvent != null)
                    LoadedCompleteEvent(this, EventArgs.Empty);
                return;
            }

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = "/statuses/friends_timeline.xml",
            };

            request.AddParameter("count", MaxCount);

            if (sinceId != 0)
                request.AddParameter("since_id", sinceId.ToString());

            request.AddParameter("include_rts", "1");

            _client.BeginRequest(request, new RestCallback(TwitterGetStatusesCompleted));
        }

        public void LoadMentions(long sinceId)
        {
            if (!_authorized)
            {
                if (LoadedCompleteEvent != null)
                    LoadedCompleteEvent(this, EventArgs.Empty);

                return;
            }

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = "/statuses/mentions.xml",
            };

            request.AddParameter("count", MaxCount);

            if (sinceId != 0)
                request.AddParameter("since_id", sinceId.ToString());

            request.AddParameter("include_rts", "1");

            _client.BeginRequest(request, new RestCallback(TwitterGetMentionsCompleted));
        }

        private void TwitterGetMentionsCompleted(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Helper.ShowMessage(String.Format("Twitter Error: {0}", response.StatusCode));
                return;
            }

            var xmlElement = XElement.Parse(response.Content);
            var mentionsList = (from item in xmlElement.Elements("status")
                                select new ItemViewModel
                                {
                                    UserName = GetChildElementValue(item, "user", "screen_name"),
                                    DisplayUserName = GetChildElementValue(item, "user", "name"),
                                    TweetText = (string)item.Element("text"),
                                    CreatedDate = GetCreatedDate((string)item.Element("created_at")),
                                    Image = GetChildElementValue(item, "user", "profile_image_url"),
                                    Id = (long)item.Element("id"),
                                    NewTweet = true,
                                    Source = (string)item.Element("source"),
                                }).ToList();

            // Load cached file and add them but only up to 200 old items
            var oldItems = Helper.LoadSetting<List<ItemViewModel>>(Constants.MentionsFileName);
            if (oldItems != null)
            {
                var maxCount = (oldItems.Count < 200) ? oldItems.Count : 200;
                for (var i = 0; i < maxCount; i++)
                {
                    oldItems[i].NewTweet = false;
                    mentionsList.Add(oldItems[i]);
                }
            }

            Helper.SaveSetting(Constants.MentionsFileName, mentionsList);

            if (LoadedCompleteEvent != null)
                LoadedCompleteEvent(this, EventArgs.Empty);
        }

        public void LoadDirectMessages(long sinceId)
        {
            if (!_authorized)
            {
                if (LoadedCompleteEvent != null)
                    LoadedCompleteEvent(this, EventArgs.Empty);

                return;
            }

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = "/direct_messages.xml",
            };

            request.AddParameter("count", MaxCount);

            if (sinceId != 0)
                request.AddParameter("since_id", sinceId.ToString());

            _client.BeginRequest(request, new RestCallback(TwitterGetDirectMessagesCompleted));
        }

        private void TwitterGetDirectMessagesCompleted(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Helper.ShowMessage(String.Format("Twitter Error: {0}", response.StatusCode));
                return;
            }

            var xmlElement = XElement.Parse(response.Content);
            var mentionsList = (from item in xmlElement.Elements("direct_message")
                                select new ItemViewModel
                                {
                                    UserName = GetChildElementValue(item, "sender", "screen_name"),
                                    DisplayUserName = GetChildElementValue(item, "sender", "name"),
                                    TweetText = (string)item.Element("text"),
                                    CreatedDate = GetCreatedDate((string)item.Element("created_at")),
                                    Image = GetChildElementValue(item, "sender", "profile_image_url"),
                                    Id = (long)item.Element("id"),
                                    NewTweet = true,
                                    Source = (string)item.Element("source"),
                                }).ToList();

            // Load cached file and add them but only up to 200 old items
            var oldItems = Helper.LoadSetting<List<ItemViewModel>>(Constants.DirectMessagesFileName);
            if (oldItems != null)
            {
                var maxCount = (oldItems.Count < 200) ? oldItems.Count : 200;
                for (var i = 0; i < maxCount; i++)
                {
                    oldItems[i].NewTweet = false;
                    mentionsList.Add(oldItems[i]);
                }
            }

            Helper.SaveSetting(Constants.DirectMessagesFileName, mentionsList);

            if (LoadedCompleteEvent != null)
                LoadedCompleteEvent(this, EventArgs.Empty);
        }

        public void LoadFavorites()
        {
            if (!_authorized)
            {
                if (LoadedCompleteEvent != null)
                    LoadedCompleteEvent(this, EventArgs.Empty);

                return;
            }

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = "/favorites.xml",
            };

            _client.BeginRequest(request, new RestCallback(TwitterGetFavoritesCompleted));
        }

        private void TwitterGetFavoritesCompleted(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Helper.ShowMessage(String.Format("Twitter Error: {0}", response.StatusCode));
                return;
            }

            var xmlElement = XElement.Parse(response.Content);
            var list = (from item in xmlElement.Elements("status")
                        select new ItemViewModel
                        {
                            UserName = GetChildElementValue(item, "user", "screen_name"),
                            DisplayUserName = GetChildElementValue(item, "user", "name"),
                            TweetText = (string)item.Element("text"),
                            CreatedDate = GetCreatedDate((string)item.Element("created_at")),
                            Image = GetChildElementValue(item, "user", "profile_image_url"),
                            Id = (long)item.Element("id"),
                            NewTweet = true,
                            Source = (string)item.Element("source"),
                        }).ToList();

            if (list.Count > 0)
            {
                Helper.SaveSetting(Constants.FavoritesFileName, list);
            }

            if (LoadedCompleteEvent != null)
                LoadedCompleteEvent(this, EventArgs.Empty);
        }

        private void TwitterGetStatusesCompleted(RestRequest request, RestResponse response, object userstate)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Helper.ShowMessage(String.Format("Twitter Error: {0}", response.StatusCode));
                return;
            }

            var xmlElement = XElement.Parse(response.Content);
            var statusList = (from item in xmlElement.Elements("status")
                              select new ItemViewModel
                              {
                                  UserName = GetChildElementValue(item, "user", "screen_name"),
                                  DisplayUserName = GetChildElementValue(item, "user", "name"),
                                  TweetText = (string)item.Element("text"),
                                  CreatedDate = GetCreatedDate((string)item.Element("created_at")),
                                  Image = GetChildElementValue(item, "user", "profile_image_url"),
                                  Id = (long)item.Element("id"),
                                  NewTweet = true,
                                  Source = (string)item.Element("source"),
                              }).ToList();

            // Load cached file and add them but only up to 200 old items
            var oldItems = Helper.LoadSetting<List<ItemViewModel>>(Constants.StatusesFileName);
            if (oldItems != null)
            {
                var maxCount = (oldItems.Count < 200) ? oldItems.Count : 200;
                for (var i = 0; i < maxCount; i++)
                {
                    oldItems[i].NewTweet = false;
                    statusList.Add(oldItems[i]);
                }
            }

            Helper.SaveSetting(Constants.StatusesFileName, statusList);

            if (LoadedCompleteEvent != null)
                LoadedCompleteEvent(this, EventArgs.Empty);
        }

        private static string GetChildElementValue(XElement itemElement, string parentElement, string childElement)
        {
            var userElement = itemElement.Element(parentElement);
            if (userElement == null)
                return String.Empty;

            var iteem = userElement.Element(childElement);
            if (iteem == null)
                return String.Empty;

            return iteem.Value;
        }

        private static string GetCreatedDate(string createdDate)
        {
            DateTime date = Helper.ParseDateTime(createdDate);
            return date.ToShortDateString() + " " + date.ToShortTimeString();
        }


        public TwitterHelper(TwitterAccess settings)
        {
            _twitterSettings = settings;

            if (_twitterSettings == null || String.IsNullOrEmpty(_twitterSettings.AccessToken) ||
               String.IsNullOrEmpty(_twitterSettings.AccessTokenSecret))
            {
                return;
            }

            _authorized = true;

            _credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = TwitterSettings.ConsumerKey,
                ConsumerSecret = TwitterSettings.ConsumerKeySecret,
                Token = _twitterSettings.AccessToken,
                TokenSecret = _twitterSettings.AccessTokenSecret,
                Version = TwitterSettings.OAuthVersion,
            };

            _client = new RestClient
            {
                Authority = "http://api.twitter.com",
                HasElevatedPermissions = true
            };
        }

        public void NewTweet(string tweetText)
        {
            if (!_authorized)
            {
                if (ErrorEvent != null)
                    ErrorEvent(this, EventArgs.Empty);
                return;
            }

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = "/statuses/update.xml",
                Method = WebMethod.Post
            };

            request.AddParameter("status", tweetText);

            _client.BeginRequest(request, new RestCallback(NewTweetCompleted));
            App.ViewModel.IsDataLoaded = false;
        }

        private void NewTweetCompleted(RestRequest request, RestResponse response, object userstate)
        {
            // We want to ensure we are running on our thread UI
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    if (TweetCompletedEvent != null)
                        TweetCompletedEvent(this, EventArgs.Empty);
                }
                else
                {
                    if (ErrorEvent != null)
                        ErrorEvent(this, EventArgs.Empty);
                }
            });
        }

        public event EventHandler FavoriteCompletedEvent;

        public void FavoriteItem(long id)
        {
            if (!_authorized)
            {
                if (ErrorEvent != null)
                    ErrorEvent(this, EventArgs.Empty);
                return;
            }

            var path = String.Format("/favorites/create/{0}.xml", id);

            var request = new RestRequest
            {
                Credentials = _credentials,
                Path = path,
                Method = WebMethod.Post
            };

            _client.BeginRequest(request, new RestCallback(FavoriteItemCompleted));
        }

        private void FavoriteItemCompleted(RestRequest request, RestResponse response, object userstate)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Helper.ShowMessage(String.Format("Error calling Twitter : {0}", response.StatusCode));
                    if (ErrorEvent != null)
                        ErrorEvent(this, EventArgs.Empty);
                    return;
                }

                if (FavoriteCompletedEvent != null)
                    FavoriteCompletedEvent(this, EventArgs.Empty);
            });
        }

    }

}
