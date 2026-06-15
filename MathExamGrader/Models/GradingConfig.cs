namespace MathExamGrader.Models;

public class GradingConfig
{
    public string AnswerKey { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string GeminiUrl { get; set; } = "https://gemini.google.com/app";
    public string GoogleFormUrl { get; set; } = string.Empty;
    public int DelayBetweenStudents { get; set; } = 5000; // ms
    public string ChromeUserDataDir { get; set; } = string.Empty;
}
