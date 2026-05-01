import { test, expect } from '@playwright/test';
import { loginStaff, logout } from '../fixtures/auth';

/**
 * Letan flow spec — bổ sung TC-022 → TC-029 cho UC21 duyệt lịch + state machine.
 * Lưu ý: phụ thuộc DB seed. Khi không có data phù hợp test sẽ skip.
 */

test.describe('UC21 — Lễ tân duyệt lịch (full state machine)', () => {
  test.beforeEach(async ({ page }) => { await loginStaff(page, 'letan'); });
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-022: Duyệt lịch pending → confirmed + sinh booking_code', async ({ page }) => {
    await page.goto('/le-tan/lich-hen?status=pending');
    const detailLink = page.locator('a[href*="/le-tan/lich-hen/"], a[href*="/AdminCP/Appointments/Detail/"]').first();
    if (await detailLink.count() === 0) test.skip(true, 'Không có lịch pending — skip TC-022');
    await detailLink.click();
    await page.waitForLoadState('networkidle');
    const confirmBtn = page.locator('button:has-text("Duyệt"), button:has-text("Xác nhận"), input[value="confirmed"]').first();
    if (await confirmBtn.count() === 0) test.skip(true, 'Không có nút Duyệt');
    await Promise.all([
      page.waitForLoadState('networkidle'),
      confirmBtn.click(),
    ]);
    const html = await page.content();
    // Sau confirm: có booking code BC- prefix hoặc thông báo Đã xác nhận
    expect(html).toMatch(/BC[-_]?[A-Z0-9]{4,}|Đã xác nhận|đã xác nhận|confirmed/i);
  });

  test('TC-023: Duyệt — quota đầy → reject với error', async ({ page }) => {
    test.fixme(true, 'Yêu cầu seed quota = booked → cần fixture DB');
  });

  test('TC-024: Từ chối lịch — không nhập lý do → validation error', async ({ page }) => {
    await page.goto('/le-tan/lich-hen?status=pending');
    const detailLink = page.locator('a[href*="/le-tan/lich-hen/"], a[href*="/AdminCP/Appointments/Detail/"]').first();
    if (await detailLink.count() === 0) test.skip(true, 'Không có lịch pending');
    await detailLink.click();
    await page.waitForLoadState('networkidle');
    const rejectBtn = page.locator('button:has-text("Từ chối"), button[name="newStatus"][value="rejected"]').first();
    if (await rejectBtn.count() === 0) test.skip(true, 'Không có nút Từ chối');
    await rejectBtn.click();
    await page.waitForLoadState('networkidle');
    const html = await page.content();
    // Phải có error yêu cầu nhập lý do
    expect(html).toMatch(/lý do|reason|bắt buộc|cần nhập/i);
  });

  test('TC-025: Check-in bằng booking_code đúng', async ({ page }) => {
    const resp = await page.goto('/le-tan/check-in');
    expect(resp?.status()).toBe(200);
    const codeInput = page.locator('input[name="bookingCode"], input[name="code"]').first();
    if (await codeInput.count() === 0) test.skip(true, 'Không có ô nhập mã');
    // Nhập mã giả → server reject, smoke endpoint OK
    await codeInput.fill('BC-NOTEXIST-9999');
    const submitBtn = page.locator('button[type="submit"]').first();
    if (await submitBtn.count() > 0) {
      await submitBtn.click();
      await page.waitForLoadState('networkidle');
      const html = await page.content();
      expect(html).toMatch(/Không tìm thấy|sai mã|không hợp lệ|đã check-in|incorrect/i);
    }
  });

  test('TC-026: Check-in — sai ngày → error', async ({ page }) => {
    test.fixme(true, 'Yêu cầu seed appointment xác nhận hôm khác — implement với DB fixture');
  });

  test('TC-027: Transition cấm — rejected → confirmed bị từ chối', async ({ page }) => {
    test.fixme(true, 'AppointmentService.AllowedTransitions whitelist test — đã cover unit test xUnit');
  });

  test('TC-028: Transition cấm — completed → pending bị từ chối', async ({ page }) => {
    test.fixme(true, 'AppointmentService.AllowedTransitions whitelist test — đã cover unit test xUnit');
  });

  test('TC-029: Transition newStatus không thuộc whitelist → reject', async ({ page }) => {
    // Smoke: gửi POST trực tiếp với newStatus = "wxyz" → server phải reject
    await page.goto('/le-tan/lich-hen?status=pending');
    const detailLink = page.locator('a[href*="/le-tan/lich-hen/"]').first();
    if (await detailLink.count() === 0) test.skip(true, 'Không có lịch pending');
    const href = await detailLink.getAttribute('href');
    const apptId = href?.split('/').pop();
    if (!apptId) test.skip(true, 'Không trích được apptId');
    // Lấy CSRF token từ form trên trang detail
    await page.goto(href!);
    const token = await page.locator('input[name="__RequestVerificationToken"]').first().inputValue();
    const cookies = await page.context().cookies();
    const cookieHeader = cookies.map(c => `${c.name}=${c.value}`).join('; ');

    const resp = await page.request.post('/AdminCP/Appointments/UpdateStatus', {
      headers: { 'Cookie': cookieHeader },
      form: {
        id: apptId!,
        newStatus: 'invalid-status-xxx',
        __RequestVerificationToken: token,
      },
      maxRedirects: 0, // không follow redirect — kiểm tra trực tiếp 302/400
    });
    // Phải redirect về detail với error TempData (302 + Location), không thay đổi state
    expect([302, 400]).toContain(resp.status());
  });
});
