using System;
using System.Globalization;
using System.Threading;

namespace SoRaVAC
{
    class Program
    {
        [STAThread()]
        public static void Main()
        {
            using (new SoRaVACUWP.App())
            {
                /* uncomment for testing purpose * /
                CultureInfo ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                /* */
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
