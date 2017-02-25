using Microsoft.OneDrive.Sdk;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;

namespace Showcase
{
    public class OneDriveItemModel : INotifyPropertyChanged
    {
        private BitmapSource bitmap;

        public OneDriveItemModel(Item item)
        {
            this.Item = item;
        }

        public string Id
        {
            get
            {
                return this.Item == null ? null : this.Item.Id;
            }
        }

        public Item Item { get; private set; }

        public string Name
        {
            get
            {
                return this.Item.Name;
            }
        }

        //INotifyPropertyChanged members
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
