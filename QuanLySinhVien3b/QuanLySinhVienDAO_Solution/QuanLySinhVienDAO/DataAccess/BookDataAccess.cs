using Microsoft.EntityFrameworkCore;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.DataAccess;

public class BookDataAccess
{
    private readonly QLSVDBContext _context;

    public BookDataAccess(QLSVDBContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Book>> SearchAsync(string? keyword = null, int? categoryId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<Book> query = _context.Books
            .AsNoTracking()
            .Include(book => book.Category)
            .Include(book => book.Copies);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(book =>
                EF.Functions.Like(book.BookCode, pattern) ||
                EF.Functions.Like(book.Title, pattern) ||
                (book.Author != null && EF.Functions.Like(book.Author, pattern)));
        }

        if (categoryId.HasValue)
            query = query.Where(book => book.CategoryId == categoryId.Value);

        return await query
            .OrderBy(book => book.BookCode)
            .ToListAsync(cancellationToken);
    }

    public Task<Book?> GetByIdAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return _context.Books
            .Include(book => book.Category)
            .Include(book => book.Copies)
            .FirstOrDefaultAsync(book => book.BookId == bookId, cancellationToken);
    }

    public Task<bool> BookCodeExistsAsync(string bookCode, int? excludingBookId = null, CancellationToken cancellationToken = default)
    {
        return _context.Books.AnyAsync(book =>
            book.BookCode == bookCode && (!excludingBookId.HasValue || book.BookId != excludingBookId.Value),
            cancellationToken);
    }

    public Task<bool> CategoryExistsAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return _context.Categories.AnyAsync(category => category.CategoryId == categoryId, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Books.FindAsync([book.BookId], cancellationToken);
        if (existing is null)
            return false;

        existing.BookCode = book.BookCode;
        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Publisher = book.Publisher;
        existing.Price = book.Price;
        existing.PublicationYear = book.PublicationYear;
        existing.CategoryId = book.CategoryId;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int bookId, CancellationToken cancellationToken = default)
    {
        var book = await _context.Books.FindAsync([bookId], cancellationToken);
        if (book is null)
            return false;

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
