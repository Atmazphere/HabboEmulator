using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Emulator
{
    public sealed class LicenceProvider
    {
        public LicenceProvider() { }

        public string GetUniqueId()
        {
            string currentDiskDrive = Environment.CurrentDirectory.Split(':')[0];
            string hddSerial = GetHDDSerialNumber(currentDiskDrive);
            string hddSize = GetHDDSize(currentDiskDrive);
            string macAddress = FindMACAddress();
            string cpuId = GetCPUId();
            string cpuManufactor = GetCPUManufacturer();

            string combinedKey = hddSerial + hddSize + macAddress + cpuId + cpuManufactor;
            string fullKey = string.Empty;

            using (MD5 md5Hash = MD5.Create())
            {
                byte[] d = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedKey));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < d.Length; i++)
                {
                    sb.Append(d[i].ToString("x2"));
                }

                fullKey = sb.ToString();
            }

            return fullKey;
        }

        public string GetHDDSerialNumber(string drive)
        {
            //check to see if the user provided a drive letter
            //if not default it to "C"
            if (string.IsNullOrEmpty(drive))
            {
                drive = "C";
            }
            //create our ManagementObject, passing it the drive letter to the
            //DevideID using WQL
            ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + drive + ":\"");
            //bind our management object
            disk.Get();
            //return the serial number
            return disk["VolumeSerialNumber"].ToString();
        }

        public string GetHDDSize(string drive)
        {
            //check to see if the user provided a drive letter
            //if not default it to "C"
            if (string.IsNullOrEmpty(drive))
            {
                drive = "C";
            }
            //create our ManagementObject, passing it the drive letter to the
            //DevideID using WQL
            ManagementObject disk = new ManagementObject("Win32_LogicalDisk.DeviceID=\"" + drive + ":\"");
            //bind our management object
            disk.Get();
            //return the HDD's initial size
            return Convert.ToString(disk["Size"]);
        }

        public string FindMACAddress()
        {
            //create out management class object using the
            //Win32_NetworkAdapterConfiguration class to get the attributes
            //of the network adapter
            ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //create our ManagementObjectCollection to get the attributes with
            ManagementObjectCollection objCol = mgmt.GetInstances();
            string address = String.Empty;
            //loop through all the objects we find
            foreach (ManagementObject obj in objCol)
            {
                if (address == String.Empty)  // only return MAC Address from first card
                {
                    //grab the value from the first network adapter we find
                    //you can change the string to an array and get all
                    //network adapters found as well
                    //check to see if the adapter's IPEnabled
                    //equals true
                    if ((bool)obj["IPEnabled"] == true)
                    {
                        address = obj["MacAddress"].ToString();
                    }
                }
                //dispose of our object
                obj.Dispose();
            }
            //replace the ":" with an empty space, this could also
            //be removed if you wish
            address = address.Replace(":", "");
            //return the mac address
            return address;
        }

        public string GetCPUId()
        {
            string cpuInfo = String.Empty;
            //create an instance of the Management class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuInfo == String.Empty)
                {
                    // only return cpuInfo from first CPU
                    cpuInfo = obj.Properties["ProcessorId"].Value.ToString();
                }
            }
            return cpuInfo;
        }

        public string GetCPUManufacturer()
        {
            string cpuMan = String.Empty;
            //create an instance of the Management class with the
            //Win32_Processor class
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            //create a ManagementObjectCollection to loop through
            ManagementObjectCollection objCol = mgmt.GetInstances();
            //start our loop for all processors found
            foreach (ManagementObject obj in objCol)
            {
                if (cpuMan == String.Empty)
                {
                    // only return manufacturer from first CPU
                    cpuMan = obj.Properties["Manufacturer"].Value.ToString();
                }
            }
            return cpuMan;
        }

        public double GetTimestamp()
        {
            TimeSpan ts = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0));
            return ts.TotalSeconds;
        }
    }
}
