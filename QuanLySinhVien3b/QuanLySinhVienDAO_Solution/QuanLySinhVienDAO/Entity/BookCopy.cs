using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVienDAO.Entity;

public class BookCopy
{
    [Key]
    public int BookCopyId { get; set; }

    [Required, MaxLength(30)]
    public string Barcode { get; set; } = "";

    [MaxLength(50)]
    public string? Location { get; set; }

    [Required, MaxLength(50)]
    public string PhysicalCondition { get; set; } = "Mới";

    public bool IsBorrowed { get; set; }

    public int BookId { get; set; }              // khóa ngoại
    public Book? Book { get; set; }
}
