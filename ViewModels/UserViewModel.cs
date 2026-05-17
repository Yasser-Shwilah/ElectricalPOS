using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class UserViewModel : ObservableObject
    {
        private readonly UserService _userService = new();

        [ObservableProperty]
        private ObservableCollection<User> users = new();

        [ObservableProperty]
        private User? selectedUser;

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string selectedRole = "Accountant";

        [ObservableProperty]
        private bool isEditMode;

        public List<string> Roles { get; } = new List<string> { "Admin", "Accountant" };

        public UserViewModel()
        {
            LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsersAsync()
        {
            var userList = await _userService.GetAllUsersAsync();
            Users.Clear();
            foreach (var user in userList)
            {
                Users.Add(user);
            }
        }

        [RelayCommand]
        private async Task AddUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = new User
            {
                Username = Username,
                Password = Password,
                Role = SelectedRole
            };

            var success = await _userService.AddUserAsync(user);
            if (success)
            {
                MessageBox.Show("User added successfully", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadUsersAsync();
            }
            else
            {
                MessageBox.Show("Failed to add user", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user to delete", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedUser.Id == 1)
            {
                MessageBox.Show("Cannot delete default admin user", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete user '{SelectedUser.Username}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _userService.DeleteUserAsync(SelectedUser.Id);
                if (success)
                {
                    MessageBox.Show("User deleted successfully", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadUsersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete user", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            SelectedRole = "Accountant";
            SelectedUser = null;
            IsEditMode = false;
        }
    }
}
