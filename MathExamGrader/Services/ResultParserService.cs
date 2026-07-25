using MathExamGrader.Models;
using System.Text.RegularExpressions;

namespace MathExamGrader.Services;

public class ResultParserService
{
    /// <summary>
    /// Parse kết quả từ text Gemini trả về thành ExamResult
    /// Hỗ trợ nhiều format khác nhau mà Gemini có thể trả về
    /// </summary>
    public ExamResult ParseResult(string geminiResponse, string fileName, string examTitle)
    {
        var result = new ExamResult
        {
            FileName = fileName,
            ExamTitle = examTitle,
            GradedAt = DateTime.Now,
            DetailedResult = geminiResponse
        };

        // Trích xuất điểm
        result.Score = ExtractScore(geminiResponse);

        // Trích xuất nhận xét
        result.Feedback = ExtractFeedback(geminiResponse);

        // Trích xuất họ tên HS (nếu Gemini tìm thấy trên bài)
        string detectedName = ExtractStudentName(geminiResponse);
        if (!string.IsNullOrEmpty(detectedName) && detectedName.ToLower() != "không rõ")
        {
            result.StudentName = detectedName;
        }
        else
        {
            // Fallback: dùng tên file
            result.StudentName = Path.GetFileNameWithoutExtension(fileName);
        }

        return result;
    }

    private string ExtractStudentName(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Pattern: "HỌ TÊN: Nguyễn Văn A"
        var match = Regex.Match(text, @"HỌ TÊN[:\s]*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            string name = match.Groups[1].Value.Trim().Trim('"', '\'', '.', ',');
            if (name.Length > 1 && name.Length < 60 && name.ToLower() != "không rõ" && name != "[]")
                return name;
        }

        // Pattern: "Họ và tên: ..." 
        match = Regex.Match(text, @"[Hh]ọ\s*(và|&)?\s*[Tt]ên[:\s]*(.+?)(?:\n|$)");
        if (match.Success)
        {
            string name = match.Groups[2].Value.Trim().Trim('"', '\'', '.', ',');
            if (name.Length > 1 && name.Length < 60 && name.ToLower() != "không rõ")
                return name;
        }

        return string.Empty;
    }

    private double ExtractScore(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;

        // Pattern 1: "ĐIỂM: 8/10" hoặc "ĐIỂM: 8.5/10" hoặc "ĐIỂM: 8,5/10"
        var match = Regex.Match(text, @"ĐIỂM[:\s]*(\d+[.,]?\d*)\s*/\s*10", RegexOptions.IgnoreCase);
        if (match.Success) return ParseDouble(match.Groups[1].Value);

        // Pattern 2: "Điểm: 8/10" (không in hoa)
        match = Regex.Match(text, @"[Đđ]iểm[:\s]*(\d+[.,]?\d*)\s*/\s*10", RegexOptions.IgnoreCase);
        if (match.Success) return ParseDouble(match.Groups[1].Value);

        // Pattern 3: "Điểm: 1,0/1,0 điểm" → quy đổi về thang 10
        match = Regex.Match(text, @"[Đđ]iểm[:\s]*(\d+[.,]?\d*)\s*/\s*(\d+[.,]?\d*)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            double score = ParseDouble(match.Groups[1].Value);
            double total = ParseDouble(match.Groups[2].Value);
            if (total > 0 && total != 10)
            {
                // Quy đổi về thang 10
                return Math.Round((score / total) * 10, 1);
            }
            return score;
        }

        // Pattern 4: "Tổng điểm: 8.5/10" hoặc "Tổng: 8.5/10"
        match = Regex.Match(text, @"[Tt]ổng[^:]*[:\s]*(\d+[.,]?\d*)\s*/\s*(\d+[.,]?\d*)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            double score = ParseDouble(match.Groups[1].Value);
            double total = ParseDouble(match.Groups[2].Value);
            if (total > 0 && total != 10) return Math.Round((score / total) * 10, 1);
            return score;
        }

        // Pattern 5: "8.5 điểm" hoặc "8,5 điểm" (đứng một mình)
        match = Regex.Match(text, @"(\d+[.,]?\d*)\s*điểm", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            double score = ParseDouble(match.Groups[1].Value);
            if (score <= 10) return score;
        }

        // Pattern 6: "X/10" bất kỳ đâu
        match = Regex.Match(text, @"(\d+[.,]?\d*)\s*/\s*10");
        if (match.Success) return ParseDouble(match.Groups[1].Value);

        // Pattern 7: "Score: X" hoặc "Point: X"
        match = Regex.Match(text, @"(?:Score|Point|Mark)[:\s]*(\d+[.,]?\d*)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            double score = ParseDouble(match.Groups[1].Value);
            if (score <= 10) return score;
        }

        return 0;
    }

    private string ExtractFeedback(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        // Chỉ lấy NHẬN XÉT CHUNG (bỏ CHI TIẾT từng câu)
        var feedbackMatch = Regex.Match(text,
            @"NHẬN XÉT CHUNG[:\s]*(.*?)$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (feedbackMatch.Success)
        {
            var feedback = feedbackMatch.Groups[1].Value.Trim();
            // Bỏ text thừa phía sau (nếu có text khác sau nhận xét)
            var lines = feedback.Split('\n');
            var cleanLines = lines.TakeWhile(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (cleanLines.Length > 0)
                return string.Join(" ", cleanLines).Trim();
        }

        // Fallback: tìm "Nhận xét:" 
        var match = Regex.Match(text, @"[Nn]hận xét[:\s]*(.*?)(?:\n\n|$)", RegexOptions.Singleline);
        if (match.Success)
        {
            var feedback = match.Groups[1].Value.Trim();
            if (feedback.Length > 5) return feedback;
        }

        return string.Empty;
    }

    private static double ParseDouble(string value)
    {
        string cleaned = value.Replace(",", ".");
        if (double.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        return 0;
    }

    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
