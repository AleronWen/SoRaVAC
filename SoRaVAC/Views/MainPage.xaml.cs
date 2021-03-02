using SoRaVAC.Views.Contracts;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml.Controls;

namespace SoRaVAC.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged, IFullScreenMode
    {
        public MainPage()
        {
            InitializeComponent();
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

        public void EnterFullScreenMode()
        {
            throw new NotImplementedException();
        }

        public void ExitFullScreenMode()
        {
            throw new NotImplementedException();
        }
    }
}
