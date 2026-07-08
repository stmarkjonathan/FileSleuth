using FileSleuth.Model;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.MFTManager.MFTMethods;
using static FileSleuth.Structs;

namespace FileSleuth.MFTManager
{
    public class MFTFile
    {

        class InvalidDriveException: Exception
        {
            public InvalidDriveException()
            {
            }

            public InvalidDriveException(string message)
                : base(message)
            {
            }

            public InvalidDriveException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }       

        const int MFT_FILE_SIZE = 1024;
        const int MFT_FILES_PER_BUFFER = 65536;

        public Volume parentVolume;
        public BootSector bootSector { get; set; }
        public byte[] mftData { get; set; }
        public FileRecordHeader mftFileRecord { get; set; }
        public MFTFile(Volume volume)
        {

            parentVolume = volume;
            bootSector = GetBootSector(volume.volumeHandle);
            mftData = GetMFTFileRecord();
            mftFileRecord = MFTMethods.BytesToStruct<FileRecordHeader>(mftData);
        }

        private static BootSector GetBootSector(SafeFileHandle handle)
        {

            BootSector bootSector = new BootSector();

            byte[] bootSecBuf = StructToBytes<BootSector>(bootSector);

            ReadHandle(handle, bootSecBuf, 0, 512);

            bootSector = BytesToStruct<BootSector>(bootSecBuf);

            if(bootSector.name == "NTFS    ")
            {
                return bootSector;
            }
            else
            {
                throw new InvalidDriveException("The given drive is not an NTFS Drive");
            }

            
        }

        private byte[] GetMFTFileRecord()
        {
            byte[] mftFile = new byte[MFT_FILE_SIZE];

            ulong bytesPerCluster = (ulong)(bootSector.bytesPerSector * bootSector.sectorsPerCluster);

            ReadHandle(parentVolume.volumeHandle, mftFile, (long)(bootSector.mftStart * bytesPerCluster), MFT_FILE_SIZE); // grab the first 1kb of the MFT

            return mftFile;
        }

    }
}
