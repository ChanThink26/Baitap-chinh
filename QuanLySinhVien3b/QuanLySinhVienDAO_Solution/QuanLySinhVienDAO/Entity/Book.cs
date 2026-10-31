using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVienDAO.Entity;

public class Book
{
    [Key]
    public int BookId { get; set; }

    [Required, MaxLength(20)]
    public string BookCode { get; set; } = "";

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(100)]
    public string? Author { get; set; }

    [MaxLength(150)]
    public string? Publisher { get; set; }

    public decimal Price { get; set; }

    public int? PublicationYear { get; set; }

    public int CategoryId { get; set; }          // khóa ngoại
    public Category? Category { get; set; }

    public List<BookCopy> Copies { get; set; } = new();
}
