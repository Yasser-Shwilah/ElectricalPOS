using ElectricalPOS.ViewModels;
using System.Windows.Controls;

namespace ElectricalPOS.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is UserViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}
