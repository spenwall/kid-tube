using KidTube.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using KidTube.Data;
using KidTube.DataModel;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Net.Http;
using Newtonsoft.Json;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace KidTube
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ApprovedChannels : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public ApprovedChannels()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //string channels = await ApprovedChannelList.ReadChannelListFile();
            //var jsonArray = JArray.Parse(channels);

            //ObservableCollection<Channels> approvedChannels = new ObservableCollection<Channels>();
            //foreach (var channel in jsonArray)
            //{
            //    approvedChannels.Add(channel.ToObject<Channels>());
            //}


            itemViewSource.Source = await ChannelDataSource.GetChannelsAsync();

        }


        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (approvedList.SelectedItems.Count > 0)
            {
                ChannelDataSource.deleteChannel(approvedList.SelectedIndex);
            }


        }

        private void channelSearch_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&type=channel&q=" + channelSearch.QueryText + "&safeSearch=strict&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            JObject jObject = JObject.Parse(result);

            var searchChannelsList = new ObservableCollection<Channel>();
            foreach (var channel in jObject["items"])
            {
                Channel channelSearchResult = new Channel((string)channel["id"]["channelId"],
                                                            (string)channel["snippet"]["title"],
                                                            (string)channel["snippet"]["description"],
                                                            (string)channel["snippet"]["thumbnails"]["default"]
                                                            ["url"]);
                searchChannelsList.Add(channelSearchResult);
            }

            seachViewSource.Source = searchChannelsList;
        }

        private async void add_Click(object sender, RoutedEventArgs e)
        {
            if (channelSearchList.SelectedItems.Count > 0)
            {
                var tempChannel = (Channel)channelSearchList.SelectedItem;
                if (tempChannel.TotalVids > 0)
                    message.Text = await ChannelDataSource.addChannel((Channel)channelSearchList.SelectedItem);
                else
                    message.Text = "Channel Cannot be added. There are no Video in Channel";
            }
        }

        private void channelSearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Channel selectedChannel = (Channel)channelSearchList.SelectedItem;
            if (selectedChannel != null)
            {
                var httpClient = new HttpClient();
                string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=" + selectedChannel.Id + "&safeSearch=strict&type=video&key=" + Constants.ApiKey;
                string result = httpClient.GetStringAsync(query).Result;

                JObject jObject = JObject.Parse(result);

                selectedChannel.TotalVids = (int)jObject["pageInfo"]["totalResults"];

                if (selectedChannel != null)
                {
                    prevTitle.Text = selectedChannel.Title;
                    prevTotalvids.Text = "Available Videos: " + selectedChannel.TotalVids;
                    prevImage.Source = new BitmapImage(new Uri(selectedChannel.ImagePath));
                    prevDescription.Text = selectedChannel.Description;
                }
            }

        }

        private void toAllChannels_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AllChannelsPage));
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GroupedItemsPage));
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ApprovedChannels));
        }
    }
}
