using System.Drawing;

namespace QuanLySinhVienDAO.UI;

internal sealed class CopyEditDialog : Form
{
    private readonly ComboBox _statusInput = new();
    private readonly ComboBox _conditionInput = new();
    private readonly TextBox _locationInput = new();

    public bool IsBorrowed => _statusInput.SelectedIndex == 1;
    public string PhysicalCondition => _conditionInput.SelectedItem?.ToString() ?? "Mới";
    public string? Location => string.IsNullOrWhiteSpace(_locationInput.Text) ? null : _locationInput.Text.Trim();

    public CopyEditDialog(bool isBorrowed, string physicalCondition, string? location)
    {
        Text = "Cập nhật bản sao";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(410, 205);
        Font = new Font("Segoe UI", 9.5F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 2,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _statusInput.DropDownStyle = ComboBoxStyle.DropDownList;
        _statusInput.Items.AddRange(["Sẵn sàng", "Đang mượn"]);
        _statusInput.SelectedIndex = isBorrowed ? 1 : 0;
        _conditionInput.DropDownStyle = ComboBoxStyle.DropDownList;
        _conditionInput.Items.AddRange(["Mới", "Tốt", "Cũ", "Hỏng"]);
        _conditionInput.SelectedItem = physicalCondition;
        if (_conditionInput.SelectedIndex < 0)
        {
            _conditionInput.Items.Add(physicalCondition);
            _conditionInput.SelectedItem = physicalCondition;
        }
        _locationInput.Text = location ?? "";
        _statusInput.Dock = DockStyle.Fill;
        _conditionInput.Dock = DockStyle.Fill;
        _locationInput.Dock = DockStyle.Fill;

        AddRow(layout, 0, "Trạng thái:", _statusInput);
        AddRow(layout, 1, "Tình trạng vật lý:", _conditionInput);
        AddRow(layout, 2, "Vị trí lưu trữ:", _locationInput);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var cancelButton = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90, Height = 30 };
        var saveButton = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 90, Height = 30 };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control editor)
    {
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        layout.Controls.Add(editor, 1, row);
    }
}
