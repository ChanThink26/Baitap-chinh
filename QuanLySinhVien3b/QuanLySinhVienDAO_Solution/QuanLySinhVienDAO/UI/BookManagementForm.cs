using System.Drawing;
using System.Globalization;
using QuanLySinhVienDAO.Business;
using QuanLySinhVienDAO.DataAccess;
using QuanLySinhVienDAO.Entity;

namespace QuanLySinhVienDAO.UI;

public sealed class BookManagementForm : Form
{
    private readonly TextBox _searchTextBox = new();
    private readonly ComboBox _filterCategoryComboBox = new();
    private readonly TextBox _bookCodeTextBox = new();
    private readonly TextBox _titleTextBox = new();
    private readonly TextBox _authorTextBox = new();
    private readonly TextBox _publisherTextBox = new();
    private readonly ComboBox _bookCategoryComboBox = new();
    private readonly TextBox _priceTextBox = new();
    private readonly TextBox _yearTextBox = new();
    private readonly TextBox _copyCountTextBox = new();
    private readonly DataGridView _booksGrid = new();
    private readonly DataGridView _copiesGrid = new();
    private readonly Label _copiesTitleLabel = new();
    private readonly ToolStripStatusLabel _statusLabel = new();

    private readonly Button _searchButton = new();
    private readonly Button _refreshButton = new();
    private readonly Button _addBookButton = new();
    private readonly Button _updateBookButton = new();
    private readonly Button _deleteBookButton = new();
    private readonly Button _clearBookButton = new();
    private readonly Button _addCopiesButton = new();
    private readonly Button _updateCopyButton = new();
    private readonly Button _deleteCopyButton = new();

    private int? _selectedBookId;
    private bool _loadingBooks;

    public BookManagementForm()
    {
        Text = "Quản lý đầu sách và bản sao";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1050, 700);
        ClientSize = new Size(1140, 740);
        Font = new Font("Segoe UI", 9.5F);
        BackColor = Color.FromArgb(248, 249, 252);

