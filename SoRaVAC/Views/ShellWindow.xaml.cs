using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using MahApps.Metro.Controls;

using SoRaVAC.Contracts.Services;
using SoRaVAC.Contracts.Views;

namespace SoRaVAC.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow, INotifyPropertyChanged
    {
        private readonly INavigationService _navigationService;
        private bool _canGoBack;
        private HamburgerMenuItem _selectedMenuItem;
        private HamburgerMenuItem _selectedOptionsMenuItem;

        public bool CanGoBack
        {
            get { return _canGoBack; }
            set { Set(ref _canGoBack, value); }
        }

        public HamburgerMenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { Set(ref _selectedMenuItem, value); }
        }

        public HamburgerMenuItem SelectedOptionsMenuItem
        {
            get { return _selectedOptionsMenuItem; }
            set { Set(ref _selectedOptionsMenuItem, value); }
        }

        // TODO WTS: Change the icons and titles for all HamburgerMenuItems here.
        public ObservableCollection<HamburgerMenuItem> MenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Properties.Resources.ShellCapturePage, Glyph = "\uE786", TargetPageType = typeof(CapturePage) },
        };

        public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Properties.Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsPage) }
        };

        public ShellWindow(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InitializeComponent();
            if (Application.Current.Properties.Contains("width"))
            {
                double.TryParse((string)Application.Current.Properties["width"], out double value);
                Width = value;
            }
            if (Application.Current.Properties.Contains("height"))
            {
                double.TryParse((string)Application.Current.Properties["height"], out double value);
                Height = value;
            }
            if (Application.Current.Properties.Contains("WindowState"))
            {
                Enum.TryParse(typeof(WindowState), (string)Application.Current.Properties["WindowState"], out object value);
                if ((WindowState)value != WindowState.Minimized)
                {
                    WindowState = (WindowState)value;
                }
                else
                {
                    /* The application has exited normally but int minimized state,
                     * we don't take the risk to have an app window larger than the screen
                     * so we reset the size to default min values.
                     */
                    WindowState = WindowState.Normal;
                    Width = MinWidth;
                    Height = MinHeight;
                }
            }

            DataContext = this;
        }

        public Frame GetNavigationFrame()
            => shellFrame;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigated += OnNavigated;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _navigationService.Navigated -= OnNavigated;
        }

        private void OnItemClick(object sender, ItemClickEventArgs args)
            => NavigateTo(SelectedMenuItem.TargetPageType);

        private void OnOptionsItemClick(object sender, ItemClickEventArgs args)
            => NavigateTo(SelectedOptionsMenuItem.TargetPageType);

        private void NavigateTo(Type targetPage)
        {
            if (targetPage != null)
            {
                _navigationService.NavigateTo(targetPage);
            }
        }

        private void OnNavigated(object sender, Type pageType)
        {
            var item = MenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => pageType == i.TargetPageType);
            if (item != null)
            {
                SelectedMenuItem = item;
            }
            else
            {
                SelectedOptionsMenuItem = OptionMenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => pageType == i.TargetPageType);
            }

            CanGoBack = _navigationService.CanGoBack;
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            _navigationService.GoBack();
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

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {            
            Application.Current.Properties["width"] = ((int)this.ActualWidth).ToString();
            Application.Current.Properties["height"] = ((int)this.ActualHeight).ToString();
        }

        private void OnStageChanged(object sender, EventArgs e)
        {
            Application.Current.Properties["WindowState"] = this.WindowState.ToString();
        }
    }
}
