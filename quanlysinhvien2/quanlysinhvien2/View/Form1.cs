using Microsoft.Data.Sqlite;
using quanlysinhvien2.Bus;
using quanlysinhvien2.Data.Entities;

namespace quanlysinhvien2.View;

public sealed class Form1 : Form
{
    private readonly SinhVienService _studentService;
    private readonly TextBox _txtMaSV = new();
    private readonly TextBox _txtHoTen = new();
    private readonly DateTimePicker _dtpNgaySinh = new();
    private readonly ComboBox _cboGioiTinh = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtSoDienThoai = new();
    private readonly ComboBox _cboLop = new();
    private readonly NumericUpDown _numDiem = new();
    private readonly ComboBox _cboTrangThai = new();
    private readonly TextBox _txtTuKhoa = new();
    private readonly ComboBox _cboLocLop = new();
    private readonly NumericUpDown _numDiemTu = new();
    private readonly DataGridView _gridStudents = new();
    private readonly Label _lblTongSo = new();
    private readonly Button _btnNhap = new();
    private readonly Button _btnSua = new();
    private readonly Button _btnXoa = new();
    private readonly Button _btnLamMoi = new();
    private readonly Button _btnTimKiem = new();
    private readonly Button _btnHienThiTatCa = new();
    private bool _isUpdatingId;
    private bool _isRefreshingGrid;

    public Form1(SinhVienService studentService)
    {
        _studentService = studentService;
        BuildInterface();
        _txtMaSV.TextChanged += TxtMaSV_TextChanged;
        _btnNhap.Click += BtnNhap_Click;
        _btnSua.Click += BtnSua_Click;
        _btnXoa.Click += BtnXoa_Click;
        _btnLamMoi.Click += (_, _) => ClearForm();
        _btnTimKiem.Click += (_, _) => ApplyFilters();
        _btnHienThiTatCa.Click += (_, _) => ShowAllStudents();
        _gridStudents.SelectionChanged += GridStudents_SelectionChanged;
        _gridStudents.CellFormatting += GridStudents_CellFormatting;
        Load += Form1_Load;
    }

