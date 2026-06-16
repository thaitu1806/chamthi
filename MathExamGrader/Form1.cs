using MathExamGrader.Models;
using MathExamGrader.Services;
using System.Text;

namespace MathExamGrader;

public partial class Form1 : Form
{
    private readonly List<string> _examFiles = new();
    private readonly List<ExamResult> _results = new();
    private GeminiWebService? _geminiService;
    private readonly GoogleFormService _formService = new();
    private readonly ResultParserService _parserService = new();
    private CancellationTokenSource? _cts;
    private bool _isGrading;
    private AppSettings _settings;
    private string? _answerKeyFilePath; // File đáp án (ảnh/pdf/docx)

    private static readonly string[] SupportedExtensions =
        { ".docx", ".doc", ".pdf", ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    public Form1()
    {
        _settings = FormSettings.LoadSettings();
        InitializeComponent();
        SetupEventHandlers();
        SetupDragDrop();
        ApplySettingsToUI();
        Log("Ứng dụng đã sẵn sàng. Hãy thêm file bài thi và nhấn 'Bắt đầu chấm bài'.");
        Log($"Account: {(_settings.GeminiConfig.GoogleAccount != "" ? _settings.GeminiConfig.GoogleAccount : "(chưa cấu hình)")}");
        Log($"Google Form: {(_settings.GoogleFormConfig.Enabled ? "BẬT" : "TẮT")}");
        Log("Nhấn ⚙️ Cấu hình để thiết lập Gemini account và Google Form.");
    }

    private void ApplySettingsToUI()
    {
        // Settings được áp dụng khi chấm bài
    }

    private void SetupEventHandlers()
    {
        btnGrade.Click += BtnGrade_Click;
        btnStop.Click += BtnStop_Click;
        btnAddFiles.Click += BtnAddFiles_Click;
        btnRemoveFile.Click += BtnRemoveFile_Click;
        btnClearFiles.Click += BtnClearFiles_Click;
        btnExportCsv.Click += BtnExportCsv_Click;
        btnSettings.Click += BtnSettings_Click;
        btnAnswerFile.Click += BtnAnswerFile_Click;
        btnClearAnswerFile.Click += BtnClearAnswerFile_Click;
        btnOpenFolder.Click += BtnOpenFolder_Click;
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
    {
        using var settingsForm = new FormSettings(_settings);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            _settings = settingsForm.Settings;
            ApplySettingsToUI();
            Log("✅ Cấu hình đã được cập nhật.");
            Log($"   Account: {_settings.GeminiConfig.GoogleAccount}");
            Log($"   Google Form: {(_settings.GoogleFormConfig.Enabled ? "BẬT" : "TẮT")}");
            Log($"   Delay: {_settings.GeminiConfig.DelayBetweenStudents}ms");
        }
    }

    private void SetupDragDrop()
    {
        // === Kéo thả bài thi học sinh ===
        panelDragDrop.DragEnter += (s, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
                panelDragDrop.BackColor = Color.FromArgb(232, 240, 254);
            }
        };

        panelDragDrop.DragLeave += (s, e) =>
        {
            panelDragDrop.BackColor = Color.White;
        };

        panelDragDrop.DragDrop += (s, e) =>
        {
            panelDragDrop.BackColor = Color.White;
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                AddFiles(files);
            }
        };

