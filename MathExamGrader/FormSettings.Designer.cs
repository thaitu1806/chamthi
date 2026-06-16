namespace MathExamGrader;

partial class FormSettings
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.Text = "⚙️ Cấu hình";
        this.ClientSize = new Size(570, 680);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = false;
        this.MinimumSize = new Size(480, 500);
        this.Font = new Font("Segoe UI", 10F);
        this.BackColor = Color.FromArgb(250, 250, 255);

        // === Tab Control ===
        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;

        // --- Tab 1: Gemini ---
        tabGemini = new TabPage("🤖 Gemini Account");
        tabGemini.Padding = new Padding(15);
        tabGemini.AutoScroll = true;

        lblGeminiUrl = new Label { Text = "Gemini URL:", Location = new Point(15, 20), AutoSize = true };
        txtGeminiUrl = new TextBox { Location = new Point(15, 45), Size = new Size(520, 30) };

        lblGoogleAccount = new Label { Text = "Google Account (Email):", Location = new Point(15, 85), AutoSize = true };
        txtGoogleAccount = new TextBox { Location = new Point(15, 110), Size = new Size(520, 30), PlaceholderText = "teacher@gmail.com" };

        lblDelay = new Label { Text = "Thời gian chờ giữa các bài (ms):", Location = new Point(15, 155), AutoSize = true };
        numDelay = new NumericUpDown { Location = new Point(15, 180), Size = new Size(150, 30), Minimum = 1000, Maximum = 30000, Value = 5000, Increment = 1000 };

        lblMaxWait = new Label { Text = "Thời gian tối đa đợi Gemini trả lời (giây):", Location = new Point(15, 220), AutoSize = true };
        numMaxWait = new NumericUpDown { Location = new Point(15, 245), Size = new Size(150, 30), Minimum = 30, Maximum = 300, Value = 120, Increment = 10 };

        chkAutoLogin = new CheckBox { Text = "Tự động đăng nhập (dùng session đã lưu)", Location = new Point(15, 290), AutoSize = true, Checked = true };

        lblGeminiNote = new Label
        {
            Text = "💡 Hướng dẫn:\n" +
                   "1. Mở Edge → đăng nhập Google tại gemini.google.com\n" +
                   "2. Nhập email Google vào ô trên\n" +
                   "3. Tool sẽ dùng session Edge đã đăng nhập",
            Location = new Point(15, 330),
            Size = new Size(520, 100),
            ForeColor = Color.FromArgb(80, 80, 120)
        };

        tabGemini.Controls.AddRange(new Control[] {
            lblGeminiUrl, txtGeminiUrl,
            lblGoogleAccount, txtGoogleAccount,
            lblDelay, numDelay,
            lblMaxWait, numMaxWait,
            chkAutoLogin, lblGeminiNote
        });

        // --- Tab 2: Google Form ---
        tabForm = new TabPage("📋 Google Form");
        tabForm.Padding = new Padding(15);
        tabForm.AutoScroll = true;

        chkFormEnabled = new CheckBox { Text = "Bật submit kết quả lên Google Form", Location = new Point(15, 15), AutoSize = true };

        lblFormUrl2 = new Label { Text = "Google Form URL:", Location = new Point(15, 45), AutoSize = true };
        txtFormUrl2 = new TextBox { Location = new Point(15, 67), Size = new Size(520, 28), PlaceholderText = "https://docs.google.com/forms/d/e/xxx/viewform" };

        lblFieldMapping = new Label
        {
            Text = "📝 Mapping Field (Entry ID):",
            Location = new Point(15, 102),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };

        int fieldY = 125;
        int fieldGap = 28;

        lblFieldName = new Label { Text = "Tên học sinh:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldName = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.123456789" };

        fieldY += fieldGap;
        lblFieldExam = new Label { Text = "Tên bài thi:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldExam = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.234567890" };

        fieldY += fieldGap;
        lblFieldScore = new Label { Text = "Điểm:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldScore = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.345678901" };

        fieldY += fieldGap;
        lblFieldFeedback = new Label { Text = "Nhận xét:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldFeedback = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.456789012" };

        fieldY += fieldGap;
        lblFieldDetail = new Label { Text = "Chi tiết:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldDetail = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.567890123" };

        fieldY += fieldGap;
        lblFieldDate = new Label { Text = "Ngày chấm:", Location = new Point(15, fieldY), AutoSize = true };
        txtFieldDate = new TextBox { Location = new Point(170, fieldY - 2), Size = new Size(365, 28), PlaceholderText = "entry.678901234" };

        fieldY += fieldGap + 5;
        txtFormNote = new TextBox
        {
            Text = "📖 HƯỚNG DẪN TẠO GOOGLE FORM:\r\n\r\n" +
                   "1. Vào https://forms.google.com → Tạo form mới\r\n" +
                   "2. Thêm 6 câu hỏi dạng \"Câu trả lời ngắn\":\r\n" +
                   "     • Tên học sinh\r\n" +
                   "     • Tên bài thi\r\n" +
                   "     • Điểm\r\n" +
                   "     • Nhận xét\r\n" +
                   "     • Chi tiết\r\n" +
                   "     • Ngày chấm\r\n\r\n" +
                   "3. LẤY ENTRY ID:\r\n" +
                   "   - Mở form ở chế độ xem trước (Preview)\r\n" +
                   "   - Nhấn F12 mở DevTools → chọn tab Elements\r\n" +
                   "   - Click vào từng ô nhập trên form\r\n" +
                   "   - Tìm thẻ <input> có name=\"entry.1234567890\"\r\n" +
                   "   - Copy entry.XXX vào ô tương ứng ở trên\r\n\r\n" +
                   "4. Copy URL form (dạng /viewform) dán vào ô URL",
            Location = new Point(15, fieldY),
            Size = new Size(530, 180),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.FromArgb(248, 248, 255),
            ForeColor = Color.FromArgb(50, 50, 90),
            Font = new Font("Segoe UI", 9F),
            BorderStyle = BorderStyle.FixedSingle
        };

        tabForm.Controls.AddRange(new Control[] {
            chkFormEnabled, lblFormUrl2, txtFormUrl2,
            lblFieldMapping,
            lblFieldName, txtFieldName,
            lblFieldExam, txtFieldExam,
            lblFieldScore, txtFieldScore,
            lblFieldFeedback, txtFieldFeedback,
            lblFieldDetail, txtFieldDetail,
            lblFieldDate, txtFieldDate,
            txtFormNote
        });

        // --- Tab 3: Export ---
        tabExport = new TabPage("💾 Xuất kết quả");
        tabExport.Padding = new Padding(15);
        tabExport.AutoScroll = true;

        chkAutoExport = new CheckBox { Text = "Tự động xuất CSV sau khi chấm xong", Location = new Point(15, 20), AutoSize = true, Checked = true };

        lblCsvFolder = new Label { Text = "Thư mục lưu CSV:", Location = new Point(15, 60), AutoSize = true };
        txtCsvFolder = new TextBox { Location = new Point(15, 85), Size = new Size(430, 30), PlaceholderText = "Để trống = thư mục chứa ứng dụng" };
        btnBrowseFolder = new Button { Text = "📂", Location = new Point(450, 83), Size = new Size(50, 33), FlatStyle = FlatStyle.Flat };

        lblEncoding = new Label { Text = "Encoding:", Location = new Point(15, 130), AutoSize = true };
        cboEncoding = new ComboBox { Location = new Point(15, 155), Size = new Size(200, 30), DropDownStyle = ComboBoxStyle.DropDownList };
        cboEncoding.Items.AddRange(new object[] { "UTF-8", "UTF-8 BOM (Excel)", "Windows-1252" });
        cboEncoding.SelectedIndex = 0;

        tabExport.Controls.AddRange(new Control[] {
            chkAutoExport,
            lblCsvFolder, txtCsvFolder, btnBrowseFolder,
            lblEncoding, cboEncoding
        });

        // Add tabs
        tabControl.TabPages.Add(tabGemini);
        tabControl.TabPages.Add(tabForm);
        tabControl.TabPages.Add(tabExport);

        // === Buttons (Panel phía dưới) ===
        var panelButtons = new Panel();
        panelButtons.Dock = DockStyle.Bottom;
        panelButtons.Height = 55;
        panelButtons.Padding = new Padding(10, 8, 10, 8);

        btnSave = new Button
        {
            Text = "💾 Lưu cấu hình",
            Size = new Size(140, 38),
            BackColor = Color.FromArgb(66, 133, 244),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            Dock = DockStyle.Right
        };

        btnCancel = new Button
        {
            Text = "❌ Hủy",
            Size = new Size(90, 38),
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Right
        };

        panelButtons.Controls.Add(btnCancel);
        panelButtons.Controls.Add(btnSave);

        this.Controls.Add(tabControl);
        this.Controls.Add(panelButtons);
    }

    #endregion

    private TabControl tabControl;
    private TabPage tabGemini;
    private TabPage tabForm;
    private TabPage tabExport;

    // Gemini tab
    private Label lblGeminiUrl;
    private TextBox txtGeminiUrl;
    private Label lblGoogleAccount;
    private TextBox txtGoogleAccount;
    private Label lblDelay;
    private NumericUpDown numDelay;
    private Label lblMaxWait;
    private NumericUpDown numMaxWait;
    private CheckBox chkAutoLogin;
    private Label lblGeminiNote;

    // Form tab
    private CheckBox chkFormEnabled;
    private Label lblFormUrl2;
    private TextBox txtFormUrl2;
    private Label lblFieldMapping;
    private Label lblFieldName;
    private TextBox txtFieldName;
    private Label lblFieldExam;
    private TextBox txtFieldExam;
    private Label lblFieldScore;
    private TextBox txtFieldScore;
    private Label lblFieldFeedback;
    private TextBox txtFieldFeedback;
    private Label lblFieldDetail;
    private TextBox txtFieldDetail;
    private Label lblFieldDate;
    private TextBox txtFieldDate;
    private TextBox txtFormNote;

    // Export tab
    private CheckBox chkAutoExport;
    private Label lblCsvFolder;
    private TextBox txtCsvFolder;
    private Button btnBrowseFolder;
    private Label lblEncoding;
    private ComboBox cboEncoding;

    // Buttons
    private Button btnSave;
    private Button btnCancel;
}
