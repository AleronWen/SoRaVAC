using SoRaVAC.Core;
using SoRaVAC.Helpers;
using SoRaVAC.Models;
using SoRaVAC.Views.Contracts;
using SoRaVAC.Views.Dialog;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Graphics.Display;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Render;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace SoRaVAC.Views
{
    public sealed partial class CapturePage : Page, INotifyPropertyChanged, IFullScreenMode
    {
        private PreferedDeviceInformation VideoSource;
        private PreferedDeviceInformation AudioSource;
        private PreferedDeviceInformation AudioRenderer;

        private MediaCapture mediaCapture;
        private AudioGraph graph;
        private AudioDeviceOutputNode deviceOutputNode;
        private AudioDeviceInputNode deviceInputNode;

        private Brush _contentAreaBackgroundBrush;
        private Thickness _contentAreaMargin;

        private double _soundVolume;
        private bool _isFullScreenMode;
        private PlayingStatusEnum _playingStatus;

        private bool _isPreferedVideoSourcePresent;
        private bool _isPreferedAudioSourcePresent;
        private bool _isPreferedAudioRendererPresent;

        private PreferedDeviceInformation _preferedVideoSource;
        private PreferedDeviceInformation _preferedAudioSource;
        private PreferedDeviceInformation _preferedAudioRenderer;

        private readonly ResourceLoader _resourceLoader;

        public bool IsPreferedVideoSourcePresent
        {
            get { return _isPreferedVideoSourcePresent; }
            set { Set(ref _isPreferedVideoSourcePresent, value); }
        }
        public bool IsPreferedAudioSourcePresent
        {
            get { return _isPreferedAudioSourcePresent; }
            set { Set(ref _isPreferedAudioSourcePresent, value); }
        }
        public bool IsPreferedAudioRendererPresent
        {
            get { return _isPreferedAudioRendererPresent; }
            set { Set(ref _isPreferedAudioRendererPresent, value); }
        }

        public double SoundVolume
        {
            get => _soundVolume;
            set
            {
                Set(ref _soundVolume, value);
            }
        }

        public bool IsFullScreenMode
        {
            get { return _isFullScreenMode; }
            set { Set(ref _isFullScreenMode, value); }
        }

        public PlayingStatusEnum PlayingStatus
        {
            get { return _playingStatus;  }
            set { Set(ref _playingStatus, value);  }
        }

        #region Device Watchers
        private DeviceWatcherHelper VideoSourceDeviceWatcherHelper;
        private DeviceWatcherHelper AudioSourceDeviceWatcherHelper;
        private DeviceWatcherHelper AudioRendererDeviceWatcherHelper;
        public ObservableCollection<DeviceInformationDisplay> VideoSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();
        public ObservableCollection<DeviceInformationDisplay> AudioSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();
        public ObservableCollection<DeviceInformationDisplay> AudioRenderersList { get; } = new ObservableCollection<DeviceInformationDisplay>();
        #endregion

        public CapturePage()
        {
            InitializeComponent();

            _resourceLoader = ResourceLoader.GetForCurrentView();

            VideoSourcesList.CollectionChanged += VideoDevicesListHandleChange;
            AudioSourcesList.CollectionChanged += AudioSourcesListHandleChange;
            AudioRenderersList.CollectionChanged += AudioRenderersListHandleChange;

            IsFullScreenMode = false;

            _contentAreaBackgroundBrush = ContentArea.Background;
            _contentAreaMargin = ContentArea.Margin;

            PlayingStatus = PlayingStatusEnum.Stopped;

            ToolTipService.SetToolTip(PlayStopButton, _resourceLoader.GetString("Capture_PlayStopButtonTooltip_play"));

            double soundVolume = AudioVideoSettingsStorageHelper.LoadSoundVolume();
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SoundVolume = soundVolume);

            _preferedVideoSource = AudioVideoSettingsStorageHelper.LoadPreferedVideoSource();
            _preferedAudioSource = AudioVideoSettingsStorageHelper.LoadPreferedAudioSource();
            _preferedAudioRenderer = AudioVideoSettingsStorageHelper.LoadPreferedAudioRenderer();

            IsPreferedVideoSourcePresent = false;
            IsPreferedAudioSourcePresent = false;
            IsPreferedAudioRendererPresent = false;

            VideoSourceDeviceWatcherHelper = new DeviceWatcherHelper(VideoSourcesList, this.Dispatcher);
            AudioSourceDeviceWatcherHelper = new DeviceWatcherHelper(AudioSourcesList, Dispatcher);
            AudioRendererDeviceWatcherHelper = new DeviceWatcherHelper(AudioRenderersList, Dispatcher);

            Window.Current.VisibilityChanged += Current_VisibilityChangedAsync;
        }

        private void VideoDevicesListHandleChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (!IsPreferedVideoSourcePresent && _preferedVideoSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (_preferedVideoSource.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedVideoSourcePresent = true);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (IsPreferedVideoSourcePresent && _preferedVideoSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (_preferedVideoSource.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedVideoSourcePresent = false);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IsPreferedVideoSourcePresent = false;
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
                    if (!IsPreferedAudioSourcePresent && _preferedAudioSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (_preferedAudioSource.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedAudioSourcePresent = true);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (IsPreferedAudioSourcePresent && _preferedAudioSource != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (_preferedAudioSource.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedAudioSourcePresent = false);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IsPreferedAudioSourcePresent = false;
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
                    if (!IsPreferedAudioRendererPresent && _preferedAudioRenderer != null)
                    {
                        foreach (DeviceInformationDisplay device in e.NewItems)
                        {
                            if (_preferedAudioRenderer.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedAudioRendererPresent = true);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (IsPreferedAudioRendererPresent && _preferedAudioRenderer != null)
                    {
                        foreach (DeviceInformationDisplay device in e.OldItems)
                        {
                            if (_preferedAudioRenderer.Id == device.Id)
                            {
                                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => IsPreferedAudioRendererPresent = false);
                                break;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    IsPreferedAudioRendererPresent = false;
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

        private void Current_VisibilityChangedAsync(object sender, VisibilityChangedEventArgs e)
        {
            if (PlayingStatus == PlayingStatusEnum.Playing)
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await StopCaptureAsync(PlayingStatusEnum.Paused));
                PlayingStatus = PlayingStatusEnum.Paused;
            }
            else if (PlayingStatus == PlayingStatusEnum.Paused)
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await StartCaptureAsync());
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartWatchers();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopWatchers();
            ResetWatchers();
            if (PlayingStatus == PlayingStatusEnum.Playing)
            {
                _ = StopCaptureAsync();
            }
        }

        private async void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayingStatus == PlayingStatusEnum.Playing)
            {
                await StopCaptureAsync();
            }
            else
            {
                try
                {
                    if (CaptureModeSwitch.IsOn)
                    {
                        VideoSource = _preferedVideoSource;
                        AudioSource = _preferedAudioSource;
                        AudioRenderer = _preferedAudioRenderer;
                    }
                    else
                    {
                        // Show a Dialog to choose sources
                        VideoSource = null;
                        AudioSource = null;
                        AudioRenderer = null;
                    }

                    await StartCaptureAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    string message = _resourceLoader.GetString("Capture_CaptureErrorDialog_UnableStartCapture_Message");
                    string caption = _resourceLoader.GetString("Capture_CaptureErrorDialog_UnableStartCapture_Caption");
                    _ = new ErrorDialog(caption, message).ShowAsync();
                }
            }
        }

        private async Task StartCaptureAsync()
        {
            await InitVideoCapture();
            await InitAudioCapture();
            await mediaCapture.StartPreviewAsync();
            graph.Start();
            NativeMethods.PreventSleep();
            PlayingStatus = PlayingStatusEnum.Playing;
            ToolTipService.SetToolTip(PlayStopButton, _resourceLoader.GetString("Capture_PlayStopButtonTooltip_stop"));
            PlayStopButton.Content = "\uE71A";
        }

        private async Task StopCaptureAsync(PlayingStatusEnum resultingPlayingStatus = PlayingStatusEnum.Stopped)
        {
            graph.Stop();
            await mediaCapture.StopPreviewAsync();
            NativeMethods.AllowSleep();
            PlayingStatus = resultingPlayingStatus;
            ToolTipService.SetToolTip(PlayStopButton, _resourceLoader.GetString("Capture_PlayStopButtonTooltip_play"));
            PlayStopButton.Content = "\uE768";
        }

        private void SoundVolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                SoundVolumeTextBlock.Text = $"{slider.Value:N0}%";
                SoundVolumeOverlayTextBlock.Text = $"{slider.Value:N0}%";

                if (IsFullScreenMode)
                {
                    if (Resources["StoryboardSoundVolumeOverlayFader"] is Storyboard anim) anim.Begin();
                }


                if (deviceOutputNode != null)
                {
                    deviceOutputNode.OutgoingGain = slider.Value / 100.0;
                }
            }
        }

        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointerProperties = e.GetCurrentPoint(null).Properties;

            // We check that it was not a lateral wheeling
            if (!pointerProperties.IsHorizontalMouseWheel)
            {
                int delta = pointerProperties.MouseWheelDelta;

                // Using mouse wheel to update sound volume
                int roundedValue = (int)Math.Round(SoundVolume);
                if (delta > 0 && roundedValue < 100)
                {
                    SoundVolume = roundedValue + 1;
                }
                else if (delta < 0 && roundedValue > 0)
                {
                    SoundVolume = roundedValue - 1;
                }
            }
        }

        private void SizeChangeDetectorGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (captureElement != null)
            {
                if (sender is Grid SizeChangeDetectorGrid)
                {
                    if (SizeChangeDetectorGrid.ActualHeight >= 2160 && SizeChangeDetectorGrid.ActualWidth >= 3840)
                    {
                        CapturePreviewUpdateSize(3840, 2160);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 1440 && SizeChangeDetectorGrid.ActualWidth >= 2560)
                    {
                        CapturePreviewUpdateSize(2560, 1440);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 1080 && SizeChangeDetectorGrid.ActualWidth >= 1920)
                    {
                        CapturePreviewUpdateSize(1920, 1080);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 944 && SizeChangeDetectorGrid.ActualWidth >= 1680)
                    {
                        CapturePreviewUpdateSize(1680, 944);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 768 && SizeChangeDetectorGrid.ActualWidth >= 1366)
                    {
                        CapturePreviewUpdateSize(1366, 768);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 720 && SizeChangeDetectorGrid.ActualWidth >= 1280)
                    {
                        CapturePreviewUpdateSize(1280, 720);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 576 && SizeChangeDetectorGrid.ActualWidth >= 1024)
                    {
                        CapturePreviewUpdateSize(1024, 576);
                    }
                    else if (SizeChangeDetectorGrid.ActualHeight >= 480 && SizeChangeDetectorGrid.ActualWidth >= 584)
                    {
                        CapturePreviewUpdateSize(854, 480);
                    }
                    else
                    {
                        // We do nothing
                    }
                }
            }
        }

        private void CapturePreviewUpdateSize(double w, double h)
        {
            if (captureElement.Width != w)
            {
                captureElement.Width = w;
                captureElement.Height = h;
                Debug.WriteLine($"Caputre size set to {h}p");
            }
        }
        private async Task InitVideoCapture()
        {

            MediaCaptureInitializationSettings captureInitSettings = new MediaCaptureInitializationSettings()
            {
                AudioDeviceId = "",
                VideoDeviceId = VideoSource.Id,
                StreamingCaptureMode = StreamingCaptureMode.Video,
                PhotoCaptureSource = PhotoCaptureSource.VideoPreview
            };
            DisplayRequest displayRequest = new DisplayRequest();
            mediaCapture = new MediaCapture();
            await mediaCapture.InitializeAsync(captureInitSettings);
            displayRequest.RequestActive();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            captureElement.Source = mediaCapture;
        }

        private async Task InitAudioCapture()
        {
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media)
            {
                QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                PrimaryRenderDevice = await DeviceInformation.CreateFromIdAsync(AudioRenderer.Id)
            };

            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);

            if (result.Status != AudioGraphCreationStatus.Success)
            {
                // Cannot create graph
                Debug.WriteLine($"AudioGraph Creation Error because {result.Status}");
                return;
            }

            graph = result.Graph;
            Debug.WriteLine("Graph successfully created!");

            // Create a device output node
            CreateAudioDeviceOutputNodeResult deviceOutputNodeResult = await graph.CreateDeviceOutputNodeAsync();
            if (deviceOutputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device output node
                Debug.WriteLine($"Audio Device Output unavailable because {deviceOutputNodeResult.Status}");
                return;
            }

            deviceOutputNode = deviceOutputNodeResult.DeviceOutputNode;
            deviceOutputNode.OutgoingGain = SoundVolume / 100.0;
            Debug.WriteLine("Device Output connection successfully created");

            // Create a device input node using the default audio input device
            CreateAudioDeviceInputNodeResult deviceInputNodeResult = await graph.CreateDeviceInputNodeAsync(MediaCategory.Other, graph.EncodingProperties, await DeviceInformation.CreateFromIdAsync(AudioSource.Id));

            if (deviceInputNodeResult.Status != AudioDeviceNodeCreationStatus.Success)
            {
                // Cannot create device input node
                Debug.WriteLine($"Audio Device Input unavailable because {deviceOutputNodeResult.Status}");
                return;
            }

            deviceInputNode = deviceInputNodeResult.DeviceInputNode;
            Debug.WriteLine("Device Input connection successfully created");

            deviceInputNode.AddOutgoingConnection(deviceOutputNode);

            // Because we are using lowest latency setting, we need to handle device disconnection errors
            graph.UnrecoverableErrorOccurred += Graph_UnrecoverableErrorOccurred;
        }

        private async void Graph_UnrecoverableErrorOccurred(AudioGraph sender, AudioGraphUnrecoverableErrorOccurredEventArgs args)
        {
            // Recreate the graph and all nodes when this happens
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                sender.Dispose();
                // Reset UI
            });
        }

        public void EnterFullScreenMode()
        {
            if (!IsFullScreenMode)
            {
                // activate full screen mode
                IsFullScreenMode = true;

                // save ContentArea Background and Margin
                _contentAreaBackgroundBrush = ContentArea.Background;
                _contentAreaMargin = ContentArea.Margin;

                // update ContentArea Background and Margin
                ContentArea.Background = new SolidColorBrush(Colors.Black);
                ContentArea.Margin = new Thickness(0);
            }
        }

        public void ExitFullScreenMode()
        {
            if (IsFullScreenMode)
            {
                // restore ContentArea Background and Margin
                ContentArea.Background = _contentAreaBackgroundBrush;
                ContentArea.Margin = _contentAreaMargin;

                // deactivate full screen mode
                IsFullScreenMode = false;
            }
        }
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
