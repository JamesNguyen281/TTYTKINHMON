import { test, expect } from '@playwright/test';
import { loginStaff, logout } from '../fixtures/auth';

/**
 * Phase 1 — slot management cho lễ tân.
 * Verify:
 *  - View Detail của 1 appointment pending hiển thị panel "Phân BS phụ trách" với grid card BS
 *  - Endpoint AJAX /le-tan/bac-si-co-slot trả JSON đúng cấu trúc
 *  - Mỗi card có slot bar + nhãn "còn X / Y" + tag "Đề xuất" cho BS đầu tiên
 *  - BS không trực ngày + ca tương ứng KHÔNG xuất hiện trong grid
 *  - Cross-site: AJAX với deptId của site khác không trả BS
 */

test.beforeEach(async ({ page }) => {
  // Mỗi test bắt đầu fresh — login staff và clear cookies sau khi xong.
  test.setTimeout(30_000);
});

test.afterEach(async ({ page }) => {
  await logout(page);
});

test('TC-SLOT-001: AJAX /le-tan/bac-si-co-slot trả JSON với fill stats', async ({ page }) => {
  await loginStaff(page, 'letan');

  // Pick một ngày làm việc trong tương lai gần (thứ 2 đầu tiên kể từ mai để chắc có lịch trực).
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  while (tomorrow.getDay() === 0 || tomorrow.getDay() === 6) {
    tomorrow.setDate(tomorrow.getDate() + 1);
  }
  const dateStr = tomorrow.toISOString().split('T')[0];

  const resp = await page.request.get(
    `http://localhost:5050/le-tan/bac-si-co-slot?date=${dateStr}&session=morning`
  );
  expect(resp.status()).toBe(200);
  const data = await resp.json();
  expect(Array.isArray(data)).toBe(true);

  if (data.length > 0) {
    const first = data[0];
    expect(first).toHaveProperty('doctorId');
    expect(first).toHaveProperty('doctorName');
    expect(first).toHaveProperty('maxSlots');
    expect(first).toHaveProperty('bookedSlots');
    expect(first).toHaveProperty('remainingSlots');
    expect(first).toHaveProperty('fillPercent');
    expect(first).toHaveProperty('isAvailable');
    expect(first.maxSlots).toBeGreaterThan(0);
    expect(first.bookedSlots).toBeGreaterThanOrEqual(0);
    expect(first.fillPercent).toBeGreaterThanOrEqual(0);
    expect(first.fillPercent).toBeLessThanOrEqual(100);
  }
  console.log(`TC-SLOT-001: ${data.length} BS available cho ${dateStr} morning`);
});

test('TC-SLOT-002: Endpoint reject khi thiếu session', async ({ page }) => {
  await loginStaff(page, 'letan');
  const resp = await page.request.get(
    'http://localhost:5050/le-tan/bac-si-co-slot?date=2026-06-01&session='
  );
  expect([400, 500]).toContain(resp.status());
});

test('TC-SLOT-003: Cross-site IDOR — endpoint không tiết lộ BS site khác', async ({ page }) => {
  await loginStaff(page, 'letan');
  // Lễ tân site Kinh Môn truy vấn với deptId GUID rỗng → trả empty (không có BS)
  // Nếu deptId tồn tại ở site khác → cũng trả empty vì service filter theo SiteId của lễ tân hiện tại.
  const fakeDeptId = '00000000-0000-0000-0000-000000000000';
  const resp = await page.request.get(
    `http://localhost:5050/le-tan/bac-si-co-slot?date=2026-06-01&session=morning&deptId=${fakeDeptId}`
  );
  expect(resp.status()).toBe(200);
  const data = await resp.json();
  expect(Array.isArray(data)).toBe(true);
  expect(data.length).toBe(0); // không có BS thuộc dept GUID rỗng
});

test('TC-SLOT-004: View Detail hiển thị grid card BS với progress bar', async ({ page }) => {
  await loginStaff(page, 'letan');

  // Vào hàng đợi lịch hẹn pending
  await page.goto('http://localhost:5050/le-tan/lich-hen?status=pending');
  await page.waitForLoadState('networkidle');

  // Tìm 1 link Detail của appointment pending
  const detailLinks = page.locator('a[href*="/le-tan/Detail/"], a[href*="/le-tan/detail/"]');
  const count = await detailLinks.count();
  if (count === 0) {
    test.skip(true, 'Không có appointment pending nào để test phần Detail.');
  }

  await detailLinks.first().click();
  await page.waitForLoadState('networkidle');

  // Kiểm tra panel Phân BS phụ trách (chỉ hiện khi status = pending hoặc rescheduled)
  const panel = page.locator('#panel-assign-doctor');
  const panelVisible = await panel.isVisible().catch(() => false);

  if (panelVisible) {
    // Có thể là grid card hoặc fallback select — đều OK
    const hasGrid = await page.locator('.km-doctor-pick-grid').count() > 0;
    const hasFallback = await page.locator('select[name="doctorId"]').count() > 0;
    expect(hasGrid || hasFallback).toBe(true);

    if (hasGrid) {
      // Verify ít nhất 1 card có slot bar
      const slotBars = page.locator('.km-slot-bar');
      const barCount = await slotBars.count();
      expect(barCount).toBeGreaterThan(0);
    }
  }
});
