using MathExamGrader.Models;
using System.Text.RegularExpressions;

namespace MathExamGrader.Services;

public class ResultParserService
{
    /// <summary>
    /// Parse kết quả từ text Gemini trả về thành ExamResult
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

        // Trích xuất điểm từ response
        // Tìm pattern: ĐIỂM: X/10 hoặc Điểm: X/10
        var scoreMatch = Regex.Match(geminiResponse,
            @"ĐIỂM[:\s]*(\d+[.,]?\d*)\s*/\s*10",
            RegexOptions.IgnoreCase);

        if (scoreMatch.Success)
        {
            string scoreStr = scoreMatch.Groups[1].Value.Replace(",", ".");
            if (double.TryParse(scoreStr, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double score))
            {
                result.Score = score;
            }
        }
        else
        {
            // Thử pattern khác: điểm số đứng một mình
            var altMatch = Regex.Match(geminiResponse, @"(\d+[.,]?\d*)\s*/\s*10");
            if (altMatch.Success)
            {
                string scoreStr = altMatch.Groups[1].Value.Replace(",", ".");
                if (double.TryParse(scoreStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double score))
                {
                    result.Score = score;
                }
            }
        }

        // Trích xuất nhận xét chung
        var feedbackMatch = Regex.Match(geminiResponse,
            @"NHẬN XÉT CHUNG[:\s]*(.*?)(?:\n\n|\z)",
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (feedbackMatch.Success)
        {
            result.Feedback = feedbackMatch.Groups[1].Value.Trim();
        }
        else
        {
            // Lấy 200 ký tự cuối làm nhận xét
            result.Feedback = geminiResponse.Length > 200
                ? geminiResponse[^200..]
                : geminiResponse;
        }

        // Tên học sinh từ tên file (bỏ extension)
        result.StudentName = Path.GetFileNameWithoutExtension(fileName);

        return result;
    }
}
