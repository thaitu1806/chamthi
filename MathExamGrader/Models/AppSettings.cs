namespace MathExamGrader.Models;

public class AppSettings
{
    public GeminiConfig GeminiConfig { get; set; } = new();
    public GoogleFormConfig GoogleFormConfig { get; set; } = new();
    public ExportConfig ExportConfig { get; set; } = new();
}

public class GeminiConfig
{
    public string GeminiUrl { get; set; } = "https://gemini.google.com/app";
    public string GoogleAccount { get; set; } = string.Empty;
    public int DelayBetweenStudents { get; set; } = 5000;
    public int MaxWaitForResponse { get; set; } = 120;
    public bool AutoLogin { get; set; } = true;
}

public class GoogleFormConfig
{
    public string FormUrl { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
    public FormFieldMapping FieldMapping { get; set; } = new();
}

public class FormFieldMapping
{
    public string StudentName { get; set; } = "entry.123456789";
    public string ExamTitle { get; set; } = "entry.234567890";
    public string Score { get; set; } = "entry.345678901";
    public string Feedback { get; set; } = "entry.456789012";
    public string DetailedResult { get; set; } = "entry.567890123";
    public string GradedDate { get; set; } = "entry.678901234";
}

public class ExportConfig
{
    public bool AutoExportCsv { get; set; } = true;
    public string CsvOutputFolder { get; set; } = string.Empty;
    public string CsvEncoding { get; set; } = "UTF-8";
}
