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
            Volume foo = new Volume(DriveInfo.GetDrives()[0].Name);
            MFTFile mftFoo = new MFTFile(foo);
            DataRunHandler dataRunHandler = new DataRunHandler(mftFoo);

            List<(byte[] mftBuffer, ulong filesToLoad)> data = dataRunHandler.EnumerateDataRuns();

            FileRecordHandler fileRecordHandler = new FileRecordHandler();

            foreach ((byte[] mftBuffer, ulong filesToLoad) dataRunData in data)
            {
                ulong filePos = 0;

                while (filePos < dataRunData.filesToLoad)
                {

                    Nullable<FileRecordHeader> fileRecordResult = fileRecordHandler.FindFileRecord(filePos, dataRunData.mftBuffer);

                    if (fileRecordResult.HasValue == false)
                    {
                        filePos++;
                        continue;
                    }

                    FileRecordHeader fileRecord = fileRecordResult.Value;

                    (FileNameAttributeHeader fileNameAttribute, int attributePosition)  = fileRecordHandler.GetFileNameAttribute(
                        dataRunData.mftBuffer, fileRecord.firstAttributeOffset, filePos, out AttributeListInfo attributeListInfo);

                    if (fileNameAttribute.namespaceType != 2)
                    {
                        if (!fileNameAttribute.Equals(default(FileNameAttributeHeader)) && fileNameAttribute.nonResident == 0)
                        {
                            FileData file = fileRecordHandler.GetFileData(fileRecord, fileNameAttribute, attributePosition, dataRunData.mftBuffer);                       
                        }

                    }


                    if (attributeListInfo.hasAttributeList)
                    {
                        
                    }

                    filePos++;

                }

                
            }

        }
    }
}