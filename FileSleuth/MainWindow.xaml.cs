using FileSleuth.FileManager;
using FileSleuth.MFTManager;
using FileSleuth.Model;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static FileSleuth.Enums;
using static FileSleuth.FileManager.FileManagerObjects;
using static FileSleuth.MFTManager.MFTObjects;
using static FileSleuth.Structs;

namespace FileSleuth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            string driveName = DriveInfo.GetDrives()[0].Name;

            Volume foo = new Volume(driveName);
            MFTFile mftFoo = new MFTFile(foo);
            DataRunHandler dataRunHandler = new DataRunHandler(mftFoo);

            List<(byte[] mftBuffer, ulong filesToLoad)> data = dataRunHandler.EnumerateDataRuns();
            Dictionary<ulong, ulong> extensionRecordNums = new Dictionary<ulong, ulong>();
            Dictionary<ulong, FileData> filesDict = new Dictionary<ulong, FileData>();
            FileRecordHandler fileRecordHandler = new FileRecordHandler();

            foreach ((byte[] mftBuffer, ulong filesToLoad) dataRunData in data)
            {
                ulong filePos = 0;

                while (filePos < dataRunData.filesToLoad)
                {

                    FileRecord fileRecord = fileRecordHandler.FindFileRecord(filePos, dataRunData.mftBuffer);

                    if (fileRecord.fileRecordHeader.HasValue == false)
                    {
                        filePos++;
                        continue;
                    }

                    FileRecordHeader fileRecordHeader = fileRecord.fileRecordHeader.Value;

                    (FileNameAttributeHeader fileNameAttribute, int attributePosition) = fileRecordHandler.GetFileNameAttribute(
                        dataRunData.mftBuffer, fileRecordHeader.firstAttributeOffset, filePos, out AttributeListInfo attributeListInfo);

                    if (!fileNameAttribute.Equals(default(FileNameAttributeHeader)))
                    {
                        FileData file = fileRecordHandler.GetFileData(fileRecord, fileNameAttribute, attributePosition, dataRunData.mftBuffer);
                        filesDict[file.FileIndex] = file;
                    }

                    if (attributeListInfo.hasAttributeList)
                    {
                        fileRecordHandler.GetAttributeListFileNameEntry(dataRunData.mftBuffer, attributeListInfo.attributeListAttribute,
                            extensionRecordNums, fileRecordHeader.recordNum, attributeListInfo.attributeListPosition);
                    }

                    filePos++;

                }

                filesDict[5].FileName = driveName.Substring(0, driveName.Length - 1);            
            }

            foreach (var file in filesDict.Values)
            {
                if (file != null)
                {
                    file.FilePath = fileRecordHandler.GetFilePath(file, filesDict, extensionRecordNums).ToString();
                }

            }

        }
    }
}