using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectricalPOS.Models;
using ElectricalPOS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ElectricalPOS.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly CategoryService _categoryService = new();

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private bool isEditMode;

        public CategoryViewModel()
        {
            LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            var categoryList = await _categoryService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categoryList)
            {
                Categories.Add(category);
            }
        }

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please enter category name", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = new Category { Name = Name };

            var success = await _categoryService.AddCategoryAsync(category);
            if (success)
            {
                MessageBox.Show("Category added successfully", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadCategoriesAsync();
            }
            else
            {
                MessageBox.Show("Failed to add category", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task UpdateCategoryAsync()
        {
            if (SelectedCategory == null || string.IsNullOrWhiteSpace(Name))
            {
                MessageBox.Show("Please fill all required fields", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedCategory.Name = Name;

            var success = await _categoryService.UpdateCategoryAsync(SelectedCategory);
            if (success)
            {
                MessageBox.Show("Category updated successfully", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                await LoadCategoriesAsync();
            }
            else
            {
                MessageBox.Show("Failed to update category", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteCategoryAsync()
        {
            if (SelectedCategory == null)
            {
                MessageBox.Show("Please select a category to delete", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete category '{SelectedCategory.Name}'?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _categoryService.DeleteCategoryAsync(SelectedCategory.Id);
                if (success)
                {
                    MessageBox.Show("Category deleted successfully", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearForm();
                    await LoadCategoriesAsync();
                }
                else
                {
                    MessageBox.Show("Failed to delete category. It may be in use by products.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void EditCategory()
        {
            if (SelectedCategory != null)
            {
                Name = SelectedCategory.Name;
                IsEditMode = true;
            }
        }

        [RelayCommand]
        private void ClearForm()
        {
            Name = string.Empty;
            SelectedCategory = null;
            IsEditMode = false;
        }
    }
}
