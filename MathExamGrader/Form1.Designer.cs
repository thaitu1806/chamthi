namespace MathExamGrader;

partial class Form1
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
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1100, 750);
        this.Text = "🎓 Chấm Bài Thi Toán - Gemini AI";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 10F);
        this.AllowDrop = true;

        // === Panel bên trái: Cấu hình ===
        panelLeft = new Panel();
        panelLeft.Dock = DockStyle.Left;
        panelLeft.Width = 450;
        panelLeft.Padding = new Padding(15);
        panelLeft.BackColor = Color.FromArgb(245, 245, 250);

        // Tiêu đề
        lblTitle = new Label();
        lblTitle.Text = "📝 CHẤM BÀI THI TOÁN";
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(50, 50, 120);
        lblTitle.Location = new Point(15, 10);
        lblTitle.AutoSize = true;

        // Tên bài thi
        lblExamTitle = new Label();
        lblExamTitle.Text = "Tên bài thi:";
        lblExamTitle.Location = new Point(15, 55);
        lblExamTitle.AutoSize = true;

        txtExamTitle = new TextBox();
        txtExamTitle.Location = new Point(15, 78);
        txtExamTitle.Size = new Size(410, 30);
        txtExamTitle.PlaceholderText = "VD: Kiểm tra 1 tiết Đại số lớp 10";

        // Đáp án
        lblAnswerKey = new Label();
        lblAnswerKey.Text = "📋 Đáp án (để trống nếu muốn AI tự chấm):";
        lblAnswerKey.Location = new Point(15, 115);
        lblAnswerKey.AutoSize = true;

        // Tab control cho đáp án: Nhập text hoặc File
        tabAnswerKey = new TabControl();
        tabAnswerKey.Location = new Point(15, 138);
        tabAnswerKey.Size = new Size(410, 195);

        tabAnswerText = new TabPage("✏️ Nhập text");
        txtAnswerKey = new RichTextBox();
        txtAnswerKey.Dock = DockStyle.Fill;
        txtAnswerKey.Font = new Font("Consolas", 9.5F);
        txtAnswerKey.BorderStyle = BorderStyle.None;
        txtAnswerKey.ScrollBars = RichTextBoxScrollBars.Vertical;
        tabAnswerText.Controls.Add(txtAnswerKey);

        tabAnswerFile = new TabPage("📁 File đáp án");
        panelAnswerDrop = new Panel();
        panelAnswerDrop.Dock = DockStyle.Fill;
        panelAnswerDrop.BackColor = Color.White;
        panelAnswerDrop.AllowDrop = true;

        lblAnswerFileHint = new Label();
        lblAnswerFileHint.Text = "🖱️ Kéo thả file đáp án vào đây\n(Ảnh, PDF, Word)";
        lblAnswerFileHint.TextAlign = ContentAlignment.MiddleCenter;
        lblAnswerFileHint.Dock = DockStyle.Fill;
        lblAnswerFileHint.Font = new Font("Segoe UI", 10F);
        lblAnswerFileHint.ForeColor = Color.Gray;

        btnAnswerFile = new Button();
        btnAnswerFile.Text = "📂 Chọn file đáp án";
        btnAnswerFile.Dock = DockStyle.Bottom;
        btnAnswerFile.Height = 32;
        btnAnswerFile.FlatStyle = FlatStyle.Flat;
        btnAnswerFile.BackColor = Color.FromArgb(240, 240, 250);

        btnClearAnswerFile = new Button();
        btnClearAnswerFile.Text = "❌ Xóa file đáp án";
        btnClearAnswerFile.Dock = DockStyle.Bottom;
        btnClearAnswerFile.Height = 28;
        btnClearAnswerFile.FlatStyle = FlatStyle.Flat;
        btnClearAnswerFile.ForeColor = Color.Gray;
        btnClearAnswerFile.Visible = false;

        panelAnswerDrop.Controls.Add(lblAnswerFileHint);
        panelAnswerDrop.Controls.Add(btnClearAnswerFile);
        panelAnswerDrop.Controls.Add(btnAnswerFile);
        tabAnswerFile.Controls.Add(panelAnswerDrop);

        tabAnswerKey.TabPages.Add(tabAnswerText);
        tabAnswerKey.TabPages.Add(tabAnswerFile);

        // Google Form URL
        lblFormUrl = new Label();
        lblFormUrl.Text = "🔗 Google Form URL (để trống nếu không cần):";
        lblFormUrl.Location = new Point(15, 340);
        lblFormUrl.AutoSize = true;

        txtFormUrl = new TextBox();
        txtFormUrl.Location = new Point(15, 365);
        txtFormUrl.Size = new Size(410, 30);
        txtFormUrl.PlaceholderText = "https://docs.google.com/forms/d/.../viewform";

        // Khu vực kéo thả file
        lblDragDrop = new Label();
        lblDragDrop.Text = "📁 Kéo thả bài thi học sinh vào đây:";
        lblDragDrop.Location = new Point(15, 400);
        lblDragDrop.AutoSize = true;

        panelDragDrop = new Panel();
        panelDragDrop.Location = new Point(15, 423);
        panelDragDrop.Size = new Size(410, 100);
        panelDragDrop.BackColor = Color.White;
        panelDragDrop.BorderStyle = BorderStyle.FixedSingle;
        panelDragDrop.AllowDrop = true;

        lblDragHint = new Label();
        lblDragHint.Text = "🖱️ Kéo thả file vào đây\n(Hỗ trợ: .docx, .pdf, .png, .jpg, .jpeg, .bmp)";
        lblDragHint.TextAlign = ContentAlignment.MiddleCenter;
        lblDragHint.Dock = DockStyle.Fill;
        lblDragHint.Font = new Font("Segoe UI", 11F);
        lblDragHint.ForeColor = Color.Gray;
        panelDragDrop.Controls.Add(lblDragHint);

        // List file đã chọn
        lstFiles = new ListBox();
        lstFiles.Location = new Point(15, 530);
        lstFiles.Size = new Size(340, 85);
        lstFiles.Font = new Font("Segoe UI", 9F);

        btnRemoveFile = new Button();
        btnRemoveFile.Text = "❌";
        btnRemoveFile.Location = new Point(360, 530);
        btnRemoveFile.Size = new Size(65, 27);
        btnRemoveFile.FlatStyle = FlatStyle.Flat;

        btnAddFiles = new Button();
        btnAddFiles.Text = "📂 Thêm";
        btnAddFiles.Location = new Point(360, 560);
        btnAddFiles.Size = new Size(65, 27);
        btnAddFiles.FlatStyle = FlatStyle.Flat;

        btnClearFiles = new Button();
        btnClearFiles.Text = "🗑️ Xóa";
        btnClearFiles.Location = new Point(360, 590);
        btnClearFiles.Size = new Size(65, 27);
        btnClearFiles.FlatStyle = FlatStyle.Flat;

        // Nút chấm bài
        btnGrade = new Button();
        btnGrade.Text = "🚀 BẮT ĐẦU CHẤM BÀI";
        btnGrade.Location = new Point(15, 625);
        btnGrade.Size = new Size(200, 45);
        btnGrade.BackColor = Color.FromArgb(66, 133, 244);
        btnGrade.ForeColor = Color.White;
        btnGrade.FlatStyle = FlatStyle.Flat;
        btnGrade.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnGrade.Cursor = Cursors.Hand;

        btnStop = new Button();
        btnStop.Text = "⏹️ DỪNG";
        btnStop.Location = new Point(225, 625);
        btnStop.Size = new Size(100, 45);
        btnStop.BackColor = Color.FromArgb(234, 67, 53);
        btnStop.ForeColor = Color.White;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnStop.Enabled = false;

        btnExportCsv = new Button();
        btnExportCsv.Text = "💾 CSV";
        btnExportCsv.Location = new Point(335, 625);
        btnExportCsv.Size = new Size(90, 45);
        btnExportCsv.FlatStyle = FlatStyle.Flat;
        btnExportCsv.Font = new Font("Segoe UI", 10F);

        btnSettings = new Button();
        btnSettings.Text = "⚙️";
        btnSettings.Location = new Point(390, 8);
        btnSettings.Size = new Size(40, 35);
        btnSettings.FlatStyle = FlatStyle.Flat;
        btnSettings.Font = new Font("Segoe UI", 14F);
        btnSettings.BackColor = Color.Transparent;
        btnSettings.FlatAppearance.BorderSize = 0;
        btnSettings.Cursor = Cursors.Hand;

        // Add controls to left panel
        panelLeft.Controls.AddRange(new Control[] {
            lblTitle, btnSettings, lblExamTitle, txtExamTitle,
            lblAnswerKey, tabAnswerKey,
            lblFormUrl, txtFormUrl,
            lblDragDrop, panelDragDrop,
            lstFiles, btnRemoveFile, btnAddFiles, btnClearFiles,
            btnGrade, btnStop, btnExportCsv
        });

        // === Panel bên phải: Kết quả ===
        panelRight = new Panel();
        panelRight.Dock = DockStyle.Fill;
        panelRight.Padding = new Padding(15);

        // Progress
        lblProgress = new Label();
        lblProgress.Text = "Trạng thái: Sẵn sàng";
        lblProgress.Location = new Point(15, 10);
        lblProgress.AutoSize = true;
        lblProgress.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        progressBar = new ProgressBar();
        progressBar.Location = new Point(15, 35);
        progressBar.Size = new Size(590, 25);
        progressBar.Style = ProgressBarStyle.Continuous;

        // Bảng kết quả
        lblResults = new Label();
        lblResults.Text = "📊 Kết quả chấm bài:";
        lblResults.Location = new Point(15, 70);
        lblResults.AutoSize = true;

        dgvResults = new DataGridView();
        dgvResults.Location = new Point(15, 95);
        dgvResults.Size = new Size(590, 280);
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvResults.ReadOnly = true;
        dgvResults.AllowUserToAddRows = false;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.BackgroundColor = Color.White;
        dgvResults.BorderStyle = BorderStyle.FixedSingle;

        // Cột cho DataGridView
        dgvResults.Columns.Add("colSTT", "STT");
        dgvResults.Columns.Add("colName", "Tên HS");
        dgvResults.Columns.Add("colScore", "Điểm");
        dgvResults.Columns.Add("colFeedback", "Nhận xét");
        dgvResults.Columns.Add("colTime", "Thời gian");
        dgvResults.Columns["colSTT"]!.Width = 40;
        dgvResults.Columns["colScore"]!.Width = 60;
        dgvResults.Columns["colTime"]!.Width = 130;

        // Log
        lblLog = new Label();
        lblLog.Text = "📜 Nhật ký:";
        lblLog.Location = new Point(15, 385);
        lblLog.AutoSize = true;

        txtLog = new RichTextBox();
        txtLog.Location = new Point(15, 410);
        txtLog.Size = new Size(590, 290);
        txtLog.ReadOnly = true;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.BackColor = Color.FromArgb(30, 30, 40);
        txtLog.ForeColor = Color.FromArgb(200, 255, 200);

        // Add controls to right panel
        panelRight.Controls.AddRange(new Control[] {
            lblProgress, progressBar,
            lblResults, dgvResults,
            lblLog, txtLog
        });

        // Add panels to form
        this.Controls.Add(panelRight);
        this.Controls.Add(panelLeft);
    }

    #endregion

    private Panel panelLeft;
    private Panel panelRight;
    private Label lblTitle;
    private Label lblExamTitle;
    private TextBox txtExamTitle;
    private Label lblAnswerKey;
    private RichTextBox txtAnswerKey;
    private TabControl tabAnswerKey;
    private TabPage tabAnswerText;
    private TabPage tabAnswerFile;
    private Panel panelAnswerDrop;
    private Label lblAnswerFileHint;
    private Button btnAnswerFile;
    private Button btnClearAnswerFile;
    private Label lblFormUrl;
    private TextBox txtFormUrl;
    private Label lblDragDrop;
    private Panel panelDragDrop;
    private Label lblDragHint;
    private ListBox lstFiles;
    private Button btnRemoveFile;
    private Button btnAddFiles;
    private Button btnClearFiles;
    private Button btnGrade;
    private Button btnStop;
    private Button btnExportCsv;
    private Button btnSettings;
    private Label lblProgress;
    private ProgressBar progressBar;
    private Label lblResults;
    private DataGridView dgvResults;
    private Label lblLog;
    private RichTextBox txtLog;
}
