using System.Drawing;

namespace QuanLySinhVienDAO.UI;

internal sealed class CopyBatchDialog : Form
{
    private readonly NumericUpDown _quantityInput = new();
    private readonly TextBox _locationInput = new();
    private readonly ComboBox _conditionInput = new();

    public int Quantity => (int)_quantityInput.Value;
    public string? Location => string.IsNullOrWhiteSpace(_locationInput.Text) ? null : _locationInput.Text.Trim();
    public string PhysicalCondition => _conditionInput.SelectedItem?.ToString() ?? "Mới";

    public CopyBatchDialog()
    {
        Text = "Nhập bổ sung bản sao";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(390, 205);
        Font = new Font("Segoe UI", 9.5F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 2,
            RowCount = 4
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _quantityInput.Minimum = 1;
        _quantityInput.Maximum = 1000;
        _quantityInput.Value = 1;
        _quantityInput.Dock = DockStyle.Fill;
        _locationInput.Dock = DockStyle.Fill;
        _conditionInput.Dock = DockStyle.Fill;
        _conditionInput.DropDownStyle = ComboBoxStyle.DropDownList;
        _conditionInput.Items.AddRange(["Mới", "Tốt", "Cũ", "Hỏng"]);
        _conditionInput.SelectedIndex = 0;

        AddRow(layout, 0, "Số bản sao:", _quantityInput);
        AddRow(layout, 1, "Vị trí lưu trữ:", _locationInput);
        AddRow(layout, 2, "Tình trạng vật lý:", _conditionInput);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        var cancelButton = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90, Height = 30 };
        var saveButton = new Button { Text = "Thêm", DialogResult = DialogResult.OK, Width = 90, Height = 30 };
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
