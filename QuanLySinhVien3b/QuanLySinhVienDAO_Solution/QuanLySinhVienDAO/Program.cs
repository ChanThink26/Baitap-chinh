using Microsoft.EntityFrameworkCore;
using QuanLySinhVienDAO;
using QuanLySinhVienDAO.Entity;
using QuanLySinhVienDAO.UI;

ApplicationConfiguration.Initialize();

try
{
    InitializeDatabase();
    Application.Run(new BookManagementForm());
}
catch (Exception exception)
{
    MessageBox.Show(exception.Message, "Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

static void InitializeDatabase()
{
    using var context = new QLSVDBContext();
    context.Database.Migrate();

    if (!context.Categories.Any())
    {
        var category = new Category { Name = "Văn học" };
        var book = new Book
        {
            BookCode = "CP001",
            Title = "Chí Phèo",
            Author = "Nam Cao",
            Publisher = "NXB Văn học",
            Price = 110000,
            PublicationYear = 2020,
            Category = category
        };

        context.Books.Add(book);
        context.BookCopies.AddRange(
            new BookCopy { Barcode = "CP001.001", Location = "Kệ A1", Book = book, PhysicalCondition = "Mới" },
            new BookCopy { Barcode = "CP001.002", Location = "Kệ A1", Book = book, IsBorrowed = true, PhysicalCondition = "Mới" });
        context.SaveChanges();
    }
}
