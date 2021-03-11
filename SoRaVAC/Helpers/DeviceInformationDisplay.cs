using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Media.Imaging;

namespace SoRaVAC.Helpers
{
    public class DeviceInformationDisplay : INotifyPropertyChanged
    {
        public DeviceInformationDisplay(DeviceInformation deviceInfoIn)
        {
            this.DeviceInformation = deviceInfoIn;
            UpdateGlyphBitmapImage();
        }

        public static async Task<DeviceInformationDisplay> GetDeviceFromIdAsync(string deviceId)
        {
            DeviceInformation deviceInfoIn = await DeviceInformation.CreateFromIdAsync(deviceId);
            return new DeviceInformationDisplay(deviceInfoIn);
        }

        public DeviceInformationKind Kind => DeviceInformation.Kind;
        public string Id => DeviceInformation.Id;
        public string Name => DeviceInformation.Name;
        public BitmapImage GlyphBitmapImage { get; private set; }
        public bool CanPair => DeviceInformation.Pairing.CanPair;
        public bool IsPaired => DeviceInformation.Pairing.IsPaired;
        public IReadOnlyDictionary<string, object> Properties => DeviceInformation.Properties;
        public DeviceInformation DeviceInformation { get; private set; }

        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);

            OnPropertyChanged("Kind");
            OnPropertyChanged("Id");
            OnPropertyChanged("Name");
            OnPropertyChanged("DeviceInformation");
            OnPropertyChanged("CanPair");
            OnPropertyChanged("IsPaired");
            OnPropertyChanged("GetPropertyForDisplay");

            UpdateGlyphBitmapImage();
        }

        public string GetPropertyForDisplay(string key) => Properties[key]?.ToString();

        private async void UpdateGlyphBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await DeviceInformation.GetGlyphThumbnailAsync();
            BitmapImage glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            GlyphBitmapImage = glyphBitmapImage;
            OnPropertyChanged("GlyphBitmapImage");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
