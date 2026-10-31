using Microsoft.EntityFrameworkCore;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.DataAccess;

public class CategoryDataAccess
{
    private readonly QLSVDBContext _context;

    public CategoryDataAccess(QLSVDBContext context)
    {
        _context = context;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return _context.Categories.FirstOrDefaultAsync(category => category.CategoryId == categoryId, cancellationToken);
    }

    public Task<bool> NameExistsAsync(string name, int? excludingCategoryId = null, CancellationToken cancellationToken = default)
    {
        return _context.Categories.AnyAsync(category =>
            category.Name == name && (!excludingCategoryId.HasValue || category.CategoryId != excludingCategoryId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Categories.FindAsync([category.CategoryId], cancellationToken);
        if (existing is null)
            return false;

        existing.Name = category.Name;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync([categoryId], cancellationToken);
        if (category is null || await _context.Books.AnyAsync(book => book.CategoryId == categoryId, cancellationToken))
            return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
