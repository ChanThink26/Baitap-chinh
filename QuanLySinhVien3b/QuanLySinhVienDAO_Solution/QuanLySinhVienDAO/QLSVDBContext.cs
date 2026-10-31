using Microsoft.EntityFrameworkCore;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO;

public class QLSVDBContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<BookCopy> BookCopies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasIndex(book => book.BookCode).IsUnique();
        modelBuilder.Entity<BookCopy>().HasIndex(copy => copy.Barcode).IsUnique();
        modelBuilder.Entity<BookCopy>().Property(copy => copy.PhysicalCondition).HasDefaultValue("Mới");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=QuanLyThuVien.db");
    }
}
