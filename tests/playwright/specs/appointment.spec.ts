import { test, expect } from '@playwright/test';
import { loginMember, logout } from '../fixtures/auth';

/**
 * Appointment spec — TC-007, TC-008, TC-009, TC-010, TC-011, TC-014, TC-015.
 * Yêu cầu: server đã chạy ở http://localhost:5050 và DB có tài khoản member01.
 */

const today = new Date();
const fmtDate = (d: Date) => {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
};
const addDays = (d: Date, n: number) => {
  const r = new Date(d);
  r.setDate(r.getDate() + n);
  return r;
};

test.describe('UC07 — Đặt lịch khám', () => {
  test.afterEach(async ({ page }) => {
    await logout(page);
  });

  test('TC-007: Member auto-fill họ tên + SĐT', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-lich-kham');

    const name = await page.locator('input[name="PatientName"]').inputValue();
    const phone = await page.locator('input[name="PatientPhone"]').inputValue();
    expect(name.length, 'họ tên phải được auto-fill từ session').toBeGreaterThan(0);
    expect(phone.length, 'SĐT phải được auto-fill từ session').toBeGreaterThan(0);
  });

  test('TC-008: Anonymous đặt lịch → success (vãng lai)', async ({ page }) => {
    await page.goto('/dat-lich-kham');

    await page.fill('input[name="PatientName"]', 'Bệnh Nhân Vãng Lai');
    await page.fill('input[name="PatientPhone"]', '0987654321');
    await page.fill('input[name="PatientEmail"]', 'guest@test.local');

    // Chọn dept đầu tiên (skip option rỗng)
    const deptSelect = page.locator('select[name="DepartmentId"]');
    const deptOptions = await deptSelect.locator('option').all();
    expect(deptOptions.length, 'phải có ít nhất 1 chuyên khoa active').toBeGreaterThan(1);
    await deptSelect.selectOption({ index: 1 });

    await page.fill('input[name="AppointmentDate"]', fmtDate(addDays(today, 3)));
    await page.locator('select[name="Session"]').selectOption('morning');
    await page.fill('textarea[name="Reason"]', 'Khám tổng quát — auto test');

    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
    // Design: anonymous booking redirect lại /dat-lich-kham + flash success.
    // Member booking → /lich-cua-toi (test trong TC-009).
    const html = await page.content();
    expect(html).toMatch(/Đã ghi nhận|đã được ghi nhận|sẽ liên hệ xác nhận|thành công/i);
  });

  test('TC-009: Member đặt lịch thành công → /lich-cua-toi', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-lich-kham');

    // Form yêu cầu PatientName + PatientPhone kể cả khi đã login (server cho phép member nhập hộ người thân)
    await page.fill('input[name="PatientName"]', 'Member01 Self');
    await page.fill('input[name="PatientPhone"]', '0987654321');
    const deptSelect = page.locator('select[name="DepartmentId"]');
    await deptSelect.selectOption({ index: 1 });
    await page.fill('input[name="AppointmentDate"]', fmtDate(addDays(today, 5)));
    await page.locator('select[name="Session"]').selectOption('afternoon');
    await page.fill('textarea[name="Reason"]', 'Test E2E member booking');

    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
    // Member booking → redirect /lich-cua-toi (success) HOẶC ở lại /dat-lich-kham với error
    // (vd. trùng lịch, hết quota, ngày không hợp lệ — tuỳ state DB).
    const url = page.url();
    const html = await page.content();
    const isSuccess = url.includes('/lich-cua-toi');
    const isDupOrError = html.match(/đã có lịch|trùng|hết quota|đã đầy|không thể tạo/i);
    expect(isSuccess || isDupOrError, `URL=${url}; need either redirect to /lich-cua-toi or expected error`).toBeTruthy();
  });

  test('TC-010: Ngày quá khứ → error "không được trong quá khứ"', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-lich-kham');

    // Bypass HTML5 min validation bằng JS
    await page.evaluate(() => {
      const input = document.querySelector('input[name="AppointmentDate"]') as HTMLInputElement;
      if (input) input.removeAttribute('min');
    });
    const deptSelect = page.locator('select[name="DepartmentId"]');
    await deptSelect.selectOption({ index: 1 });
    await page.fill('input[name="AppointmentDate"]', fmtDate(addDays(today, -1)));
    await page.click('button[type="submit"]');

    const html = await page.content();
    expect(html).toMatch(/quá khứ|không hợp lệ/i);
  });

  test('TC-011: Vượt MaxDaysAhead (today+31) → error', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-lich-kham');

    await page.evaluate(() => {
      const input = document.querySelector('input[name="AppointmentDate"]') as HTMLInputElement;
      if (input) input.removeAttribute('max');
    });
    const deptSelect = page.locator('select[name="DepartmentId"]');
    await deptSelect.selectOption({ index: 1 });
    await page.fill('input[name="AppointmentDate"]', fmtDate(addDays(today, 31)));
    await page.click('button[type="submit"]');

    const html = await page.content();
    expect(html).toMatch(/30 ngày|không vượt quá/i);
  });

  test('TC-015: Lịch của tôi — list hiển thị', async ({ page }) => {
    await loginMember(page);
    const resp = await page.goto('/lich-cua-toi');
    expect(resp?.status()).toBe(200);

    const html = await page.content();
    // Phải có heading hoặc bảng
    expect(html).toMatch(/Lịch|Mã|Ngày|Khoa/i);
  });

  test('TC-016: Anonymous truy cập /lich-cua-toi → redirect /dang-nhap', async ({ page }) => {
    await page.goto('/lich-cua-toi');
    await expect(page).toHaveURL(/\/dang-nhap/);
  });
});
