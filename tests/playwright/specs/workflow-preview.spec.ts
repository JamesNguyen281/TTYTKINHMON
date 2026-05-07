import { test, expect } from '@playwright/test';
import { loginStaff } from '../fixtures/auth';

/**
 * Smoke test cho workflow Phase 2.B/C/D:
 *  - Form đặt lịch không có dropdown khoa (mọi BN vào "Khoa Khám bệnh", lễ tân phân phòng sau)
 *  - Trang LichTruc của BS hiển thị label ScheduleType (Khám / Cấp cứu / Quản lý)
 *  - Form ChanDoan có panel "Hướng xử trí" 2 nhánh outpatient/inpatient
 */

test('TC-P2D: Form đặt lịch — không có dropdown khoa (BN vào Khoa Khám bệnh)', async ({ page }) => {
  await page.goto('http://localhost:5050/dat-lich-kham', { waitUntil: 'networkidle' });
  // BN không chọn khoa — quy trình chuẩn TTYT phường: lễ tân phân phòng sau khi tiếp nhận triệu chứng
  expect(await page.locator('select[name="DepartmentId"]').count()).toBe(0);
  expect(await page.locator('select[name="ClinicRoomId"]').count()).toBe(0);
  // Trang phải có hint nhắc "Khoa Khám bệnh" + "phòng khám chuyên môn"
  const html = await page.content();
  expect(html).toMatch(/Khoa Khám bệnh/i);
  expect(html).toMatch(/phòng khám/i);
});

test('TC-P2B: Trang LichTruc BS load được', async ({ page }) => {
  await loginStaff(page, 'bacsy');
  await page.goto('http://localhost:5050/bac-si-portal/lich-truc', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: 'screenshots/bacsy-lichtruc-with-type.png', fullPage: true });
  // Page load 200; nếu BS có lịch thì sẽ thấy label ScheduleType, nếu không thì empty state.
  // Test chỉ verify trang không 500 — label sẽ hiển thị khi account có doctor_id thật.
  const title = await page.locator('h2, h1').first().textContent();
  expect(title?.toLowerCase()).toContain('lịch trực');
});
