using Microsoft.EntityFrameworkCore;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.DataAccess;

public class BookCopyDataAccess
{
    private readonly QLSVDBContext _context;

    public BookCopyDataAccess(QLSVDBContext context)
    {
        _context = context;
    }

    public Task<List<BookCopy>> GetByBookIdAsync(int bookId, CancellationToken cancellationToken = default)
    {
        return _context.BookCopies
            .AsNoTracking()
            .Where(copy => copy.BookId == bookId)
            .OrderBy(copy => copy.Barcode)
            .ToListAsync(cancellationToken);
    }

    public Task<BookCopy?> GetByIdAsync(int bookCopyId, CancellationToken cancellationToken = default)
    {
        return _context.BookCopies.FirstOrDefaultAsync(copy => copy.BookCopyId == bookCopyId, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<BookCopy> copies, CancellationToken cancellationToken = default)
    {
        await _context.BookCopies.AddRangeAsync(copies, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(BookCopy copy, CancellationToken cancellationToken = default)
    {
        var existing = await _context.BookCopies.FindAsync([copy.BookCopyId], cancellationToken);
        if (existing is null)
            return false;

        existing.IsBorrowed = copy.IsBorrowed;
        existing.PhysicalCondition = copy.PhysicalCondition;
        existing.Location = copy.Location;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int bookCopyId, CancellationToken cancellationToken = default)
    {
        var copy = await _context.BookCopies.FindAsync([bookCopyId], cancellationToken);
        if (copy is null)
            return false;

        _context.BookCopies.Remove(copy);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
