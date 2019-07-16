using KidTube.Common;
using KidTube.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Multimedia;
using KidTube.DataModel;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace KidTube
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class ItemDetailPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public string VideoId { get; set; }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ItemDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            try
            {
                var video = (Video)e.NavigationParameter;
                var channelVids = await ChannelDataSource.GetChannelAsync(video.ChannelId);

                this.defaultViewModel["Channel"] = new IncrementalVideos(video.ChannelId);
                this.DefaultViewModel["Item"] = video;


                string videoId = video.Id;
                this.VideoId = videoId;
                var url = await YouTube.GetVideoUriAsync(videoId, Settings.VideoQuality);
                if (url != null)
                {
                    this.mediaPlayer.Source = url.Uri;
                    this.mediaPlayer.Pause();
                }
            }
            catch
            {
                pageTitle.Text = "Video Didn't Load";
            }

            if (VideoId != null)
            {
                SetPlayerSize();
            }

            Window.Current.SizeChanged += OnWindowSizeChanged;
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

        private void approvedList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedVideo = (Video)e.ClickedItem;
            this.Frame.Navigate(typeof(ItemDetailPage), selectedVideo);
        }



        void mediaPlayer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            mediaPlayer.IsFullWindow = !mediaPlayer.IsFullWindow;

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

        private void Quality_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem selectedItem = sender as MenuFlyoutItem;

            if (selectedItem != null)
            {
                if (selectedItem.Tag.ToString() == "480p")
                {
                    Settings.VideoQuality = YouTubeQuality.Quality480P;
                    PlayWithUpdateQuality();
                }

                else if (selectedItem.Tag.ToString() == "720p")
                {
                    Settings.VideoQuality = YouTubeQuality.Quality720P;
                    PlayWithUpdateQuality();
                }
                else if (selectedItem.Tag.ToString() == "1080p")
                {
                    Settings.VideoQuality = YouTubeQuality.Quality1080P;
                    PlayWithUpdateQuality();
                }
            }
        }

        private async void PlayWithUpdateQuality()
        {
            var CurPosition = this.mediaPlayer.Position;
            var url = await YouTube.GetVideoUriAsync(VideoId, Settings.VideoQuality);
            if (url != null)
            {
                this.mediaPlayer.Source = url.Uri;
                this.mediaPlayer.StartupPosition = CurPosition;
            }
        }

        private void mediaPlayer_Ended(object sender, RoutedEventArgs e)
        {
            if(mediaPlayer.IsFullWindow)
                mediaPlayer.IsFullWindow = !mediaPlayer.IsFullWindow;
        }

        private void mediaPlayer_keyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                if (mediaPlayer.CurrentState == MediaElementState.Playing)
                    mediaPlayer.Pause();
                else
                    mediaPlayer.Play();
            }
        }

        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            SetPlayerSize();                
        }

        private void SetPlayerSize()
        {
            if ((mainRow.ActualHeight * 1.77777777) < Window.Current.Bounds.Width)
            {
                mediaPlayer.Height = mainRow.ActualHeight - 10;
                mediaPlayer.Width = mediaPlayer.Height * 1.777777777;
            }
            else
            {
                mediaPlayer.Width = Window.Current.Bounds.Width;
                mediaPlayer.Height = mediaPlayer.Width / 1.7777777;
            }
        }

    }


}
