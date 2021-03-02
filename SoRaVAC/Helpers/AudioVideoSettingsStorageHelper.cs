using SoRaVAC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SoRaVAC.Helpers
{
    class AudioVideoSettingsStorageHelper
    {
        private const string SOUND_VOLUME = "SoundVolume";
        private const string VIDEO_SOURCE_ID = "VideoSourceId";
        private const string VIDEO_SOURCE_NAME = "VideoSourceName";
        private const string AUDIO_SOURCE_ID = "AudioSourceId";
        private const string AUDIO_SOURCE_NAME = "AudioSourceName";
        private const string AUDIO_RENDERER_ID = "AudioRendererId";
        private const string AUDIO_RENDERER_NAME = "AudioRendererName";
        private const double VOLUME_MAX = 100;

        public static double LoadSoundVolume()
        {
            string soundVolumeStr = Task.Run(() => ApplicationData.Current.LocalSettings.ReadAsync<string>(SOUND_VOLUME)).Result;
            if (soundVolumeStr != null)
            {
                if (double.TryParse(soundVolumeStr, out double soundVolume))
                {
                    return soundVolume;
                }
            }
            return VOLUME_MAX;
        }

        public static async Task SaveSoundVolumeAsync(double value)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(SOUND_VOLUME, value.ToString());
        }

        public static PreferedDeviceInformation LoadPreferedVideoSource()
        {
            return LoadPreferedDevice(VIDEO_SOURCE_ID, VIDEO_SOURCE_NAME);
        }

        public static PreferedDeviceInformation LoadPreferedAudioSource()
        {
            return LoadPreferedDevice(AUDIO_SOURCE_ID, AUDIO_SOURCE_NAME);
        }

        public static PreferedDeviceInformation LoadPreferedAudioRenderer()
        {
            return LoadPreferedDevice(AUDIO_RENDERER_ID, AUDIO_RENDERER_NAME);
        }

        private static PreferedDeviceInformation LoadPreferedDevice(string keyId, string keyName)
        {
            PreferedDeviceInformation preferedDevice = null;

            string id = Task.Run(() => ApplicationData.Current.LocalSettings.ReadAsync<string>(keyId)).Result;
            string name = Task.Run(() => ApplicationData.Current.LocalSettings.ReadAsync<string>(keyName)).Result;

            if (id != null && name != null)
            {
                preferedDevice = new PreferedDeviceInformation(id, name);
            }

            return preferedDevice;
        }

        public static async void SavePreferedVideoSource(PreferedDeviceInformation preferedDeviceInformation)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(VIDEO_SOURCE_ID, preferedDeviceInformation.Id);
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(VIDEO_SOURCE_NAME, preferedDeviceInformation.Name);
        }

        public static async void SavePreferedAudioSource(PreferedDeviceInformation preferedDeviceInformation)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(AUDIO_SOURCE_ID, preferedDeviceInformation.Id);
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(AUDIO_SOURCE_NAME, preferedDeviceInformation.Name);
        }

        public static async void SavePreferedAudioRenderer(PreferedDeviceInformation preferedDeviceInformation)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(AUDIO_RENDERER_ID, preferedDeviceInformation.Id);
            await ApplicationData.Current.LocalSettings.SaveAsync<string>(AUDIO_RENDERER_NAME, preferedDeviceInformation.Name);
        }
    }
}
