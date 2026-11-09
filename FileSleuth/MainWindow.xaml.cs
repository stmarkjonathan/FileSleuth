using FileSleuth.ViewModels;
using System.Windows;


namespace FileSleuth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MainWindowViewModel ViewModel;
        public MainWindow()
        {
            DataContext = ViewModel = new MainWindowViewModel();
            InitializeComponent();           
        }       
    }
}