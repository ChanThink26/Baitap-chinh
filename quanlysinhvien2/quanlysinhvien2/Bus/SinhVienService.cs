using quanlysinhvien2.Data.DAL;
using quanlysinhvien2.Data.Entities;

namespace quanlysinhvien2.Bus;

public sealed class SinhVienService(IStudentRepository repository)
{
    public IReadOnlyList<LopHoc> GetClasses() => repository.GetClasses();

    public IReadOnlyList<SinhVien> GetAllStudents() => repository.GetAllStudents();

    public SinhVien? GetStudentById(string studentId) => repository.GetStudentById(studentId);

    public IReadOnlyList<SinhVien> SearchStudents(string? keyword, string? classId, decimal? minimumScore)
    {
        var normalizedKeyword = keyword?.Trim();
        return repository.GetAllStudents()
            .Where(student => string.IsNullOrWhiteSpace(normalizedKeyword) ||
                student.MaSV.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ||
                student.HoTen.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ||
                student.Email.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ||
                student.SoDienThoai.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
            .Where(student => string.IsNullOrWhiteSpace(classId) ||
                string.Equals(student.MaLop, classId, StringComparison.OrdinalIgnoreCase))
            .Where(student => !minimumScore.HasValue || student.Diem >= minimumScore.Value)
            .ToList();
    }

    public void AddStudent(SinhVien student)
    {
        Normalize(student);
        ValidateStudent(student);

        if (repository.GetStudentById(student.MaSV) is not null)
        {
            throw new BusinessRuleException("Mã sinh viên đã tồn tại.");
        }

        repository.AddStudent(student);
    }

    public void UpdateStudent(SinhVien student)
    {
        Normalize(student);
        ValidateStudent(student);

        if (repository.GetStudentById(student.MaSV) is null)
        {
            throw new BusinessRuleException("Không tìm thấy sinh viên cần sửa.");
        }

        repository.UpdateStudent(student);
    }

    public void DeleteStudent(string studentId)
    {
        if (repository.GetStudentById(studentId) is null)
        {
            throw new BusinessRuleException("Không tìm thấy sinh viên cần xóa.");
        }

        repository.DeleteStudent(studentId);
    }

    private void ValidateStudent(SinhVien student)
    {
        if (!student.IsValid(out var validationResults))
        {
            throw new BusinessRuleException(
                string.Join(Environment.NewLine, validationResults.Select(result => result.ErrorMessage)));
        }

        if (!repository.GetClasses().Any(classItem =>
                string.Equals(classItem.MaLop, student.MaLop, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleException("Lớp học được chọn không tồn tại.");
        }
    }

    private static void Normalize(SinhVien student)
    {
        student.MaSV = student.MaSV.Trim();
        student.HoTen = student.HoTen.Trim();
        student.GioiTinh = student.GioiTinh.Trim();
        student.Email = student.Email.Trim();
        student.SoDienThoai = student.SoDienThoai.Trim();
        student.MaLop = student.MaLop.Trim();
        student.TrangThai = student.TrangThai.Trim();
    }
}
