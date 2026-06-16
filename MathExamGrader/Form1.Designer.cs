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
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.ClientSize = new System.Drawing.Size(1280, 780);
        this.Text = "🎓 Chấm Bài Thi Toán - Gemini AI";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9.5F);
        this.AllowDrop = true;
        this.MinimumSize = new Size(1100, 700);
        this.WindowState = FormWindowState.Maximized;
        this.BackColor = Color.FromArgb(248, 249, 252);

        // === SPLIT CONTAINER (responsive layout) ===
        splitMain = new SplitContainer();
        splitMain.Dock = DockStyle.Fill;
        splitMain.SplitterDistance = 420;
        splitMain.SplitterWidth = 3;
        splitMain.BackColor = Color.FromArgb(220, 225, 235);
        splitMain.Panel1MinSize = 350;
        splitMain.Panel2MinSize = 500;

        // === PANEL TRÁI ===
        panelLeft = new Panel();
        panelLeft.Dock = DockStyle.Fill;
        panelLeft.Padding = new Padding(16, 12, 16, 12);
        panelLeft.BackColor = Color.White;
        panelLeft.AutoScroll = true;

        // Header
        panelHeader = new Panel();
        panelHeader.Dock = DockStyle.Top;
        panelHeader.Height = 50;
        panelHeader.BackColor = Color.FromArgb(55, 65, 110);
        panelHeader.Padding = new Padding(16, 0, 16, 0);

        lblTitle = new Label();
        lblTitle.Text = "📝 CHẤM BÀI THI TOÁN";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Dock = DockStyle.Left;
        lblTitle.TextAlign = ContentAlignment.MiddleLeft;
        lblTitle.AutoSize = true;

        btnSettings = new Button();
        btnSettings.Text = "⚙️";
        btnSettings.Dock = DockStyle.Right;
        btnSettings.Width = 45;
        btnSettings.FlatStyle = FlatStyle.Flat;
        btnSettings.FlatAppearance.BorderSize = 0;
        btnSettings.Font = new Font("Segoe UI", 14F);
        btnSettings.ForeColor = Color.White;
        btnSettings.BackColor = Color.Transparent;
        btnSettings.Cursor = Cursors.Hand;

        panelHeader.Controls.Add(lblTitle);
        panelHeader.Controls.Add(btnSettings);

        // --- Tên bài thi ---
        lblExamTitle = new Label();
        lblExamTitle.Text = "Tên bài thi:";
        lblExamTitle.Dock = DockStyle.Top;
        lblExamTitle.Height = 22;
        lblExamTitle.Padding = new Padding(0, 6, 0, 0);

        txtExamTitle = new TextBox();
        txtExamTitle.Dock = DockStyle.Top;
        txtExamTitle.Height = 30;
        txtExamTitle.PlaceholderText = "VD: Kiểm tra 1 tiết Đại số lớp 10";

        // --- Đáp án ---
        lblAnswerKey = new Label();
        lblAnswerKey.Text = "📋 Đáp án (để trống = AI tự chấm):";
        lblAnswerKey.Dock = DockStyle.Top;
        lblAnswerKey.Height = 28;
        lblAnswerKey.Padding = new Padding(0, 8, 0, 0);

        tabAnswerKey = new TabControl();
        tabAnswerKey.Dock = DockStyle.Top;
        tabAnswerKey.Height = 160;

        tabAnswerText = new TabPage("✏️ Nhập text");
        txtAnswerKey = new RichTextBox();
        txtAnswerKey.Dock = DockStyle.Fill;
        txtAnswerKey.Font = new Font("Consolas", 9F);
        txtAnswerKey.BorderStyle = BorderStyle.None;
        tabAnswerText.Controls.Add(txtAnswerKey);

        tabAnswerFile = new TabPage("📁 File đáp án");
        panelAnswerDrop = new Panel();
        panelAnswerDrop.Dock = DockStyle.Fill;
        panelAnswerDrop.BackColor = Color.FromArgb(250, 252, 255);
        panelAnswerDrop.AllowDrop = true;

        lblAnswerFileHint = new Label();
        lblAnswerFileHint.Text = "🖱️ Kéo thả file đáp án vào đây\n(Ảnh, PDF, Word)";
        lblAnswerFileHint.TextAlign = ContentAlignment.MiddleCenter;
        lblAnswerFileHint.Dock = DockStyle.Fill;
        lblAnswerFileHint.Font = new Font("Segoe UI", 9.5F);
        lblAnswerFileHint.ForeColor = Color.Gray;

        btnAnswerFile = new Button();
        btnAnswerFile.Text = "📂 Chọn file đáp án";
        btnAnswerFile.Dock = DockStyle.Bottom;
        btnAnswerFile.Height = 30;
        btnAnswerFile.FlatStyle = FlatStyle.Flat;
        btnAnswerFile.BackColor = Color.FromArgb(240, 242, 248);

        btnClearAnswerFile = new Button();
        btnClearAnswerFile.Text = "❌ Xóa file";
        btnClearAnswerFile.Dock = DockStyle.Bottom;
        btnClearAnswerFile.Height = 26;
        btnClearAnswerFile.FlatStyle = FlatStyle.Flat;
        btnClearAnswerFile.ForeColor = Color.Gray;
        btnClearAnswerFile.Visible = false;

        panelAnswerDrop.Controls.Add(lblAnswerFileHint);
        panelAnswerDrop.Controls.Add(btnClearAnswerFile);
        panelAnswerDrop.Controls.Add(btnAnswerFile);
        tabAnswerFile.Controls.Add(panelAnswerDrop);

        tabAnswerKey.TabPages.Add(tabAnswerText);
        tabAnswerKey.TabPages.Add(tabAnswerFile);

        // --- Bài thi học sinh ---
        lblDragDrop = new Label();
        lblDragDrop.Text = "📁 Bài thi học sinh:";
        lblDragDrop.Dock = DockStyle.Top;
        lblDragDrop.Height = 28;
        lblDragDrop.Padding = new Padding(0, 8, 0, 0);

        panelDragDrop = new Panel();
        panelDragDrop.Dock = DockStyle.Top;
        panelDragDrop.Height = 80;
        panelDragDrop.BackColor = Color.FromArgb(250, 252, 255);
        panelDragDrop.BorderStyle = BorderStyle.FixedSingle;
        panelDragDrop.AllowDrop = true;

        lblDragHint = new Label();
        lblDragHint.Text = "🖱️ Kéo thả file vào đây\n(.docx, .pdf, .png, .jpg)";
        lblDragHint.TextAlign = ContentAlignment.MiddleCenter;
        lblDragHint.Dock = DockStyle.Fill;
        lblDragHint.Font = new Font("Segoe UI", 9.5F);
        lblDragHint.ForeColor = Color.Gray;
        panelDragDrop.Controls.Add(lblDragHint);

        // File list + buttons
        panelFileList = new Panel();
        panelFileList.Dock = DockStyle.Top;
        panelFileList.Height = 95;

        lstFiles = new ListBox();
        lstFiles.Dock = DockStyle.Fill;
        lstFiles.Font = new Font("Segoe UI", 8.5F);
        lstFiles.BorderStyle = BorderStyle.FixedSingle;

        panelFileButtons = new FlowLayoutPanel();
        panelFileButtons.Dock = DockStyle.Right;
        panelFileButtons.Width = 72;
        panelFileButtons.FlowDirection = FlowDirection.TopDown;
        panelFileButtons.Padding = new Padding(4, 0, 0, 0);

        btnAddFiles = new Button();
        btnAddFiles.Text = "📂 Thêm";
        btnAddFiles.Size = new Size(66, 28);
        btnAddFiles.FlatStyle = FlatStyle.Flat;
        btnAddFiles.Font = new Font("Segoe UI", 8F);

        btnRemoveFile = new Button();
        btnRemoveFile.Text = "❌ Xóa";
        btnRemoveFile.Size = new Size(66, 28);
        btnRemoveFile.FlatStyle = FlatStyle.Flat;
        btnRemoveFile.Font = new Font("Segoe UI", 8F);

        btnClearFiles = new Button();
        btnClearFiles.Text = "🗑️ Tất cả";
        btnClearFiles.Size = new Size(66, 28);
        btnClearFiles.FlatStyle = FlatStyle.Flat;
        btnClearFiles.Font = new Font("Segoe UI", 8F);

        panelFileButtons.Controls.AddRange(new Control[] { btnAddFiles, btnRemoveFile, btnClearFiles });
        panelFileList.Controls.Add(lstFiles);
        panelFileList.Controls.Add(panelFileButtons);

        // --- Nút hành động ---
        panelActions = new FlowLayoutPanel();
        panelActions.Dock = DockStyle.Top;
        panelActions.Height = 55;
        panelActions.Padding = new Padding(0, 8, 0, 0);
        panelActions.FlowDirection = FlowDirection.LeftToRight;

        btnGrade = new Button();
        btnGrade.Text = "🚀 CHẤM BÀI";
        btnGrade.Size = new Size(130, 42);
        btnGrade.BackColor = Color.FromArgb(66, 133, 244);
        btnGrade.ForeColor = Color.White;
        btnGrade.FlatStyle = FlatStyle.Flat;
        btnGrade.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnGrade.Cursor = Cursors.Hand;

        btnStop = new Button();
        btnStop.Text = "⏹️ Dừng";
        btnStop.Size = new Size(80, 42);
        btnStop.BackColor = Color.FromArgb(234, 67, 53);
        btnStop.ForeColor = Color.White;
        btnStop.FlatStyle = FlatStyle.Flat;
        btnStop.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnStop.Enabled = false;

        btnExportCsv = new Button();
        btnExportCsv.Text = "💾 CSV";
        btnExportCsv.Size = new Size(70, 42);
        btnExportCsv.FlatStyle = FlatStyle.Flat;
        btnExportCsv.Font = new Font("Segoe UI", 9F);

        btnOpenFolder = new Button();
        btnOpenFolder.Text = "📂 Mở TM";
        btnOpenFolder.Size = new Size(80, 42);
        btnOpenFolder.FlatStyle = FlatStyle.Flat;
        btnOpenFolder.Font = new Font("Segoe UI", 9F);

        panelActions.Controls.AddRange(new Control[] { btnGrade, btnStop, btnExportCsv, btnOpenFolder });

        // Add left panel controls (thứ tự ngược vì Dock Top)
        panelLeft.Controls.Add(panelActions);
        panelLeft.Controls.Add(panelFileList);
        panelLeft.Controls.Add(panelDragDrop);
        panelLeft.Controls.Add(lblDragDrop);
        panelLeft.Controls.Add(tabAnswerKey);
        panelLeft.Controls.Add(lblAnswerKey);
        panelLeft.Controls.Add(txtExamTitle);
        panelLeft.Controls.Add(lblExamTitle);

        // === PANEL PHẢI ===
        panelRight = new Panel();
        panelRight.Dock = DockStyle.Fill;
        panelRight.Padding = new Padding(16, 12, 16, 12);
        panelRight.BackColor = Color.FromArgb(248, 249, 252);

        // Progress
        lblProgress = new Label();
        lblProgress.Text = "Trạng thái: Sẵn sàng";
        lblProgress.Dock = DockStyle.Top;
        lblProgress.Height = 24;
        lblProgress.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblProgress.ForeColor = Color.FromArgb(55, 65, 110);

        progressBar = new ProgressBar();
        progressBar.Dock = DockStyle.Top;
        progressBar.Height = 22;
        progressBar.Style = ProgressBarStyle.Continuous;

        // Separator
        var sep1 = new Panel { Dock = DockStyle.Top, Height = 8 };

        // Bảng kết quả
        lblResults = new Label();
        lblResults.Text = "📊 Kết quả chấm bài:";
        lblResults.Dock = DockStyle.Top;
        lblResults.Height = 24;
        lblResults.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

        dgvResults = new DataGridView();
        dgvResults.Dock = DockStyle.Top;
        dgvResults.Height = 220;
        dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvResults.ReadOnly = true;
        dgvResults.AllowUserToAddRows = false;
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResults.BackgroundColor = Color.White;
        dgvResults.BorderStyle = BorderStyle.FixedSingle;
        dgvResults.RowHeadersWidth = 30;
        dgvResults.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
        dgvResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

        dgvResults.Columns.Add("colSTT", "STT");
        dgvResults.Columns.Add("colName", "Họ tên HS");
        dgvResults.Columns.Add("colScore", "Điểm");
        dgvResults.Columns.Add("colFeedback", "Nhận xét");
        dgvResults.Columns.Add("colTime", "Thời gian");
        dgvResults.Columns["colSTT"]!.Width = 35;
        dgvResults.Columns["colSTT"]!.FillWeight = 8;
        dgvResults.Columns["colName"]!.FillWeight = 25;
        dgvResults.Columns["colScore"]!.Width = 50;
        dgvResults.Columns["colScore"]!.FillWeight = 10;
        dgvResults.Columns["colFeedback"]!.FillWeight = 40;
        dgvResults.Columns["colTime"]!.FillWeight = 17;

        // Separator
        var sep2 = new Panel { Dock = DockStyle.Top, Height = 8 };

        // Log
        lblLog = new Label();
        lblLog.Text = "📜 Nhật ký:";
        lblLog.Dock = DockStyle.Top;
        lblLog.Height = 24;
        lblLog.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);

        txtLog = new RichTextBox();
        txtLog.Dock = DockStyle.Fill;
        txtLog.ReadOnly = true;
        txtLog.Font = new Font("Cascadia Code, Consolas", 8.5F);
        txtLog.BackColor = Color.FromArgb(25, 28, 38);
        txtLog.ForeColor = Color.FromArgb(180, 230, 180);
        txtLog.BorderStyle = BorderStyle.None;

        // Add right panel controls (ngược thứ tự)
        panelRight.Controls.Add(txtLog);
        panelRight.Controls.Add(lblLog);
        panelRight.Controls.Add(sep2);
        panelRight.Controls.Add(dgvResults);
        panelRight.Controls.Add(lblResults);
        panelRight.Controls.Add(sep1);
        panelRight.Controls.Add(progressBar);
        panelRight.Controls.Add(lblProgress);

        // === Assemble ===
        splitMain.Panel1.Controls.Add(panelLeft);
        splitMain.Panel1.Controls.Add(panelHeader);
        splitMain.Panel2.Controls.Add(panelRight);

        this.Controls.Add(splitMain);
    }

    #endregion

    private SplitContainer splitMain;
    private Panel panelLeft;
    private Panel panelRight;
    private Panel panelHeader;
    private Label lblTitle;
    private Button btnSettings;
    private Label lblExamTitle;
    private TextBox txtExamTitle;
    private Label lblAnswerKey;
    private TabControl tabAnswerKey;
    private TabPage tabAnswerText;
    private TabPage tabAnswerFile;
    private RichTextBox txtAnswerKey;
    private Panel panelAnswerDrop;
    private Label lblAnswerFileHint;
    private Button btnAnswerFile;
    private Button btnClearAnswerFile;
    private Label lblDragDrop;
    private Panel panelDragDrop;
    private Label lblDragHint;
    private Panel panelFileList;
    private ListBox lstFiles;
    private FlowLayoutPanel panelFileButtons;
    private Button btnAddFiles;
    private Button btnRemoveFile;
    private Button btnClearFiles;
    private FlowLayoutPanel panelActions;
    private Button btnGrade;
    private Button btnStop;
    private Button btnExportCsv;
    private Button btnOpenFolder;
    private Label lblProgress;
    private ProgressBar progressBar;
    private Label lblResults;
    private DataGridView dgvResults;
    private Label lblLog;
    private RichTextBox txtLog;
}
