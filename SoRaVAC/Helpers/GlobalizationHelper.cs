using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SoRaVAC.Helpers
{
    class GlobalizationHelper
    {
        public static readonly string[] AvailableLanguages = new string[] { "en-US", "fr-FR" };

        public static LanguageDisplayTemplate GetLanguageDisplayTemplate(ResourceManager manager, string languageCode)
        {
            return new LanguageDisplayTemplate(languageCode, manager.GetString("LanguageDisplayName", new CultureInfo(languageCode)));
        }

        public static async Task<StorageFile> GetHelpFileAsync()
        {
            string code = CultureInfo.CurrentCulture.Name;

            StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            string helpPath = $@"Help\{code}\help.md";

            return await appFolder.GetFileAsync(helpPath);
        }

        internal static async Task<Uri> GetAssetFileAsync(string url)
        {
            try
            {
                string code = CultureInfo.CurrentCulture.Name;
                string assetFilePath = $@"Help\{code}\{url}".Replace("/", @"\");

                StorageFolder appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFile assetFile = await appFolder.GetFileAsync(assetFilePath);

                return new Uri(assetFile.Path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
