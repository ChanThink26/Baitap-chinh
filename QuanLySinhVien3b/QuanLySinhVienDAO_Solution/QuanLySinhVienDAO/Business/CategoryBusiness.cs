using QuanLySinhVienDAO.DataAccess;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.Business;

public class CategoryBusiness
{
    private readonly CategoryDataAccess _categories;

    public CategoryBusiness(CategoryDataAccess categories)
    {
        _categories = categories;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _categories.GetAllAsync(cancellationToken);
    }

    public Task<Category?> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return _categories.GetByIdAsync(categoryId, cancellationToken);
    }

    public async Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        NormalizeAndValidate(category);
        if (await _categories.NameExistsAsync(category.Name, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Tên thể loại đã tồn tại.");

        await _categories.AddAsync(category, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        if (category.CategoryId <= 0)
            throw new ArgumentException("Mã định danh thể loại không hợp lệ.", nameof(category));

        NormalizeAndValidate(category);
        if (await _categories.NameExistsAsync(category.Name, category.CategoryId, cancellationToken))
            throw new InvalidOperationException("Tên thể loại đã được sử dụng.");

        return await _categories.UpdateAsync(category, cancellationToken);
    }

    public Task<bool> DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return _categories.DeleteAsync(categoryId, cancellationToken);
    }

    private static void NormalizeAndValidate(Category category)
    {
        category.Name = category.Name?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Tên thể loại không được để trống.", nameof(category));
        if (category.Name.Length > 100)
            throw new ArgumentException("Tên thể loại không được dài quá 100 ký tự.", nameof(category));
    }
}
