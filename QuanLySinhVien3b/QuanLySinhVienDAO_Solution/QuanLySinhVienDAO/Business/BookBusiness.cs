using QuanLySinhVienDAO.DataAccess;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.Business;

public class BookBusiness
{
    private readonly BookDataAccess _books;

    public BookBusiness(BookDataAccess books)
    {
        _books = books;
    }

    public Task<IReadOnlyList<Book>> SearchAsync(string? keyword = null, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        return _books.SearchAsync(keyword, categoryId, cancellationToken);
    }

    public Task<Book?> GetByIdAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return _books.GetByIdAsync(bookId, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        NormalizeAndValidate(book);
        if (!await _books.CategoryExistsAsync(book.CategoryId, cancellationToken))
            throw new InvalidOperationException("Thể loại sách không tồn tại.");
        if (await _books.BookCodeExistsAsync(book.BookCode, cancellationToken: cancellationToken))
            throw new InvalidOperationException("Mã sách đã tồn tại.");

        await _books.AddAsync(book, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        if (book.BookId <= 0)
            throw new ArgumentException("Mã định danh đầu sách không hợp lệ.", nameof(book));

        NormalizeAndValidate(book);
        if (!await _books.CategoryExistsAsync(book.CategoryId, cancellationToken))
            throw new InvalidOperationException("Thể loại sách không tồn tại.");
        if (await _books.BookCodeExistsAsync(book.BookCode, book.BookId, cancellationToken))
            throw new InvalidOperationException("Mã sách đã được sử dụng.");

        return await _books.UpdateAsync(book, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var book = await _books.GetByIdAsync(bookId, cancellationToken);
        if (book is null)
            return false;
        if (book.Copies.Any(copy => copy.IsBorrowed))
            throw new InvalidOperationException("Không thể xóa đầu sách đang có bản sao được mượn.");

        return await _books.DeleteAsync(bookId, cancellationToken);
    }

    private static void NormalizeAndValidate(Book book)
    {
        book.BookCode = book.BookCode?.Trim() ?? "";
        book.Title = book.Title?.Trim() ?? "";
        book.Author = string.IsNullOrWhiteSpace(book.Author) ? null : book.Author.Trim();
        book.Publisher = string.IsNullOrWhiteSpace(book.Publisher) ? null : book.Publisher.Trim();

        if (string.IsNullOrWhiteSpace(book.BookCode))
            throw new ArgumentException("Mã sách không được để trống.", nameof(book));
        if (book.BookCode.Length > 20)
            throw new ArgumentException("Mã sách không được dài quá 20 ký tự.", nameof(book));
        if (string.IsNullOrWhiteSpace(book.Title))
            throw new ArgumentException("Tên sách không được để trống.", nameof(book));
        if (book.Title.Length > 200)
            throw new ArgumentException("Tên sách không được dài quá 200 ký tự.", nameof(book));
        if (string.IsNullOrWhiteSpace(book.Author))
            throw new ArgumentException("Tác giả không được để trống.", nameof(book));
        if (book.Author?.Length > 100)
            throw new ArgumentException("Tên tác giả không được dài quá 100 ký tự.", nameof(book));
        if (book.Publisher?.Length > 150)
            throw new ArgumentException("Nhà xuất bản không được dài quá 150 ký tự.", nameof(book));
        if (book.Price < 0)
            throw new ArgumentException("Giá sách không được âm.", nameof(book));
        if (book.PublicationYear is <= 0)
            throw new ArgumentException("Năm xuất bản không hợp lệ.", nameof(book));
    }
}
