using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoRaVAC.Helpers
{
    public class LanguageDisplayTemplate
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public LanguageDisplayTemplate(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
