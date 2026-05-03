import { test, expect } from '@playwright/test';

test.describe.configure({ mode: 'serial' });

test('modal doctor — desktop preview', async ({ page }) => {
  await page.setViewportSize({ width: 1366, height: 768 });
  await page.goto('http://localhost:5050/', { waitUntil: 'networkidle' });
  const btn = page.locator('.btn-doctor-detail').first();
  await btn.scrollIntoViewIfNeeded();
  await btn.click();
  await page.waitForSelector('#detaildoctor1.in', { timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(800);
  await page.screenshot({ path: 'screenshots/modal-doctor-desktop.png', fullPage: false });
});

test('modal doctor — mobile preview (iPhone X)', async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
  await page.goto('http://localhost:5050/', { waitUntil: 'networkidle' });
  const btn = page.locator('.btn-doctor-detail').first();
  await btn.scrollIntoViewIfNeeded();
  await btn.click();
  await page.waitForSelector('#detaildoctor1.in', { timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(800);
  await page.screenshot({ path: 'screenshots/modal-doctor-mobile.png', fullPage: false });
});

test('modal doctor — scroll reset khi switch BS', async ({ page }) => {
  await page.setViewportSize({ width: 1366, height: 768 });
  await page.goto('http://localhost:5050/', { waitUntil: 'networkidle' });
  const btns = page.locator('.btn-doctor-detail');
  const count = await btns.count();
  if (count < 2) test.skip();

  // Open BS đầu tiên
  await btns.nth(0).scrollIntoViewIfNeeded();
  await btns.nth(0).click();
  await page.waitForTimeout(600);
  // Cuộn xuống cuối bên phải
  const scroll1 = page.locator('#detaildoctor1 .km-doc-scroll').first();
  await scroll1.evaluate(el => { el.scrollTop = el.scrollHeight; });
  const scrollAfterFirst = await scroll1.evaluate(el => el.scrollTop);
  expect(scrollAfterFirst).toBeGreaterThan(0);

  // Đóng modal
  await page.locator('#detaildoctor1 .km-modal-close').click();
  await page.waitForTimeout(400);

  // Mở BS thứ hai
  await btns.nth(1).click();
  await page.waitForTimeout(600);

  // Scroll phải reset về 0
  const scroll2 = page.locator('#detaildoctor1 .km-doc-scroll').first();
  const scrollAfterSecond = await scroll2.evaluate(el => el.scrollTop);
  expect(scrollAfterSecond).toBe(0);
});
