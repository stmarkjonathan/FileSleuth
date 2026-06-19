using FileSleuth.Model;
using System.Text.RegularExpressions;

namespace FileSleuthTests
{
    [TestClass]
    public sealed class VolumeTests
    {

        [TestMethod]
        public void ConvertDriveName_ValidFormat()
        {
            //volumeName format will be "X:\"

            //return volumeName formatted as "\\.\X:" for CreateFile, where X is drive letter

            string volumeName = DriveInfo.GetDrives()[0].Name;

            Volume testVolume = new Volume(volumeName);

            var match = Regex.Match(testVolume.convertedVolumeName, @"^\\\\.\\.:$");

            Assert.IsTrue(match.Success);

        }

        [TestMethod]
        public void AccessVolumeHandle_ValidHandle()
        {
            string volumeName = DriveInfo.GetDrives()[0].Name;

            Volume testVolume = new Volume(volumeName);

            Assert.IsFalse(testVolume.volumeHandle.IsInvalid);

            testVolume.volumeHandle.Close();

        }
    }
}

