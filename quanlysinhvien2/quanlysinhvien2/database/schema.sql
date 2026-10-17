CREATE DATABASE IF NOT EXISTS quanlysinhvien
	CHARACTER SET utf8mb4
	COLLATE utf8mb4_unicode_ci;

USE quanlysinhvien;

CREATE TABLE IF NOT EXISTS LopHoc
(
	MaLop VARCHAR(10) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
	TenLop VARCHAR(100) NOT NULL,
	PRIMARY KEY (MaLop)
) ENGINE = InnoDB
  DEFAULT CHARACTER SET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;

INSERT IGNORE INTO LopHoc (MaLop, TenLop)
VALUES
	('CNTT01', 'Kỹ thuật phần mềm 01'),
	('CNTT02', 'Trí tuệ nhân tạo 01'),
	('CNTT03', 'Khoa học dữ liệu 01'),
	('QTKD01', 'Quản trị kinh doanh 01');

CREATE TABLE IF NOT EXISTS SinhVien
(
	MaSV VARCHAR(15) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
	HoTen VARCHAR(100) NOT NULL,
	NgaySinh DATE NOT NULL,
	GioiTinh ENUM('Nam', 'Nữ', 'Khác') NOT NULL,
	Email VARCHAR(100) NOT NULL,
	SoDienThoai VARCHAR(20) NOT NULL,
	MaLop VARCHAR(10) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
	Diem DECIMAL(4, 2) NOT NULL DEFAULT 0.00,
	TrangThai VARCHAR(30) NOT NULL DEFAULT 'Đang học',
	PRIMARY KEY (MaSV),
	INDEX IX_SinhVien_MaLop (MaLop),
	CONSTRAINT FK_SinhVien_LopHoc
		FOREIGN KEY (MaLop) REFERENCES LopHoc (MaLop)
		ON UPDATE CASCADE
		ON DELETE RESTRICT,
	CONSTRAINT CK_SinhVien_Diem
		CHECK (Diem >= 0.00 AND Diem <= 10.00),
	CONSTRAINT CK_SinhVien_TrangThai
		CHECK (TrangThai IN ('Đang học', 'Bảo lưu', 'Đã tốt nghiệp'))
) ENGINE = InnoDB
  DEFAULT CHARACTER SET = utf8mb4
  COLLATE = utf8mb4_unicode_ci;
