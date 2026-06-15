using MathExamGrader.Models;

namespace MathExamGrader.Services;

public class GoogleFormService
{
    public event Action<string>? OnLog;

    /// <summary>
    /// Submit kết quả lên Google Form qua HTTP POST
    /// </summary>
    public async Task<bool> SubmitResultAsync(GoogleFormConfig formConfig, ExamResult result)
    {
        if (!formConfig.Enabled || string.IsNullOrWhiteSpace(formConfig.FormUrl))
        {
            return false;
        }

        try
        {
            // Chuyển Google Form URL sang dạng formResponse
            string postUrl = formConfig.FormUrl
                .Replace("/viewform", "/formResponse")
                .Replace("/edit", "/formResponse");

            if (!postUrl.Contains("/formResponse"))
            {
                postUrl = postUrl.TrimEnd('/') + "/formResponse";
            }

            // Dùng field mapping từ config
            var mapping = formConfig.FieldMapping;
            var formData = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(mapping.StudentName))
                formData[mapping.StudentName] = result.StudentName;

            if (!string.IsNullOrEmpty(mapping.ExamTitle))
                formData[mapping.ExamTitle] = result.ExamTitle;

            if (!string.IsNullOrEmpty(mapping.Score))
                formData[mapping.Score] = result.Score.ToString("F1");

            if (!string.IsNullOrEmpty(mapping.Feedback))
                formData[mapping.Feedback] = result.Feedback;

            if (!string.IsNullOrEmpty(mapping.DetailedResult))
                formData[mapping.DetailedResult] = TruncateText(result.DetailedResult, 5000);

            if (!string.IsNullOrEmpty(mapping.GradedDate))
                formData[mapping.GradedDate] = result.GradedAt.ToString("dd/MM/yyyy HH:mm");

            using var httpClient = new HttpClient();
            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync(postUrl, content);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                Log($"✓ Đã submit kết quả của {result.StudentName} lên Google Form");
                return true;
            }
            else
            {
                Log($"✗ Lỗi submit form: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Log($"✗ Lỗi: {ex.Message}");
            return false;
        }
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }

    private void Log(string message)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }
}
