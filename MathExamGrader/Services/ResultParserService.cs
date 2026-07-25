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

        // Lấy CHI TIẾT + NHẬN XÉT CHUNG
        var sb = new System.Text.StringBuilder();

        // Lấy phần CHI TIẾT
        var detailMatch = Regex.Match(text,
            @"CHI TIẾT[:\s]*(.*?)(?=NHẬN XÉT CHUNG|NHẬN XÉT|$)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (detailMatch.Success)
        {
            var detail = detailMatch.Groups[1].Value.Trim();
            if (detail.Length > 0) sb.AppendLine(detail);
        }

        // Lấy phần NHẬN XÉT CHUNG
        var feedbackMatch = Regex.Match(text,
            @"NHẬN XÉT(?:\s*CHUNG)?[:\s]*(.*?)$",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (feedbackMatch.Success)
        {
            var feedback = feedbackMatch.Groups[1].Value.Trim();
            if (feedback.Length > 0)
            {
                if (sb.Length > 0) sb.AppendLine();
                sb.Append("NHẬN XÉT: " + feedback);
            }
        }

        string result = sb.Length > 0 ? sb.ToString() : text;

        // === LỌC SẠCH thông tin lộ AI / tên file / reference ===
        result = CleanAITraces(result);

        return result;
    }

    /// <summary>
    /// Loại bỏ những dấu vết AI và thông tin file khỏi nhận xét
    /// Để gửi cho học sinh mà không biết là AI chấm
    /// </summary>
    private static string CleanAITraces(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Bỏ "Gemini đã nói", "Gemini said"
        text = Regex.Replace(text, @"Gemini đã nói\.?\s*", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"Gemini said\.?\s*", "", RegexOptions.IgnoreCase);

        // Bỏ tên file (pattern: chuỗi dài không dấu cách + đuôi file)
        text = Regex.Replace(text, @"\b\S+\.(pdf|docx|doc|png|jpg|jpeg|bmp)\b", "", RegexOptions.IgnoreCase);

        // Bỏ tên file dạng hash dài (2aOboQcXFZNP7hif4nVk...)
        text = Regex.Replace(text, @"\b[a-zA-Z0-9]{20,}\b", "");

        // Bỏ "trong file ...", "từ file ...", "file đã upload"
        text = Regex.Replace(text, @"(?:trong|từ|của|với)\s*file\s*\S*", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"file đã upload\S*", "", RegexOptions.IgnoreCase);

        // Bỏ "barem điểm của ĐỀ_..."
        text = Regex.Replace(text, @"barem\s*điểm\s*(của\s*)?[^\.,\n]*", "barem điểm", RegexOptions.IgnoreCase);

        // Bỏ "khớp với barem điểm của ..."
        text = Regex.Replace(text, @"khớp với barem điểm\s*(của\s*)?[^\.,\n]*", "khớp với barem điểm", RegexOptions.IgnoreCase);

        // Bỏ markers "PDF", "PDF + 1", "PDF + 2" (reference từ Gemini)
        text = Regex.Replace(text, @"\s*PDF\s*(\+\s*\d+)?\s*", " ");

        // Bỏ "đáp án đã cho ở trên", "đáp án ở trên"
        text = Regex.Replace(text, @"đáp án\s*(đã cho\s*)?(ở trên|phía trên)", "đáp án", RegexOptions.IgnoreCase);

        // Bỏ double spaces và trim
        text = Regex.Replace(text, @"[ \t]{2,}", " ");
        text = Regex.Replace(text, @"\n\s*\n\s*\n", "\n\n");
        text = text.Trim();

        return text;
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
