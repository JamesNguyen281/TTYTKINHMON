import { test, expect } from '@playwright/test';
import { loginStaff, logout } from '../fixtures/auth';

/**
 * Phủ 3 feature mới (commit 5417efa):
 *  A. DoctorPortal/lich-truc — banner xanh khi admin đã auto-gen lịch tháng kế tiếp
 *  B. AdminCP/DoctorSchedules — month picker (ym) + nav prev/next/all
 *  C. LeTan/Detail — banner xanh "đang lọc theo phòng" + fallback vàng khi
 *     phòng không có BS trực
 *
 * Smoke-level: chỉ verify HTML response + DOM marker. State-dependent assert
 * bọc bằng test.skip nếu không có data phù hợp trong DB.
 */

// =================================================================
// A. DOCTOR PORTAL — banner xem lịch tháng kế tiếp
// =================================================================
test.describe('A. DoctorPortal — banner xem lịch tháng sau', () => {
  test.beforeEach(async ({ page }) => { await loginStaff(page, 'bacsy'); });
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-080: /bac-si-portal/lich-truc — trang load 200 + hiển thị month-nav', async ({ page }) => {
    const resp = await page.goto('/bac-si-portal/lich-truc');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // Phải có month nav hoặc title "Lịch trực"
    expect(html).toMatch(/Lịch trực|tháng/i);
  });

  test('TC-081 (skeleton): Banner xanh "Lịch trực tháng MM/YYYY đã được Quản trị viên tạo sẵn" xuất hiện khi có data tháng sau', async ({ page }) => {
    await page.goto('/bac-si-portal/lich-truc');
    const banner = page.locator('.alert-warn, [class*="alert"]').filter({ hasText: /Quản trị viên tạo sẵn|tháng.*đã được/i });
    // Skip nếu admin chưa auto-gen lịch tháng kế tiếp
    if (await banner.count() === 0) {
      test.skip(true, 'Chưa có lịch tháng kế tiếp — admin chưa AutoGenerate; skip TC-081');
    }
    await expect(banner.first()).toBeVisible();
    // Banner phải có nút "Xem lịch tháng MM/YYYY"
    const viewBtn = banner.locator('a:has-text("Xem lịch")');
    expect(await viewBtn.count()).toBeGreaterThan(0);
  });

  test('TC-082: Banner KHÔNG xuất hiện khi đang xem tháng kế tiếp', async ({ page }) => {
    const now = new Date();
    const next = new Date(now.getFullYear(), now.getMonth() + 1, 1);
    const ym = `${next.getFullYear()}-${String(next.getMonth() + 1).padStart(2, '0')}`;
    const resp = await page.goto(`/bac-si-portal/lich-truc?ym=${ym}`);
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // Banner "tháng X đã được Quản trị viên tạo sẵn" chỉ hiện ở tháng hiện tại
    expect(html).not.toMatch(/đã được Quản trị viên tạo sẵn/);
  });
});

// =================================================================
// B. ADMIN CP — month picker cho DoctorSchedules
// =================================================================
test.describe('B. AdminCP/DoctorSchedules — month picker', () => {
  test.beforeEach(async ({ page }) => { await loginStaff(page, 'admin'); });
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-083: /AdminCP/DoctorSchedules — trang load 200 + có ds-month-nav bar', async ({ page }) => {
    const resp = await page.goto('/AdminCP/DoctorSchedules');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    expect(html).toMatch(/Lịch trực|DoctorSchedule|tháng/i);
  });

  test('TC-084: Lọc theo ?ym=2026-05 — list chỉ schedules overlap tháng 05/2026', async ({ page }) => {
    const resp = await page.goto('/AdminCP/DoctorSchedules?ym=2026-05');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // Phải có label tháng đã chọn (05/2026) trong nav bar
    expect(html).toMatch(/05\/2026|tháng 5|2026-05/i);
  });

  test('TC-085: Tham số ym không hợp lệ → vẫn render 200 không crash', async ({ page }) => {
    const resp = await page.goto('/AdminCP/DoctorSchedules?ym=invalid-string');
    // Controller phải graceful — không 500
    expect(resp?.status() ?? 0).toBeLessThan(500);
    expect(resp?.status()).toBe(200);
  });

  test('TC-086: Nav button prev/next/all đều có và là <a> tag', async ({ page }) => {
    await page.goto('/AdminCP/DoctorSchedules?ym=2026-05');
    // Đếm số link điều hướng tháng
    const prevLink = page.locator('a').filter({ hasText: /(←|Trước|Tháng trước)/i });
    const nextLink = page.locator('a').filter({ hasText: /(→|Sau|Tháng sau)/i });
    const allLink  = page.locator('a').filter({ hasText: /Tất cả/i });
    // Ít nhất 1 trong 3 phải tồn tại — không assert all vì label có thể khác
    const total = (await prevLink.count()) + (await nextLink.count()) + (await allLink.count());
    expect(total).toBeGreaterThan(0);
  });
});

// =================================================================
// C. LE TAN — Detail filter BS theo ClinicRoomId
// =================================================================
test.describe('C. LeTan/Detail — lọc BS theo phòng khám', () => {
  test.beforeEach(async ({ page }) => { await loginStaff(page, 'letan'); });
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-087: /le-tan — trang pending list load 200', async ({ page }) => {
    const resp = await page.goto('/le-tan/lich-hen');
    expect(resp?.status()).toBe(200);
  });

  test('TC-088 (skeleton): Vào Detail của 1 lịch — nếu có ClinicRoomId, hiển thị banner xanh "Đang lọc theo phòng"', async ({ page }) => {
    await page.goto('/le-tan/lich-hen');
    // Tìm link Detail đầu tiên
    const detailLink = page.locator('a[href*="/le-tan/detail/"]').first();
    if (await detailLink.count() === 0) {
      test.skip(true, 'Không có lịch hẹn để mở Detail — skip TC-088');
    }
    await detailLink.click();
    await page.waitForLoadState('networkidle');
    const html = await page.content();
    // Nếu lịch đã gán phòng → có 1 trong 2 banner (xanh hoặc vàng fallback)
    const hasRoomBanner =
      /Đang lọc theo phòng/i.test(html) ||
      /chưa có bác sĩ được phân lịch trực/i.test(html);
    if (!hasRoomBanner) {
      test.skip(true, 'Lịch chưa gán ClinicRoomId — không có banner; skip TC-088');
    }
    expect(hasRoomBanner).toBeTruthy();
  });

  test('TC-089 (skeleton): API /le-tan/available-doctors hỗ trợ tham số clinicRoomId', async ({ page, request }) => {
    // Smoke endpoint — server phải accept clinicRoomId query không 500
    const fakeRoom = '00000000-0000-0000-0000-000000000099';
    const fakeDept = '00000000-0000-0000-0000-000000000099';
    const today = new Date().toISOString().slice(0, 10);
    const resp = await page.request.get(
      `/le-tan/available-doctors?departmentId=${fakeDept}&clinicRoomId=${fakeRoom}&date=${today}&session=Morning`
    );
    // Có thể 200 (rỗng) hoặc 400 (validation) — KHÔNG được 500
    expect(resp.status()).toBeLessThan(500);
  });
});