        // === Kéo thả file đáp án ===
        panelAnswerDrop.DragEnter += (s, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
                panelAnswerDrop.BackColor = Color.FromArgb(232, 254, 240);
            }
        };

        panelAnswerDrop.DragLeave += (s, e) =>
        {
            panelAnswerDrop.BackColor = Color.White;
        };

        panelAnswerDrop.DragDrop += (s, e) =>
        {
            panelAnswerDrop.BackColor = Color.White;
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                SetAnswerKeyFile(files[0]);
            }
        };

        // Cho phép drag drop trên cả form
        this.DragEnter += (s, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        };

        this.DragDrop += (s, e) =>
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                AddFiles(files);
            }
        };
    }

    private void BtnAnswerFile_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Chọn file đáp án";
        ofd.Filter = "Tất cả file hỗ trợ|*.docx;*.doc;*.pdf;*.png;*.jpg;*.jpeg;*.bmp;*.gif|" +
                     "Ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.gif|" +
                     "PDF|*.pdf|" +
                     "Word|*.docx;*.doc";
        ofd.Multiselect = false;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            SetAnswerKeyFile(ofd.FileName);
        }
    }

    private void BtnClearAnswerFile_Click(object? sender, EventArgs e)
    {
        _answerKeyFilePath = null;
        lblAnswerFileHint.Text = "🖱️ Kéo thả file đáp án vào đây\n(Ảnh, PDF, Word)";
        lblAnswerFileHint.ForeColor = Color.Gray;
        btnClearAnswerFile.Visible = false;
        Log("Đã xóa file đáp án.");
    }

    private void SetAnswerKeyFile(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();
        if (!SupportedExtensions.Contains(ext))
        {
            MessageBox.Show("File không được hỗ trợ. Vui lòng chọn file ảnh, PDF hoặc Word.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _answerKeyFilePath = filePath;
        string fileName = Path.GetFileName(filePath);
        lblAnswerFileHint.Text = $"✅ {fileName}";
        lblAnswerFileHint.ForeColor = Color.FromArgb(0, 128, 0);
        btnClearAnswerFile.Visible = true;
        Log($"Đã chọn file đáp án: {fileName}");
    }

    private void AddFiles(string[] files)
    {
        int added = 0;
        foreach (var file in files)
        {
            if (Directory.Exists(file))
            {
                var dirFiles = Directory.GetFiles(file, "*.*", SearchOption.AllDirectories)
                    .Where(f => SupportedExtensions.Contains(Path.GetExtension(f).ToLower()));
                foreach (var f in dirFiles)
                {
                    if (!_examFiles.Contains(f))
                    {
                        _examFiles.Add(f);
                        added++;
                    }
                }
            }
            else
            {
                string ext = Path.GetExtension(file).ToLower();
                if (SupportedExtensions.Contains(ext) && !_examFiles.Contains(file))
                {
                    _examFiles.Add(file);
                    added++;
                }
            }
        }

        RefreshFileList();
        Log($"Đã thêm {added} file. Tổng: {_examFiles.Count} bài thi.");
    }

    private void RefreshFileList()
    {
        lstFiles.Items.Clear();
        for (int i = 0; i < _examFiles.Count; i++)
        {
            lstFiles.Items.Add($"{i + 1}. {Path.GetFileName(_examFiles[i])}");
        }
        lblDragHint.Text = _examFiles.Count > 0
            ? $"✅ Đã chọn {_examFiles.Count} file"
            : "🖱️ Kéo thả file vào đây\n(Hỗ trợ: .docx, .pdf, .png, .jpg, .jpeg, .bmp)";
    }

    private void BtnAddFiles_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Chọn bài thi học sinh";
        ofd.Filter = "Tất cả file hỗ trợ|*.docx;*.doc;*.pdf;*.png;*.jpg;*.jpeg;*.bmp;*.gif|" +
                     "Ảnh|*.png;*.jpg;*.jpeg;*.bmp;*.gif|" +
                     "PDF|*.pdf|" +
                     "Word|*.docx;*.doc";
        ofd.Multiselect = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            AddFiles(ofd.FileNames);
        }
    }

    private void BtnRemoveFile_Click(object? sender, EventArgs e)
    {
        if (lstFiles.SelectedIndex >= 0)
        {
            _examFiles.RemoveAt(lstFiles.SelectedIndex);
            RefreshFileList();
        }
    }

    private void BtnClearFiles_Click(object? sender, EventArgs e)
    {
        _examFiles.Clear();
        RefreshFileList();
        Log("Đã xóa tất cả file.");
    }

    private async void BtnGrade_Click(object? sender, EventArgs e)
    {
        if (_examFiles.Count == 0)
        {
            MessageBox.Show("Vui lòng thêm ít nhất 1 file bài thi!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_isGrading) return;

        _isGrading = true;
        _cts = new CancellationTokenSource();
        SetGradingUI(true);

        try
        {
            // Khởi tạo Gemini Service
            string userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MathExamGrader", "BrowserData");

            Directory.CreateDirectory(userDataDir);

            _geminiService = new GeminiWebService();
            _geminiService.OnLog += (msg) => Invoke(() => Log(msg));
            _geminiService.MaxWaitSeconds = _settings.GeminiConfig.MaxWaitForResponse;
            _formService.OnLog += (msg) => Invoke(() => Log(msg));

            Log("Đang khởi động browser...");
            await _geminiService.InitializeAsync(userDataDir);
            await _geminiService.NavigateToGemini();

            // Dump thông tin DOM để debug (hiển thị trong log)
            await _geminiService.DumpPageInfoAsync();

            // Chấm từng bài
            string answerKey = txtAnswerKey.Text.Trim();
            string examTitle = txtExamTitle.Text.Trim();
            if (string.IsNullOrEmpty(examTitle)) examTitle = "Bài thi Toán";

            // Xác định có file đáp án không
            bool hasAnswerFile = _answerKeyFilePath != null && File.Exists(_answerKeyFilePath);
            if (hasAnswerFile)
            {
                Log($"📋 Sử dụng file đáp án: {Path.GetFileName(_answerKeyFilePath!)}");
            }
            else if (!string.IsNullOrEmpty(answerKey))
            {
                Log($"📋 Sử dụng đáp án text ({answerKey.Length} ký tự)");
            }
            else
            {
                Log("📋 Không có đáp án → Gemini sẽ tự chấm");
            }

            progressBar.Maximum = _examFiles.Count;
            progressBar.Value = 0;

            int delay = _settings.GeminiConfig.DelayBetweenStudents;

            // === LOGIC MỚI: 1 CHAT DUY NHẤT ===
            // Bước 1: Gửi đáp án trước để Gemini ghi nhớ context
            if (hasAnswerFile || !string.IsNullOrEmpty(answerKey))
            {
                Log("📋 Bước 1: Gửi đáp án cho Gemini ghi nhớ...");
                await _geminiService.SendAnswerKeyAsync(_answerKeyFilePath, answerKey);
                Log("✅ Gemini đã nhận đáp án. Bắt đầu chấm từng bài...");
                await Task.Delay(3000);
            }

            // Bước 2: Chấm từng bài trong CÙNG CHAT (Gemini nhớ đáp án)
            for (int i = 0; i < _examFiles.Count; i++)
            {
                if (_cts.Token.IsCancellationRequested) break;

                string file = _examFiles[i];
                string fileName = Path.GetFileName(file);

                Invoke(() =>
                {
                    lblProgress.Text = $"Đang chấm: {fileName} ({i + 1}/{_examFiles.Count})";
                    progressBar.Value = i;
                });

                Log($"--- Chấm bài {i + 1}/{_examFiles.Count}: {fileName} ---");

                // Upload bài HS và yêu cầu chấm (Gemini đã có context đáp án)
                bool hasAnswer = hasAnswerFile || !string.IsNullOrEmpty(answerKey);
                string response = await _geminiService.GradeStudentExamAsync(file, hasAnswer, i + 1);

                // Parse kết quả
                var result = _parserService.ParseResult(response, fileName, examTitle);
                result.HasAnswerKey = hasAnswer;
                _results.Add(result);

                // Hiển thị kết quả
                Invoke(() =>
                {
                    dgvResults.Rows.Add(
                        (i + 1).ToString(),
                        result.StudentName,
                        result.Score.ToString("F1"),
                        result.Feedback.Length > 80 ? result.Feedback[..80] + "..." : result.Feedback,
                        result.GradedAt.ToString("dd/MM/yyyy HH:mm")
                    );
                });

                // Submit lên Google Form nếu bật
                if (_settings.GoogleFormConfig.Enabled)
                {
                    await _formService.SubmitResultAsync(_settings.GoogleFormConfig, result);
                }

                // Đợi giữa các bài
                if (i < _examFiles.Count - 1)
                {
                    Log($"Đợi {delay / 1000} giây trước khi chấm bài tiếp...");
                    await Task.Delay(delay, _cts.Token);
                }
            }

            Invoke(() =>
            {
                progressBar.Value = progressBar.Maximum;
                lblProgress.Text = $"✅ Hoàn thành! Đã chấm {_results.Count}/{_examFiles.Count} bài.";
            });

            Log($"=== HOÀN THÀNH ===");
            Log($"Tổng bài đã chấm: {_results.Count}");
            if (_results.Count > 0)
            {
                Log($"Điểm trung bình: {_results.Average(r => r.Score):F1}/10");
                Log($"Điểm cao nhất: {_results.Max(r => r.Score):F1}/10");
                Log($"Điểm thấp nhất: {_results.Min(r => r.Score):F1}/10");
            }

            // Auto export CSV nếu bật
            if (_settings.ExportConfig.AutoExportCsv && _results.Count > 0)
            {
                string folder = string.IsNullOrEmpty(_settings.ExportConfig.CsvOutputFolder)
                    ? AppContext.BaseDirectory
                    : _settings.ExportConfig.CsvOutputFolder;
                string csvPath = Path.Combine(folder, $"KetQua_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                ExportToCsv(csvPath);
                Log($"📁 Đã tự động xuất CSV: {csvPath}");
            }
        }
        catch (OperationCanceledException)
        {
            Log("⏹️ Đã dừng chấm bài theo yêu cầu.");
        }
        catch (Exception ex)
        {
            Log($"❌ Lỗi: {ex.Message}");
            MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _geminiService?.Dispose();
            _geminiService = null;
            _isGrading = false;
            SetGradingUI(false);
        }
    }

    private void BtnStop_Click(object? sender, EventArgs e)
    {
        _cts?.Cancel();
        Log("Đang dừng...");
    }

    private void BtnExportCsv_Click(object? sender, EventArgs e)
    {
        if (_results.Count == 0)
        {
            MessageBox.Show("Chưa có kết quả để xuất!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var sfd = new SaveFileDialog();
        sfd.Filter = "CSV file|*.csv";
        sfd.FileName = $"KetQua_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            ExportToCsv(sfd.FileName);
            Log($"Đã xuất kết quả ra: {sfd.FileName}");
            MessageBox.Show("Xuất file thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void BtnOpenFolder_Click(object? sender, EventArgs e)
    {
        string folder = string.IsNullOrEmpty(_settings.ExportConfig.CsvOutputFolder)
            ? AppContext.BaseDirectory
            : _settings.ExportConfig.CsvOutputFolder;

        if (Directory.Exists(folder))
        {
            System.Diagnostics.Process.Start("explorer.exe", folder);
        }
        else
        {
            System.Diagnostics.Process.Start("explorer.exe", AppContext.BaseDirectory);
        }
    }

    private void ExportToCsv(string filePath)
    {
        // Mặc định dùng UTF-8 BOM để Excel đọc đúng tiếng Việt
        var encoding = _settings.ExportConfig.CsvEncoding switch
        {
            "UTF-8" => new UTF8Encoding(false),
            "Windows-1252" => Encoding.GetEncoding(1252),
            _ => new UTF8Encoding(true) // UTF-8 BOM (Excel) - mặc định
        };

        var sb = new StringBuilder();
        sb.AppendLine("STT,Tên học sinh,File,Điểm,Nhận xét,Ngày chấm,Bài thi");

        for (int i = 0; i < _results.Count; i++)
        {
            var r = _results[i];
            string feedback = r.Feedback.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
            string detail = r.DetailedResult.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
            // Giới hạn detail để CSV không quá nặng
            if (detail.Length > 1000) detail = detail[..1000] + "...";
            sb.AppendLine($"{i + 1},\"{r.StudentName}\",\"{r.FileName}\",{r.Score:F1},\"{feedback}\",\"{r.GradedAt:dd/MM/yyyy HH:mm}\",\"{r.ExamTitle}\"");
        }

        File.WriteAllText(filePath, sb.ToString(), encoding);
    }

    private void SetGradingUI(bool isGrading)
    {
        Invoke(() =>
        {
            btnGrade.Enabled = !isGrading;
            btnStop.Enabled = isGrading;
            btnAddFiles.Enabled = !isGrading;
            btnClearFiles.Enabled = !isGrading;
            btnSettings.Enabled = !isGrading;
            panelDragDrop.Enabled = !isGrading;
            btnOpenFolder.Enabled = !isGrading;
        });
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }

        string logLine = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        txtLog.AppendText(logLine);
        txtLog.ScrollToCaret();
    }
}
