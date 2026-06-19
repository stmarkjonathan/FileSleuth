using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.MFTManager.MFTMethods;

namespace FileSleuth.MFTManager
{
    public class MFTFile
    {

        #region Structs

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BootSector
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string name;
            public ushort bytesPerSector;
            public byte sectorsPerCluster;
            public ushort reservedSectors;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] unused0;
            public ushort unused1;
            public byte media;
            public ushort unused2;
            public ushort sectorsPerTrack;
            public ushort headsPerCylinder;
            public uint hiddenSectors;
            public uint unused3;
            public uint unused4;
            public ulong totalSectors;
            public ulong mftStart;
            public ulong mftMirrorStart;
            public uint clustersPerFileRecord;
            public uint clustersPerIndexBlock;
            public ulong serialNumber;
            public uint checksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 426)]
            public byte[] bootLoader;
            public ushort bootSignature;
        }

        #endregion

        const int MFT_FILE_SIZE = 1024;
        const int MFT_FILES_PER_BUFFER = 65536;

        SafeFileHandle parentDriveHandle;
        BootSector bootSector;
        byte[] mftData;
        public MFTFile(SafeFileHandle handle)
        {

            parentDriveHandle = handle;
            bootSector = GetBootSector(handle);
            mftData = GetMFTFileRecord();

        }

        private static BootSector GetBootSector(SafeFileHandle handle)
        {

            BootSector bootSector = new BootSector();

            byte[] bootSecBuf = StructToBytes<BootSector>(bootSector);

            ReadHandle(handle, bootSecBuf, 0, 512);

            bootSector = BytesToStruct<BootSector>(bootSecBuf);

            return bootSector;
        }

        //when unit testing, check for magic number: 0x454C4946
        private byte[] GetMFTFileRecord()
        {
            byte[] mftFile = new byte[MFT_FILE_SIZE];

            ulong bytesPerCluster = (ulong)(bootSector.bytesPerSector * bootSector.sectorsPerCluster);

            ReadHandle(parentDriveHandle, mftFile, (long)(bootSector.mftStart * bytesPerCluster), MFT_FILE_SIZE); // grab the first 1kb of the MFT

            return mftFile;
        }

    }
}
