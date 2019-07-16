using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KidTube.Data;
using Windows.Storage;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace KidTube.DataModel
{
    class ApprovedChannelList
    {
        private static ApprovedChannelList _approvedChannelsList = new ApprovedChannelList();

        private ObservableCollection<Channel> _approvedChannels = new ObservableCollection<Channel>();
        public ObservableCollection<Channel> ApprovedChannels
        {
            get { return this._approvedChannels; }
        }

        public static async Task<IEnumerable<Channel>> GetApprovedChannelsAsync()
        {
            await _approvedChannelsList.LoadChannelsFromFile();

            return _approvedChannelsList.ApprovedChannels;
        }
        
        private async Task LoadChannelsFromFile()
        {
            if (this._approvedChannels.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/SavedChannelList.txt");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string fileText = await FileIO.ReadTextAsync(file);

            var jsonObject = JObject.Parse(fileText);
            foreach (var item in jsonObject["Result"])
            {
                Channel group = item.ToObject<Channel>();
                this.ApprovedChannels.Add(group);
            }

        }

        public static void deleteChannel(int index)
        {
            _approvedChannelsList.ApprovedChannels.RemoveAt(index);
        }

        public static void addChannel(Channel channel)
        {
            _approvedChannelsList.ApprovedChannels.Add(channel);
        }
    }
}
