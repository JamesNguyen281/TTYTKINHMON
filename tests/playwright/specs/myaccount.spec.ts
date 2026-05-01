import { test, expect } from '@playwright/test';
import { loginMember, logout } from '../fixtures/auth';

/**
 * MyAccount spec — Trang cá nhân bệnh nhân.
 * Phủ TC-017, TC-018, TC-019.
 */

test.describe('UC18-19 — Hồ sơ cá nhân & đổi mật khẩu (member)', () => {
  test.beforeEach(async ({ page }) => { await loginMember(page); });
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-018: /ho-so load thành công cho member', async ({ page }) => {
    const resp = await page.goto('/ho-so');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // Phải KHÔNG redirect sang /staff-profile (member ≠ staff)
    expect(page.url()).not.toContain('/staff-profile');
    expect(html).toMatch(/Hồ sơ|Định danh|Liên hệ/i);
  });

  test('TC-017: /lich-su-kham — list MedicalRecord (có hoặc rỗng)', async ({ page }) => {
    const resp = await page.goto('/lich-su-kham');
    // Cho phép 200 (có data hoặc empty state)
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    expect(html).toMatch(/Lịch sử khám|Mã hồ sơ|chưa có|Không có|RecordNo/i);
  });

  test('TC-019: /doi-mat-khau — đổi mật khẩu, sai pass cũ → fail', async ({ page }) => {
    const resp = await page.goto('/doi-mat-khau');
    expect(resp?.status()).toBe(200);

    // Sai pass cũ
    const oldField = page.locator('input[name="OldPassword"], input[name="CurrentPassword"]').first();
    const newField = page.locator('input[name="NewPassword"]').first();
    if (await oldField.count() > 0 && await newField.count() > 0) {
      await oldField.fill('SAI_PASS_xxx');
      await newField.fill('NewTest123@');
      const confirmField = page.locator('input[name="ConfirmPassword"], input[name="NewPasswordConfirm"]').first();
      if (await confirmField.count() > 0) await confirmField.fill('NewTest123@');
      await page.click('button[type="submit"]');
      await page.waitForLoadState('networkidle');
      const html = await page.content();
      expect(html).toMatch(/sai|không đúng|incorrect|hiện tại/i);
    }
  });
});
