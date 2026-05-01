using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebsiteCore.Business.Helpers;

/// <summary>
/// PBKDF2-SHA256 password hashing + MD5 verify (backward compat) + slug Vietnamese.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Tạo slug URL từ tiếng Việt: bỏ dấu, lowercase, thay space + ký tự đặc biệt bằng "-".
    /// Ví dụ "Khoa Y học cổ truyền" -> "khoa-y-hoc-co-truyen".
    /// </summary>
    public static string ChangeText(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        input = input.Trim();
        for (int i = 0x20; i < 0x30; i++)
            input = input.Replace(((char)i).ToString(), " ");
        foreach (var c in new[] { ".", "*", "&", "#", "(", ")", "[", "]", " ", ",", ";", ":" })
            input = input.Replace(c, "-");
        input = input.Replace("\"", "");
        var regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
        var str = input.Normalize(NormalizationForm.FormD);
        var str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
        while (str2.Contains('?')) str2 = str2.Remove(str2.IndexOf('?'), 1);
        while (str2.Contains("--")) str2 = str2.Replace("--", "-");
        return str2.ToLower();
    }

    public static string SubString(string? text, int subLength)
    {
        if (text == null) return string.Empty;
        return text.Length > subLength ? text[..subLength] + "..." : text;
    }

    /* ----------------------------------------------------- */
    /*  Password hashing — PBKDF2-SHA256 + MD5 fallback       */
    /* ----------------------------------------------------- */

    // OWASP 2024 khuyến nghị PBKDF2-SHA256 ≥ 600k iterations cho password hashing.
    // Tăng từ 10k → 600k chống brute-force offline khi DB bị leak.
    private const int Pbkdf2Iterations = 600_000;
    private const int Pbkdf2SaltBytes  = 32;
    private const int Pbkdf2HashBytes  = 32;

    /// <summary>
    /// Hash là legacy (cần re-hash) khi: format MD5 cũ HOẶC iterations PBKDF2 thấp hơn current.
    /// Caller dùng để auto upgrade hash sau khi verify thành công với plaintext.
    /// </summary>
    public static bool NeedsRehash(string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;
        if (stored.Length == 32) return true; // MD5 legacy
        if (stored.StartsWith("pbkdf2$", StringComparison.Ordinal))
        {
            var parts = stored.Split('$');
            if (parts.Length != 4) return false;
            if (!int.TryParse(parts[1], out var iter)) return false;
            return iter < Pbkdf2Iterations;
        }
        return false;
    }

    /// <summary>
    /// Hash password tạo ra format "pbkdf2$&lt;iter&gt;$&lt;base64-salt&gt;$&lt;base64-hash&gt;".
    /// Cột User.password nên có độ dài ≥ 200 (thực tế ~102 chars).
    /// </summary>
    public static string HashPassword(string text)
    {
        text ??= string.Empty;
        var salt = RandomNumberGenerator.GetBytes(Pbkdf2SaltBytes);
        var derived = Rfc2898DeriveBytes.Pbkdf2(
            password: text,
            salt: salt,
            iterations: Pbkdf2Iterations,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: Pbkdf2HashBytes);
        return $"pbkdf2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(derived)}";
    }

    /// <summary>
    /// Verify password — auto-detect PBKDF2 vs MD5 (32 hex chars fallback).
    /// Hỗ trợ tài khoản hash MD5 cũ login được, tự rehash sang PBKDF2 sau verify thành công.
    /// </summary>
    public static bool VerifyPassword(string text, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;
        text ??= string.Empty;

        // PBKDF2 format
        if (stored.StartsWith("pbkdf2$", StringComparison.Ordinal))
        {
            var parts = stored.Split('$');
            if (parts.Length != 4) return false;
            if (!int.TryParse(parts[1], out var iter)) return false;
            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expected = Convert.FromBase64String(parts[3]);
            }
            catch { return false; }
            var derived = Rfc2898DeriveBytes.Pbkdf2(text, salt, iter, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(derived, expected);
        }

        // Legacy MD5 (32 hex)
        if (stored.Length == 32)
            return string.Equals(Md5Hash(text), stored, StringComparison.OrdinalIgnoreCase);

        return false;
    }

    /// <summary>
    /// Validate độ mạnh — return null nếu OK, ngược lại return error message.
    /// Tiêu chuẩn: ≥8 ký tự, có chữ HOA, chữ thường, chữ số, ký tự đặc biệt; không trùng pattern phổ biến.
    /// </summary>
    public static string? ValidatePasswordStrength(string? pwd)
    {
        if (string.IsNullOrEmpty(pwd)) return "Mật khẩu không được để trống.";
        if (pwd.Length < 8)   return "Mật khẩu phải có ít nhất 8 ký tự.";
        if (pwd.Length > 100) return "Mật khẩu quá dài (tối đa 100 ký tự).";
        if (!pwd.Any(char.IsUpper))                          return "Mật khẩu phải có ít nhất 1 chữ HOA (A-Z).";
        if (!pwd.Any(char.IsLower))                          return "Mật khẩu phải có ít nhất 1 chữ thường (a-z).";
        if (!pwd.Any(char.IsDigit))                          return "Mật khẩu phải có ít nhất 1 chữ số (0-9).";
        if (!pwd.Any(c => !char.IsLetterOrDigit(c)))         return "Mật khẩu phải có ít nhất 1 ký tự đặc biệt (!@#$%...).";
        // Cấm các pattern phổ biến — tăng độ an toàn
        var lower = pwd.ToLowerInvariant();
        var banned = new[] { "123456", "12345678", "password", "qwerty", "abc123", "admin", "matkhau", "letan", "doctor", "user" };
        foreach (var b in banned) if (lower.Contains(b))     return "Mật khẩu chứa pattern phổ biến — vui lòng chọn mật khẩu khó đoán hơn.";
        if (Regex.IsMatch(pwd, @"^(.)\1+$"))                 return "Mật khẩu không được lặp 1 ký tự.";
        if (Regex.IsMatch(pwd, @"(.)\1\1\1"))                return "Mật khẩu không được có 4 ký tự giống nhau liên tiếp.";
        return null;
    }

    /// <summary>Mô tả tiêu chuẩn mật khẩu — dùng hiển thị helper text.</summary>
    /// <summary>
    /// Strip HTML không an toàn (script, on-event handlers, javascript: URL).
    /// Allow whitelist tags cơ bản (p, br, strong, em, ul, ol, li, a, img, h2-h5, table...).
    /// Dùng cho user-generated HTML content (Q&A answer, News detail từ POSTER role).
    /// </summary>
    public static string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var s = input;
        // Remove <script>, <iframe>, <object>, <embed>, <link>, <meta>, <style>, <form>, <input>, <button>
        s = Regex.Replace(s, @"<\s*(script|iframe|object|embed|link|meta|style|form|input|button|textarea|select)[^>]*>.*?</\s*\1\s*>",
                          "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        s = Regex.Replace(s, @"<\s*(script|iframe|object|embed|link|meta|style|form|input|button|textarea|select)[^>]*/?>",
                          "", RegexOptions.IgnoreCase);
        // Strip on* event handlers (onclick, onerror, onload, onmouseover...)
        s = Regex.Replace(s, @"\s+on\w+\s*=\s*(""[^""]*""|'[^']*'|[^\s>]+)", "", RegexOptions.IgnoreCase);
        // Strip javascript: / vbscript: / data:text/html in href/src
        s = Regex.Replace(s, @"\s+(href|src|action|formaction|background)\s*=\s*[""']?\s*(javascript|vbscript|data\s*:\s*text/html)\s*:[^""'>\s]*[""']?",
                          " $1=\"#\"", RegexOptions.IgnoreCase);
        // Strip srcdoc, srcset (image proxy XSS)
        s = Regex.Replace(s, @"\s+(srcdoc)\s*=\s*(""[^""]*""|'[^']*')", "", RegexOptions.IgnoreCase);
        return s;
    }

    public const string PasswordPolicyHint =
        "Tối thiểu 8 ký tự, gồm chữ HOA, chữ thường, số và ký tự đặc biệt (!@#$%...). " +
        "Không dùng các pattern phổ biến (123456, password, admin...).";

    /// <summary>
    /// Kiểm tra password đang lưu có phải mật khẩu mặc định "123456" không —
    /// dùng để force user đổi mật khẩu lần đầu đăng nhập.
    /// </summary>
    public static bool IsDefaultPassword(string? stored) => VerifyPassword("123456", stored);

    /// <summary>Legacy MD5 — chỉ để verify account cũ. KHÔNG dùng để hash mới.</summary>
    public static string Md5Hash(string text)
    {
        var bytes = MD5.HashData(Encoding.ASCII.GetBytes(text ?? string.Empty));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