        BuildLayout();
        WireEvents();
        Shown += async (_, _) => await RunUiActionAsync(LoadDataAsync);
    }

    private void WireEvents()
    {
        _searchButton.Click += async (_, _) => await RunUiActionAsync(() => LoadBooksAsync(_selectedBookId));
        _refreshButton.Click += async (_, _) =>
        {
            _searchTextBox.Clear();
            await RunUiActionAsync(async () =>
            {
                ClearBookForm();
                await LoadDataAsync();
            });
        };
        _addBookButton.Click += async (_, _) => await RunUiActionAsync(AddBookAsync);
        _updateBookButton.Click += async (_, _) => await RunUiActionAsync(UpdateBookAsync);
        _deleteBookButton.Click += async (_, _) => await RunUiActionAsync(DeleteBookAsync);
        _clearBookButton.Click += (_, _) => ClearBookForm();
        _addCopiesButton.Click += async (_, _) => await RunUiActionAsync(AddCopiesAsync);
        _updateCopyButton.Click += async (_, _) => await RunUiActionAsync(UpdateCopyAsync);
        _deleteCopyButton.Click += async (_, _) => await RunUiActionAsync(DeleteCopyAsync);
        _booksGrid.SelectionChanged += async (_, _) =>
        {
            if (_loadingBooks || _booksGrid.CurrentRow?.DataBoundItem is not BookGridItem row)
                return;

            await RunUiActionAsync(() => DisplayBookRowAsync(row));
        };
    }

    private async Task LoadDataAsync()
    {
        var categories = await UseServicesAsync((_, _, categoryService) => categoryService.GetAllAsync());
        PopulateCategories(categories);
        await LoadBooksAsync();
    }

    private void PopulateCategories(IReadOnlyList<Category> categories)
    {
        _bookCategoryComboBox.DataSource = null;
        _bookCategoryComboBox.DisplayMember = nameof(Category.Name);
        _bookCategoryComboBox.ValueMember = nameof(Category.CategoryId);
        _bookCategoryComboBox.DataSource = categories.ToList();
        if (categories.Count > 0)
            _bookCategoryComboBox.SelectedIndex = 0;

        var filterItems = new List<CategoryChoice> { new() { CategoryId = -1, Name = "-- Tất cả thể loại --" } };
        filterItems.AddRange(categories.Select(category => new CategoryChoice
        {
            CategoryId = category.CategoryId,
            Name = category.Name
        }));
        _filterCategoryComboBox.DataSource = null;
        _filterCategoryComboBox.DisplayMember = nameof(CategoryChoice.Name);
        _filterCategoryComboBox.ValueMember = nameof(CategoryChoice.CategoryId);
        _filterCategoryComboBox.DataSource = filterItems;
        _filterCategoryComboBox.SelectedIndex = 0;
    }

    private async Task LoadBooksAsync(int? preserveBookId = null)
    {
        int? categoryId = _filterCategoryComboBox.SelectedValue is int selectedCategoryId && selectedCategoryId > 0
            ? selectedCategoryId
            : null;
        var books = await UseServicesAsync((bookService, _, _) =>
            bookService.SearchAsync(_searchTextBox.Text, categoryId));
        var rows = books.Select(book => new BookGridItem
        {
            BookId = book.BookId,
            BookCode = book.BookCode,
            Title = book.Title,
            CategoryId = book.CategoryId,
            CategoryName = book.Category?.Name ?? "",
            Author = book.Author,
            Publisher = book.Publisher,
            Price = book.Price,
            PublicationYear = book.PublicationYear,
            CopyCount = book.Copies.Count
        }).ToList();

        _loadingBooks = true;
        _booksGrid.DataSource = null;
        _booksGrid.DataSource = rows;
        var selectedIndex = preserveBookId.HasValue
            ? rows.FindIndex(row => row.BookId == preserveBookId.Value)
            : -1;
        if (selectedIndex < 0 && rows.Count > 0)
            selectedIndex = 0;

        if (selectedIndex >= 0)
        {
            _booksGrid.Rows[selectedIndex].Selected = true;
            _booksGrid.CurrentCell = _booksGrid.Rows[selectedIndex].Cells[0];
        }
        else
        {
            ClearBookForm();
        }
        _loadingBooks = false;

        _statusLabel.Text = $"Đã tải {rows.Count} đầu sách lúc {DateTime.Now:HH:mm:ss}";
        if (selectedIndex >= 0)
            await DisplayBookRowAsync(rows[selectedIndex]);
    }

    private async Task DisplayBookRowAsync(BookGridItem row)
    {
        _selectedBookId = row.BookId;
        _bookCodeTextBox.Text = row.BookCode;
        _titleTextBox.Text = row.Title;
        _authorTextBox.Text = row.Author ?? "";
        _publisherTextBox.Text = row.Publisher ?? "";
        _priceTextBox.Text = row.Price.ToString("N0", CultureInfo.CurrentCulture);
        _yearTextBox.Text = row.PublicationYear?.ToString(CultureInfo.CurrentCulture) ?? "";
        _copyCountTextBox.Text = row.CopyCount.ToString(CultureInfo.CurrentCulture);
        _bookCategoryComboBox.SelectedValue = row.CategoryId;
        await LoadCopiesAsync(row.BookId, row.BookCode);
    }

    private async Task LoadCopiesAsync(int bookId, string? bookCode = null)
    {
        var copies = await UseServicesAsync((_, copyService, _) => copyService.GetByBookIdAsync(bookId));
        var rows = copies.Select(copy => new BookCopyGridItem
        {
            BookCopyId = copy.BookCopyId,
            Barcode = copy.Barcode,
            Status = copy.IsBorrowed ? "Đang mượn" : "Sẵn sàng",
            PhysicalCondition = copy.PhysicalCondition,
            IsBorrowed = copy.IsBorrowed,
            Location = copy.Location
        }).ToList();
        _copiesGrid.DataSource = rows;
        _copiesTitleLabel.Text = $"Danh sách bản sao của sách {bookCode ?? ""} ({rows.Count} bản sao)";
        _copyCountTextBox.Text = rows.Count.ToString(CultureInfo.CurrentCulture);
    }

    private async Task AddBookAsync()
    {
        var book = ReadBookFromForm();
        await UseServicesAsync(async (bookService, _, _) =>
        {
            await bookService.AddAsync(book);
            return true;
        });
        ClearBookForm();
        await LoadBooksAsync(book.BookId);
    }

    private async Task UpdateBookAsync()
    {
        if (!_selectedBookId.HasValue)
            throw new InvalidOperationException("Chọn đầu sách cần cập nhật trước.");

        var book = ReadBookFromForm();
        book.BookId = _selectedBookId.Value;
        var updated = await UseServicesAsync((bookService, _, _) => bookService.UpdateAsync(book));
        if (!updated)
            throw new InvalidOperationException("Không tìm thấy đầu sách cần cập nhật.");

        await LoadBooksAsync(book.BookId);
    }

    private async Task DeleteBookAsync()
    {
        if (!_selectedBookId.HasValue)
            throw new InvalidOperationException("Chọn đầu sách cần xóa trước.");
        if (MessageBox.Show(this, "Bạn có chắc muốn xóa đầu sách và các bản sao của sách này?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var deleted = await UseServicesAsync((bookService, _, _) => bookService.DeleteAsync(_selectedBookId.Value));
        if (!deleted)
            throw new InvalidOperationException("Không tìm thấy đầu sách cần xóa.");

        ClearBookForm();
        await LoadBooksAsync();
    }

    private async Task AddCopiesAsync()
    {
        if (!_selectedBookId.HasValue)
            throw new InvalidOperationException("Chọn đầu sách cần nhập bổ sung bản sao.");

        using var dialog = new CopyBatchDialog();
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var bookId = _selectedBookId.Value;
        await UseServicesAsync(async (_, copyService, _) =>
        {
            await copyService.AddCopiesAsync(bookId, dialog.Quantity, dialog.Location, dialog.PhysicalCondition);
            return true;
        });
        await LoadBooksAsync(bookId);
    }

    private async Task UpdateCopyAsync()
    {
        if (_copiesGrid.CurrentRow?.DataBoundItem is not BookCopyGridItem copy)
            throw new InvalidOperationException("Chọn bản sao cần cập nhật trước.");

        using var dialog = new CopyEditDialog(copy.IsBorrowed, copy.PhysicalCondition, copy.Location);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        await UseServicesAsync(async (_, copyService, _) =>
        {
            var updated = await copyService.UpdateAsync(copy.BookCopyId, dialog.IsBorrowed,
                dialog.PhysicalCondition, dialog.Location);
            if (!updated)
                throw new InvalidOperationException("Không tìm thấy bản sao cần cập nhật.");
            return true;
        });
        await LoadBooksAsync(_selectedBookId);
    }

    private async Task DeleteCopyAsync()
    {
        if (_copiesGrid.CurrentRow?.DataBoundItem is not BookCopyGridItem copy)
            throw new InvalidOperationException("Chọn bản sao cần xóa trước.");
        if (MessageBox.Show(this, $"Bạn có chắc muốn xóa bản sao {copy.Barcode}?", "Xác nhận xóa",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        await UseServicesAsync(async (_, copyService, _) =>
        {
            var deleted = await copyService.DeleteAsync(copy.BookCopyId);
            if (!deleted)
                throw new InvalidOperationException("Không tìm thấy bản sao cần xóa.");
            return true;
        });
        await LoadBooksAsync(_selectedBookId);
    }

    private Book ReadBookFromForm()
    {
        if (_bookCategoryComboBox.SelectedValue is not int categoryId)
            throw new InvalidOperationException("Chọn thể loại cho đầu sách.");
        if (!string.IsNullOrWhiteSpace(_priceTextBox.Text) &&
            !decimal.TryParse(_priceTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out _) &&
            !decimal.TryParse(_priceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            throw new ArgumentException("Giá sách phải là số hợp lệ.");
        if (!string.IsNullOrWhiteSpace(_yearTextBox.Text) &&
            !int.TryParse(_yearTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out _))
            throw new ArgumentException("Năm xuất bản phải là số nguyên hợp lệ.");

        decimal.TryParse(_priceTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out var price);
        if (price == 0)
            decimal.TryParse(_priceTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
        int? year = int.TryParse(_yearTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedYear)
            ? parsedYear
            : null;

        return new Book
        {
            BookCode = _bookCodeTextBox.Text,
            Title = _titleTextBox.Text,
            Author = _authorTextBox.Text,
            Publisher = _publisherTextBox.Text,
            CategoryId = categoryId,
            Price = price,
            PublicationYear = year
        };
    }

    private void ClearBookForm()
    {
        _selectedBookId = null;
        _bookCodeTextBox.Clear();
        _titleTextBox.Clear();
        _authorTextBox.Clear();
        _publisherTextBox.Clear();
        _priceTextBox.Clear();
        _yearTextBox.Clear();
        _copyCountTextBox.Text = "0";
        if (_bookCategoryComboBox.Items.Count > 0)
            _bookCategoryComboBox.SelectedIndex = 0;
        _booksGrid.ClearSelection();
        _copiesGrid.DataSource = null;
        _copiesTitleLabel.Text = "Danh sách bản sao";
    }

    private async Task RunUiActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            _statusLabel.Text = "Thao tác không thành công";
            MessageBox.Show(this, exception.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static async Task<TResult> UseServicesAsync<TResult>(
        Func<BookBusiness, BookCopyBusiness, CategoryBusiness, Task<TResult>> action)
    {
        await using var context = new QLSVDBContext();
        var bookDataAccess = new BookDataAccess(context);
        var copyDataAccess = new BookCopyDataAccess(context);
        var categoryDataAccess = new CategoryDataAccess(context);
        return await action(
            new BookBusiness(bookDataAccess),
            new BookCopyBusiness(bookDataAccess, copyDataAccess),
            new CategoryBusiness(categoryDataAccess));
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            ColumnCount = 1,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        Controls.Add(root);

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildSearchBar(), 0, 1);
        root.Controls.Add(BuildMainContent(), 0, 2);

        var statusStrip = new StatusStrip { Dock = DockStyle.Fill, SizingGrip = false };
        _statusLabel.Text = "Sẵn sàng";
        statusStrip.Items.Add(_statusLabel);
        root.Controls.Add(statusStrip, 0, 3);
    }

    private static Control BuildHeader()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(224, 242, 252),
            Padding = new Padding(16, 8, 8, 8)
        };
        var title = new Label
        {
            Text = "QUẢN LÝ ĐẦU SÁCH VÀ BẢN SAO",
            Dock = DockStyle.Top,
            Height = 38,
            Font = new Font("Segoe UI", 21, FontStyle.Bold),
            ForeColor = Color.FromArgb(20, 105, 150)
        };
        var subtitle = new Label
        {
            Text = "Tra cứu, cập nhật thông tin đầu sách và quản lý bản sao",
            Dock = DockStyle.Top,
            Height = 25,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = Color.FromArgb(80, 95, 105)
        };
        panel.Controls.Add(subtitle);
        panel.Controls.Add(title);
        return panel;
    }

    private Control BuildSearchBar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 10, 0, 0)
        };
        panel.Controls.Add(MakeSearchLabel("Từ khóa tra"));
        _searchTextBox.Width = 300;
        _searchTextBox.Margin = new Padding(0, 4, 12, 0);
        _searchTextBox.PlaceholderText = "Mã sách, tên sách, tác giả...";
        panel.Controls.Add(_searchTextBox);

        panel.Controls.Add(MakeSearchLabel("Thể loại"));
        _filterCategoryComboBox.Width = 230;
        _filterCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _filterCategoryComboBox.Margin = new Padding(0, 4, 8, 0);
        panel.Controls.Add(_filterCategoryComboBox);

        ConfigureButton(_searchButton, "Tìm kiếm", Color.FromArgb(0, 135, 190));
        _searchButton.Width = 105;
        panel.Controls.Add(_searchButton);
        ConfigureButton(_refreshButton, "Làm mới", Color.FromArgb(235, 238, 241), Color.FromArgb(55, 65, 75));
        _refreshButton.Width = 95;
        panel.Controls.Add(_refreshButton);
        return panel;
    }

    private Control BuildMainContent()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Size = new Size(1080, 600),
            Orientation = Orientation.Vertical,
            SplitterDistance = 350,
            Panel1MinSize = 330,
            Panel2MinSize = 600,
            BackColor = Color.FromArgb(220, 224, 230)
        };
        split.Panel1.Padding = new Padding(0, 0, 6, 0);
        split.Panel2.Padding = new Padding(6, 0, 0, 0);
        split.Panel1.Controls.Add(BuildBookEditor());
        split.Panel2.Controls.Add(BuildBookAndCopyLists());
        return split;
    }

    private Control BuildBookEditor()
    {
        var group = new GroupBox
        {
            Text = "Thông tin đầu sách",
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(0, 8, 0, 0)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var row = 0; row < 8; row++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        ConfigureInput(_bookCodeTextBox);
        ConfigureInput(_titleTextBox);
        ConfigureInput(_authorTextBox);
        ConfigureInput(_publisherTextBox);
        ConfigureInput(_priceTextBox);
        ConfigureInput(_yearTextBox);
        ConfigureInput(_copyCountTextBox);
        _copyCountTextBox.ReadOnly = true;
        _copyCountTextBox.TextAlign = HorizontalAlignment.Right;
        _bookCategoryComboBox.Dock = DockStyle.Fill;
        _bookCategoryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _bookCategoryComboBox.Margin = new Padding(0, 3, 0, 3);

        AddEditorRow(layout, 0, "Mã sách (*):", _bookCodeTextBox);
        AddEditorRow(layout, 1, "Tên sách (*):", _titleTextBox);
        AddEditorRow(layout, 2, "Tác giả (*):", _authorTextBox);
        AddEditorRow(layout, 3, "Nhà xuất bản:", _publisherTextBox);
        AddEditorRow(layout, 4, "Thể loại (*):", _bookCategoryComboBox);
        AddEditorRow(layout, 5, "Giá sách:", _priceTextBox);
        AddEditorRow(layout, 6, "Năm xuất bản:", _yearTextBox);
        AddEditorRow(layout, 7, "Số bản sao:", _copyCountTextBox);

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(0, 8, 0, 0)
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        buttons.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        ConfigureButton(_addBookButton, "Thêm đầu sách", Color.FromArgb(35, 165, 90));
        ConfigureButton(_updateBookButton, "Lưu chỉnh sửa", Color.FromArgb(225, 125, 20));
        ConfigureButton(_deleteBookButton, "Xóa đầu sách", Color.FromArgb(205, 45, 50));
        ConfigureButton(_clearBookButton, "Làm trống", Color.FromArgb(235, 238, 241), Color.FromArgb(55, 65, 75));
        ConfigureButton(_addCopiesButton, "NHẬP BỔ SUNG BẢN SAO", Color.FromArgb(0, 135, 190));
        buttons.Controls.Add(_addBookButton, 0, 0);
        buttons.Controls.Add(_updateBookButton, 1, 0);
        buttons.Controls.Add(_deleteBookButton, 0, 1);
        buttons.Controls.Add(_clearBookButton, 1, 1);
        buttons.Controls.Add(_addCopiesButton, 0, 2);
        buttons.SetColumnSpan(_addCopiesButton, 2);
        layout.Controls.Add(buttons, 0, 8);
        layout.SetColumnSpan(buttons, 2);
        group.Controls.Add(layout);
        return group;
    }

    private Control BuildBookAndCopyLists()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        layout.Controls.Add(BuildBooksGroup(), 0, 0);
        layout.Controls.Add(BuildCopiesGroup(), 0, 1);
        return layout;
    }

    private Control BuildBooksGroup()
    {
        var group = new GroupBox { Text = "Danh sách đầu sách", Dock = DockStyle.Fill, Padding = new Padding(8) };
        ConfigureGrid(_booksGrid);
        AddTextColumn(_booksGrid, nameof(BookGridItem.BookCode), "Mã sách", 85);
        AddTextColumn(_booksGrid, nameof(BookGridItem.Title), "Tên đầu sách", 180);
        AddTextColumn(_booksGrid, nameof(BookGridItem.CategoryName), "Thể loại", 105);
        AddTextColumn(_booksGrid, nameof(BookGridItem.Author), "Tác giả", 130);
        AddTextColumn(_booksGrid, nameof(BookGridItem.Publisher), "Nhà xuất bản", 130);
        AddTextColumn(_booksGrid, nameof(BookGridItem.Price), "Giá sách", 90, "N0");
        AddTextColumn(_booksGrid, nameof(BookGridItem.PublicationYear), "Năm", 60);
        group.Controls.Add(_booksGrid);
        return group;
    }

    private Control BuildCopiesGroup()
    {
        var group = new GroupBox { Text = "Bản sao", Dock = DockStyle.Fill, Padding = new Padding(8) };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

        var gridPanel = new Panel { Dock = DockStyle.Fill };
        _copiesTitleLabel.Text = "Danh sách bản sao";
        _copiesTitleLabel.Dock = DockStyle.Top;
        _copiesTitleLabel.Height = 25;
        _copiesTitleLabel.Font = new Font(Font, FontStyle.Bold);
        ConfigureGrid(_copiesGrid);
        _copiesGrid.Dock = DockStyle.Fill;
        AddTextColumn(_copiesGrid, nameof(BookCopyGridItem.Barcode), "Mã bản sao", 180);
        AddTextColumn(_copiesGrid, nameof(BookCopyGridItem.Status), "Trạng thái", 150);
        AddTextColumn(_copiesGrid, nameof(BookCopyGridItem.PhysicalCondition), "Tình trạng vật lý", 180);
        gridPanel.Controls.Add(_copiesGrid);
        gridPanel.Controls.Add(_copiesTitleLabel);
        layout.Controls.Add(gridPanel, 0, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };
        ConfigureButton(_updateCopyButton, "Cập nhật bản sao", Color.FromArgb(235, 238, 241), Color.FromArgb(55, 65, 75));
        ConfigureButton(_deleteCopyButton, "Xóa bản sao", Color.FromArgb(245, 240, 240), Color.FromArgb(150, 65, 65));
        buttons.Controls.Add(_updateCopyButton);
        buttons.Controls.Add(_deleteCopyButton);
        layout.Controls.Add(buttons, 0, 1);
        group.Controls.Add(layout);
        return group;
    }

    private static Label MakeSearchLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Margin = new Padding(0, 8, 8, 0)
        };
    }

    private static void ConfigureInput(TextBox textBox)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Margin = new Padding(0, 3, 0, 3);
    }

    private static void AddEditorRow(TableLayoutPanel layout, int row, string label, Control editor)
    {
        layout.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 5, 0)
        }, 0, row);
        layout.Controls.Add(editor, 1, row);
    }

    private static void ConfigureButton(Button button, string text, Color backColor, Color? foreColor = null)
    {
        button.Text = text;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(3);
        button.BackColor = backColor;
        button.ForeColor = foreColor ?? Color.White;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.AutoGenerateColumns = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.BorderStyle = BorderStyle.FixedSingle;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(225, 238, 248);
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        grid.RowTemplate.Height = 30;
    }

    private static void AddTextColumn(DataGridView grid, string propertyName, string header, int minimumWidth, string? format = null)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = header,
            MinimumWidth = minimumWidth,
            FillWeight = minimumWidth,
            DefaultCellStyle = { Format = format ?? "" }
        });
    }

    private sealed class BookGridItem
    {
        public int BookId { get; init; }
        public string BookCode { get; init; } = "";
        public string Title { get; init; } = "";
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = "";
        public string? Author { get; init; }
        public string? Publisher { get; init; }
        public decimal Price { get; init; }
        public int? PublicationYear { get; init; }
        public int CopyCount { get; init; }
    }

    private sealed class BookCopyGridItem
    {
        public int BookCopyId { get; init; }
        public string Barcode { get; init; } = "";
        public string Status { get; init; } = "";
        public string PhysicalCondition { get; init; } = "";
        public bool IsBorrowed { get; init; }
        public string? Location { get; init; }
    }

    private sealed class CategoryChoice
    {
        public int CategoryId { get; init; }
        public string Name { get; init; } = "";
    }
}
