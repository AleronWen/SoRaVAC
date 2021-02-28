using System;
using System.Collections.Generic;
using System.Text;

namespace SoRaVAC.Core.Models
{
    public class ReleaseInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public bool IsPreRelease { get; set; }
        public string Url { get; set; }
    }
}
