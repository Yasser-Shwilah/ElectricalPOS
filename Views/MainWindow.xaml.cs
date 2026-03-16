using ElectricalPOS.Models;
using ElectricalPOS.ViewModels;
using System.Windows;

namespace ElectricalPOS.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(User user)
        {
            InitializeComponent();
            DataContext = new MainViewModel(user);
        }
    }
}
