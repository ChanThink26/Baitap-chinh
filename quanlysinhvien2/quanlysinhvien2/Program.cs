using quanlysinhvien2.Bus;
using quanlysinhvien2.Data.DAL;
using quanlysinhvien2.Data.Database;
using quanlysinhvien2.View;
using System;
using System.Windows.Forms;

namespace quanlysinhvien2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            try
            {
                SqliteDatabase.Initialize();
                var repository = new SqliteStudentRepository();
                var studentService = new SinhVienService(repository);
                Application.Run(new Form1(studentService));
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    $"Không thể khởi tạo hoặc truy cập cơ sở dữ liệu SQLite tại:{Environment.NewLine}{SqliteDatabase.DatabasePath}{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                    "Lỗi cơ sở dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}