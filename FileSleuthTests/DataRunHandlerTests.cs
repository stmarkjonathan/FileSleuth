using FileSleuth;
using FileSleuth.MFTManager;
using FileSleuth.Model;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FileSleuth.Structs;

namespace FileSleuthTests
{
    [TestClass]
    public sealed class DataRunHandlerTests
    {

        [TestMethod]
        public void EnumerateDataRuns_Success()
        {

            Volume volume = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFile = new MFTFile(volume);
            DataRunHandler dataRunHandler = new DataRunHandler(mftFile);

            List<ValueTuple<byte[], ulong>> data = dataRunHandler.EnumerateDataRuns();

            Assert.AreEqual(Constants.MFT_MAGIC_NUMBER, BitConverter.ToUInt32(data[0].Item1, 0));
        }

    }
}
