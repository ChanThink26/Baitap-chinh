using Microsoft.Data.Sqlite;

namespace quanlysinhvien2.Data.Database;

internal static class SqliteDatabase
{
    private const string ApplicationFolder = "QuanLySinhVien";
    private const string DatabaseFileName = "quanlysinhvien.db";

    public static string DatabasePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        ApplicationFolder,
        DatabaseFileName);

    public static void Initialize()
    {
        var directory = Path.GetDirectoryName(DatabasePath)!;
        Directory.CreateDirectory(directory);

        using var connection = CreateOpenConnection();
        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS LopHoc
            (
                MaLop TEXT NOT NULL COLLATE NOCASE PRIMARY KEY,
                TenLop TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS SinhVien
            (
                MaSV TEXT NOT NULL COLLATE NOCASE PRIMARY KEY,
                HoTen TEXT NOT NULL,
                NgaySinh TEXT NOT NULL,
                GioiTinh TEXT NOT NULL CHECK (GioiTinh IN ('Nam', 'Nữ', 'Khác')),
                Email TEXT NOT NULL,
                SoDienThoai TEXT NOT NULL,
                MaLop TEXT NOT NULL,
                Diem REAL NOT NULL CHECK (Diem >= 0 AND Diem <= 10),
                TrangThai TEXT NOT NULL CHECK (TrangThai IN ('Đang học', 'Bảo lưu', 'Đã tốt nghiệp')),
                CONSTRAINT FK_SinhVien_LopHoc
                    FOREIGN KEY (MaLop) REFERENCES LopHoc (MaLop)
                    ON UPDATE CASCADE
                    ON DELETE RESTRICT
            );

            CREATE INDEX IF NOT EXISTS IX_SinhVien_MaLop ON SinhVien (MaLop);

            INSERT OR IGNORE INTO LopHoc (MaLop, TenLop) VALUES
                ('CNTT01', 'Kỹ thuật phần mềm 01'),
                ('CNTT02', 'Trí tuệ nhân tạo 01'),
                ('CNTT03', 'Khoa học dữ liệu 01'),
                ('QTKD01', 'Quản trị kinh doanh 01');
            """;
        command.ExecuteNonQuery();
        transaction.Commit();
    }

    public static SqliteConnection CreateOpenConnection()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            DefaultTimeout = 5
        };
        var connection = new SqliteConnection(builder.ConnectionString);

        try
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA foreign_keys = ON;";
            command.ExecuteNonQuery();
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
