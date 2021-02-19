using SoRaVAC.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

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
            if (Application.Current.Properties.Contains(SOUND_VOLUME))
            {
                string soundVolumeStr = Application.Current.Properties[SOUND_VOLUME].ToString();
                if (double.TryParse(soundVolumeStr, out double soundVolume))
                {
                    return soundVolume;
                }
            }
            return VOLUME_MAX;
        }

        public static void SaveSoundVolume(double value)
        {
            Application.Current.Properties[SOUND_VOLUME] = value.ToString();
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

            if (Application.Current.Properties.Contains(keyId) && Application.Current.Properties.Contains(keyName))
            {
                string id = Application.Current.Properties[keyId].ToString();
                string name = Application.Current.Properties[keyName].ToString();
                if (id != null && name != null)
                {
                    preferedDevice = new PreferedDeviceInformation(id, name);
                }
            }

            return preferedDevice;
        }

        public static void SavePreferedVideoSource(PreferedDeviceInformation preferedDeviceInformation)
        {
            Application.Current.Properties[VIDEO_SOURCE_ID] = preferedDeviceInformation.Id;
            Application.Current.Properties[VIDEO_SOURCE_NAME] = preferedDeviceInformation.Name;
        }

        public static void SavePreferedAudioSource(PreferedDeviceInformation preferedDeviceInformation)
        {
            Application.Current.Properties[AUDIO_SOURCE_ID] = preferedDeviceInformation.Id;
            Application.Current.Properties[AUDIO_SOURCE_NAME] = preferedDeviceInformation.Name;
        }

        public static void SavePreferedAudioRenderer(PreferedDeviceInformation preferedDeviceInformation)
        {
            Application.Current.Properties[AUDIO_RENDERER_ID] = preferedDeviceInformation.Id;
            Application.Current.Properties[AUDIO_RENDERER_NAME] = preferedDeviceInformation.Name;
        }
    }
}
