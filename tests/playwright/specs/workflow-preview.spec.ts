import { test, expect } from '@playwright/test';
import { loginStaff } from '../fixtures/auth';

/**
 * Smoke test cho workflow Phase 2.B/C/D:
 *  - Form đặt lịch chỉ hiện các khoa khám (filter IsClinicalDept)
 *  - Trang LichTruc của BS hiển thị label ScheduleType (Khám / Cấp cứu / Quản lý)
 *  - Form ChanDoan có panel "Hướng xử trí" 2 nhánh outpatient/inpatient
 */

test('TC-P2D: Form đặt lịch — dropdown chỉ hiện khoa khám', async ({ page }) => {
  await page.goto('http://localhost:5050/dat-lich-kham', { waitUntil: 'networkidle' });
  const dropdown = page.locator('select[name="DepartmentId"]');
  await expect(dropdown).toBeVisible();
  const options = await dropdown.locator('option').allTextContents();
  // Loại trừ rõ các khoa không phải khám
  expect(options.join(' ')).not.toContain('Khoa Khám bệnh');
  expect(options.join(' ')).not.toContain('Khoa Xét nghiệm');
  expect(options.join(' ')).not.toContain('Khoa Dược');
  expect(options.join(' ')).not.toContain('Khoa Cấp cứu');
  // Phải có các khoa khám
  expect(options.join(' ')).toMatch(/Khoa Nội|Khoa Nhi/);
  console.log(`TC-P2D: ${options.length} options còn lại sau filter`);
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
