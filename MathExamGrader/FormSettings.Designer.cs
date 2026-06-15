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
        this.ClientSize = new Size(600, 580);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Font = new Font("Segoe UI", 10F);
        this.BackColor = Color.FromArgb(250, 250, 255);

        // === Tab Control ===
        tabControl = new TabControl();
        tabControl.Location = new Point(10, 10);
        tabControl.Size = new Size(575, 480);

        // --- Tab 1: Gemini ---
        tabGemini = new TabPage("🤖 Gemini Account");
        tabGemini.Padding = new Padding(15);

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

        chkFormEnabled = new CheckBox { Text = "Bật submit kết quả lên Google Form", Location = new Point(15, 20), AutoSize = true };

        lblFormUrl2 = new Label { Text = "Google Form URL:", Location = new Point(15, 55), AutoSize = true };
        txtFormUrl2 = new TextBox { Location = new Point(15, 80), Size = new Size(520, 30), PlaceholderText = "https://docs.google.com/forms/d/e/xxx/viewform" };

        lblFieldMapping = new Label
        {
            Text = "📝 Mapping Field (Entry ID từ Google Form):",
            Location = new Point(15, 120),
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        lblFieldName = new Label { Text = "Tên học sinh:", Location = new Point(15, 150), AutoSize = true };
        txtFieldName = new TextBox { Location = new Point(200, 148), Size = new Size(335, 30), PlaceholderText = "entry.123456789" };

        lblFieldExam = new Label { Text = "Tên bài thi:", Location = new Point(15, 185), AutoSize = true };
        txtFieldExam = new TextBox { Location = new Point(200, 183), Size = new Size(335, 30), PlaceholderText = "entry.234567890" };

        lblFieldScore = new Label { Text = "Điểm:", Location = new Point(15, 220), AutoSize = true };
        txtFieldScore = new TextBox { Location = new Point(200, 218), Size = new Size(335, 30), PlaceholderText = "entry.345678901" };

        lblFieldFeedback = new Label { Text = "Nhận xét:", Location = new Point(15, 255), AutoSize = true };
        txtFieldFeedback = new TextBox { Location = new Point(200, 253), Size = new Size(335, 30), PlaceholderText = "entry.456789012" };

        lblFieldDetail = new Label { Text = "Chi tiết:", Location = new Point(15, 290), AutoSize = true };
        txtFieldDetail = new TextBox { Location = new Point(200, 288), Size = new Size(335, 30), PlaceholderText = "entry.567890123" };

        lblFieldDate = new Label { Text = "Ngày chấm:", Location = new Point(15, 325), AutoSize = true };
        txtFieldDate = new TextBox { Location = new Point(200, 323), Size = new Size(335, 30), PlaceholderText = "entry.678901234" };

        lblFormNote = new Label
        {
            Text = "💡 Cách lấy Entry ID:\n" +
                   "1. Mở Google Form → F12 (DevTools)\n" +
                   "2. Inspect từng field → Tìm name=\"entry.XXXXXXX\"\n" +
                   "3. Copy entry ID vào các ô tương ứng",
            Location = new Point(15, 365),
            Size = new Size(520, 80),
            ForeColor = Color.FromArgb(80, 80, 120)
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
            lblFormNote
        });

        // --- Tab 3: Export ---
        tabExport = new TabPage("💾 Xuất kết quả");
        tabExport.Padding = new Padding(15);

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

        // === Buttons ===
        btnSave = new Button
        {
            Text = "💾 Lưu cấu hình",
            Location = new Point(330, 500),
            Size = new Size(140, 40),
            BackColor = Color.FromArgb(66, 133, 244),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold)
        };

        btnCancel = new Button
        {
            Text = "❌ Hủy",
            Location = new Point(480, 500),
            Size = new Size(90, 40),
            FlatStyle = FlatStyle.Flat
        };

        this.Controls.AddRange(new Control[] { tabControl, btnSave, btnCancel });
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
    private Label lblFormNote;

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
