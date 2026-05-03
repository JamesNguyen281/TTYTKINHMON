import { test, expect } from '@playwright/test';
import { loginStaff } from '../fixtures/auth';

/** P3.A — AdminCP ClinicRoom CRUD smoke. */

test('TC-P3A-1: AdminCP ClinicRooms Index list 7 phòng', async ({ page }) => {
  await loginStaff(page, 'admin');
  await page.goto('http://localhost:5050/AdminCP/ClinicRooms', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  await page.screenshot({ path: 'screenshots/admincp-clinicrooms.png', fullPage: true });
  // Phải có ít nhất 6 hàng (7 phòng active sau seed)
  const rows = page.locator('table tbody tr');
  const count = await rows.count();
  expect(count).toBeGreaterThan(5);
});

test('TC-P3A-2: AdminCP ClinicRooms/Create form load', async ({ page }) => {
  await loginStaff(page, 'admin');
  await page.goto('http://localhost:5050/AdminCP/ClinicRooms/Create', { waitUntil: 'networkidle' });
  await expect(page.locator('input[name="RoomCode"]')).toBeVisible();
  await expect(page.locator('input[name="RoomName"]')).toBeVisible();
  await expect(page.locator('select[name="DepartmentId"]')).toBeVisible();
});

test('TC-P3A-3: Default dashboard có card Phòng khám', async ({ page }) => {
  await loginStaff(page, 'admin');
  await page.goto('http://localhost:5050/AdminCP', { waitUntil: 'networkidle' });
  const html = await page.content();
  expect(html).toContain('Phòng khám');
});
