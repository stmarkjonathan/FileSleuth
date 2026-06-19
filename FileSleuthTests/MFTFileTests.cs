using FileSleuth.MFTManager;
using FileSleuth.Model;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FileSleuthTests
{
    [TestClass]
    public sealed class MFTFileTests
    {

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FileRecordHeader // need to check header of mft file record to ensure it is correctly grabbed
        {
            public uint magicNum;
            public ushort updateSequenceOffset;
            public ushort updateSequenceSize;
            public ulong logSequenceNum;
            public ushort sequenceNum;
            public ushort hardLinkCount;
            public ushort firstAttributeOffset;
            public ushort flags;
            public uint realSize;
            public uint allocatedSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            byte[] baseRecordNumber;
            public ushort baseSequenceNum;
            public ushort nextAttributeID;
            public ushort unused;
            public uint recordNum;
        }

        [TestMethod]
        public void GetBootSector_ValidDriveFormat()
        {
            Volume foo = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFile = new MFTFile(foo.volumeHandle);

            Assert.AreEqual(mftFile.bootSector.name, "NTFS    ");
        }

        [TestMethod]
        public void GetMFTFileRecords_ValidMagicNumber()
        {
            Volume foo = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFile = new MFTFile(foo.volumeHandle);
            FileRecordHeader mftRecord = MFTMethods.BytesToStruct<FileRecordHeader>(mftFile.mftData);

            Assert.AreEqual(mftRecord.magicNum, (uint)0x454C4946);
        }
        
    }
}

