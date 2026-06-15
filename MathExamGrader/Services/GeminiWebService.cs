using Microsoft.Playwright;
using MathExamGrader.Models;
using System.Diagnostics;

namespace MathExamGrader.Services;

public class GeminiWebService : IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private int _maxWaitSeconds = 120;
    private Process? _edgeProcess;

    public event Action<string>? OnLog;

    public int MaxWaitSeconds
    {
        get => _maxWaitSeconds;
        set => _maxWaitSeconds = value;
    }

    public async Task InitializeAsync(string userDataDir)
    {
        _playwright = await Playwright.CreateAsync();

        string edgePath = GetEdgePath();
        int debugPort = 9222;

        // Bước 1: Đóng tất cả Edge đang chạy
        Log("Đang đóng Edge hiện tại (nếu có)...");
        KillAllEdgeProcesses();
        await Task.Delay(2000);

        // Bước 2: Mở Edge mới với remote debugging + profile thật
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + @"\Microsoft\Edge\User Data";

        Log($"Đang mở Edge với profile: {userProfile}");

        var startInfo = new ProcessStartInfo
        {
            FileName = edgePath,
            Arguments = $"--remote-debugging-port={debugPort} " +
                        $"--user-data-dir=\"{userProfile}\" " +
                        $"--no-first-run " +
                        $"--no-default-browser-check " +
                        $"--disable-blink-features=AutomationControlled " +
                        $"--start-maximized",
            UseShellExecute = true
        };

        _edgeProcess = Process.Start(startInfo);
        Log("Đã mở Edge, đang đợi khởi động...");

        // Bước 3: Retry kết nối CDP
        _browser = await ConnectWithRetryAsync(debugPort, maxRetries: 10, delayMs: 2000);

        var contexts = _browser.Contexts;
        if (contexts.Count > 0)
        {
            var context = contexts[0];
            _page = await context.NewPageAsync();
        }
        else
        {
            var context = await _browser.NewContextAsync();
            _page = await context.NewPageAsync();
        }

        // Set default timeout
        _page.SetDefaultTimeout(60000);
        _page.SetDefaultNavigationTimeout(60000);

        Log("✅ Playwright đã kết nối vào Edge thành công!");
    }

    private async Task<IBrowser> ConnectWithRetryAsync(int port, int maxRetries, int delayMs)
    {
        for (int i = 1; i <= maxRetries; i++)
        {
            try
            {
                Log($"Đang kết nối CDP (lần {i}/{maxRetries})...");
                var browser = await _playwright!.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{port}");
                return browser;
            }
            catch (Exception ex)
            {
                if (i == maxRetries)
                {
                    throw new Exception(
                        $"Không thể kết nối vào Edge sau {maxRetries} lần thử.\n" +
                        $"Lỗi: {ex.Message}\n\n" +
                        $"Hãy đóng Edge thủ công rồi thử lại.", ex);
                }
                Log($"Chưa kết nối được, đợi {delayMs / 1000}s...");
                await Task.Delay(delayMs);
            }
        }
        throw new Exception("Unexpected");
    }

    private static void KillAllEdgeProcesses()
    {
        try
        {
            var procs = Process.GetProcessesByName("msedge");
            foreach (var p in procs) { try { p.Kill(); } catch { } }
        }
        catch { }
    }

    private static string GetEdgePath()
    {
        string[] paths = {
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Microsoft\Edge\Application\msedge.exe")
        };
        foreach (var p in paths) { if (File.Exists(p)) return p; }
        return "msedge.exe";
    }

    // ========== NAVIGATION ==========

    public async Task NavigateToGemini()
    {
        if (_page == null) throw new InvalidOperationException("Browser chưa được khởi tạo.");

        Log("Đang mở Gemini...");
        try
        {
            await _page.GotoAsync("https://gemini.google.com/app", new PageGotoOptions
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
        }
        catch (TimeoutException)
        {
            Log("Trang load chậm, thử lại...");
            await Task.Delay(3000);
            await _page.GotoAsync("https://gemini.google.com/app", new PageGotoOptions
            {
                Timeout = 60000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
        }

        await Task.Delay(5000);
        Log("✅ Đã mở Gemini thành công.");
    }

    // ========== GRADING ==========

    public async Task<string> GradeExamAsync(string filePath, string answerKey)
    {
        if (_page == null) throw new InvalidOperationException("Browser chưa được khởi tạo.");

        try
        {
            string prompt = BuildGradingPrompt(answerKey, filePath);

            // Upload file
            await UploadFileAsync(filePath);
            await Task.Delay(3000);

            // Nhập prompt
            await TypePromptAsync(prompt);
            await Task.Delay(1000);

            // Gửi
            await SubmitAndWaitAsync();

            // Lấy kết quả
            string result = await GetLatestResponseAsync();
            Log($"✅ Đã chấm xong: {Path.GetFileName(filePath)}");
            return result;
        }
        catch (Exception ex)
        {
            Log($"❌ Lỗi khi chấm {Path.GetFileName(filePath)}: {ex.Message}");
            return $"LỖI: {ex.Message}";
        }
    }

    public async Task<string> GradeExamWithAnswerFileAsync(string examFilePath, string answerFilePath, string additionalNotes)
    {
        if (_page == null) throw new InvalidOperationException("Browser chưa được khởi tạo.");

        try
        {
            // Upload đáp án
            await UploadFileAsync(answerFilePath);
            await Task.Delay(2000);
            Log($"📄 Đã upload đáp án: {Path.GetFileName(answerFilePath)}");

            // Upload bài thi
            await UploadFileAsync(examFilePath);
            await Task.Delay(2000);
            Log($"📄 Đã upload bài thi: {Path.GetFileName(examFilePath)}");

            // Prompt
            string prompt = BuildGradingPromptWithFile(answerFilePath, examFilePath, additionalNotes);
            await TypePromptAsync(prompt);
            await Task.Delay(1000);

            // Gửi
            await SubmitAndWaitAsync();

            string result = await GetLatestResponseAsync();
            Log($"✅ Đã chấm xong: {Path.GetFileName(examFilePath)}");
            return result;
        }
        catch (Exception ex)
        {
            Log($"❌ Lỗi khi chấm {Path.GetFileName(examFilePath)}: {ex.Message}");
            return $"LỖI: {ex.Message}";
        }
    }

    // ========== UPLOAD FILE ==========
    // Gemini dùng hidden <input type="file">, cần dùng FileChooser event

    private async Task UploadFileAsync(string filePath)
    {
        if (_page == null) return;

        // Cách 1: Dùng FileChooser event (chuẩn nhất cho hidden input)
        // Click nút "Add file" / "Upload" để trigger file dialog
        var addFileButton = await FindUploadButtonAsync();

        if (addFileButton != null)
        {
            // Lắng nghe FileChooser event khi click nút upload
            var fileChooser = await _page.RunAndWaitForFileChooserAsync(async () =>
            {
                await addFileButton.ClickAsync();
            });

            await fileChooser.SetFilesAsync(filePath);
            
            Log($"📎 Upload (FileChooser): {Path.GetFileName(filePath)}");
            await Task.Delay(3000);
            return;
        }

        // Cách 2: Fallback - tìm input[type=file] trực tiếp (kể cả hidden)
        var fileInput = _page.Locator("input[type='file']").First;
        if (await fileInput.CountAsync() > 0)
        {
            await fileInput.SetInputFilesAsync(filePath);
            Log($"📎 Upload (input): {Path.GetFileName(filePath)}");
            await Task.Delay(3000);
            return;
        }

        throw new Exception("Không tìm thấy nút upload file trên trang Gemini.");
    }

    private async Task<ILocator?> FindUploadButtonAsync()
    {
        if (_page == null) return null;

        // Danh sách selector cho nút upload trên Gemini (thử từng cái)
        string[] selectors = {
            // Nút "Add file" hoặc icon attachment (thường là nút + hoặc icon paperclip)
            "button[aria-label='Upload file']",
            "button[aria-label='Add file']",
            "button[aria-label='Tải tệp lên']",
            "button[aria-label='Thêm tệp']",
            "button[aria-label='Attach file']",
            "button[aria-label='Upload']",
            // Selector dựa trên tooltip
            "[data-tooltip='Upload file']",
            "[data-tooltip='Add file']",
            // Selector dựa trên mat-icon hoặc icon class
            "button:has(mat-icon:text('attach_file'))",
            "button:has(mat-icon:text('add'))",
            // Nút có icon upload trong input area
            ".input-area-container button[mattooltip]",
            ".chat-input button[aria-label*='file']",
            ".chat-input button[aria-label*='File']",
            // Gemini thường có nút "+" bên cạnh input
            "button[aria-label*='Add']",
            "button[aria-label*='Thêm']",
        };

        foreach (var selector in selectors)
        {
            try
            {
                var btn = _page.Locator(selector).First;
                if (await btn.CountAsync() > 0 && await btn.IsVisibleAsync())
                {
                    Log($"   Tìm thấy nút upload: {selector}");
                    return btn;
                }
            }
            catch { }
        }

        // Fallback: tìm tất cả button, check aria-label chứa từ khóa
        var allButtons = await _page.Locator("button").AllAsync();
        foreach (var btn in allButtons)
        {
            try
            {
                var label = await btn.GetAttributeAsync("aria-label") ?? "";
                var tooltip = await btn.GetAttributeAsync("data-tooltip") ?? "";
                var matTooltip = await btn.GetAttributeAsync("mattooltip") ?? "";
                var combined = $"{label} {tooltip} {matTooltip}".ToLower();

                if (combined.Contains("upload") || combined.Contains("file") ||
                    combined.Contains("attach") || combined.Contains("tệp") ||
                    combined.Contains("tải"))
                {
                    if (await btn.IsVisibleAsync())
                    {
                        Log($"   Tìm thấy nút upload (scan): aria-label='{label}'");
                        return _page.Locator($"button[aria-label='{label}']").First;
                    }
                }
            }
            catch { }
        }

        return null;
    }

    // ========== TYPE PROMPT ==========
    // Gemini dùng Quill editor với class .ql-editor (contenteditable div)

    private async Task TypePromptAsync(string prompt)
    {
        if (_page == null) return;

        // Selectors cho ô nhập prompt trên Gemini
        string[] inputSelectors = {
            // Quill editor (Gemini hiện tại dùng Quill)
            ".ql-editor",
            "div.ql-editor[contenteditable='true']",
            // ContentEditable div
            "[contenteditable='true'][aria-label*='prompt']",
            "[contenteditable='true'][aria-label*='Enter']",
            "[contenteditable='true'][aria-label*='Nhập']",
            "div[contenteditable='true'][role='textbox']",
            // ProseMirror (dự phòng nếu Google đổi editor)
            ".ProseMirror",
            // Textarea fallback
            "textarea[aria-label*='prompt']",
            "textarea[placeholder]",
            // Generic contenteditable trong input area
            ".input-area [contenteditable='true']",
            ".chat-input [contenteditable='true']",
            "[contenteditable='true']",
        };

        ILocator? inputElement = null;

        foreach (var selector in inputSelectors)
        {
            try
            {
                var el = _page.Locator(selector).First;
                if (await el.CountAsync() > 0 && await el.IsVisibleAsync())
                {
                    inputElement = el;
                    Log($"   Tìm thấy ô nhập: {selector}");
                    break;
                }
            }
            catch { }
        }

        if (inputElement == null)
        {
            throw new Exception("Không tìm thấy ô nhập prompt trên trang Gemini.");
        }

        // Focus vào ô nhập
        await inputElement.ClickAsync();
        await Task.Delay(300);

        // Clear nội dung cũ (nếu có)
        await _page.Keyboard.PressAsync("Control+a");
        await Task.Delay(100);

        // Dùng clipboard để paste prompt (tránh vấn đề với special characters và xuống dòng)
        await _page.EvaluateAsync($"navigator.clipboard.writeText({System.Text.Json.JsonSerializer.Serialize(prompt)})");
        await _page.Keyboard.PressAsync("Control+v");
        await Task.Delay(500);

        // Nếu paste không hoạt động, fallback dùng type
        var currentText = await inputElement.InnerTextAsync();
        if (string.IsNullOrWhiteSpace(currentText))
        {
            Log("   Paste không hoạt động, dùng keyboard type...");
            await inputElement.FillAsync(prompt);
            await Task.Delay(300);
            
            // Nếu Fill cũng không hoạt động
            currentText = await inputElement.InnerTextAsync();
            if (string.IsNullOrWhiteSpace(currentText))
            {
                await inputElement.PressSequentiallyAsync(prompt, new LocatorPressSequentiallyOptions { Delay = 5 });
            }
        }

        Log("   ✅ Đã nhập prompt.");
    }

    // ========== SUBMIT & WAIT ==========

    private async Task SubmitAndWaitAsync()
    {
        if (_page == null) return;

        // Tìm nút Send/Gửi
        string[] sendSelectors = {
            "button[aria-label='Send message']",
            "button[aria-label='Send']",
            "button[aria-label='Gửi']",
            "button[aria-label='Submit']",
            "[data-tooltip='Send message']",
            "button[mattooltip='Send message']",
            "button[mattooltip='Gửi']",
            // Icon send (material icon)
            "button:has(mat-icon:text('send'))",
            // Selector chung
            ".send-button",
            "button.send-button",
        };

        ILocator? sendButton = null;

        foreach (var selector in sendSelectors)
        {
            try
            {
                var btn = _page.Locator(selector).First;
                if (await btn.CountAsync() > 0 && await btn.IsVisibleAsync())
                {
                    sendButton = btn;
                    Log($"   Tìm thấy nút Gửi: {selector}");
                    break;
                }
            }
            catch { }
        }

        // Fallback: tìm nút gửi bằng cách scan tất cả button
        if (sendButton == null)
        {
            var allBtns = await _page.Locator("button").AllAsync();
            foreach (var btn in allBtns)
            {
                try
                {
                    var label = await btn.GetAttributeAsync("aria-label") ?? "";
                    if (label.ToLower().Contains("send") || label.ToLower().Contains("gửi") ||
                        label.ToLower().Contains("submit"))
                    {
                        if (await btn.IsVisibleAsync() && await btn.IsEnabledAsync())
                        {
                            sendButton = btn;
                            Log($"   Tìm thấy nút Gửi (scan): {label}");
                            break;
                        }
                    }
                }
                catch { }
            }
        }

        // Fallback cuối: dùng Enter
        if (sendButton == null)
        {
            Log("   Không tìm thấy nút Gửi, dùng Enter...");
            await _page.Keyboard.PressAsync("Enter");
        }
        else
        {
            await sendButton.ClickAsync();
        }

        Log("⏳ Đã gửi, đang đợi Gemini trả lời...");

        // Đợi Gemini bắt đầu generate (xuất hiện nút Stop hoặc loading indicator)
        await Task.Delay(3000);

        // Đợi cho đến khi Gemini ngừng generate
        int maxWait = _maxWaitSeconds;
        for (int i = 0; i < maxWait; i++)
        {
            await Task.Delay(1000);

            // Kiểm tra nút Stop/Dừng (nếu còn hiện = đang generate)
            bool stillGenerating = false;
            string[] stopSelectors = {
                "button[aria-label='Stop']",
                "button[aria-label='Dừng']",
                "button[aria-label='Stop generating']",
                "button[mattooltip='Stop']",
                "button[mattooltip='Dừng']",
                ".loading-indicator",
                "[aria-label*='loading']",
            };

            foreach (var sel in stopSelectors)
            {
                try
                {
                    var stopBtn = _page.Locator(sel).First;
                    if (await stopBtn.CountAsync() > 0 && await stopBtn.IsVisibleAsync())
                    {
                        stillGenerating = true;
                        break;
                    }
                }
                catch { }
            }

            if (!stillGenerating)
            {
                // Đợi thêm 2s cho chắc (tránh false positive)
                await Task.Delay(2000);
                break;
            }

            // Log progress mỗi 15 giây
            if (i > 0 && i % 15 == 0)
            {
                Log($"   Vẫn đang đợi... ({i}s)");
            }
        }

        Log("   ✅ Gemini đã trả lời xong.");
    }

    // ========== GET RESPONSE ==========

    private async Task<string> GetLatestResponseAsync()
    {
        if (_page == null) return "Không lấy được kết quả.";

        // Selectors cho response message từ Gemini
        string[] responseSelectors = {
            // Model response containers
            "[data-message-author-role='model']",
            ".model-response-text",
            ".response-container .markdown",
            ".markdown-main-panel",
            // Message bubble từ AI
            ".message-content[data-author='model']",
            ".conversation-turn .model-response",
            // Gemini specific
            "message-content.model-response-text",
            ".response-content",
            // Fallback - bất kỳ div nào có class chứa 'response' hoặc 'answer'
            "[class*='response'][class*='text']",
            "[class*='message'][class*='model']",
        };

        foreach (var selector in responseSelectors)
        {
            try
            {
                var elements = await _page.Locator(selector).AllAsync();
                if (elements.Count > 0)
                {
                    // Lấy phần tử cuối cùng (response mới nhất)
                    var lastElement = elements[^1];
                    var text = await lastElement.InnerTextAsync();
                    if (!string.IsNullOrWhiteSpace(text) && text.Length > 10)
                    {
                        Log($"   Lấy response từ: {selector} ({text.Length} chars)");
                        return text;
                    }
                }
            }
            catch { }
        }

        // Fallback: lấy tất cả text trong main content area
        try
        {
            var mainContent = _page.Locator("[role='main']")
                .Or(_page.Locator("main"))
                .Or(_page.Locator(".conversation-container")).First;
            
            var fullText = await mainContent.InnerTextAsync();
            if (!string.IsNullOrWhiteSpace(fullText))
            {
                // Cố gắng tách phần response cuối cùng
                Log($"   Lấy response từ main content ({fullText.Length} chars)");
                return fullText;
            }
        }
        catch { }

        return "Không thể đọc kết quả từ Gemini. Vui lòng kiểm tra trang web.";
    }

    // ========== NEW CHAT ==========

    public async Task StartNewChatAsync()
    {
        if (_page == null) return;

        string[] newChatSelectors = {
            "a[aria-label='New chat']",
            "a[aria-label='Cuộc trò chuyện mới']",
            "button[aria-label='New chat']",
            "button[aria-label='Cuộc trò chuyện mới']",
            "[data-tooltip='New chat']",
            "[mattooltip='New chat']",
            "a[href='/app']",
        };

        foreach (var selector in newChatSelectors)
        {
            try
            {
                var btn = _page.Locator(selector).First;
                if (await btn.CountAsync() > 0 && await btn.IsVisibleAsync())
                {
                    await btn.ClickAsync();
                    await Task.Delay(3000);
                    Log("🔄 Đã tạo chat mới.");
                    return;
                }
            }
            catch { }
        }

        // Fallback: navigate lại trang
        Log("   Không tìm thấy nút New Chat, reload trang...");
        await _page.GotoAsync("https://gemini.google.com/app", new PageGotoOptions
        {
            Timeout = 60000,
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await Task.Delay(5000);
        Log("🔄 Đã reload trang Gemini.");
    }

    // ========== PROMPT BUILDERS ==========

    private string BuildGradingPrompt(string answerKey, string filePath)
    {
        string fileExt = Path.GetExtension(filePath).ToLower();
        string fileType = fileExt switch
        {
            ".pdf" => "PDF",
            ".docx" or ".doc" => "Word",
            ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" => "ảnh",
            _ => "file"
        };

        if (string.IsNullOrWhiteSpace(answerKey))
        {
            return $@"Hãy đọc bài thi Toán trong {fileType} đã upload và chấm điểm.
Yêu cầu:
1. Đọc từng câu hỏi và bài làm của học sinh
2. Chấm điểm từng câu (đúng/sai/đúng một phần)
3. Tổng điểm trên thang 10
4. Nhận xét ngắn gọn

Trả lời theo format:
ĐIỂM: [số]/10
CHI TIẾT:
- Câu 1: [điểm] - [nhận xét]
- Câu 2: [điểm] - [nhận xét]
...
NHẬN XÉT CHUNG: [nhận xét tổng thể]";
        }
        else
        {
            return $@"Hãy chấm bài thi Toán trong {fileType} đã upload dựa trên ĐÁP ÁN sau:

=== ĐÁP ÁN ===
{answerKey}
=== HẾT ĐÁP ÁN ===

Yêu cầu:
1. So sánh bài làm với đáp án
2. Chấm điểm từng câu
3. Tổng điểm trên thang 10

Trả lời theo format:
ĐIỂM: [số]/10
CHI TIẾT:
- Câu 1: [điểm] - [đúng/sai] - [nhận xét]
- Câu 2: [điểm] - [đúng/sai] - [nhận xét]
...
NHẬN XÉT CHUNG: [nhận xét tổng thể]";
        }
    }

    private string BuildGradingPromptWithFile(string answerFilePath, string examFilePath, string additionalNotes)
    {
        string answerFileName = Path.GetFileName(answerFilePath);
        string examFileName = Path.GetFileName(examFilePath);

        string extra = string.IsNullOrWhiteSpace(additionalNotes)
            ? "" : $"\n\nGhi chú thêm:\n{additionalNotes}";

        return $@"Tôi đã upload 2 file:
- File 1 ({answerFileName}): ĐÁP ÁN
- File 2 ({examFileName}): BÀI LÀM của học sinh

Hãy so sánh bài làm với đáp án và chấm điểm Toán.
Yêu cầu:
1. Đọc đáp án từ file 1
2. Đọc bài làm từ file 2
3. So sánh từng câu, chấm điểm
4. Tổng điểm trên thang 10

Trả lời theo format:
ĐIỂM: [số]/10
CHI TIẾT:
- Câu 1: [điểm] - [đúng/sai/một phần] - [nhận xét]
- Câu 2: [điểm] - [đúng/sai/một phần] - [nhận xét]
...
NHẬN XÉT CHUNG: [nhận xét tổng thể]{extra}";
    }

    // ========== HELPERS ==========

    private void Log(string message)
    {
        OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public void Dispose()
    {
        try { _browser?.CloseAsync().GetAwaiter().GetResult(); } catch { }
        _playwright?.Dispose();
    }
}
