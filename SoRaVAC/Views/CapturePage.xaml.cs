using SoRaVAC.Core;
using SoRaVAC.Helpers;
using SoRaVAC.Models;
using SoRaVAC.Views.Contracts;
using SoRaVAC.Views.Dialog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

        public PlayingStatusEnum PlayingStatus;

        public CapturePage()
        {
            InitializeComponent();

            IsFullScreenMode = false;

            _contentAreaBackgroundBrush = ContentArea.Background;
            _contentAreaMargin = ContentArea.Margin;

            PlayingStatus = PlayingStatusEnum.Stopped;

            double soundVolume = AudioVideoSettingsStorageHelper.LoadSoundVolume();
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SoundVolume = soundVolume);

            VideoSource = AudioVideoSettingsStorageHelper.LoadPreferedVideoSource();
            AudioSource = AudioVideoSettingsStorageHelper.LoadPreferedAudioSource();
            AudioRenderer = AudioVideoSettingsStorageHelper.LoadPreferedAudioRenderer();

            Window.Current.VisibilityChanged += Current_VisibilityChangedAsync;
        }

        private void Current_VisibilityChangedAsync(object sender, VisibilityChangedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode)
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
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (PlayingStatus == PlayingStatusEnum.Playing)
            {
                _ = StopCaptureAsync();
            }
        }

        private async void PlayStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoSource != null && AudioSource != null && AudioRenderer != null)
            {
                if (PlayingStatus == PlayingStatusEnum.Playing)
                {
                    await StopCaptureAsync();
                }
                else
                {
                    try
                    {
                        await StartCaptureAsync();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        string message = $"Unable to start the capture, please check if the device is plugged.{Environment.NewLine}You may also go in Settings and select another device.";
                        string caption = "Capture error";
                        _ = new ErrorDialog(caption, message).ShowAsync();
                        //MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                string message = "Please setup video and audio devices in the Settings panel before starting the capture.";
                string caption = "No Capture device selected";
                _ = new ErrorDialog(caption, message).ShowAsync();
                //MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task StartCaptureAsync()
        {
            MediaCaptureInitializationSettings captureInitSettings = InitVideoCaptureSettings();
            await InitVideoCapture(captureInitSettings);
            await InitAudioCapture();
            await mediaCapture.StartPreviewAsync();
            graph.Start();
            NativeMethods.PreventSleep();
            PlayingStatus = PlayingStatusEnum.Playing;
            SizeChangeDetectorGrid.Visibility = Visibility.Visible;
            ToolTipService.SetToolTip(PlayStopButton, "Stop capture");
            PlayStopButton.Content = "\uE71A";
        }

        private async Task StopCaptureAsync(PlayingStatusEnum resultingPlayingStatus = PlayingStatusEnum.Stopped)
        {
            graph.Stop();
            await mediaCapture.StopPreviewAsync();
            NativeMethods.AllowSleep();
            PlayingStatus = resultingPlayingStatus;
            SizeChangeDetectorGrid.Visibility = Visibility.Collapsed;
            ToolTipService.SetToolTip(PlayStopButton, "Start capture");
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
                    var anim = this.Resources["Storyboard1"] as Storyboard;
                    if (anim != null) anim.Begin();
                }


                if (deviceOutputNode != null)
                {
                    deviceOutputNode.OutgoingGain = slider.Value / 100.0;
                }
            }
        }

        private void Page_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            HandleMouseWheel(e.GetCurrentPoint(null).Properties.MouseWheelDelta);
        }

        private void captureElement_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            //HandleMouseWheel(e.GetCurrentPoint(null).Properties.MouseWheelDelta);
        }

        private void HandleMouseWheel(int delta)
        {
            // Using mouse wheel to update sound volume
            int roundedValue = (int)Math.Round(SoundVolume);
            if (delta > 0 && roundedValue < 100)
            {
                SoundVolume = roundedValue + 1;
            }
            else if (roundedValue > 0)
            {
                SoundVolume = roundedValue - 1;
            }
        }

        private void SizeChangeDetectorGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (captureElement != null)
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

        private void CapturePreviewUpdateSize(double w, double h)
        {
            if (captureElement.Width != w)
            {
                captureElement.Width = w;
                captureElement.Height = h;
                Debug.WriteLine($"Caputre size set to {h}p");
            }
        }

        private MediaCaptureInitializationSettings InitVideoCaptureSettings()
        {
            return new MediaCaptureInitializationSettings()
            {
                AudioDeviceId = "",
                VideoDeviceId = VideoSource.Id,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
                PhotoCaptureSource = PhotoCaptureSource.VideoPreview
            };
        }
        private async Task InitVideoCapture(MediaCaptureInitializationSettings captureInitSettings)
        {
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                sender.Dispose();
                // Re-query for devices
                //_ = EnumerateVideoAndAudioDevices();
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

                // hide CommandPanel
                CommandPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void ExitFullScreenMode()
        {
            if (IsFullScreenMode)
            {
                // restore ContentArea Background and Margin
                ContentArea.Background = _contentAreaBackgroundBrush;
                ContentArea.Margin = _contentAreaMargin;

                // restore CommandPanel visibility
                CommandPanel.Visibility = Visibility.Visible;

                // deactivate full screen mode
                IsFullScreenMode = false;
            }
        }
    }

    public enum PlayingStatusEnum
    {
        Playing, Paused, Stopped,
    }
}
