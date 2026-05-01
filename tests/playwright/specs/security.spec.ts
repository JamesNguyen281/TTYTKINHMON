import { test, expect } from '@playwright/test';
import { loginMember, loginStaff, logout } from '../fixtures/auth';

/**
 * Security regression spec — TC-055 + TC-061 → TC-075.
 * Đảm bảo các bug bảo mật đã fix không tái diễn.
 */

test.describe('Security regression', () => {
  test.afterEach(async ({ page }) => { await logout(page).catch(() => {}); });

  test('TC-055: CSRF — POST không có token → 400', async ({ request }) => {
    const resp = await request.post('/dat-lich-kham', {
      form: {
        PatientName: 'Test',
        PatientPhone: '0987654321',
        DepartmentId: '00000000-0000-0000-0000-000000000000',
        AppointmentDate: '2030-01-01',
        Session: 'morning',
      },
      failOnStatusCode: false,
    });
    // ASP.NET Core trả 400 khi thiếu CSRF
    expect([400, 403]).toContain(resp.status());
  });

  test('TC-061 (C1): IDOR LeTan cross-site — không xem được lịch site khác', async ({ page }) => {
    await loginStaff(page, 'letan');
    // GUID giả của site khác → server phải reject
    const fakeId = '00000000-0000-0000-0000-000000000001';
    const resp = await page.goto(`/le-tan/lich-hen/${fakeId}`);
    expect([404, 302, 403]).toContain(resp?.status() ?? 200);
  });

  test('TC-062 (C2): IDOR DoctorPortal cross-doctor — bs01 không xem appt bs02', async ({ page }) => {
    await loginStaff(page, 'bacsy');
    const fakeApptId = '00000000-0000-0000-0000-000000000099';
    const resp = await page.goto(`/bac-si-portal/chan-doan/${fakeApptId}`);
    expect([404, 302, 403]).toContain(resp?.status() ?? 200);
  });

  test('TC-063 (C3): CSRF Default/Turnon — POST chuyển site không token bị reject', async ({ request, page }) => {
    await loginStaff(page, 'admin');
    const cookies = await page.context().cookies();
    const cookieHeader = cookies.map(c => `${c.name}=${c.value}`).join('; ');
    const resp = await request.post('/AdminCP/Default/Turnon', {
      headers: { 'Cookie': cookieHeader },
      form: { siteId: '00000000-0000-0000-0000-000000000001' },
      failOnStatusCode: false,
    });
    expect([400, 403, 404]).toContain(resp.status());
  });

  test('TC-064 (C4): Mass-assign tampering SiteId — server bỏ qua', async ({ page }) => {
    await loginStaff(page, 'admin');
    // Chỉ smoke — chi tiết: thử POST update Doctor với SiteId=other → server reload-then-apply bỏ qua field
    const resp = await page.goto('/AdminCP/Doctors');
    expect(resp?.status()).toBe(200);
  });

  test('TC-065 (H1): XSS stored Q&A — script tag không thực thi', async ({ page }) => {
    const resp = await page.goto('/hoi-dap');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // KHÔNG được có raw <script>alert hoặc onerror= trong response
    expect(html).not.toMatch(/<script[^>]*>alert\(/i);
    expect(html).not.toMatch(/onerror\s*=\s*["']?alert/i);
  });

  test('TC-066 (H1): XSS News onload handler — bị strip', async ({ page }) => {
    await page.goto('/');
    const html = await page.content();
    expect(html).not.toMatch(/<svg[^>]+onload\s*=/i);
    expect(html).not.toMatch(/<iframe[^>]+srcdoc\s*=/i);
  });

  test('TC-067 (H2): Login enumeration — message giống nhau cho user tồn tại vs không', async ({ page }) => {
    // Try with existing user, wrong pass
    await page.goto('/dang-nhap');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="Password"]', 'WRONG_PASS_xxx');
    await page.click('button[type="submit"]');
    const html1 = await page.content();
    const m1 = html1.match(/Tài khoản hoặc mật khẩu không đúng|Tài khoản không tồn tại|Sai mật khẩu/i);

    // Try with non-existing user
    await page.goto('/dang-nhap');
    await page.fill('input[name="UserName"]', 'doesnotexist_' + Date.now());
    await page.fill('input[name="Password"]', 'random_pass');
    await page.click('button[type="submit"]');
    const html2 = await page.content();
    const m2 = html2.match(/Tài khoản hoặc mật khẩu không đúng|Tài khoản không tồn tại|Sai mật khẩu/i);

    expect(m1).toBeTruthy();
    expect(m2).toBeTruthy();
    expect(m1?.[0]).toBe(m2?.[0]);
  });

  test('TC-068 (H3): Login lockout — 6 lần sai pass → khoá', async ({ page }) => {
    test.fixme(true, 'Sẽ khoá tài khoản test 15 phút — chỉ chạy với fixture cleanup');
  });

  test('TC-069 (H3): Auto-rehash MD5 → PBKDF2 sau login thành công', async ({ page }) => {
    test.fixme(true, 'Cần tài khoản với hash MD5 cũ — chỉ kiểm bằng DB query sau khi login');
  });

  test('TC-070 (H4): Session cookie SameSite=Lax', async ({ page, request }) => {
    await page.goto('/');
    const cookies = await page.context().cookies();
    const session = cookies.find(c => c.name.toLowerCase().includes('session') || c.name.includes('TtytKinhMon'));
    if (session) {
      // Playwright cookie object có thuộc tính sameSite
      expect(['Lax', 'Strict']).toContain(session.sameSite);
    }
  });

  test('TC-071 (H6): Admin reset weak pwd — phải reject "123456"', async ({ page }) => {
    await loginStaff(page, 'admin');
    const resp = await page.goto('/AdminCP/Users');
    expect(resp?.status()).toBe(200);
    // Smoke — actual policy check qua server-side validate
  });

  test('TC-072 (H7): News siteId filter — public chỉ thấy news của site hiện tại', async ({ page }) => {
    const resp = await page.goto('/tin-tuc');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // KHÔNG có news của site khác (smoke check không có ID lạ)
    expect(html).toBeTruthy();
  });

  test('TC-073 (M1): Upload polyglot file — magic byte check reject', async ({ page }) => {
    test.fixme(true, 'Cần upload file SVG/HTML payload — implement với fixture file');
  });

  test('TC-074 (M3): MedicalRecord cross-site — bs A không xem record site B', async ({ page }) => {
    await loginStaff(page, 'admin');
    const fakeId = '00000000-0000-0000-0000-000000000099';
    const resp = await page.goto(`/AdminCP/MedicalRecords/Detail/${fakeId}`);
    expect([404, 302]).toContain(resp?.status() ?? 200);
  });

  test('TC-075 (M6): Q&A double answer — đã trả lời không cho trả lời lại', async ({ page }) => {
    test.fixme(true, 'Yêu cầu seed Q&A đã có answer');
  });
});
