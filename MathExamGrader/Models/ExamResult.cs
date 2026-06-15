namespace MathExamGrader.Models;

public class ExamResult
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string DetailedResult { get; set; } = string.Empty;
    public DateTime GradedAt { get; set; } = DateTime.Now;
    public string ExamTitle { get; set; } = string.Empty;
    public bool HasAnswerKey { get; set; }
}
