using quanlysinhvien2.Data.Database;
using quanlysinhvien2.Data.Entities;

namespace quanlysinhvien2.Data.DAL;

public sealed class SqliteStudentRepository : IStudentRepository
{
    private readonly LopHocDAO _classDao;
    private readonly SinhVienDAO _studentDao;

    public SqliteStudentRepository()
        : this(new LopHocDAO(), new SinhVienDAO())
    {
    }

    public SqliteStudentRepository(LopHocDAO classDao, SinhVienDAO studentDao)
    {
        _classDao = classDao;
        _studentDao = studentDao;
    }

    public IReadOnlyList<LopHoc> GetClasses()
    {
        var (classes, _) = LoadClassGraph();
        return classes.AsReadOnly();
    }

    public IReadOnlyList<SinhVien> GetAllStudents()
    {
        var (_, students) = LoadClassGraph();
        return students.AsReadOnly();
    }

    public SinhVien? GetStudentById(string studentId)
    {
        var student = _studentDao.GetById(studentId);
        if (student is null)
        {
            return null;
        }

        student.LopHoc = _classDao.GetById(student.MaLop);
        student.LopHoc?.DanhSachSinhVien.Add(student);
        return student;
    }

    public void AddStudent(SinhVien student) => _studentDao.Add(student);

    public void UpdateStudent(SinhVien student) => _studentDao.Update(student);

    public void DeleteStudent(string studentId) => _studentDao.Delete(studentId);

    private (List<LopHoc> Classes, List<SinhVien> Students) LoadClassGraph()
    {
        var classes = _classDao.GetAll();
        var classesById = classes.ToDictionary(classItem => classItem.MaLop, StringComparer.OrdinalIgnoreCase);
        var students = _studentDao.GetAll();

        foreach (var student in students)
        {
            if (classesById.TryGetValue(student.MaLop, out var classItem))
            {
                student.LopHoc = classItem;
                classItem.DanhSachSinhVien.Add(student);
            }
        }

        return (classes, students);
    }
}
