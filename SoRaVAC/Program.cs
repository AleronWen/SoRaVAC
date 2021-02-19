using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace SoRaVAC
{
    class Program
    {
        [System.STAThreadAttribute()]
        public static void Main()
        {
            using (new SoRaVACUWP.App())
            {
                /* uncomment for testing purpose */
                CultureInfo ci = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = ci;
                Thread.CurrentThread.CurrentUICulture = ci;
                /* */
                SoRaVAC.App app = new SoRaVAC.App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
