import { test, expect } from '@playwright/test';
import { loginStaff } from '../fixtures/auth';

test('preview: trang Detail có panel Phân phòng khám', async ({ page }) => {
  await loginStaff(page, 'letan');
  await page.goto('http://localhost:5050/le-tan/lich-hen?status=pending');
  await page.waitForLoadState('networkidle');

  const detailLinks = page.locator('a[href*="/le-tan/lich-hen/"]');
  const count = await detailLinks.count();
  if (count === 0) test.skip(true, 'Không có pending appointment để smoke');

  await detailLinks.first().click();
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500);
  await page.screenshot({ path: 'screenshots/letan-detail-with-rooms.png', fullPage: true });

  const roomPanel = page.locator('#panel-assign-room');
  await expect(roomPanel).toBeVisible();
  const roomCards = page.locator('.km-room-card');
  const cardCount = await roomCards.count();
  expect(cardCount).toBeGreaterThan(5); // phải có ít nhất 6 cards (7 rooms + 1 skip)
});
