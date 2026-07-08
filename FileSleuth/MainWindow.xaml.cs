using FileSleuth.MFTManager;
using FileSleuth.Model;
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

            List<byte[]> data = dataRunHandler.EnumerateDataRuns();
        }
    }
}