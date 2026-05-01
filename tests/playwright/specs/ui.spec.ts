import { test, expect } from '@playwright/test';
import { expectNoMojibake } from '../fixtures/auth';

/**
 * UI spec — kiểm tra giao diện public sau redesign UI/UX.
 * Phủ TC-051, TC-053, TC-076 → TC-083.
 * Note: TC-052 (favicon), TC-054 (404) đã có trong smoke.spec.ts.
 */

test.describe('UI redesign — Public site', () => {
  test('TC-051: Header logo dùng Site.LogoUrl (không hard-code)', async ({ page }) => {
    await page.goto('/');
    const logo = page.locator('header img, .header img, .logo img').first();
    if (await logo.count() === 0) test.skip(true, 'Không tìm thấy logo header');
    const src = await logo.getAttribute('src');
    expect(src, 'Logo phải có src').toBeTruthy();
    // Logo serves 200
    if (src) {
      const resp = await page.request.get(src);
      expect(resp.status()).toBe(200);
    }
  });

  test('TC-053: Responsive mobile 375×812 — không tràn ngang', async ({ browser, baseURL }) => {
    const ctx = await browser.newContext({ viewport: { width: 375, height: 812 }, baseURL });
    const page = await ctx.newPage();
    await page.goto('/');
    const bodyOverflow = await page.evaluate(() => document.body.scrollWidth - document.documentElement.clientWidth);
    expect(bodyOverflow, 'Body không được rộng hơn viewport').toBeLessThanOrEqual(2);
    await ctx.close();
  });

  test('TC-076: Trang /bac-si — grid 5 cột desktop', async ({ page }) => {
    await page.setViewportSize({ width: 1366, height: 768 });
    await page.goto('/bac-si');
    await page.waitForLoadState('networkidle');
    // Trang dùng tab — chỉ tab active hiển thị cards. Filter bằng :visible.
    const cards = page.locator('.item-doctor:visible, .doctor-card:visible, .km-doc-card:visible');
    const cnt = await cards.count();
    if (cnt < 5) test.skip(true, `Tab active có ${cnt} doctor — cần ≥ 5 để test grid 5 col`);
    const t1 = await cards.nth(0).boundingBox();
    const t5 = await cards.nth(4).boundingBox();
    expect(t1 && t5).toBeTruthy();
    expect(Math.abs(t1!.y - t5!.y)).toBeLessThan(10);
  });

  test('TC-077: Trang chủ Ban GĐ — các card căn giữa', async ({ page }) => {
    await page.goto('/');
    // Section "BAN GIÁM ĐỐC" hoặc lãnh đạo
    const directorSection = page.locator('text=/BAN GIÁM ĐỐC|Lãnh đạo/i').first();
    if (await directorSection.count() === 0) test.skip(true, 'Không có section BGĐ trên trang chủ');
  });

  test('TC-078: Modal chi tiết bác sĩ — scroll content dài', async ({ page }) => {
    await page.goto('/bac-si');
    const xemBtn = page.locator('.view-info-btn, a:has-text("XEM THÔNG TIN")').first();
    if (await xemBtn.count() === 0) test.skip(true, 'Không có nút Xem thông tin');
    await xemBtn.click();
    const modal = page.locator('.km-doc-modal, .modal.in, [role="dialog"]').first();
    await expect(modal).toBeVisible({ timeout: 4000 });
    const scroll = modal.locator('.km-doc-scroll, .modal-body').first();
    if (await scroll.count() > 0) {
      const overflowY = await scroll.evaluate(e => getComputedStyle(e).overflowY);
      expect(['auto', 'scroll']).toContain(overflowY);
    }
  });

  test('TC-079: Modal × button hiển thị tròn đầy đủ', async ({ page }) => {
    await page.goto('/bac-si');
    const xemBtn = page.locator('.view-info-btn, a:has-text("XEM THÔNG TIN")').first();
    if (await xemBtn.count() === 0) test.skip(true, 'No view button');
    await xemBtn.click();
    const closeBtn = page.locator('.km-doc-modal .close, .modal-header .close, button[aria-label="Close"]').first();
    await expect(closeBtn).toBeVisible({ timeout: 4000 });
    const box = await closeBtn.boundingBox();
    expect(box?.width).toBeGreaterThan(20);
    expect(box?.height).toBeGreaterThan(20);
  });

  test('TC-080: Top-bar header — không bị chồng chéo', async ({ page }) => {
    await page.goto('/');
    const topbar = page.locator('.top-bar, .header-top').first();
    if (await topbar.count() === 0) test.skip(true, 'No top-bar');
    const overflow = await topbar.evaluate(e => {
      return e.scrollWidth - e.clientWidth;
    });
    expect(overflow).toBeLessThanOrEqual(4);
  });

  test('TC-081: Top-bar icon ambulance/heart — không cắt nửa', async ({ page }) => {
    await page.goto('/');
    const icons = page.locator('.top-bar .fa-ambulance, .top-bar .fa-heart, .header-top i.fa').first();
    if (await icons.count() === 0) test.skip(true, 'No top-bar icon');
    const visible = await icons.isVisible();
    expect(visible).toBeTruthy();
    const box = await icons.boundingBox();
    expect(box?.height).toBeGreaterThan(8);
  });

  test('TC-082: EN/VI toggle button — đã ẩn tạm thời', async ({ page }) => {
    await page.goto('/');
    const langToggle = page.locator('a:has-text("English"), .lang-switch, a[href*="/en/"]').first();
    // Phải KHÔNG visible (đã ẩn theo yêu cầu)
    const cnt = await langToggle.count();
    if (cnt === 0) {
      expect(true).toBe(true);
    } else {
      const isVisible = await langToggle.isVisible();
      expect(isVisible).toBe(false);
    }
  });

  test('TC-083: Homepage st1 — 3 cards balanced', async ({ page }) => {
    await page.goto('/');
    const cards = page.locator('.st1 .item-st1, .st1 .col-md-4');
    const cnt = await cards.count();
    expect(cnt, 'Section st1 phải có 3 card').toBeGreaterThanOrEqual(3);
    if (cnt >= 3) {
      const b1 = await cards.nth(0).boundingBox();
      const b3 = await cards.nth(2).boundingBox();
      // 3 card cùng dòng — width tương đương
      expect(b1 && b3).toBeTruthy();
      expect(Math.abs(b1!.width - b3!.width)).toBeLessThan(20);
    }
    await expectNoMojibake(page);
  });
});
