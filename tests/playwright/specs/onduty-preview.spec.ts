import { test } from '@playwright/test';
import { loginStaff } from '../fixtures/auth';

test('preview: trang bac-si-truc với ngày Mon-Fri có BS', async ({ page }) => {
  await loginStaff(page, 'letan');

  // Pick một thứ Hai gần nhất kể từ mai để có BS trực
  const monday = new Date();
  monday.setDate(monday.getDate() + 1);
  while (monday.getDay() !== 1) monday.setDate(monday.getDate() + 1);
  const dateStr = monday.toISOString().split('T')[0];

  await page.goto(`http://localhost:5050/le-tan/bac-si-truc?date=${dateStr}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: 'screenshots/onduty-monday.png', fullPage: true });
});

test('preview: trang bac-si-truc Sunday — empty state', async ({ page }) => {
  await loginStaff(page, 'letan');
  const sunday = new Date();
  sunday.setDate(sunday.getDate() + 1);
  while (sunday.getDay() !== 0) sunday.setDate(sunday.getDate() + 1);
  const dateStr = sunday.toISOString().split('T')[0];
  await page.goto(`http://localhost:5050/le-tan/bac-si-truc?date=${dateStr}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: 'screenshots/onduty-sunday.png', fullPage: true });
});
