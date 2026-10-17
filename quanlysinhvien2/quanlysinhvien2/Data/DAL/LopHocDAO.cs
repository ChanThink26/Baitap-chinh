using Microsoft.Data.Sqlite;
using quanlysinhvien2.Data.Database;
using quanlysinhvien2.Data.Entities;

namespace quanlysinhvien2.Data.DAL;

public sealed class LopHocDAO
{
    public List<LopHoc> GetAll()
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT MaLop, TenLop FROM LopHoc ORDER BY MaLop;";
        using var reader = command.ExecuteReader();

        var classes = new List<LopHoc>();
        while (reader.Read())
        {
            classes.Add(new LopHoc
            {
                MaLop = reader.GetString(reader.GetOrdinal("MaLop")),
                TenLop = reader.GetString(reader.GetOrdinal("TenLop"))
            });
        }

        return classes;
    }

    public LopHoc? GetById(string classId)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT MaLop, TenLop FROM LopHoc WHERE MaLop = @MaLop;";
        command.Parameters.AddWithValue("@MaLop", classId);
        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return null;
        }

        return new LopHoc
        {
            MaLop = reader.GetString(reader.GetOrdinal("MaLop")),
            TenLop = reader.GetString(reader.GetOrdinal("TenLop"))
        };
    }

    public void Add(LopHoc classItem)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO LopHoc (MaLop, TenLop) VALUES (@MaLop, @TenLop);";
        AddClassParameters(command, classItem);
        command.ExecuteNonQuery();
    }

    public void Update(LopHoc classItem)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE LopHoc SET TenLop = @TenLop WHERE MaLop = @MaLop;";
        AddClassParameters(command, classItem);
        command.ExecuteNonQuery();
    }

    public void Delete(string classId)
    {
        using var connection = SqliteDatabase.CreateOpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM LopHoc WHERE MaLop = @MaLop;";
        command.Parameters.AddWithValue("@MaLop", classId);
        command.ExecuteNonQuery();
    }

    private static void AddClassParameters(SqliteCommand command, LopHoc classItem)
    {
        command.Parameters.AddWithValue("@MaLop", classItem.MaLop);
        command.Parameters.AddWithValue("@TenLop", classItem.TenLop);
    }
}
