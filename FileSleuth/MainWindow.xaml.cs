using FileSleuth.FileManager;
using FileSleuth.ViewModels;
using System.Windows;
using System.Windows.Controls;


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

        private async void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (textBox.Text != string.Empty)
            {
                List<FileData> list = new List<FileData>(await Task.Run(() => ViewModel.SearchFiles()));

                FilesDataGrid.ItemsSource = list;
            }
            else
            {
                FilesDataGrid.ItemsSource = ViewModel.DisplayList;
            }
           
        }
    }
}