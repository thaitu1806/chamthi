using MathExamGrader.Models;
using Newtonsoft.Json;

namespace MathExamGrader;

public partial class FormSettings : Form
{
    private AppSettings _settings;
    private readonly string _settingsPath;

    public AppSettings Settings => _settings;

    public FormSettings(AppSettings settings)
    {
        _settings = settings;
        _settingsPath = GetSettingsFilePath();
        InitializeComponent();
        LoadSettingsToUI();
        SetupEvents();
    }

    private void SetupEvents()
    {
        btnSave.Click += BtnSave_Click;
        btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        btnBrowseFolder.Click += BtnBrowseFolder_Click;
    }

    private void LoadSettingsToUI()
    {
        // Gemini
        txtGeminiUrl.Text = _settings.GeminiConfig.GeminiUrl;
        txtGoogleAccount.Text = _settings.GeminiConfig.GoogleAccount;
        numDelay.Value = Math.Clamp(_settings.GeminiConfig.DelayBetweenStudents, 1000, 30000);
        numMaxWait.Value = Math.Clamp(_settings.GeminiConfig.MaxWaitForResponse, 30, 300);
        chkAutoLogin.Checked = _settings.GeminiConfig.AutoLogin;
        chkHideEdge.Checked = _settings.GeminiConfig.HideEdge;

        // Google Form
        chkFormEnabled.Checked = _settings.GoogleFormConfig.Enabled;
        txtFormUrl2.Text = _settings.GoogleFormConfig.FormUrl;
        txtFieldName.Text = _settings.GoogleFormConfig.FieldMapping.StudentName;
        txtFieldExam.Text = _settings.GoogleFormConfig.FieldMapping.ExamTitle;
        txtFieldScore.Text = _settings.GoogleFormConfig.FieldMapping.Score;
        txtFieldFeedback.Text = _settings.GoogleFormConfig.FieldMapping.Feedback;
        txtFieldDetail.Text = _settings.GoogleFormConfig.FieldMapping.DetailedResult;
        txtFieldDate.Text = _settings.GoogleFormConfig.FieldMapping.GradedDate;

        // Export
        chkAutoExport.Checked = _settings.ExportConfig.AutoExportCsv;
        txtCsvFolder.Text = _settings.ExportConfig.CsvOutputFolder;
        int encodingIdx = _settings.ExportConfig.CsvEncoding switch
        {
            "UTF-8" => 0,
            "UTF-8 BOM (Excel)" => 1,
            "Windows-1252" => 2,
            _ => 0
        };
        cboEncoding.SelectedIndex = encodingIdx;
    }

    private void SaveUIToSettings()
    {
        // Gemini
        _settings.GeminiConfig.GeminiUrl = txtGeminiUrl.Text.Trim();
        _settings.GeminiConfig.GoogleAccount = txtGoogleAccount.Text.Trim();
        _settings.GeminiConfig.DelayBetweenStudents = (int)numDelay.Value;
        _settings.GeminiConfig.MaxWaitForResponse = (int)numMaxWait.Value;
        _settings.GeminiConfig.AutoLogin = chkAutoLogin.Checked;
        _settings.GeminiConfig.HideEdge = chkHideEdge.Checked;

        // Google Form
        _settings.GoogleFormConfig.Enabled = chkFormEnabled.Checked;
        _settings.GoogleFormConfig.FormUrl = txtFormUrl2.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.StudentName = txtFieldName.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.ExamTitle = txtFieldExam.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.Score = txtFieldScore.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.Feedback = txtFieldFeedback.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.DetailedResult = txtFieldDetail.Text.Trim();
        _settings.GoogleFormConfig.FieldMapping.GradedDate = txtFieldDate.Text.Trim();

        // Export
        _settings.ExportConfig.AutoExportCsv = chkAutoExport.Checked;
        _settings.ExportConfig.CsvOutputFolder = txtCsvFolder.Text.Trim();
        _settings.ExportConfig.CsvEncoding = cboEncoding.SelectedItem?.ToString() ?? "UTF-8";
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        SaveUIToSettings();

        try
        {
            string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
            MessageBox.Show("Đã lưu cấu hình thành công!", "✅ Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi lưu cấu hình: {ex.Message}", "❌ Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBrowseFolder_Click(object? sender, EventArgs e)
    {
        using var fbd = new FolderBrowserDialog();
        fbd.Description = "Chọn thư mục lưu file CSV";
        if (fbd.ShowDialog() == DialogResult.OK)
        {
            txtCsvFolder.Text = fbd.SelectedPath;
        }
    }

    public static string GetSettingsFilePath()
    {
        string appDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MathExamGrader");
        Directory.CreateDirectory(appDir);
        return Path.Combine(appDir, "appsettings.json");
    }

    public static AppSettings LoadSettings()
    {
        string path = GetSettingsFilePath();

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        // Nếu chưa có file, thử đọc từ appsettings.json cùng thư mục app
        string localPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(localPath))
        {
            try
            {
                string json = File.ReadAllText(localPath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
                // Lưu vào AppData
                File.WriteAllText(path, JsonConvert.SerializeObject(settings, Formatting.Indented));
                return settings;
            }
            catch
            {
                return new AppSettings();
            }
        }

        return new AppSettings();
    }
}
