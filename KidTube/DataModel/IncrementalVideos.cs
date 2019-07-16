using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using KidTube.Data;
using Windows.Foundation;
using KidTube.DataModel;


namespace KidTube.DataModel
{
    class IncrementalVideos : ObservableCollection<Video>, ISupportIncrementalLoading
    {
        public bool HasMoreItems { get; set; }
        public string ChannelId { get; set; }

        public IncrementalVideos(string channelId)
        {
            HasMoreItems = true;
            ChannelId = channelId;
            loadInitialVideos(channelId);
        }

        public async void loadInitialVideos(string channelId)
        {
            var videos = await ChannelDataSource.GetVideosAsync(channelId);
            foreach (var vid in videos)
            {
                Add(vid);
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return InnerLoadMoreItemsAsync(count).AsAsyncOperation();
        }

        private async Task<LoadMoreItemsResult> InnerLoadMoreItemsAsync(uint expectedCount)
        {
            var actualCount = 0;
            ObservableCollection<Video> videos = null;

            try
            {
                if (!ChannelDataSource.NextPageAvailable(ChannelId))
                    HasMoreItems = false;
                else
                    videos = ChannelDataSource.QuickLoadVideos(ChannelId);
            }
            catch (Exception)
            {
                HasMoreItems = false;
                throw;
            }

            if (videos != null && videos.Any())
            {
                foreach (var video in videos)
                    Add(video);
            }
            else
            {
                HasMoreItems = false;
            }

            return new LoadMoreItemsResult
            {
                Count = (uint)actualCount
            };
            
        }
    }
}
