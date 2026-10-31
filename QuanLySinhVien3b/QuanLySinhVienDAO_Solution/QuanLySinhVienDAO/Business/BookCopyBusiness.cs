using System.Globalization;
using QuanLySinhVienDAO.DataAccess;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.Business;

public class BookCopyBusiness
{
    private readonly BookDataAccess _books;
    private readonly BookCopyDataAccess _copies;

    public BookCopyBusiness(BookDataAccess books, BookCopyDataAccess copies)
    {
        _books = books;
        _copies = copies;
    }

    public Task<List<BookCopy>> GetByBookIdAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return _copies.GetByBookIdAsync(bookId, cancellationToken);
    }

    public async Task AddCopiesAsync(
        int bookId,
        int quantity,
        string? location = null,
        string physicalCondition = "Mới",
        CancellationToken cancellationToken = default)
    {
        if (quantity is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Số bản sao phải từ 1 đến 1000.");

        var book = await _books.GetByIdAsync(bookId, cancellationToken)
            ?? throw new InvalidOperationException("Đầu sách không tồn tại.");

        physicalCondition = NormalizeCondition(physicalCondition);
        location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        if (location?.Length > 50)
            throw new ArgumentException("Vị trí lưu trữ không được dài quá 50 ký tự.", nameof(location));

        var prefix = $"{book.BookCode}.";
        var nextSequence = book.Copies
            .Where(copy => copy.Barcode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(copy => int.TryParse(copy.Barcode[prefix.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out var sequence) ? sequence : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var newCopies = Enumerable.Range(nextSequence, quantity)
            .Select(sequence => new BookCopy
            {
                Barcode = $"{prefix}{sequence:D3}",
                BookId = bookId,
                Location = location,
                PhysicalCondition = physicalCondition,
                IsBorrowed = false
            });

        await _copies.AddRangeAsync(newCopies, cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        int bookCopyId,
        bool isBorrowed,
        string physicalCondition,
        string? location = null,
        CancellationToken cancellationToken = default)
    {
        var copy = await _copies.GetByIdAsync(bookCopyId, cancellationToken);
        if (copy is null)
            return false;

        copy.IsBorrowed = isBorrowed;
        copy.PhysicalCondition = NormalizeCondition(physicalCondition);
        copy.Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        if (copy.Location?.Length > 50)
            throw new ArgumentException("Vị trí lưu trữ không được dài quá 50 ký tự.", nameof(location));

        return await _copies.UpdateAsync(copy, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int bookCopyId, CancellationToken cancellationToken = default)
    {
        var copy = await _copies.GetByIdAsync(bookCopyId, cancellationToken);
        if (copy is null)
            return false;
        if (copy.IsBorrowed)
            throw new InvalidOperationException("Không thể xóa bản sao đang được mượn.");

        return await _copies.DeleteAsync(bookCopyId, cancellationToken);
    }

    private static string NormalizeCondition(string physicalCondition)
    {
        var normalized = physicalCondition?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Tình trạng vật lý không được để trống.", nameof(physicalCondition));
        if (normalized.Length > 50)
            throw new ArgumentException("Tình trạng vật lý không được dài quá 50 ký tự.", nameof(physicalCondition));

        return normalized;
    }
}
