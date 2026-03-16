using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly UserService _userService = new();

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public User? CurrentUser { get; private set; }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "الرجاء إدخال اسم المستخدم وكلمة المرور";
                return;
            }

            var user = await _userService.AuthenticateAsync(Username, Password);
            
            if (user != null)
            {
                CurrentUser = user;
                ErrorMessage = string.Empty;
                
                // Close login window and open main window
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var mainWindow = new Views.MainWindow(user);
                    mainWindow.Show();
                    
                    // Close login window
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Views.LoginWindow)
                        {
                            window.Close();
                            break;
                        }
                    }
                });
            }
            else
            {
                ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
            }
        }
    }
}
