USE quanlysinhvien;

INSERT IGNORE INTO SinhVien
	(MaSV, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, MaLop, Diem, TrangThai)
VALUES
	('SV001', 'Nguyễn Văn An', '2004-05-15', 'Nam', 'an.nguyen@example.com', '0912345678', 'CNTT01', 8.50, 'Đang học'),
	('SV002', 'Trần Thị Bình', '2004-09-22', 'Nữ', 'binh.tran@example.com', '0987654321', 'CNTT02', 9.00, 'Đang học'),
	('SV003', 'Lê Minh Châu', '2003-12-08', 'Khác', 'chau.le@example.com', '0901234567', 'QTKD01', 7.40, 'Bảo lưu'),
	('SV004', 'Đỗ Thị Hồng', '2004-11-30', 'Nữ', 'hong.do@example.com', '0777888999', 'CNTT03', 8.10, 'Đang học');
