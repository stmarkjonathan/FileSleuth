using FileSleuth.MFTManager;
using FileSleuth.Model;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static FileSleuth.Structs;

namespace FileSleuthTests
{

    [TestClass]
    public sealed class MFTFileTests
    {      

        [TestMethod]
        public void GetBootSector_ValidDriveFormat()
        {
            Volume foo = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFile = new MFTFile(foo);

            Assert.AreEqual(mftFile.bootSector.name, "NTFS    ");
        }

        [TestMethod]
        public void GetMFTFileRecords_ValidMagicNumber()
        {
            Volume foo = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFile = new MFTFile(foo);
            Assert.AreEqual(mftFile.mftFileRecord.magicNum, (uint)0x454C4946);
        }
        
    }
}

