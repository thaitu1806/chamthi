using Microsoft.Playwright;
using MathExamGrader.Models;
using System.Diagnostics;
using System.Text.Json;

namespace MathExamGrader.Services;

public class GeminiWebService : IDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private int _maxWaitSeconds = 120;
    private Process? _edgeProcess;

    // Selectors được phát hiện tự động từ DOM thật
    private string? _inputSelector;
    private string? _sendButtonSelector;
    private string? _uploadButtonSelector;
    private string? _responseSelector;
    private string? _newChatSelector;

    public event Action<string>? OnLog;

    public int MaxWaitSeconds
    {
        get => _maxWaitSeconds;
        set => _maxWaitSeconds = value;
    }

    // ========== INITIALIZATION ==========

    public async Task InitializeAsync(string userDataDir)
    {
        _playwright = await Playwright.CreateAsync();

        string edgePath = GetEdgePath();
        int debugPort = 9222;

        Log("Đang đóng Edge hiện tại (nếu có)...");
        KillAllEdgeProcesses();
        await Task.Delay(2000);

        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            + @"\Microsoft\Edge\User Data";

        Log($"Đang mở Edge với profile thật...");

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
                    throw new Exception($"Không thể kết nối Edge sau {maxRetries} lần.\nLỗi: {ex.Message}", ex);
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
            foreach (var p in Process.GetProcessesByName("msedge"))
            { try { p.Kill(); } catch { } }
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

    // ========== NAVIGATE & AUTO-DETECT SELECTORS ==========

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

        // Đợi page ổn định
        await Task.Delay(6000);

        // === TỰ ĐỘNG PHÂN TÍCH DOM ĐỂ LẤY SELECTOR CHÍNH XÁC ===
        Log("🔍 Đang phân tích DOM trang Gemini...");
        await DetectSelectorsAsync();

        Log("✅ Đã mở Gemini và phân tích xong DOM.");
    }

    /// <summary>
    /// Phân tích DOM thật của Gemini để tìm đúng selector cho từng thành phần
    /// </summary>
    private async Task DetectSelectorsAsync()
    {
        if (_page == null) return;

        // 1. Tìm ô nhập prompt (input area)
        _inputSelector = await DetectInputAsync();
        Log($"   📝 Input: {_inputSelector ?? "KHÔNG TÌM THẤY"}");

        // 2. Tìm nút gửi (send button)
        _sendButtonSelector = await DetectSendButtonAsync();
        Log($"   📤 Send: {_sendButtonSelector ?? "KHÔNG TÌM THẤY"}");

        // 3. Tìm nút upload file
        _uploadButtonSelector = await DetectUploadButtonAsync();
        Log($"   📎 Upload: {_uploadButtonSelector ?? "KHÔNG TÌM THẤY"}");

        // 4. Tìm selector response
        _responseSelector = await DetectResponseAreaAsync();
        Log($"   💬 Response: {_responseSelector ?? "(sẽ detect sau khi có response)"}");

        // 5. Tìm nút New Chat
        _newChatSelector = await DetectNewChatAsync();
        Log($"   🔄 New Chat: {_newChatSelector ?? "KHÔNG TÌM THẤY"}");
    }

    private async Task<string?> DetectInputAsync()
    {
        if (_page == null) return null;

        // Dùng JavaScript để tìm chính xác ô nhập trên trang Gemini
        var result = await _page.EvaluateAsync<string?>(@"() => {
            // Tìm tất cả contenteditable elements
            const editables = document.querySelectorAll('[contenteditable=""true""]');
            for (const el of editables) {
                const rect = el.getBoundingClientRect();
                // Ô nhập phải visible và có kích thước hợp lý
                if (rect.width > 100 && rect.height > 20 && rect.bottom > 0) {
                    // Tạo selector unique
                    if (el.classList.length > 0) {
                        // Thử dùng class
                        for (const cls of el.classList) {
                            if (document.querySelectorAll('.' + cls + '[contenteditable=""true""]').length === 1) {
                                return '.' + cls + '[contenteditable=""true""]';
                            }
                        }
                        return '.' + el.classList[0] + '[contenteditable=""true""]';
                    }
                    if (el.getAttribute('aria-label')) {
                        return '[contenteditable=""true""][aria-label=""' + el.getAttribute('aria-label') + '""]';
                    }
                    if (el.getAttribute('role')) {
                        return '[contenteditable=""true""][role=""' + el.getAttribute('role') + '""]';
                    }
                    return '[contenteditable=""true""]';
                }
            }
            // Fallback: tìm textarea
            const textarea = document.querySelector('textarea');
            if (textarea) {
                if (textarea.getAttribute('aria-label'))
                    return 'textarea[aria-label=""' + textarea.getAttribute('aria-label') + '""]';
                return 'textarea';
            }
            return null;
        }");

        return result;
    }

    private async Task<string?> DetectSendButtonAsync()
    {
        if (_page == null) return null;

        var result = await _page.EvaluateAsync<string?>(@"() => {
            const buttons = document.querySelectorAll('button');
            for (const btn of buttons) {
                const label = (btn.getAttribute('aria-label') || '').toLowerCase();
                const tooltip = (btn.getAttribute('data-tooltip') || '').toLowerCase();
                const matTooltip = (btn.getAttribute('mattooltip') || '').toLowerCase();
                const allText = label + ' ' + tooltip + ' ' + matTooltip;
                
                if (allText.includes('send') || allText.includes('gửi') || allText.includes('submit')) {
                    const rect = btn.getBoundingClientRect();
                    if (rect.width > 0 && rect.height > 0) {
                        if (btn.getAttribute('aria-label'))
                            return 'button[aria-label=""' + btn.getAttribute('aria-label') + '""]';
                        if (btn.getAttribute('data-tooltip'))
                            return 'button[data-tooltip=""' + btn.getAttribute('data-tooltip') + '""]';
                        if (btn.getAttribute('mattooltip'))
                            return 'button[mattooltip=""' + btn.getAttribute('mattooltip') + '""]';
                    }
                }
            }
            // Fallback: tìm button có icon send (SVG path thường có d attribute đặc trưng)
            // hoặc button cuối cùng trong input area
            const inputArea = document.querySelector('[contenteditable=""true""]');
            if (inputArea) {
                const parent = inputArea.closest('form') || inputArea.parentElement?.parentElement?.parentElement;
                if (parent) {
                    const btns = parent.querySelectorAll('button');
                    const lastBtn = btns[btns.length - 1];
                    if (lastBtn && lastBtn.getAttribute('aria-label'))
                        return 'button[aria-label=""' + lastBtn.getAttribute('aria-label') + '""]';
                }
            }
            return null;
        }");

        return result;
    }

    private async Task<string?> DetectUploadButtonAsync()
    {
        if (_page == null) return null;

        var result = await _page.EvaluateAsync<string?>(@"() => {
            const buttons = document.querySelectorAll('button');
            for (const btn of buttons) {
                const label = (btn.getAttribute('aria-label') || '').toLowerCase();
                const tooltip = (btn.getAttribute('data-tooltip') || '').toLowerCase();
                const matTooltip = (btn.getAttribute('mattooltip') || '').toLowerCase();
                const allText = label + ' ' + tooltip + ' ' + matTooltip;
                
                if (allText.includes('upload') || allText.includes('file') || 
                    allText.includes('attach') || allText.includes('tệp') ||
                    allText.includes('tải') || allText.includes('add image') ||
                    allText.includes('thêm')) {
                    const rect = btn.getBoundingClientRect();
                    if (rect.width > 0 && rect.height > 0) {
                        if (btn.getAttribute('aria-label'))
                            return 'button[aria-label=""' + btn.getAttribute('aria-label') + '""]';
                        if (btn.getAttribute('data-tooltip'))
                            return 'button[data-tooltip=""' + btn.getAttribute('data-tooltip') + '""]';
                        if (btn.getAttribute('mattooltip'))
                            return 'button[mattooltip=""' + btn.getAttribute('mattooltip') + '""]';
                    }
                }
            }
            // Tìm input[type=file]
            const fileInput = document.querySelector('input[type=""file""]');
            if (fileInput) return '__FILE_INPUT__';
            return null;
        }");

        return result;
    }

    private async Task<string?> DetectResponseAreaAsync()
    {
        if (_page == null) return null;

        var result = await _page.EvaluateAsync<string?>(@"() => {
            // Tìm container chứa response từ model
            const selectors = [
                '[data-message-author-role=""model""]',
                '.model-response-text',
                '.response-container',
                '.markdown-main-panel',
                'message-content',
            ];
            for (const sel of selectors) {
                if (document.querySelector(sel)) return sel;
            }
            return null;
        }");

        return result;
    }

    private async Task<string?> DetectNewChatAsync()
    {
        if (_page == null) return null;

        var result = await _page.EvaluateAsync<string?>(@"() => {
            // Tìm link/button 'New chat'
            const elements = document.querySelectorAll('a, button');
            for (const el of elements) {
                const label = (el.getAttribute('aria-label') || '').toLowerCase();
                const text = (el.textContent || '').toLowerCase();
                const href = (el.getAttribute('href') || '').toLowerCase();
                
                if (label.includes('new chat') || label.includes('cuộc trò chuyện mới') ||
                    text.includes('new chat') || href === '/app') {
                    if (el.getAttribute('aria-label'))
                        return (el.tagName.toLowerCase()) + '[aria-label=""' + el.getAttribute('aria-label') + '""]';
                    if (el.getAttribute('href'))
                        return el.tagName.toLowerCase() + '[href=""' + el.getAttribute('href') + '""]';
                }
            }
            return null;
        }");

        return result;
    }

    /// <summary>
    /// Dump HTML của trang để debug - ghi ra log
    /// </summary>
    public async Task DumpPageInfoAsync()
    {
        if (_page == null) return;

        var info = await _page.EvaluateAsync<string>(@"() => {
            let result = '=== PAGE INFO ===\n';
            result += 'URL: ' + window.location.href + '\n';
            result += 'Title: ' + document.title + '\n\n';
            
            // Tìm tất cả contenteditable
            const editables = document.querySelectorAll('[contenteditable=""true""]');
            result += '--- Contenteditable elements: ' + editables.length + ' ---\n';
            editables.forEach((el, i) => {
                const rect = el.getBoundingClientRect();
                result += i + ': tag=' + el.tagName + ' class=""' + el.className + '"" role=""' + (el.getAttribute('role')||'') + '"" aria=""' + (el.getAttribute('aria-label')||'') + '"" size=' + Math.round(rect.width) + 'x' + Math.round(rect.height) + '\n';
            });
            
            // Tìm tất cả buttons visible
            result += '\n--- Buttons (visible, có aria-label): ---\n';
            const buttons = document.querySelectorAll('button');
            let btnCount = 0;
            buttons.forEach((btn) => {
                const rect = btn.getBoundingClientRect();
                if (rect.width > 0 && rect.height > 0 && btn.getAttribute('aria-label')) {
                    result += btnCount + ': aria=""' + btn.getAttribute('aria-label') + '"" tooltip=""' + (btn.getAttribute('data-tooltip')||btn.getAttribute('mattooltip')||'') + '"" size=' + Math.round(rect.width) + 'x' + Math.round(rect.height) + '\n';
                    btnCount++;
                }
            });
            
            // Tìm input[type=file]
            const fileInputs = document.querySelectorAll('input[type=""file""]');
            result += '\n--- File inputs: ' + fileInputs.length + ' ---\n';
            fileInputs.forEach((el, i) => {
                result += i + ': accept=""' + (el.getAttribute('accept')||'') + '"" hidden=' + el.hidden + ' class=""' + el.className + '""\n';
            });
            
            return result;
        }");

        // Log từng dòng
        foreach (var line in info.Split('\n'))
        {
            Log(line);
        }
    }

    // ========== GRADING ==========

    public async Task<string> GradeExamAsync(string filePath, string answerKey)
    {
        if (_page == null) throw new InvalidOperationException("Browser chưa được khởi tạo.");

        try
        {
            string prompt = BuildGradingPrompt(answerKey, filePath);

            await UploadFileAsync(filePath);
            await Task.Delay(3000);

            await TypePromptAsync(prompt);
            await Task.Delay(1000);

            await SubmitAndWaitAsync();

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
            await UploadFileAsync(answerFilePath);
            await Task.Delay(2000);
            Log($"📄 Đã upload đáp án: {Path.GetFileName(answerFilePath)}");

            await UploadFileAsync(examFilePath);
            await Task.Delay(2000);
            Log($"📄 Đã upload bài thi: {Path.GetFileName(examFilePath)}");

            string prompt = BuildGradingPromptWithFile(answerFilePath, examFilePath, additionalNotes);
            await TypePromptAsync(prompt);
            await Task.Delay(1000);

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

    private async Task UploadFileAsync(string filePath)
    {
        if (_page == null) return;

        // Trên Gemini, nút "Nội dung tải lên và công cụ" (hoặc "+") sẽ mở MENU
        // Trong menu có option "Upload file" → click vào đó mới mở file dialog

        // Bước 1: Click nút "+" hoặc "Nội dung tải lên và công cụ" để mở menu
        Log($"   📎 Đang upload: {Path.GetFileName(filePath)}");
        
        var menuButton = await FindMenuButtonAsync();
        if (menuButton != null)
        {
            await menuButton.ClickAsync();
            await Task.Delay(1500); // Đợi menu hiện ra

            // Bước 2: Tìm và click item "Upload file" / "Tải tệp lên" trong menu
            var uploadMenuItem = await FindUploadMenuItemAsync();
            
            if (uploadMenuItem != null)
            {
                // Bước 3: Click menu item và đợi FileChooser
                try
                {
                    var fileChooser = await _page.RunAndWaitForFileChooserAsync(async () =>
                    {
                        await uploadMenuItem.ClickAsync();
                    });
                    await fileChooser.SetFilesAsync(filePath);
                    Log($"   ✅ Upload thành công (menu → file chooser)");
                    await Task.Delay(4000); // Đợi file được xử lý
                    return;
                }
                catch (Exception ex)
                {
                    Log($"   ⚠️ FileChooser lỗi: {ex.Message}");
                }
            }
            else
            {
                Log("   ⚠️ Không tìm thấy menu item 'Upload file'");
                // Đóng menu bằng Escape
                await _page.Keyboard.PressAsync("Escape");
                await Task.Delay(500);
            }
        }

        // Fallback: Thử tìm input[type=file] (có thể đã xuất hiện sau khi click menu)
        var fileInput = _page.Locator("input[type='file']").First;
        if (await fileInput.CountAsync() > 0)
        {
            await fileInput.SetInputFilesAsync(filePath);
            Log($"   ✅ Upload thành công (direct input)");
            await Task.Delay(4000);
            return;
        }

        // Fallback 2: Thử paste file (một số version Gemini hỗ trợ drag/paste)
        Log("   ⚠️ Thử cách khác...");
        
        // Click nút "+" lần nữa nếu menu đã đóng
        if (menuButton != null)
        {
            await menuButton.ClickAsync();
            await Task.Delay(1500);
        }

        // Tìm bất kỳ element nào cho phép file upload trong menu
        var anyFileOption = await _page.EvaluateAsync<string?>(@"() => {
            // Tìm trong menu items
            const menuItems = document.querySelectorAll('[role=""menuitem""], [role=""option""], .menu-item, mat-menu-item, [class*=""menu""] button, [class*=""menu""] [role=""button""]');
            for (const item of menuItems) {
                const text = (item.textContent || '').toLowerCase();
                const label = (item.getAttribute('aria-label') || '').toLowerCase();
                if (text.includes('upload') || text.includes('tải') || text.includes('tệp') || 
                    text.includes('file') || label.includes('upload') || label.includes('file')) {
                    // Trả về text để log
                    return item.textContent.trim().substring(0, 50);
                }
            }
            return null;
        }");

        if (anyFileOption != null)
        {
            Log($"   Tìm thấy menu item: '{anyFileOption}'");
            
            // Click vào item đó bằng text
            try
            {
                var fileChooser = await _page.RunAndWaitForFileChooserAsync(async () =>
                {
                    await _page.GetByText(anyFileOption.Split('\n')[0].Trim()).First.ClickAsync();
                });
                await fileChooser.SetFilesAsync(filePath);
                Log($"   ✅ Upload thành công (text match)");
                await Task.Delay(4000);
                return;
            }
            catch
            {
                await _page.Keyboard.PressAsync("Escape");
                await Task.Delay(500);
            }
        }

        // Final fallback: check lại input[type=file]
        fileInput = _page.Locator("input[type='file']").First;
        if (await fileInput.CountAsync() > 0)
        {
            await fileInput.SetInputFilesAsync(filePath);
            Log($"   ✅ Upload thành công (late input)");
            await Task.Delay(4000);
            return;
        }

        await DumpPageInfoAsync();
        throw new Exception("Không thể upload file. Kiểm tra log.");
    }

    /// <summary>
    /// Tìm nút "+" hoặc "Nội dung tải lên và công cụ" trên thanh input
    /// </summary>
    private async Task<ILocator?> FindMenuButtonAsync()
    {
        if (_page == null) return null;

        // Tìm bằng JS chính xác
        var selector = await _page.EvaluateAsync<string?>(@"() => {
            const buttons = document.querySelectorAll('button');
            for (const btn of buttons) {
                const label = (btn.getAttribute('aria-label') || '').toLowerCase();
                const tooltip = (btn.getAttribute('data-tooltip') || btn.getAttribute('mattooltip') || '').toLowerCase();
                const combined = label + ' ' + tooltip;
                
                // Nút upload/attach trên Gemini
                if (combined.includes('nội dung tải lên') || combined.includes('upload') || 
                    combined.includes('add') || combined.includes('attach') || 
                    combined.includes('thêm tệp') || combined.includes('công cụ')) {
                    const rect = btn.getBoundingClientRect();
                    if (rect.width > 0 && rect.height > 0) {
                        const ariaLabel = btn.getAttribute('aria-label');
                        if (ariaLabel) return 'button[aria-label=""' + ariaLabel + '""]';
                    }
                }
            }
            return null;
        }");

        if (!string.IsNullOrEmpty(selector))
        {
            return _page.Locator(selector).First;
        }

        // Fallback: dùng selector đã detect trước đó
        if (!string.IsNullOrEmpty(_uploadButtonSelector) && _uploadButtonSelector != "__FILE_INPUT__")
        {
            return _page.Locator(_uploadButtonSelector).First;
        }

        return null;
    }

    /// <summary>
    /// Sau khi menu mở, tìm item "Upload file" / "Tải tệp lên"
    /// </summary>
    private async Task<ILocator?> FindUploadMenuItemAsync()
    {
        if (_page == null) return null;

        // Đợi menu animation
        await Task.Delay(500);

        // Tìm menu item bằng JS
        var itemInfo = await _page.EvaluateAsync<string?>(@"() => {
            // Tìm tất cả menu items visible
            const candidates = document.querySelectorAll(
                '[role=""menuitem""], [role=""option""], [class*=""menu""] button, ' +
                '[class*=""menu""] [role=""button""], [class*=""dropdown""] button, ' +
                'mat-menu-item, .mat-mdc-menu-item, [class*=""menu-item""]'
            );
            
            for (const item of candidates) {
                const text = (item.textContent || '').toLowerCase().trim();
                const label = (item.getAttribute('aria-label') || '').toLowerCase();
                const rect = item.getBoundingClientRect();
                
                // Phải visible
                if (rect.width === 0 || rect.height === 0) continue;
                
                // Tìm item liên quan upload file
                if (text.includes('upload') || text.includes('tải tệp lên') || 
                    text.includes('tải lên') || text.includes('chọn tệp') ||
                    label.includes('upload') || label.includes('tải tệp')) {
                    // Trả về selector có thể dùng
                    const ariaLabel = item.getAttribute('aria-label');
                    if (ariaLabel) return '[aria-label=""' + ariaLabel + '""]';
                    // Dùng text content
                    return '__TEXT__' + item.textContent.trim().split('\n')[0];
                }
            }
            
            // Nếu không tìm thấy cụ thể, tìm item đầu tiên có icon file
            for (const item of candidates) {
                const rect = item.getBoundingClientRect();
                if (rect.width === 0 || rect.height === 0) continue;
                const text = (item.textContent || '').toLowerCase();
                if (text.includes('file') || text.includes('tệp') || text.includes('máy tính')) {
                    const ariaLabel = item.getAttribute('aria-label');
                    if (ariaLabel) return '[aria-label=""' + ariaLabel + '""]';
                    return '__TEXT__' + item.textContent.trim().split('\n')[0];
                }
            }
            
            return null;
        }");

        if (string.IsNullOrEmpty(itemInfo)) return null;

        if (itemInfo.StartsWith("__TEXT__"))
        {
            // Tìm bằng text
            string text = itemInfo.Substring(7).Trim();
            Log($"   📂 Menu item (text): '{text}'");
            return _page.GetByText(text, new PageGetByTextOptions { Exact = false }).First;
        }
        else
        {
            Log($"   📂 Menu item (selector): '{itemInfo}'");
            return _page.Locator(itemInfo).First;
        }
    }

    // ========== TYPE PROMPT ==========

    private async Task TypePromptAsync(string prompt)
    {
        if (_page == null) return;

        ILocator? inputElement = null;

        // Dùng selector đã detect
        if (!string.IsNullOrEmpty(_inputSelector))
        {
            var el = _page.Locator(_inputSelector).First;
            if (await el.CountAsync() > 0 && await el.IsVisibleAsync())
            {
                inputElement = el;
            }
        }

        // Nếu không tìm thấy, detect lại realtime
        if (inputElement == null)
        {
            Log("   🔍 Tìm lại ô nhập...");
            var sel = await _page.EvaluateAsync<string?>(@"() => {
                const editables = document.querySelectorAll('[contenteditable=""true""]');
                for (const el of editables) {
                    const rect = el.getBoundingClientRect();
                    if (rect.width > 100 && rect.height > 20 && rect.bottom > 0 &&
                        getComputedStyle(el).display !== 'none') {
                        if (el.classList.length > 0) return '.' + el.classList[0];
                        return '[contenteditable=""true""]';
                    }
                }
                const ta = document.querySelector('textarea');
                if (ta) return 'textarea';
                return null;
            }");

            if (!string.IsNullOrEmpty(sel))
            {
                inputElement = _page.Locator(sel).First;
                _inputSelector = sel; // Cập nhật cache
            }
        }

        if (inputElement == null)
        {
            await DumpPageInfoAsync();
            throw new Exception("Không tìm thấy ô nhập prompt.");
        }

        // Click focus
        await inputElement.ClickAsync();
        await Task.Delay(300);

        // Clear
        await _page.Keyboard.PressAsync("Control+a");
        await _page.Keyboard.PressAsync("Backspace");
        await Task.Delay(200);

        // Paste prompt qua clipboard (tốt nhất cho rich editor)
        await _page.EvaluateAsync(@"(text) => {
            const el = document.activeElement;
            if (el && el.getAttribute('contenteditable') === 'true') {
                el.innerHTML = '<p>' + text.replace(/\n/g, '</p><p>') + '</p>';
                el.dispatchEvent(new Event('input', { bubbles: true }));
            }
        }", prompt);

        await Task.Delay(500);

        // Verify prompt đã được nhập
        var currentText = await inputElement.InnerTextAsync();
        if (string.IsNullOrWhiteSpace(currentText))
        {
            // Fallback: dùng keyboard
            Log("   ⚠️ innerHTML không hoạt động, thử clipboard paste...");
            await inputElement.ClickAsync();
            await _page.Keyboard.PressAsync("Control+a");
            await _page.Keyboard.PressAsync("Backspace");

            // Copy to clipboard and paste
            await _page.EvaluateAsync("(text) => navigator.clipboard.writeText(text)", prompt);
            await Task.Delay(200);
            await _page.Keyboard.PressAsync("Control+v");
            await Task.Delay(500);

            currentText = await inputElement.InnerTextAsync();
            if (string.IsNullOrWhiteSpace(currentText))
            {
                // Final fallback: FillAsync
                Log("   ⚠️ Clipboard không hoạt động, thử Fill...");
                try { await inputElement.FillAsync(prompt); }
                catch { await inputElement.PressSequentiallyAsync(prompt.Substring(0, Math.Min(200, prompt.Length))); }
            }
        }

        Log("   ✅ Đã nhập prompt.");
    }

    // ========== SUBMIT & WAIT ==========

    private async Task SubmitAndWaitAsync()
    {
        if (_page == null) return;

        // Dùng selector đã detect
        if (!string.IsNullOrEmpty(_sendButtonSelector))
        {
            try
            {
                var btn = _page.Locator(_sendButtonSelector).First;
                if (await btn.CountAsync() > 0 && await btn.IsEnabledAsync())
                {
                    await btn.ClickAsync();
                    Log("⏳ Đã gửi (detected button)...");
                    await WaitForResponseCompleteAsync();
                    return;
                }
            }
            catch { }
        }

        // Detect lại realtime
        var sendSel = await _page.EvaluateAsync<string?>(@"() => {
            const btns = document.querySelectorAll('button');
            for (const btn of btns) {
                const label = (btn.getAttribute('aria-label') || '').toLowerCase();
                const tooltip = (btn.getAttribute('data-tooltip') || btn.getAttribute('mattooltip') || '').toLowerCase();
                const combined = label + ' ' + tooltip;
                if (combined.includes('send') || combined.includes('gửi') || combined.includes('submit')) {
                    const rect = btn.getBoundingClientRect();
                    if (rect.width > 0 && rect.height > 0 && !btn.disabled) {
                        if (btn.getAttribute('aria-label'))
                            return 'button[aria-label=""' + btn.getAttribute('aria-label') + '""]';
                    }
                }
            }
            return null;
        }");

        if (!string.IsNullOrEmpty(sendSel))
        {
            var btn = _page.Locator(sendSel).First;
            await btn.ClickAsync();
            _sendButtonSelector = sendSel;
            Log("⏳ Đã gửi (realtime detect)...");
        }
        else
        {
            // Fallback: Enter
            Log("⏳ Dùng Enter để gửi...");
            await _page.Keyboard.PressAsync("Enter");
        }

        await WaitForResponseCompleteAsync();
    }

    private async Task WaitForResponseCompleteAsync()
    {
        if (_page == null) return;

        await Task.Delay(4000); // Đợi Gemini bắt đầu

        int maxWait = _maxWaitSeconds;
        for (int i = 0; i < maxWait; i++)
        {
            await Task.Delay(1000);

            // Check xem Gemini còn đang generate không (bằng JS)
            var isGenerating = await _page.EvaluateAsync<bool>(@"() => {
                // Tìm nút Stop hoặc indicator loading
                const btns = document.querySelectorAll('button');
                for (const btn of btns) {
                    const label = (btn.getAttribute('aria-label') || '').toLowerCase();
                    if ((label.includes('stop') || label.includes('dừng')) &&
                        btn.getBoundingClientRect().width > 0) {
                        return true;
                    }
                }
                // Check loading spinner/indicator
                const loaders = document.querySelectorAll('[class*=""loading""], [class*=""generating""], [class*=""typing""]');
                for (const l of loaders) {
                    if (l.getBoundingClientRect().width > 0) return true;
                }
                return false;
            }");

            if (!isGenerating)
            {
                await Task.Delay(2000); // Buffer
                break;
            }

            if (i > 0 && i % 15 == 0) Log($"   Vẫn đang đợi... ({i}s)");
        }

        Log("   ✅ Gemini đã trả lời xong.");
    }

    // ========== GET RESPONSE ==========

    private async Task<string> GetLatestResponseAsync()
    {
        if (_page == null) return "Không lấy được kết quả.";

        // Dùng JS để lấy response mới nhất
        var response = await _page.EvaluateAsync<string?>(@"() => {
            // Thử nhiều selector
            const selectors = [
                '[data-message-author-role=""model""]',
                '.model-response-text',
                '.markdown-main-panel', 
                '.response-container',
                'message-content.model-response-text',
                '[class*=""response""][class*=""text""]',
                '[class*=""model""][class*=""response""]',
            ];
            
            for (const sel of selectors) {
                const elements = document.querySelectorAll(sel);
                if (elements.length > 0) {
                    const last = elements[elements.length - 1];
                    const text = last.innerText || last.textContent;
                    if (text && text.trim().length > 10) return text.trim();
                }
            }

            // Fallback: tìm tất cả message turns và lấy cái cuối cùng từ model
            const allTurns = document.querySelectorAll('[class*=""turn""], [class*=""message""]');
            if (allTurns.length > 0) {
                const last = allTurns[allTurns.length - 1];
                const text = last.innerText || last.textContent;
                if (text && text.trim().length > 20) return text.trim();
            }

            // Final fallback: lấy main content
            const main = document.querySelector('[role=""main""]') || document.querySelector('main');
            if (main) return main.innerText;
            
            return null;
        }");

        if (!string.IsNullOrEmpty(response))
        {
            Log($"   📋 Response: {response.Length} chars");
            return response;
        }

        return "Không thể đọc kết quả từ Gemini.";
    }

    // ========== NEW CHAT ==========

    public async Task StartNewChatAsync()
    {
        if (_page == null) return;

        if (!string.IsNullOrEmpty(_newChatSelector))
        {
            try
            {
                var btn = _page.Locator(_newChatSelector).First;
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

        // Fallback: reload page
        Log("   Reload trang Gemini...");
        await _page.GotoAsync("https://gemini.google.com/app", new PageGotoOptions
        {
            Timeout = 60000,
            WaitUntil = WaitUntilState.DOMContentLoaded
        });
        await Task.Delay(5000);
        Log("🔄 Đã reload Gemini.");
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
            return $"Hãy đọc bài thi Toán trong {fileType} đã upload và chấm điểm.\n" +
                   "Yêu cầu:\n1. Đọc từng câu và bài làm\n2. Chấm điểm từng câu\n3. Tổng điểm /10\n4. Nhận xét\n\n" +
                   "Trả lời theo format:\nĐIỂM: [số]/10\nCHI TIẾT:\n- Câu 1: [điểm] - [nhận xét]\n- Câu 2: [điểm] - [nhận xét]\n...\nNHẬN XÉT CHUNG: [nhận xét]";
        }
        else
        {
            return $"Chấm bài thi Toán ({fileType} đã upload) theo ĐÁP ÁN:\n\n{answerKey}\n\n" +
                   "Yêu cầu: So sánh với đáp án, chấm từng câu, tổng /10.\n\n" +
                   "Format:\nĐIỂM: [số]/10\nCHI TIẾT:\n- Câu 1: [điểm] - [đúng/sai] - [nhận xét]\n...\nNHẬN XÉT CHUNG: [nhận xét]";
        }
    }

    private string BuildGradingPromptWithFile(string answerFilePath, string examFilePath, string additionalNotes)
    {
        string a = Path.GetFileName(answerFilePath);
        string e = Path.GetFileName(examFilePath);
        string extra = string.IsNullOrWhiteSpace(additionalNotes) ? "" : $"\nGhi chú: {additionalNotes}";

        return $"Đã upload 2 file:\n- File 1 ({a}): ĐÁP ÁN\n- File 2 ({e}): BÀI LÀM học sinh\n\n" +
               "So sánh bài làm với đáp án, chấm điểm Toán.\n" +
               "Format:\nĐIỂM: [số]/10\nCHI TIẾT:\n- Câu 1: [điểm] - [đúng/sai] - [nhận xét]\n...\nNHẬN XÉT CHUNG: [nhận xét]" + extra;
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
