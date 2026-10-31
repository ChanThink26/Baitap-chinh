using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVienDAO.Entity;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";

    public List<Book> Books { get; set; } = new();
}
