import { test, expect } from '@playwright/test';
import { expectNoMojibake } from '../fixtures/auth';

/**
 * Register spec — UC02 Member đăng ký tài khoản.
 * Phủ TC-002, TC-003, TC-004.
 */

function unique(prefix: string) {
  return `${prefix}_${Date.now().toString(36)}_${Math.random().toString(36).slice(2, 6)}`;
}

test.describe('UC02 — Đăng ký tài khoản', () => {
  test('TC-002: Form đăng ký hiển thị đầy đủ field', async ({ page }) => {
    const resp = await page.goto('/dang-ky');
    expect(resp?.status()).toBe(200);

    // Anti-forgery token
    await expect(page.locator('input[name="__RequestVerificationToken"]')).toHaveCount(1);
    // Field tối thiểu
    await expect(page.locator('input[name="UserName"]')).toBeVisible();
    await expect(page.locator('input[name="Password"]')).toBeVisible();
    await expect(page.locator('input[name="FullName"]')).toBeVisible();
    await expect(page.locator('input[name="Phone"]')).toBeVisible();

    await expectNoMojibake(page);
  });

  test('TC-003: Đăng ký thành công — tài khoản mới được tạo', async ({ page }) => {
    const u = unique('newuser');
    const phone = '09' + Math.floor(10_000_000 + Math.random() * 90_000_000);

    await page.goto('/dang-ky');
    await page.fill('input[name="UserName"]', u);
    await page.fill('input[name="FullName"]', 'Test User ' + u);
    await page.fill('input[name="Phone"]', phone);
    const emailInput = page.locator('input[name="Email"]');
    if (await emailInput.count() > 0) await emailInput.fill(`${u}@test.local`);
    // Phải fill cả Password lẫn ConfirmPassword — jquery-validate có rule equalTo, thiếu ConfirmPassword sẽ block submit
    await page.fill('input[name="Password"]', 'Test1234@');
    await page.fill('input[name="ConfirmPassword"]', 'Test1234@');

    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');

    // Sau đăng ký: hoặc auto-login và redirect /, hoặc về /dang-nhap với flash success
    expect(page.url()).not.toContain('/dang-ky');
  });

  test('TC-004: Đăng ký thất bại — SĐT/username đã tồn tại', async ({ page }) => {
    // Dùng admin username để chắc chắn duplicate
    await page.goto('/dang-ky');
    await page.fill('input[name="UserName"]', 'admin');
    await page.fill('input[name="FullName"]', 'Trung admin');
    await page.fill('input[name="Phone"]', '0901234567');
    await page.fill('input[name="Password"]', 'Test1234@');
    await page.fill('input[name="ConfirmPassword"]', 'Test1234@');
    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');

    // Vẫn ở trang đăng ký + hiện lỗi (server-side validation)
    await expect(page).toHaveURL(/\/dang-ky/);
    const html = await page.content();
    expect(html).toMatch(/tồn tại|đã được dùng|đã đăng ký|đã có|trùng/i);
  });
});
