using System.Collections.Concurrent;

namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Đếm lượt truy cập cho footer.
///
/// 3 con số hiển thị:
///   • OnlineCount — số session active trong 15 phút gần nhất (in-memory, sliding window).
///   • TodayCount  — số session mới trong ngày hiện tại (file-backed, reset khi đổi ngày).
///   • TotalCount  — tổng số session từ trước đến nay (file-backed, chỉ tăng).
///
/// Invariant: OnlineCount ≤ TodayCount ≤ TotalCount.
/// </summary>
public class VisitorCounter
{
    private readonly string _root;
    private static readonly object _fileLock = new();
    private static readonly TimeSpan ActiveWindow = TimeSpan.FromMinutes(15);

    /// <summary>sessionId → timestamp request gần nhất. Sliding window 15'.</summary>
    private static readonly ConcurrentDictionary<string, DateTime> _activeSessions = new();

    public VisitorCounter(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.WebRootPath, "assets", "admin", "count");
        Directory.CreateDirectory(_root);
        EnsureFile("Count_Visited.txt", "0");
        EnsureFile("Count_Vstoday.txt", DateTime.Now.ToString("dd/MM/yyyy") + "0");
    }

    public int OnlineCount
    {
        get
        {
            var cutoff = DateTime.UtcNow - ActiveWindow;
            foreach (var kv in _activeSessions)
            {
                if (kv.Value < cutoff)
                    _activeSessions.TryRemove(kv.Key, out _);
            }
            return _activeSessions.Count;
        }
    }

    public int TodayCount => ReadTodayCount();
    public int TotalCount => ReadInt("Count_Visited.txt");

    /// <summary>Gọi mỗi request để cập nhật activity timestamp của session hiện tại.</summary>
    public void Touch(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId)) return;
        _activeSessions[sessionId] = DateTime.UtcNow;
    }

    /// <summary>Reset bộ đếm online về 0 (dùng cho AdminCP reset counter).</summary>
    public void ResetOnline()
    {
        _activeSessions.Clear();
    }

    /// <summary>Gọi 1 lần khi session mới được tạo (VisitedFlag chưa set).</summary>
    public void OnNewSession()
    {
        lock (_fileLock)
        {
            WriteInt("Count_Visited.txt", ReadInt("Count_Visited.txt") + 1);

            var raw = SafeRead("Count_Vstoday.txt");
            var todayStr = DateTime.Now.ToString("dd/MM/yyyy");
            int todayN = 0;
            if (!string.IsNullOrEmpty(raw) && raw.Length >= 10 && raw.StartsWith(todayStr))
            {
                int.TryParse(raw.Substring(10), out todayN);
            }
            todayN++;
            SafeWrite("Count_Vstoday.txt", todayStr + todayN);
        }
    }

    private int ReadInt(string name)
    {
        var v = SafeRead(name);
        return int.TryParse(v?.Trim(), out var n) ? n : 0;
    }

    private int ReadTodayCount()
    {
        var raw = SafeRead("Count_Vstoday.txt");
        if (string.IsNullOrEmpty(raw) || raw.Length < 10) return 0;
        var todayStr = DateTime.Now.ToString("dd/MM/yyyy");
        if (!raw.StartsWith(todayStr)) return 0;
        int.TryParse(raw.Substring(10), out var n);
        return n;
    }

    private void WriteInt(string name, int v) => SafeWrite(name, v.ToString());

    private void EnsureFile(string name, string defaultValue)
    {
        var p = Path.Combine(_root, name);
        if (!File.Exists(p)) File.WriteAllText(p, defaultValue);
    }

    private string? SafeRead(string name)
    {
        try { return File.ReadAllText(Path.Combine(_root, name)); }
        catch { return null; }
    }

    private void SafeWrite(string name, string content)
    {
        try { File.WriteAllText(Path.Combine(_root, name), content); } catch { }
    }
}
