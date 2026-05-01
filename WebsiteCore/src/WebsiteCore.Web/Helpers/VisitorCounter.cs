namespace WebsiteCore.Web.Helpers;

/// <summary>
/// Đếm lượt truy cập file-backed.
/// 3 file:
///   Count_Access.txt   — số session đang online (tăng Session_Start, giảm Session_End)
///   Count_Vstoday.txt  — định dạng "dd/MM/yyyyN" (10 ký tự ngày + counter) — reset khi sang ngày mới
///   Count_Visited.txt  — tổng lượt truy cập (tăng mỗi session mới)
/// </summary>
public class VisitorCounter
{
    private readonly string _root;
    private static readonly object _lock = new();

    public VisitorCounter(IWebHostEnvironment env)
    {
        _root = Path.Combine(env.WebRootPath, "assets", "admin", "count");
        Directory.CreateDirectory(_root);
        EnsureFile("Count_Access.txt", "0");
        EnsureFile("Count_Visited.txt", "0");
        EnsureFile("Count_Vstoday.txt", DateTime.Now.ToString("dd/MM/yyyy") + "0");
    }

    public int OnlineCount => ReadInt("Count_Access.txt");
    public int TodayCount  => ReadTodayCount();
    public int TotalCount  => ReadInt("Count_Visited.txt");

    public void OnSessionStart()
    {
        lock (_lock)
        {
            WriteInt("Count_Access.txt", ReadInt("Count_Access.txt") + 1);
            WriteInt("Count_Visited.txt", ReadInt("Count_Visited.txt") + 1);

            // Vstoday: prefix là dd/MM/yyyy, suffix là số. Sang ngày mới reset.
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

    public void OnSessionEnd()
    {
        lock (_lock)
        {
            var v = ReadInt("Count_Access.txt") - 1;
            if (v < 0) v = 0;
            WriteInt("Count_Access.txt", v);
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