    private void BuildInterface()
    {
        Text = "Quản lý sinh viên";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 700);
        Size = new Size(1240, 780);
        Font = new Font("Segoe UI", 9F);
        BackColor = Color.FromArgb(241, 245, 249);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12, 0, 12, 8),
            ColumnCount = 1,
            RowCount = 7,
            BackColor = BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 198));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        Controls.Add(layout);

        var topBar = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(27, 73, 112) };
        var appName = new Label
        {
            Text = "▣ Ứng dụng quản lý sinh viên",
            Dock = DockStyle.Left,
            Width = 330,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.White,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(8, 0, 0, 0)
        };
        topBar.Controls.Add(appName);
        layout.Controls.Add(topBar, 0, 0);

        var titlePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.White,
            Padding = new Padding(2, 0, 8, 0)
        };
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        titlePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        var heading = new Label
        {
            Text = "QUẢN LÝ SINH VIÊN",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = Color.FromArgb(27, 73, 112)
        };
        var subtitle = new Label
        {
            Text = "Bài tập Windows Forms • Quản lý sinh viên",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(85, 102, 120),
            Font = new Font("Segoe UI", 8F)
        };
        titlePanel.Controls.Add(heading, 0, 0);
        titlePanel.Controls.Add(subtitle, 1, 0);
        layout.Controls.Add(titlePanel, 0, 1);

        var fieldsGroup = new GroupBox
        {
            Text = "Thông tin sinh viên",
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(27, 73, 112),
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 2)
        };
        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 3,
            Padding = new Padding(2, 8, 2, 2),
            BackColor = Color.White
        };
        for (var column = 0; column < 3; column++)
        {
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        }
        for (var row = 0; row < 3; row++)
        {
            fields.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        }

        ConfigureTextBox(_txtMaSV, 0);
        _txtMaSV.CharacterCasing = CharacterCasing.Upper;
        ConfigureTextBox(_txtHoTen, 1);
        ConfigureComboBox(_cboLop, 2);
        _cboLop.DropDownStyle = ComboBoxStyle.DropDownList;
        _dtpNgaySinh.Format = DateTimePickerFormat.Custom;
        _dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
        _dtpNgaySinh.Dock = DockStyle.Fill;
        _dtpNgaySinh.TabIndex = 3;
        _dtpNgaySinh.Margin = new Padding(4, 5, 8, 4);
        _cboGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboGioiTinh.Items.AddRange(["Nam", "Nữ", "Khác"]);
        ConfigureComboBox(_cboGioiTinh, 4);
        ConfigureScoreControl(_numDiem, 5);
        ConfigureTextBox(_txtEmail, 6);
        ConfigureTextBox(_txtSoDienThoai, 7);
        _cboTrangThai.Items.AddRange(["Đang học", "Bảo lưu", "Đã tốt nghiệp"]);
        ConfigureComboBox(_cboTrangThai, 8);
        _cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;

        AddField(fields, "Mã sinh viên *", _txtMaSV, 0, 0);
        AddField(fields, "Họ và tên *", _txtHoTen, 0, 2);
        AddField(fields, "Lớp học *", _cboLop, 0, 4);
        AddField(fields, "Ngày sinh", _dtpNgaySinh, 1, 0);
        AddField(fields, "Giới tính", _cboGioiTinh, 1, 2);
        AddField(fields, "Điểm", _numDiem, 1, 4);
        AddField(fields, "Email", _txtEmail, 2, 0);
        AddField(fields, "Điện thoại", _txtSoDienThoai, 2, 2);
        AddField(fields, "Trạng thái", _cboTrangThai, 2, 4);
        fieldsGroup.Controls.Add(fields);
        layout.Controls.Add(fieldsGroup, 0, 2);

        var actionPanel = new Panel { Dock = DockStyle.Fill, BackColor = BackColor };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 3, 0, 0),
            BackColor = BackColor
        };
        ConfigureButton(_btnNhap, "Thêm", Color.FromArgb(43, 137, 91));
        ConfigureButton(_btnSua, "Sửa", Color.FromArgb(40, 111, 176));
        ConfigureButton(_btnXoa, "Xóa", Color.FromArgb(197, 77, 82));
        ConfigureButton(_btnLamMoi, "Làm mới", Color.FromArgb(100, 112, 124));
        _btnNhap.TabIndex = 9;
        _btnSua.TabIndex = 10;
        _btnXoa.TabIndex = 11;
        _btnLamMoi.TabIndex = 12;
        buttons.Controls.AddRange([_btnNhap, _btnSua, _btnXoa, _btnLamMoi]);
        actionPanel.Controls.Add(buttons);
        layout.Controls.Add(actionPanel, 0, 3);

        var searchGroup = new GroupBox
        {
            Text = "Tìm kiếm và lọc",
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(27, 73, 112),
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(8, 10, 8, 4),
            Margin = new Padding(0, 0, 0, 4)
        };
        var searchLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 1,
            BackColor = Color.White
        };
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94));
        searchLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        ConfigureSearchInput(_txtTuKhoa);
        ConfigureSearchInput(_cboLocLop);
        _txtTuKhoa.TabIndex = 13;
        _cboLocLop.TabIndex = 14;
        _cboLocLop.DropDownStyle = ComboBoxStyle.DropDownList;
        ConfigureScoreControl(_numDiemTu, 15);
        _numDiemTu.DecimalPlaces = 1;
        _numDiemTu.Increment = 0.5m;
        _numDiemTu.Width = 74;
        ConfigureButton(_btnTimKiem, "Tìm kiếm", Color.FromArgb(40, 111, 176));
        ConfigureButton(_btnHienThiTatCa, "Hiển thị tất cả", Color.FromArgb(228, 239, 250));
        _btnHienThiTatCa.ForeColor = Color.FromArgb(35, 86, 130);
        _btnTimKiem.Size = new Size(88, 30);
        _btnHienThiTatCa.Size = new Size(106, 30);
        _btnTimKiem.TabIndex = 16;
        _btnHienThiTatCa.TabIndex = 17;
        AddSearchField(searchLayout, "Từ khóa", _txtTuKhoa, 0, 1);
        AddSearchField(searchLayout, "Lớp", _cboLocLop, 2, 3);
        AddSearchField(searchLayout, "Điểm từ", _numDiemTu, 4, 5);
        searchLayout.Controls.Add(_btnTimKiem, 6, 0);
        searchLayout.Controls.Add(_btnHienThiTatCa, 7, 0);
        searchGroup.Controls.Add(searchLayout);
        layout.Controls.Add(searchGroup, 0, 4);

        var listGroup = new GroupBox
        {
            Text = string.Empty,
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            BackColor = Color.White,
            ForeColor = Color.FromArgb(27, 73, 112),
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 2)
        };
        var listLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            Padding = new Padding(0, 2, 0, 0)
        };
        listLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        listLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var listHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
        var listTitle = new Label
        {
            Text = "Danh sách sinh viên",
            Dock = DockStyle.Left,
            Width = 240,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(27, 73, 112),
            Font = new Font(Font, FontStyle.Bold)
        };
        _lblTongSo.Dock = DockStyle.Right;
        _lblTongSo.Width = 180;
        _lblTongSo.TextAlign = ContentAlignment.MiddleRight;
        _lblTongSo.ForeColor = Color.FromArgb(75, 92, 106);
        _lblTongSo.Font = new Font(Font, FontStyle.Regular);
        listHeader.Controls.Add(_lblTongSo);
        listHeader.Controls.Add(listTitle);
        ConfigureStudentGrid();
        listLayout.Controls.Add(listHeader, 0, 0);
        listLayout.Controls.Add(_gridStudents, 0, 1);
        listGroup.Controls.Add(listLayout);
        layout.Controls.Add(listGroup, 0, 5);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.FromArgb(231, 237, 243)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        footer.Controls.Add(new Label
        {
            Text = "Bài tập xây dựng Windows Forms quản lý sinh viên",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(75, 92, 106)
        }, 0, 0);
        footer.Controls.Add(new Label
        {
            Text = "Thêm • Sửa • Xóa • Tìm kiếm • Lọc theo lớp",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(75, 92, 106)
        }, 1, 0);
        layout.Controls.Add(footer, 0, 6);

        SetStudentMode(isExistingStudent: false);
    }

    private static void ConfigureTextBox(TextBox textBox, int tabIndex)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.TabIndex = tabIndex;
        textBox.Margin = new Padding(4, 5, 8, 4);
    }

    private static void ConfigureComboBox(ComboBox comboBox, int tabIndex)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.TabIndex = tabIndex;
        comboBox.Margin = new Padding(4, 5, 8, 4);
    }

    private static void ConfigureScoreControl(NumericUpDown numeric, int tabIndex)
    {
        numeric.Minimum = 0;
        numeric.Maximum = 10;
        numeric.DecimalPlaces = 1;
        numeric.Increment = 0.1m;
        numeric.Dock = DockStyle.Fill;
        numeric.TabIndex = tabIndex;
        numeric.Margin = new Padding(4, 5, 8, 4);
    }

    private static void ConfigureSearchInput(Control control)
    {
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(3, 2, 8, 2);
        control.Font = new Font("Segoe UI", 8.5F);
    }

    private static void AddSearchField(TableLayoutPanel layout, string labelText, Control editor, int labelColumn, int editorColumn)
    {
        layout.Controls.Add(new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(55, 72, 90),
            Font = new Font("Segoe UI", 8F),
            Margin = new Padding(2)
        }, labelColumn, 0);
        layout.Controls.Add(editor, editorColumn, 0);
    }

    private static void AddField(TableLayoutPanel layout, string labelText, Control editor, int row, int column)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            TabStop = false,
            ForeColor = Color.FromArgb(55, 72, 90),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            Margin = new Padding(2, 4, 5, 4)
        };
        layout.Controls.Add(label, column, row);
        layout.Controls.Add(editor, column + 1, row);
    }

    private static void ConfigureButton(Button button, string text, Color color)
    {
        button.Text = text;
        button.Size = new Size(90, 32);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Margin = new Padding(0, 0, 8, 0);
        button.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
    }

    private void ConfigureStudentGrid()
    {
        _gridStudents.Dock = DockStyle.Fill;
        _gridStudents.ReadOnly = true;
        _gridStudents.TabStop = false;
        _gridStudents.AllowUserToAddRows = false;
        _gridStudents.AllowUserToDeleteRows = false;
        _gridStudents.AllowUserToResizeRows = false;
        _gridStudents.AutoGenerateColumns = false;
        _gridStudents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _gridStudents.MultiSelect = false;
        _gridStudents.RowHeadersVisible = false;
        _gridStudents.BackgroundColor = Color.White;
        _gridStudents.GridColor = Color.FromArgb(223, 231, 238);
        _gridStudents.BorderStyle = BorderStyle.FixedSingle;
        _gridStudents.EnableHeadersVisualStyles = false;
        _gridStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(225, 237, 247);
        _gridStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 65, 91);
        _gridStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        _gridStudents.ColumnHeadersHeight = 28;
        _gridStudents.DefaultCellStyle.Font = new Font("Segoe UI", 8F);
        _gridStudents.DefaultCellStyle.SelectionBackColor = Color.FromArgb(207, 230, 246);
        _gridStudents.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 47, 62);
        _gridStudents.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 251);
        _gridStudents.RowTemplate.Height = 25;
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "MaSV", HeaderText = "Mã SV", DataPropertyName = "MaSV", Width = 88 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "HoTen", HeaderText = "Họ và tên", DataPropertyName = "HoTen", Width = 140 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "NgaySinh", HeaderText = "Ngày sinh", DataPropertyName = "NgaySinh", Width = 90 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "GioiTinh", HeaderText = "Giới tính", DataPropertyName = "GioiTinh", Width = 68 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 150, MinimumWidth = 125 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "SoDienThoai", HeaderText = "Điện thoại", DataPropertyName = "SoDienThoai", Width = 100 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "Diem", HeaderText = "Điểm", DataPropertyName = "Diem", Width = 55 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "TenLop", HeaderText = "Lớp", DataPropertyName = "TenLop", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, FillWeight = 120, MinimumWidth = 140 });
        _gridStudents.Columns.Add(new DataGridViewTextBoxColumn { Name = "TrangThai", HeaderText = "Trạng thái", DataPropertyName = "TrangThai", Width = 100 });
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        try
        {
            _cboLop.DisplayMember = nameof(LopHoc.TenLop);
            _cboLop.ValueMember = nameof(LopHoc.MaLop);
            _cboLop.DataSource = _studentService.GetClasses().ToList();
            _cboLocLop.DisplayMember = nameof(LopHoc.TenLop);
            _cboLocLop.ValueMember = nameof(LopHoc.MaLop);
            _cboLocLop.DataSource = new[] { new LopHoc { MaLop = string.Empty, TenLop = "Tất cả lớp" } }
                .Concat(_studentService.GetClasses())
                .ToList();
            RefreshStudentGrid();
            ClearForm();
        }
        catch (Exception exception)
        {
            SetStudentMode(isExistingStudent: false);
            ShowError(exception);
        }
    }

    private void TxtMaSV_TextChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingId)
        {
            return;
        }

        try
        {
            var student = _studentService.GetStudentById(_txtMaSV.Text.Trim());
            if (student is not null)
            {
                SetStudentFields(student);
                return;
            }

            ClearStudentFields();
            SetStudentMode(isExistingStudent: false);
        }
        catch (Exception exception)
        {
            SetStudentMode(isExistingStudent: false);
            ShowError(exception);
        }
    }

    private void GridStudents_SelectionChanged(object? sender, EventArgs e)
    {
        if (_isRefreshingGrid)
        {
            return;
        }

        if (_gridStudents.CurrentRow?.Cells[nameof(SinhVien.MaSV)].Value is not string studentId)
        {
            return;
        }

        try
        {
            var student = _studentService.GetStudentById(studentId);
            if (student is not null)
            {
                SetStudentFields(student);
            }
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private void BtnNhap_Click(object? sender, EventArgs e)
    {
        try
        {
            _studentService.AddStudent(CreateStudentFromForm());
            RefreshStudentGrid();
            ClearForm();
            MessageBox.Show(this, "Đã thêm sinh viên thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private void BtnSua_Click(object? sender, EventArgs e)
    {
        try
        {
            var student = CreateStudentFromForm();
            _studentService.UpdateStudent(student);
            RefreshStudentGrid();
            SetStudentFields(_studentService.GetStudentById(student.MaSV)!);
            MessageBox.Show(this, "Đã cập nhật thông tin sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private void BtnXoa_Click(object? sender, EventArgs e)
    {
        var studentId = _txtMaSV.Text.Trim();
        if (MessageBox.Show(this, $"Bạn có chắc muốn xóa sinh viên {studentId}?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            _studentService.DeleteStudent(studentId);
            RefreshStudentGrid();
            ClearForm();
            MessageBox.Show(this, "Đã xóa sinh viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private SinhVien CreateStudentFromForm() => new()
    {
        MaSV = _txtMaSV.Text,
        HoTen = _txtHoTen.Text,
        NgaySinh = _dtpNgaySinh.Value.Date,
        GioiTinh = _cboGioiTinh.SelectedItem as string ?? string.Empty,
        Email = _txtEmail.Text,
        SoDienThoai = _txtSoDienThoai.Text,
        MaLop = _cboLop.SelectedValue as string ?? string.Empty,
        Diem = _numDiem.Value,
        TrangThai = _cboTrangThai.SelectedItem as string ?? string.Empty
    };

    private void SetStudentFields(SinhVien student)
    {
        _isUpdatingId = true;
        _txtMaSV.Text = student.MaSV;
        _isUpdatingId = false;
        _txtHoTen.Text = student.HoTen;
        _dtpNgaySinh.Value = student.NgaySinh;
        _cboGioiTinh.SelectedItem = student.GioiTinh;
        _txtEmail.Text = student.Email;
        _txtSoDienThoai.Text = student.SoDienThoai;
        _cboLop.SelectedValue = student.MaLop;
        _numDiem.Value = student.Diem;
        _cboTrangThai.SelectedItem = student.TrangThai;
        SetStudentMode(isExistingStudent: true);
    }

    private void ClearStudentFields()
    {
        _txtHoTen.Clear();
        _dtpNgaySinh.Value = DateTime.Today.AddYears(-18);
        _cboGioiTinh.SelectedIndex = -1;
        _txtEmail.Clear();
        _txtSoDienThoai.Clear();
        _cboLop.SelectedIndex = _cboLop.Items.Count > 0 ? 0 : -1;
        _numDiem.Value = 0;
        _cboTrangThai.SelectedItem = "Đang học";
    }

    private void ClearForm()
    {
        _isUpdatingId = true;
        _txtMaSV.Clear();
        _isUpdatingId = false;
        ClearStudentFields();
        SetStudentMode(isExistingStudent: false);
        _gridStudents.ClearSelection();
        _txtMaSV.Focus();
    }

    private void SetStudentMode(bool isExistingStudent)
    {
        _txtMaSV.ReadOnly = isExistingStudent;
        _btnNhap.Enabled = !isExistingStudent;
        _btnSua.Enabled = isExistingStudent;
        _btnXoa.Enabled = isExistingStudent;
    }

    private void RefreshStudentGrid(IEnumerable<SinhVien>? students = null)
    {
        var studentList = (students ?? _studentService.GetAllStudents()).ToList();
        _isRefreshingGrid = true;
        _gridStudents.DataSource = studentList
            .Select(student => new
            {
                student.MaSV,
                student.HoTen,
                NgaySinh = student.NgaySinh.ToString("dd/MM/yyyy"),
                student.GioiTinh,
                student.Email,
                student.SoDienThoai,
                student.Diem,
                TenLop = student.LopHoc?.TenLop ?? student.MaLop,
                student.TrangThai
            })
            .ToList();
        _gridStudents.ClearSelection();
        _gridStudents.CurrentCell = null;
        _lblTongSo.Text = $"Tổng số: {studentList.Count} sinh viên";
        _isRefreshingGrid = false;
    }

    private void ApplyFilters()
    {
        try
        {
            var classId = _cboLocLop.SelectedValue as string;
            RefreshStudentGrid(_studentService.SearchStudents(_txtTuKhoa.Text, classId, _numDiemTu.Value));
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private void ShowAllStudents()
    {
        try
        {
            _txtTuKhoa.Clear();
            _cboLocLop.SelectedIndex = 0;
            _numDiemTu.Value = 0;
            RefreshStudentGrid();
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
    }

    private void GridStudents_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_gridStudents.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value is string status)
        {
            e.CellStyle.ForeColor = status == "Đang học"
                ? Color.FromArgb(42, 109, 79)
                : Color.FromArgb(133, 94, 37);
            e.CellStyle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        }
    }

    private void ShowError(Exception exception)
    {
        var message = exception switch
        {
            BusinessRuleException businessRuleException => businessRuleException.Message,
            KeyNotFoundException => "Không tìm thấy bản ghi cần thao tác.",
            SqliteException { SqliteExtendedErrorCode: 1555 or 2067 } => "Mã sinh viên hoặc mã lớp đã tồn tại.",
            SqliteException { SqliteExtendedErrorCode: 787 } => "Không thể xóa lớp vì vẫn còn sinh viên thuộc lớp đó.",
            SqliteException { SqliteErrorCode: 19 } => "Dữ liệu không hợp lệ hoặc vi phạm ràng buộc của cơ sở dữ liệu.",
            SqliteException { SqliteErrorCode: 14 } => "Không thể mở tệp cơ sở dữ liệu. Kiểm tra quyền ghi vào thư mục dữ liệu ứng dụng.",
            UnauthorizedAccessException => "Không có quyền truy cập tệp cơ sở dữ liệu.",
            IOException => "Không thể đọc hoặc ghi tệp cơ sở dữ liệu.",
            SqliteException sqliteException => $"Lỗi SQLite ({sqliteException.SqliteErrorCode}): {sqliteException.Message}",
            _ => $"Không thể hoàn thành thao tác. {exception.Message}"
        };

        MessageBox.Show(this, message, "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
