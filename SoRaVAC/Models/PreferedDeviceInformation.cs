using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace SoRaVAC.Models
{
    class PreferedDeviceInformation
    {
        public PreferedDeviceInformation(DeviceInformation deviceInformation)
        {
            Id = deviceInformation.Id;
            Name = deviceInformation.Name;
        }

        public PreferedDeviceInformation(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Name { get; }
        public string Id { get; }
    }
}
