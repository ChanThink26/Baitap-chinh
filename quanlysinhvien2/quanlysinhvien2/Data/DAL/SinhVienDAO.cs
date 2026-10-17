using Microsoft.Data.Sqlite;
using System.Globalization;
using quanlysinhvien2.Data.Database;
using quanlysinhvien2.Data.Entities;

namespace quanlysinhvien2.Data.DAL;

public sealed class SinhVienDAO
{
    private const string SelectColumns = "MaSV, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, MaLop, Diem, TrangThai";

    public List<SinhVien> GetAll()
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM SinhVien ORDER BY MaSV;";
        using var reader = command.ExecuteReader();

        var students = new List<SinhVien>();
        while (reader.Read())
        {
            students.Add(MapStudent(reader));
        }

        return students;
    }

    public SinhVien? GetById(string studentId)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM SinhVien WHERE MaSV = @MaSV;";
        command.Parameters.AddWithValue("@MaSV", studentId);
        using var reader = command.ExecuteReader();

        return reader.Read() ? MapStudent(reader) : null;
    }

    public List<SinhVien> GetByClassId(string classId)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM SinhVien WHERE MaLop = @MaLop ORDER BY MaSV;";
        command.Parameters.AddWithValue("@MaLop", classId);
        using var reader = command.ExecuteReader();

        var students = new List<SinhVien>();
        while (reader.Read())
        {
            students.Add(MapStudent(reader));
        }

        return students;
    }

    public void Add(SinhVien student)
    {
        const string sql = """
            INSERT INTO SinhVien
                (MaSV, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, MaLop, Diem, TrangThai)
            VALUES
                (@MaSV, @HoTen, @NgaySinh, @GioiTinh, @Email, @SoDienThoai, @MaLop, @Diem, @TrangThai);
            """;
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddStudentParameters(command, student);
        command.ExecuteNonQuery();
    }

    public void Update(SinhVien student)
    {
        const string sql = """
            UPDATE SinhVien
            SET HoTen = @HoTen,
                NgaySinh = @NgaySinh,
                GioiTinh = @GioiTinh,
                Email = @Email,
                SoDienThoai = @SoDienThoai,
                MaLop = @MaLop,
                Diem = @Diem,
                TrangThai = @TrangThai
            WHERE MaSV = @MaSV;
            """;
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddStudentParameters(command, student);
        if (command.ExecuteNonQuery() == 0)
        {
            throw new KeyNotFoundException($"Không tìm thấy sinh viên có mã '{student.MaSV}'.");
        }
    }

    public void Delete(string studentId)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM SinhVien WHERE MaSV = @MaSV;";
        command.Parameters.AddWithValue("@MaSV", studentId);
        if (command.ExecuteNonQuery() == 0)
        {
            throw new KeyNotFoundException($"Không tìm thấy sinh viên có mã '{studentId}'.");
        }
    }

    private static void AddStudentParameters(SqliteCommand command, SinhVien student)
    {
        command.Parameters.AddWithValue("@MaSV", student.MaSV);
        command.Parameters.AddWithValue("@HoTen", student.HoTen);
        command.Parameters.AddWithValue("@NgaySinh", student.NgaySinh.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("@GioiTinh", student.GioiTinh);
        command.Parameters.AddWithValue("@Email", student.Email);
        command.Parameters.AddWithValue("@SoDienThoai", student.SoDienThoai);
        command.Parameters.AddWithValue("@MaLop", student.MaLop);
        command.Parameters.AddWithValue("@Diem", (double)student.Diem);
        command.Parameters.AddWithValue("@TrangThai", student.TrangThai);
    }

    private static SinhVien MapStudent(SqliteDataReader reader)
    {
        var birthDate = reader.GetString(reader.GetOrdinal("NgaySinh"));
        return new SinhVien
        {
            MaSV = reader.GetString(reader.GetOrdinal("MaSV")),
            HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
            NgaySinh = DateTime.ParseExact(birthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            GioiTinh = reader.GetString(reader.GetOrdinal("GioiTinh")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            SoDienThoai = reader.GetString(reader.GetOrdinal("SoDienThoai")),
            MaLop = reader.GetString(reader.GetOrdinal("MaLop")),
            Diem = Convert.ToDecimal(reader.GetDouble(reader.GetOrdinal("Diem")), CultureInfo.InvariantCulture),
            TrangThai = reader.GetString(reader.GetOrdinal("TrangThai"))
        };
    }
}
