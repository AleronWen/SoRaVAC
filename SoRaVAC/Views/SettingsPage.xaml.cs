using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SoRaVAC.Core;
using SoRaVAC.Helpers;
using SoRaVAC.Models;
using SoRaVAC.Services;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Globalization;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

namespace SoRaVAC.Views
{
    // DONE WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/release/docs/UWP/pages/settings-codebehind.md
    // DONE WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page, INotifyPropertyChanged
    {
        #region Video Source properties
        private DeviceWatcherHelper VideoSourceDeviceWatcherHelper;
        public ObservableCollection<DeviceInformationDisplay> VideoSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();


        private DeviceInformationDisplay _selectedVideoSource;
        public DeviceInformationDisplay SelectedVideoSource
        {
            get => _selectedVideoSource;
            set
            {
                Set(ref _selectedVideoSource, value);
            }
        }

        private bool _preferedVideoSourceIsMissing;
        public bool PreferedVideoSourceIsMissing
        {
            get => _preferedVideoSourceIsMissing;
            set
            {
                Set(ref _preferedVideoSourceIsMissing, value);
            }
        }

        private PreferedDeviceInformation _preferedVideoSource;
        public PreferedDeviceInformation PreferedVideoSource
        {
            get => _preferedVideoSource;
            set
            {
                Set(ref _preferedVideoSource, value);
            }
        }
        #endregion

        #region Audio Source properties
        private DeviceWatcherHelper AudioSourceDeviceWatcherHelper;
        public ObservableCollection<DeviceInformationDisplay> AudioSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();

        private DeviceInformationDisplay _selectedAudioSource;
        public DeviceInformationDisplay SelectedAudioSource
        {
            get => _selectedAudioSource;
            set
            {
                Set(ref _selectedAudioSource, value);
            }
        }

        private bool _preferedAudioSourceIsMissing;
        public bool PreferedAudioSourceIsMissing
        {
            get => _preferedAudioSourceIsMissing;
            set
            {
                Set(ref _preferedAudioSourceIsMissing, value);
            }
        }

        private PreferedDeviceInformation _preferedAudioSource;
        public PreferedDeviceInformation PreferedAudioSource
        {
            get => _preferedAudioSource;
            set
            {
                Set(ref _preferedAudioSource, value);
            }
        }
        #endregion

        #region Audio Renderer properties
        private DeviceWatcherHelper AudioRendererDeviceWatcherHelper;
        public ObservableCollection<DeviceInformationDisplay> AudioRenderersList { get; } = new ObservableCollection<DeviceInformationDisplay>();

        private DeviceInformationDisplay _selectedAudioRenderer;

        public DeviceInformationDisplay SelectedAudioRenderer
        {
            get => _selectedAudioRenderer;
            set
            {
                Set(ref _selectedAudioRenderer, value);
            }
        }

        private bool _preferedAudioRendererIsMissing;
        public bool PreferedAudioRendererIsMissing
        {
            get => _preferedAudioRendererIsMissing;
            set
            {
                Set(ref _preferedAudioRendererIsMissing, value);
            }
        }

        private PreferedDeviceInformation _preferedAudioRenderer;
        public PreferedDeviceInformation PreferedAudioRenderer
        {
            get => _preferedAudioRenderer;
            set
            {
                Set(ref _preferedAudioRenderer, value);
            }
        }
        #endregion

        #region Other settings properties
        private double _soundVolume;

        public double SoundVolume
        {
            get => _soundVolume;
            set
            {
                Set(ref _soundVolume, value);
            }
        }

        private bool _isNewReleaseAvailable;
        public bool IsNewReleaseAvailable
        {
            get { return _isNewReleaseAvailable; }
            set { Set(ref _isNewReleaseAvailable, value); }
        }

        private Octokit.Release _newRelease;
        public Octokit.Release NewRelease
        {
            get { return _newRelease; }
            set { Set(ref _newRelease, value); }
        }

        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        public ElementTheme ElementTheme
        {
            get { return _elementTheme; }

            set { Set(ref _elementTheme, value); }
        }

        private string _versionDescription;

        public string VersionDescription
        {
            get { return _versionDescription; }

            set { Set(ref _versionDescription, value); }
        }

        private static string[] _availableLanguages = new string[] { "en-US", "fr-FR" };
        public ObservableCollection<LanguageDisplayTemplate> LanguageList { get; } = new ObservableCollection<LanguageDisplayTemplate>();

        private LanguageDisplayTemplate _selectedLanguage;

        public LanguageDisplayTemplate SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                Set(ref _selectedLanguage, value);
            }
        }
        #endregion

        public SettingsPage()
        {
            InitializeComponent();

            VideoSourcesList.CollectionChanged += VideoDevicesListHandleChange;
            AudioSourcesList.CollectionChanged += AudioSourcesListHandleChange;
            AudioRenderersList.CollectionChanged += AudioRenderersListHandleChange;

            PreferedVideoSource = AudioVideoSettingsStorageHelper.LoadPreferedVideoSource();
            if (PreferedVideoSource != null)
            {
                VideoSourceSetPreferedDevice.Text = PreferedVideoSource.Name;
            }

            PreferedAudioSource = AudioVideoSettingsStorageHelper.LoadPreferedAudioSource();
            if (PreferedAudioSource != null)
            {
                AudioSourceSetPreferedDevice.Text = PreferedAudioSource.Name;
            }

            PreferedAudioRenderer = AudioVideoSettingsStorageHelper.LoadPreferedAudioRenderer();
            if (PreferedAudioRenderer != null)
            {
                AudioRendererSetPreferedDevice.Text = PreferedAudioRenderer.Name;
            }

            double soundVolume = AudioVideoSettingsStorageHelper.LoadSoundVolume();
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SoundVolume = soundVolume);

            PreferedVideoSourceIsMissing = true;
            PreferedAudioSourceIsMissing = true;
            PreferedAudioRendererIsMissing = true;

            VideoSourceDeviceWatcherHelper = new DeviceWatcherHelper(VideoSourcesList, this.Dispatcher);
            AudioSourceDeviceWatcherHelper = new DeviceWatcherHelper(AudioSourcesList, Dispatcher);
            AudioRendererDeviceWatcherHelper = new DeviceWatcherHelper(AudioRenderersList, Dispatcher);

            IsNewReleaseAvailable = NewReleaseChecker.GetInstance().CheckForNewRelease(Assembly.GetEntryAssembly());
            if (IsNewReleaseAvailable)
            {
                NewRelease = NewReleaseChecker.GetInstance().LastRelease;
            }

            ResourceManager rm = new ResourceManager("Resources", Assembly.GetEntryAssembly());
            
            foreach (string languageCode in _availableLanguages)
            {
                var cultureInfo = new CultureInfo(languageCode);                
                LanguageDisplayTemplate language = new LanguageDisplayTemplate(languageCode, rm.GetString("LanguageDisplayName", cultureInfo));
                LanguageList.Add(language);

                if (languageCode == Thread.CurrentThread.CurrentUICulture.Name)
                {
                    SelectedLanguage = language;
                }
            }
        }

        private async Task InitializeAsync()
        {
            VersionDescription = GetVersionDescription();

            await Task.CompletedTask;
        }

        private string GetVersionDescription()
        {
            var appName = "AppDisplayName".GetLocalized();
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{appName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        #region Event Handling
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeAsync();

            StartWatchers();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopWatchers();
            ResetWatchers();
        }

        private void VideoDevicesListHandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    /*
                     * IF RegistredVideoSource is not present
                     *      THEN IF NewItems contains RegistredVideoSource
                     *              THEN set RegistredVideoSource as present and hide the error message
                     */
                    if (PreferedVideoSourceIsMissing && PreferedVideoSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (PreferedVideoSource.Id == device.Id)
                            {
                                PreferedVideoSourceIsMissing = false;
                                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SelectedVideoSource = VideoSourcesList[e.NewStartingIndex]);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    /*
                     * IF RegistredVideoSource is present
                     *      THEN IF NewItems does not contains RegistredVideoSource
                     *              THEN set RegistredVideoSource as missing and show the error message
                     */
                    if (!PreferedVideoSourceIsMissing && PreferedVideoSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (PreferedVideoSource.Id == device.Id)
                            {
                                PreferedVideoSourceIsMissing = true;
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    /*
                     * IF RegistredVideoSource is not null
                     *      THEN set RegistredVideoSource as missing and show the error message
                     */
                    PreferedVideoSourceIsMissing = PreferedVideoSource != null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Probably nothing to do
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Probably nothing to do
                    break;
                default:
                    // Nothing to do
                    break;
            }
        }

        private void AudioSourcesListHandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    /*
                     * IF RegistredAudioSource is not present
                     *      THEN IF NewItems contains RegistredAudioSource
                     *              THEN set RegistredAudioSource as present and hide the error message
                     */
                    if (PreferedAudioSourceIsMissing && PreferedAudioSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (PreferedAudioSource.Id == device.Id)
                            {
                                PreferedAudioSourceIsMissing = false;
                                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SelectedAudioSource = AudioSourcesList[e.NewStartingIndex]);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    /*
                     * IF RegistredAudioSource is present
                     *      THEN IF NewItems does not contains RegistredAudioSource
                     *              THEN set RegistredAudioSource as missing and show the error message
                     */
                    if (!PreferedAudioSourceIsMissing && PreferedAudioSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (PreferedAudioSource.Id == device.Id)
                            {
                                PreferedAudioSourceIsMissing = true;
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    /*
                     * IF RegistredAudioSource is not null
                     *      THEN set RegistredAudioSource as missing and show the error message
                     */
                    PreferedAudioSourceIsMissing = PreferedAudioSource != null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Probably nothing to do
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Probably nothing to do
                    break;
                default:
                    // Nothing to do
                    break;
            }
        }

        private void AudioRenderersListHandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    /*
                     * IF RegistredAudioSource is not present
                     *      THEN IF NewItems contains RegistredAudioSource
                     *              THEN set RegistredAudioSource as present and hide the error message
                     */
                    if (PreferedAudioRendererIsMissing && PreferedAudioRenderer != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (PreferedAudioRenderer.Id == device.Id)
                            {
                                PreferedAudioRendererIsMissing = false;
                                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SelectedAudioRenderer = AudioRenderersList[e.NewStartingIndex]);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    /*
                     * IF RegistredAudioSource is present
                     *      THEN IF NewItems does not contains RegistredAudioSource
                     *              THEN set RegistredAudioSource as missing and show the error message
                     */
                    if (!PreferedAudioRendererIsMissing && PreferedAudioRenderer != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (PreferedAudioRenderer.Id == device.Id)
                            {
                                PreferedAudioRendererIsMissing = true;
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    /*
                     * IF RegistredAudioSource is not null
                     *      THEN set RegistredAudioSource as missing and show the error message
                     */
                    PreferedAudioRendererIsMissing = PreferedAudioRenderer != null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Probably nothing to do
                    break;
                case NotifyCollectionChangedAction.Move:
                    // Probably nothing to do
                    break;
                default:
                    // Nothing to do
                    break;
            }
        }

        private void VideoSourceSetPreferedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedVideoSource != null)
            {
                PreferedVideoSource = new PreferedDeviceInformation(SelectedVideoSource.DeviceInformation);
                PreferedVideoSourceIsMissing = false;
                VideoSourceSetPreferedDevice.Text = PreferedVideoSource.Name;
                Task.Run(() => AudioVideoSettingsStorageHelper.SavePreferedVideoSource(PreferedVideoSource));
            }
        }

        private void AudioSourceSetPreferedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAudioSource != null)
            {
                PreferedAudioSource = new PreferedDeviceInformation(SelectedAudioSource.DeviceInformation);
                PreferedAudioSourceIsMissing = false;
                AudioSourceSetPreferedDevice.Text = PreferedAudioSource.Name;
                Task.Run(() => AudioVideoSettingsStorageHelper.SavePreferedAudioSource(PreferedAudioSource));
            }
        }

        private void AudioRendererSetPreferedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAudioRenderer != null)
            {
                PreferedAudioRenderer = new PreferedDeviceInformation(SelectedAudioRenderer.DeviceInformation);
                PreferedAudioRendererIsMissing = false;
                AudioRendererSetPreferedDevice.Text = PreferedAudioRenderer.Name;
                Task.Run(() => AudioVideoSettingsStorageHelper.SavePreferedAudioRenderer(PreferedAudioRenderer));
            }
        }

        private void SoundVolumeSlier_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                int roundedValue = (int)Math.Round(slider.Value);
                SoundVolumeTextBlock.Text = $"{roundedValue}%";
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await AudioVideoSettingsStorageHelper.SaveSoundVolumeAsync(roundedValue));
            }
        }

        private async void ThemeChanged_CheckedAsync(object sender, RoutedEventArgs e)
        {
            var param = (sender as RadioButton)?.CommandParameter;

            if (param != null)
            {
                await ThemeSelectorService.SetThemeAsync((ElementTheme)param);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        private void StartWatchers()
        {
            VideoSourcesList.Clear();
            AudioSourcesList.Clear();
            AudioRenderersList.Clear();

            DeviceWatcher videoDeviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.VideoCapture);
            DeviceWatcher audioInputDeviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.AudioCapture);
            DeviceWatcher audioOutputDeviceWatcher = DeviceInformation.CreateWatcher(DeviceClass.AudioRender);

            Debug.WriteLine("Starting Watchers...");
            VideoSourceDeviceWatcherHelper.StartWatcher(videoDeviceWatcher);
            AudioSourceDeviceWatcherHelper.StartWatcher(audioInputDeviceWatcher);
            AudioRendererDeviceWatcherHelper.StartWatcher(audioOutputDeviceWatcher);
        }

        private void StopWatchers()
        {
            VideoSourceDeviceWatcherHelper.StopWatcher();
            AudioSourceDeviceWatcherHelper.StopWatcher();
            AudioRendererDeviceWatcherHelper.StopWatcher();
            Debug.WriteLine("Watchers are stopped.");
        }

        private void ResetWatchers()
        {
            VideoSourceDeviceWatcherHelper.Reset();
            AudioSourceDeviceWatcherHelper.Reset();
            AudioRendererDeviceWatcherHelper.Reset();
        }

        private void LanguageApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationLanguages.PrimaryLanguageOverride = SelectedLanguage.Code;
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
            Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            Frame.Navigate(this.GetType());
        }
    }
}
