using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;

using SoRaVAC.Contracts.Services;
using SoRaVAC.Contracts.Views;

namespace SoRaVAC.Views
{
    public partial class ShellWindow : MetroWindow, IShellWindow, INotifyPropertyChanged, IFullScreenMode
    {
        private readonly INavigationService _navigationService;
        private bool _canGoBack;
        private bool _isFullScreenMode;
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

        public bool IsFullScreenMode {
            get { return _isFullScreenMode; }
            set { Set(ref _isFullScreenMode, value); }
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
            _isFullScreenMode = false;

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

            Application.Current.MainWindow.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (IsFullScreenMode)
                {
                    ExitFullScreenMode();
                }
            }
            else if (e.Key == Key.F11)
            {
                if (IsFullScreenMode)
                {
                    ExitFullScreenMode();
                }
                else
                {
                    EnterFullScreenMode();
                }
            }
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
            // We do not save things in full screen mode
            if (!IsFullScreenMode)
            {
                Application.Current.Properties["width"] = ((int)this.ActualWidth).ToString();
                Application.Current.Properties["height"] = ((int)this.ActualHeight).ToString();
            }
        }

        private void OnStageChanged(object sender, EventArgs e)
        {
            // We do not save things in full screen mode
            if (!IsFullScreenMode)
            {
                Application.Current.Properties["WindowState"] = this.WindowState.ToString();
            }
        }


        public void EnterFullScreenMode()
        {
            // Check that full screen mode is activated is delegated to caller
            // Now checking that the current content is the Capture Page
            if (GetNavigationFrame().Content is IFullScreenMode page)
            {
                // activating full screen mode to prevent side effects
                IsFullScreenMode = true;

                page.EnterFullScreenMode();

                WindowStyle = WindowStyle.None;
                IgnoreTaskbarOnMaximize = true;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                Topmost = true;

                ShowTitleBar = false;
                hamburgerMenu.DisplayMode = SplitViewDisplayMode.Overlay;
                hamburgerMenu.IsPaneOpen = false;
            }
        }

        public void ExitFullScreenMode()
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            IgnoreTaskbarOnMaximize = false;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
            Topmost = false;

            ShowTitleBar = true;
            hamburgerMenu.DisplayMode = SplitViewDisplayMode.CompactInline;
            hamburgerMenu.IsPaneOpen = true;

            ((IFullScreenMode)GetNavigationFrame().Content).ExitFullScreenMode();

            // Deactivating full screen mode
            IsFullScreenMode = false;
        }
    }
}
