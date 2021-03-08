using Microsoft.Toolkit.Uwp.UI.Controls;
using SoRaVAC.Helpers;
using SoRaVAC.Views.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=234238

namespace SoRaVAC.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
        private readonly ResourceLoader _resourceLoader;

        public HelpPage()
        {
            InitializeComponent();
            _resourceLoader = ResourceLoader.GetForCurrentView();
        }

        private async void MarkdownTextBlock_MarkdownRendered(object sender, Microsoft.Toolkit.Uwp.UI.Controls.MarkdownRenderedEventArgs e)
        {
            try
            {
                string initalMarkdownText = await FileIO.ReadTextAsync(await GlobalizationHelper.GetHelpFileAsync());

                if (sender is MarkdownTextBlock mdTB)
                {
                    mdTB.Text = initalMarkdownText;
                }
            }
            catch (Exception)
            {                
                string message = _resourceLoader.GetString("Help_ErrorDialog_UnableGettingHelpFile_Message");
                string caption = _resourceLoader.GetString("Help_ErrorDialog_UnableGettingHelpFile_Caption");
                _ = new ErrorDialog(caption, message).ShowAsync();
            }
        }

        private void MarkdownTextBlock_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e)
        {
            var deferral = e.GetDeferral();

            try
            {
                e.Image = new BitmapImage(new Uri(e.Url));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            e.Handled = true;

            deferral.Complete();
        }
    }
}
