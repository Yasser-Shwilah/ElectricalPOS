using ElectricalPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectricalPOS.Services
{
    public class CategoryService
    {
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using var context = new ElectricalPOSContext();
            return await context.Categories.ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            using var context = new ElectricalPOSContext();
            return await context.Categories.FindAsync(id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                context.Categories.Update(category);
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                using var context = new ElectricalPOSContext();
                var category = await context.Categories.FindAsync(id);
                if (category != null)
                {
                    context.Categories.Remove(category);
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
