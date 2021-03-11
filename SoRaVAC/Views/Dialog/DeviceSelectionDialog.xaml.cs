using SoRaVAC.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Boîte de dialogue de contenu, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace SoRaVAC.Views.Dialog
{
    public sealed partial class DeviceSelectionDialog : ContentDialog, INotifyPropertyChanged
    {
        public ObservableCollection<DeviceInformationDisplay> VideoSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();
        public ObservableCollection<DeviceInformationDisplay> AudioSourcesList { get; } = new ObservableCollection<DeviceInformationDisplay>();
        public ObservableCollection<DeviceInformationDisplay> AudioRenderersList { get; } = new ObservableCollection<DeviceInformationDisplay>();

        private DeviceInformationDisplay _selectedVideoSource;
        public DeviceInformationDisplay SelectedVideoSource
        {
            get => _selectedVideoSource;
            set
            {
                Set(ref _selectedVideoSource, value);
            }
        }

        private DeviceInformationDisplay _selectedAudioSource;
        public DeviceInformationDisplay SelectedAudioSource
        {
            get => _selectedAudioSource;
            set
            {
                Set(ref _selectedAudioSource, value);
            }
        }

        private DeviceInformationDisplay _selectedAudioRenderer;

        public DeviceInformationDisplay SelectedAudioRenderer
        {
            get => _selectedAudioRenderer;
            set
            {
                Set(ref _selectedAudioRenderer, value);
            }
        }

        public DeviceSelectionDialog(ObservableCollection<DeviceInformationDisplay> videoSourcesList, ObservableCollection<DeviceInformationDisplay> audioSourcesList, ObservableCollection<DeviceInformationDisplay> audioRenderersList)
        {
            InitializeComponent();

            VideoSourcesList = videoSourcesList;
            AudioSourcesList = audioSourcesList;
            AudioRenderersList = audioRenderersList;
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

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Nothing to do as ComboBox_SelectionChanged handle the enabling of this button
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Probably nothing to do
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedVideoSource != null && SelectedAudioSource != null && SelectedAudioRenderer != null)
            {
                IsPrimaryButtonEnabled = true;
            }
        }
    }
}
