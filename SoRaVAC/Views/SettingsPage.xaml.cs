using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Extensions.Options;

using SoRaVAC.Contracts.Services;
using SoRaVAC.Contracts.Views;
using SoRaVAC.Core.Models;
using SoRaVAC.Helpers;
using SoRaVAC.Models;
using Windows.Devices.Enumeration;

namespace SoRaVAC.Views
{
    public partial class SettingsPage : Page, INotifyPropertyChanged, INavigationAware
    {
        private readonly AppConfig _appConfig;
        private readonly IThemeSelectorService _themeSelectorService;
        private readonly ISystemService _systemService;
        private readonly IApplicationInfoService _applicationInfoService;
        private bool _isInitialized;
        private AppTheme _theme;
        private string _versionDescription;


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


        private ReleaseInfo _releaseInfo;
        public ReleaseInfo ReleaseInfo
        {
            get { return _releaseInfo; }
            set { Set(ref _releaseInfo, value); }
        }

        private bool _displayNewVersion;
        public bool DisplayNewVersion
        {
            get { return _displayNewVersion; }
            set { Set(ref _displayNewVersion, value); }
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
        public AppTheme Theme
        {
            get { return _theme; }
            set { Set(ref _theme, value); }
        }

        public string VersionDescription
        {
            get { return _versionDescription; }
            set { Set(ref _versionDescription, value); }
        }
        #endregion

        public SettingsPage(IOptions<AppConfig> appConfig, IThemeSelectorService themeSelectorService, ISystemService systemService, IApplicationInfoService applicationInfoService)
        {
            _appConfig = appConfig.Value;
            _themeSelectorService = themeSelectorService;
            _systemService = systemService;
            _applicationInfoService = applicationInfoService;
            InitializeComponent();
            DataContext = this;

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
            _ = Dispatcher.InvokeAsync(() => SoundVolume = soundVolume);

            PreferedVideoSourceIsMissing = true;
            PreferedAudioSourceIsMissing = true;
            PreferedAudioRendererIsMissing = true;

            ReleaseInfo = Core.Services.NewReleaseChecker.GetInstance().ReleaseInfo;
            DisplayNewVersion = ReleaseInfo != null;

            VideoSourceDeviceWatcherHelper = new DeviceWatcherHelper(VideoSourcesList, this.Dispatcher);
            AudioSourceDeviceWatcherHelper = new DeviceWatcherHelper(AudioSourcesList, Dispatcher);
            AudioRendererDeviceWatcherHelper = new DeviceWatcherHelper(AudioRenderersList, Dispatcher);
        }

        #region Event Handling
        public void OnNavigatedTo(object parameter)
        {
            VersionDescription = $"SoRaVAC - {_applicationInfoService.GetVersion()}";
            Theme = _themeSelectorService.GetCurrentTheme();

            StartWatchers();

            _isInitialized = true;
        }

        public void OnNavigatedFrom()
        {
            StopWatchers();
            ResetWatchers();
        }

        private void OnLightChecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                _themeSelectorService.SetTheme(AppTheme.Light);
            }
        }

        private void OnDarkChecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                _themeSelectorService.SetTheme(AppTheme.Dark);
            }
        }

        private void OnDefaultChecked(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                _themeSelectorService.SetTheme(AppTheme.Default);
            }
        }

        private void OnPrivacyStatementClick(object sender, RoutedEventArgs e)
            => _systemService.OpenInWebBrowser(_appConfig.PrivacyStatement);
        private void LicenseLink_Click(object sender, RoutedEventArgs e)
        => _systemService.OpenInWebBrowser(Properties.Resources.SettingsPageLicenseLink);

        private void CodeRepositoryLink_Click(object sender, RoutedEventArgs e)
        => _systemService.OpenInWebBrowser(Properties.Resources.SettingsPageCodeRepositoryLink);

        private void NewReleaseLink_Click(object sender, RoutedEventArgs e)
        => _systemService.OpenInWebBrowser(ReleaseInfo.Url);

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
                                var task = Dispatcher.InvokeAsync(() => SelectedVideoSource = VideoSourcesList[e.NewStartingIndex]);
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
                                Task.Run(() => SelectedAudioSource = AudioSourcesList[e.NewStartingIndex]);
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
                                Task.Run(() => SelectedAudioRenderer = AudioRenderersList[e.NewStartingIndex]);
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

        private void SoundVolumeSlier_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                int roundedValue = (int)Math.Round(slider.Value);
                SoundVolumeTextBlock.Text = $"{roundedValue}%";
                _ = Dispatcher.InvokeAsync(() => AudioVideoSettingsStorageHelper.SaveSoundVolume(roundedValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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
    }
}
