using System.Windows;

namespace WPF_EXAM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel viewModel = new MainViewModel(mediaElement);

            this.DataContext = viewModel;

            viewModel.Load();
        }
    }
}
