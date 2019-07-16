using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using KidTube.DataModel;
using Windows.Networking.Connectivity;
using System.Xml;
using MyToolkit.Multimedia;
using Windows.UI.Popups;


// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.

namespace KidTube.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class Video
    {
        public Video(String id, String title, String description, String smallimagePath, String largeImagePath, String channelId)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.SmallImagePath = smallimagePath;
            this.LargeImagePath = largeImagePath;
            this.ChannelId = channelId;


        }

        public string Id { get; private set; }
        public string Title { get; set; }
        public string Time { get; set; }
        public string Description { get; private set; }
        public string SmallImagePath { get; private set; }
        public string LargeImagePath { get; private set; }
        public string ChannelId { get; private set; }
        public string NextPage { get; set; }
        public object ColSpan = 1;
        public object RowSpan = 1;


        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class Channel
    {
        public Channel(String id, String title, String description, String imagePath)
        {
            this.Id = id;
            this.Title = title;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Videos = new ObservableCollection<Video>();
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string NextPage { get; set; }
        public int TotalVids { get; set; }
        public ObservableCollection<Video> Videos { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class ChannelDataSource
    {

        private static ChannelDataSource _channelDataSource = new ChannelDataSource();

        private ObservableCollection<Channel> _groups = new ObservableCollection<Channel>();
        public ObservableCollection<Channel> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<Channel>> GetChannelsAsync()
        {
            await _channelDataSource.GetSampleDataAsync();

            return _channelDataSource.Groups;
        }

        public static async Task<Channel> GetChannelAsync(string uniqueId)
        {
            await _channelDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _channelDataSource.Groups.Where((group) => group.Id.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();

            return null;
        }

        public static async Task<Video> GetVideoAsync(string uniqueId)
        {
            await _channelDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _channelDataSource.Groups.SelectMany(group => group.Videos).Where((item) => item.Id.Equals(uniqueId));
            if (matches.Count() >= 1) return matches.First();

            return null;
        }

        public async Task GetSampleDataAsync()
        {
            if (this._groups.Count != 0)
                return;

            Settings.VideoQuality = YouTubeQuality.Quality720P;

            string channelIDs = await GetChannelIdFromFile();
            if (channelIDs == null)
                channelIDs = await GetChannelIdFromDefault();

            string channelResults = GetChannelInfo(channelIDs);
            loadChannelsFromJSON(channelResults);

        }

        public static string GetChannelInfo(string channelIds)
        {

            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/channels?part=snippet&id=" +
                channelIds + "&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            return result;
        }

        public static string GetChannelVideos(string channelId)
        {
            Channel channel = _channelDataSource.Groups.Where((group) =>
                group.Id.Equals(channelId)).FirstOrDefault();
            string nextPage = channel.NextPage;
            string maxResults = "10";
            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=" +
                channelId + "&pageToken=" + nextPage + "&safeSearch=strict&type=video&maxResults=" + maxResults + "&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            return result;
        }

        //public static string GetChannelVideos(string channelId, string nextPage)
        //{

        //    string maxResults = "10";
        //    var httpClient = new HttpClient();
        //    string query = "https://www.googleapis.com/youtube/v3/search?part=snippet&channelId=" +
        //        channelId + "&pageToken=" +
        //        nextPage + "&safeSearch=strict&maxResults=" +
        //        maxResults + "&type=video&key=" +
        //        Constants.ApiKey;
        //    string result = httpClient.GetStringAsync(query).Result;

        //    return result;
        //}

        //public static async Task<IEnumerable<Channel>> GetThreeGroups()
        //{
        //    if (_sampleDataSource._groups.Count == 0)
        //    {
        //        await GetChannelsAsync();
        //    }
        //    var justThree = new ObservableCollection<Channel>();

        //    int numOfChannels = _sampleDataSource.Groups.Count;

        //    Random random = new Random();
        //    for (int index = 0; index < 3; index++)
        //    {

        //        int rando = random.Next(numOfChannels) + 1;
        //        if (_sampleDataSource.Groups.ElementAt(rando).Videos.Count == 0)
        //        {
        //            loadVideos(_sampleDataSource._groups.ElementAt(rando).Id);
        //        }
        //        justThree.Add(_sampleDataSource.Groups.ElementAt(rando));
        //    }


        //    return justThree;
        //}

        public static async void deleteChannel(int index)
        {
            string channelId = _channelDataSource.Groups.ElementAt(index).Id;
            _channelDataSource.Groups.RemoveAt(index);

            string localData = await ReadUserFile();

            localData = localData.Replace(channelId, string.Empty);

            await ChannelDataSource.saveChannelIdToFile(localData);
        }

        public static async Task<string> addChannel(Channel channel)
        {
            for (int i = 0; i < _channelDataSource.Groups.Count; i++)
            {
                if (_channelDataSource.Groups.ElementAt(i).Id == channel.Id)
                {
                    return "The channel is already in your list";
                }
            }

            _channelDataSource.Groups.Add(channel);
            string localData = await ReadUserFile();
            localData = localData + "," + channel.Id;

            await ChannelDataSource.saveChannelIdToFile(localData);

            return "Added channel " + channel.Title;
        }

        public static async Task saveChannelIdToFile(string channelIdList)
        {
            //string localData = JsonConvert.SerializeObject(SampleDataSource.GetGroupsAsync());
            ////channelList.Text = localData;

            ////todo check roamingFolder size
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile localFile = await roamingFolder.CreateFileAsync("userChannelID.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(localFile, channelIdList);
        }

        public static async Task<string> readChannelsFromFile()
        {
            try
            {
                var roamingFolder = ApplicationData.Current.RoamingFolder;
                StorageFile localFile;
                localFile = await roamingFolder.GetFileAsync("userChannelID.txt");

                string localData = await FileIO.ReadTextAsync(localFile);
                return localData;
            }
            catch
            {
                return null;
            }

        }

        public void loadChannelsFromJSON(string file)
        {
            var jsonObject = JObject.Parse(file);
            foreach (var item in jsonObject["items"])
            {
                Channel group = new Channel((string)item["id"],
                                               (string)item["snippet"]["title"],
                                               (string)item["snippet"]["description"],
                                               (string)item["snippet"]["thumbnails"]["high"]["url"]);
                this.Groups.Add(group);
            }
        }

        public static void loadVideos(string channelId)
        {
            Channel channel = _channelDataSource.Groups.Where((group) =>
                group.Id.Equals(channelId)).FirstOrDefault();
            if (channel.Videos.Count == 0)
            {
                string videos = GetChannelVideos(channel.Id);
                JObject videoObject = JObject.Parse(videos);

                var index = _channelDataSource.Groups.IndexOf(channel);

                string nextPage = (string)videoObject["nextPageToken"];
                _channelDataSource.Groups.ElementAt(index).NextPage = nextPage;

                string videoIds = string.Empty;

                foreach (var item in videoObject["items"])
                {
                    _channelDataSource.Groups.ElementAt(index).Videos.Add(new Video((string)item["id"]["videoId"],
                                                            (string)item["snippet"]["title"],
                                                            (string)item["snippet"]["description"],
                                                            (string)item["snippet"]["thumbnails"]["medium"]["url"],
                                                            (string)item["snippet"]["thumbnails"]["high"]["url"],
                                                            channel.Id));
                    videoIds += (string)item["id"]["videoId"];
                    videoIds += ",";
                }

                var httpClient = new HttpClient();
                string query = "https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id=" +
                    videoIds + "&key=" + Constants.ApiKey;
                string result = httpClient.GetStringAsync(query).Result;

                JObject contentDetails = JObject.Parse(result);

                int vidIndex = 0;
                foreach (var item in contentDetails["items"])
                {
                    if (_channelDataSource.Groups.ElementAt(index).Videos.ElementAt(vidIndex).Id.Equals((string)item["id"]))
                    {
                        TimeSpan duration = XmlConvert.ToTimeSpan((string)item["contentDetails"]["duration"]);
                        string displayTime = string.Empty;
                        if (duration.Hours > 0)
                            displayTime += duration.Hours.ToString() + ":";

                        if (duration.Minutes > 0)
                            displayTime += duration.Minutes.ToString() + ":";

                        if (duration.Seconds < 1)
                        {
                            displayTime += "00";
                        }
                        else if (duration.Seconds < 10 && duration.Seconds > 1)
                        {
                            displayTime += "0" + duration.Seconds.ToString();
                        }
                        else if (duration.Seconds >= 10)
                        {
                            displayTime += duration.Seconds.ToString();
                        }





                        _channelDataSource.Groups.ElementAt(index).Videos.ElementAt(vidIndex).Time = displayTime;
                    }

                    ++vidIndex;

                }

            }

            //_sampleDataSource.Groups.Where((group) => group.Id.Equals(id)).First() = channel;
        }

        public async static Task<ObservableCollection<Video>> GetVideosAsync(String id)
        {
            //Check if vidoes are loaded on channel
            Channel channel = _channelDataSource.Groups.Where((group) => group.Id.Equals(id)).FirstOrDefault();
            if (channel.Videos.Count == 0)
            {
                loadVideos(id);
            }

            return channel.Videos;
        }

        public async Task<string> GetChannelIdFromFile()
        {
            try
            {

                var roamingFolder = ApplicationData.Current.RoamingFolder;
                StorageFile localFile;
                localFile = await roamingFolder.GetFileAsync("userChannelID.txt");

                string localData = await FileIO.ReadTextAsync(localFile);

                return localData;
            }
            catch
            {
                return null;
            }

        }

        public async Task<string> GetChannelIdFromDefault()
        {
            Uri dataUri = new Uri("ms-appx:///DataModel/DefaultChannelID.txt");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string fileText = await FileIO.ReadTextAsync(file);

            //save defualt to roamingFolder
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile localFile = await roamingFolder.CreateFileAsync("userChannelID.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(localFile, fileText);

            return fileText;
        }

        public static async Task<string> ReadUserFile()
        {
            var roamingFolder = ApplicationData.Current.RoamingFolder;
            StorageFile localFile;
            localFile = await roamingFolder.GetFileAsync("userChannelID.txt");

            string localData = await FileIO.ReadTextAsync(localFile);
            return localData;
        }

        public static async Task<ObservableCollection<Video>> GetRandomVideosAsync()
        {

            ObservableCollection<Video> randomVideos = new ObservableCollection<Video>();

            int count = _channelDataSource.Groups.Count;
            List<int> candidates = new List<int>();
            for (int i = 0; i <= count; i++)
            {
                candidates.Add(i);
            }
            Random rando = new Random();
            for (int i = 0; i < 4 && i < count; i++)
            {
                int randomNum = rando.Next(candidates.Count - 1);
                int random = candidates[randomNum];
                candidates.RemoveAt(randomNum);
                for (int j = 0; j < 5; j++)
                {
                    if (_channelDataSource.Groups.ElementAt(random).Videos.Count == 0)
                        loadVideos(_channelDataSource.Groups.ElementAt(random).Id);

                    if (_channelDataSource._groups.ElementAt(random).Videos.Count() >= j)
                        randomVideos.Add(_channelDataSource._groups.ElementAt(random).Videos.ElementAt(j));
                }
            }

            return randomVideos;





        }

        //public static void LoadNextPage(string channelId)
        //{
        //    Channel channel = _channelDataSource.Groups.Where((group) =>
        //        group.Id.Equals(channelId)).FirstOrDefault();

        //    string videos = GetChannelVideos(channelId, channel.NextPage);
        //    JObject videoObject = JObject.Parse(videos);

        //    var index = _channelDataSource.Groups.IndexOf(channel);

        //    string nextPage = (string)videoObject["nextPageToken"];
        //    _channelDataSource.Groups.ElementAt(index).NextPage = nextPage;

        //    string videoIds = string.Empty;
        //    foreach (var item in videoObject["items"])
        //    {
        //        _channelDataSource.Groups.ElementAt(index).Videos.Add(new Video((string)item["id"]["videoId"],
        //                                                (string)item["snippet"]["title"],
        //                                                (string)item["snippet"]["description"],
        //                                                (string)item["snippet"]["thumbnails"]["medium"]["url"],
        //                                                (string)item["snippet"]["thumbnails"]["high"]["url"],
        //                                                channel.Id));
        //        videoIds += (string)item["id"]["videoId"];
        //        videoIds += ",";
        //    }

        //    var httpClient = new HttpClient();
        //    string query = "https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id=" +
        //        videoIds + "&key=" + Constants.ApiKey;
        //    string result = httpClient.GetStringAsync(query).Result;

        //    JObject contentDetails = JObject.Parse(result);

        //    int vidIndex = 0;
        //    foreach (var item in contentDetails["items"])
        //    {
        //        if (_channelDataSource.Groups.ElementAt(index).Videos.ElementAt(vidIndex).Id.Equals((string)item["id"]))
        //        {
        //            TimeSpan duration = XmlConvert.ToTimeSpan((string)item["contentDetails"]["duration"]);
        //            string displayTime = string.Empty;
        //            if (duration.Hours > 0)
        //                displayTime += duration.Hours.ToString() + ":";

        //            if (duration.Minutes > 0)
        //                displayTime += duration.Minutes.ToString() + ":";

        //            if (duration.Seconds > 0)
        //                displayTime += duration.Seconds.ToString();

        //            _channelDataSource.Groups.ElementAt(index).Videos.ElementAt(vidIndex).Time = displayTime;
        //        }

        //        ++vidIndex;

        //    }


        //}

        public static ObservableCollection<Video> QuickLoadVideos(string channelId)
        {
            Channel channel = _channelDataSource.Groups.Where((group) =>
                group.Id.Equals(channelId)).FirstOrDefault();

            var videos = new ObservableCollection<Video>();

            string videoResults = GetChannelVideos(channel.Id);
            JObject videoObject = JObject.Parse(videoResults);

            var index = _channelDataSource.Groups.IndexOf(channel);

            string nextPage = (string)videoObject["nextPageToken"];
            if (nextPage != null)
                _channelDataSource.Groups.ElementAt(index).NextPage = nextPage;
            else

                _channelDataSource.Groups.ElementAt(index).NextPage = "end";


            string videoIds = string.Empty;
            foreach (var item in videoObject["items"])
            {
                videos.Add(new Video((string)item["id"]["videoId"],
                                     (string)item["snippet"]["title"],                  
                                     (string)item["snippet"]["description"],
                                     (string)item["snippet"]["thumbnails"]["medium"]["url"],
                                     (string)item["snippet"]["thumbnails"]["high"]["url"],
                                     channel.Id));
                videoIds += (string)item["id"]["videoId"];
                videoIds += ",";
            }

            string result = VideoDurationResults(videoIds);

            JObject contentDetails = JObject.Parse(result);

            int vidIndex = 0;
            foreach (var item in contentDetails["items"])
            {
                if (videos.ElementAt(vidIndex).Id.Equals((string)item["id"]))
                {
                    string displayTime = DisplayTime((string)item["contentDetails"]["duration"]);
                    videos.ElementAt(vidIndex).Time = displayTime;
                }

                ++vidIndex;

            }

            foreach (var vid in videos)
            {
                _channelDataSource.Groups.Where((group) =>
                group.Id.Equals(channelId)).FirstOrDefault().Videos.Add(vid);
            }

            return videos;
        }

        public static bool NextPageAvailable(string channelId)
        {
            Channel channel = _channelDataSource.Groups.Where((group) =>
                group.Id.Equals(channelId)).FirstOrDefault();

            if (channel.NextPage == "end")
                return false;
            else
                return true;
        }

        public static string VideoDurationResults(string videoIds)
        {
            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/videos?part=contentDetails&id=" +
                videoIds + "&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            return result;
        }

        public static string VideoSnippetResults(string videoIds)
        {
            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/videos?part=snippet&id=" +
                videoIds + "&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            return result;
        }

        public static ObservableCollection<Video> SearchChannelResults(string ChannelId, string q)
        {
            var httpClient = new HttpClient();
            string query = "https://www.googleapis.com/youtube/v3/search?safeSearch=strict&part=id&" +
                "q=" + q + "&channelId=" + ChannelId + "&type=video&maxResults=20&key=" + Constants.ApiKey;
            string result = httpClient.GetStringAsync(query).Result;

            JObject idResults = JObject.Parse(result);

            string videoIds = string.Empty;

            foreach (var id in idResults["items"])
            {
                videoIds += id["id"]["videoId"];
                videoIds += ",";
            }
            
            //snippet
            string videoResults = VideoSnippetResults(videoIds);
            JObject videoSnippet = JObject.Parse(videoResults);

            

            var searchResults = new ObservableCollection<Video>();

            
            foreach (var video in videoSnippet["items"])
            {
                var searchVideo = new Video((string)video["id"],
                                            (string)video["snippet"]["title"],
                                            (string)video["snippet"]["description"],
                                            (string)video["snippet"]["thumbnails"]["medium"]["url"],
                                            (string)video["snippet"]["thumbnails"]["high"]["url"],
                                            (string)video["snippet"]["channelId"]);

                searchResults.Add(searchVideo);
            }

            //duration
            string durationResults = VideoDurationResults(videoIds);
            JObject videoDurations = JObject.Parse(durationResults);

            int vidIndex = 0;
            foreach (var item in videoDurations["items"])
            {
                if (searchResults.ElementAt(vidIndex).Id.Equals((string)item["id"]))
                {
                    string displayTime = DisplayTime((string)item["contentDetails"]["duration"]);
                    searchResults.ElementAt(vidIndex).Time = displayTime;
                }

                ++vidIndex;

            }


            return searchResults;

        }

        private static string DisplayTime(string time)
        {

            TimeSpan duration = XmlConvert.ToTimeSpan(time);
            string displayTime = string.Empty;
            if (duration.Hours > 0)
                displayTime += duration.Hours.ToString() + ":";

            if (duration.Minutes > 0)
                displayTime += duration.Minutes.ToString() + ":";

            if (duration.Minutes < 1)
                displayTime += "0:";

            if (duration.Seconds == 0)
            {
                displayTime += "00";
            }
            else if (duration.Seconds < 10 && duration.Seconds > 0)
            {
                displayTime += "0" + duration.Seconds.ToString();
            }
            else if (duration.Seconds > 10)
            {
                displayTime += duration.Seconds.ToString();
            }

            return displayTime;
        }

    }
}