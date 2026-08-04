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

            List<ValueTuple<byte[], ulong>> data = dataRunHandler.EnumerateDataRuns();

            FileRecordHandler fileRecordHandler = new FileRecordHandler();

            foreach (ValueTuple<byte[], ulong> dataRunData in data)
            {
                ulong filePos = 0;

                while (filePos < dataRunData.Item2)
                {

                    Nullable<FileRecordHeader> fileRecordResult = fileRecordHandler.FindFileRecord(filePos, dataRunData.Item1);

                    if (fileRecordResult.HasValue == false)
                    {
                        filePos++;
                        continue;
                    }

                    FileRecordHeader fileRecord = fileRecordResult.Value;

                    FileNameAttributeHeader fileNameAttribute = fileRecordHandler.GetFileNameAttribute(
                        dataRunData.Item1, fileRecord.firstAttributeOffset, filePos, out AttributeListInfo hasAttributeList);

                    //need to still check for namespace 2 etc. in another method

                    filePos++;

                }

                
            }

        }
    }
}