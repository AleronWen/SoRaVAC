using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Enumeration;

namespace SoRaVAC.Models
{
    public class PreferedDeviceInformation
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
