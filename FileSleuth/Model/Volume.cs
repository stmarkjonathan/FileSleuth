using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace FileSleuth.Model
{
    public class Volume
    {
        public string driveName { get; set; }
        public string convertedVolumeName { get; set; }
        public SafeFileHandle volumeHandle { get; set; }

        public Volume(string driveName) 
        { 
            this.driveName = driveName;
            convertedVolumeName = ConvertDriveName(driveName);
            volumeHandle = AccessVolumeHandle();
        }

        ~Volume()
        {
            volumeHandle.Close();
        }

        private SafeFileHandle AccessVolumeHandle()
        {
           SafeFileHandle handle = PInvoke.CreateFile(convertedVolumeName, 
           (uint)Enums.GenericAccessRights.GENERIC_READ,
           FILE_SHARE_MODE.FILE_SHARE_WRITE | FILE_SHARE_MODE.FILE_SHARE_READ,
           null,
           FILE_CREATION_DISPOSITION.OPEN_EXISTING,
           0,
           null);

           if(handle.IsInvalid)
           {
               int errorCode = Marshal.GetLastWin32Error();
               string error = Marshal.GetPInvokeErrorMessage(errorCode);

               throw new Exception(error + " Error code: " + errorCode);
           }

           return handle;
        }

        private string ConvertDriveName(string driveName) 
        {
            //volumeName format will be "X:\"

            //return volumeName formatted as "\\.\X:" for CreateFile, where X is drive letter
            return @"\\.\" + driveName.Remove(driveName.Length - 1);
        }

    }
}
