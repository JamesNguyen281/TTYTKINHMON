import { test, expect } from '@playwright/test';
import { TestAccounts, loginMember, loginStaff, logout } from '../fixtures/auth';

/**
 * Auth spec — TC-005, TC-006, TC-020, TC-030, TC-042, TC-043, TC-044, TC-056, TC-057.
 * Phụ thuộc seed: 4 tài khoản test (admin / member01 / letan01 / bacsy01) — xem docs/05_TestCases.md.
 */

test.describe('Authentication & role-based redirect', () => {
  test.afterEach(async ({ page }) => {
    await logout(page);
  });

  test('TC-005: Member đăng nhập thành công → cookie set', async ({ page }) => {
    await loginMember(page);
    const cookies = await page.context().cookies();
    const sessionCookie = cookies.find(c => c.name.includes('Session') || c.name.includes('Cookie'));
    expect(sessionCookie, 'Session cookie phải được set').toBeTruthy();
  });

  test('TC-006: Member sai mật khẩu → vẫn ở trang đăng nhập + flash error', async ({ page }) => {
    await page.goto('/dang-nhap');
    await page.fill('input[name="UserName"]', TestAccounts.member.user);
    await page.fill('input[name="Password"]', 'sai-mat-khau');
    await page.click('button[type="submit"]');

    await expect(page).toHaveURL(/\/dang-nhap/);
    const html = await page.content();
    expect(html).toMatch(/Sai|không đúng|không tồn tại/i);
  });

  test('TC-020: Lễ tân login → redirect /le-tan', async ({ page }) => {
    await loginStaff(page, 'letan');
    expect(page.url()).toContain('/le-tan');
  });

  test('TC-030: Bác sĩ login → redirect /bac-si-portal', async ({ page }) => {
    await loginStaff(page, 'bacsy');
    expect(page.url()).toContain('/bac-si-portal');
  });

  test('TC-042: Admin login → redirect /AdminCP/Default', async ({ page }) => {
    await loginStaff(page, 'admin');
    expect(page.url()).toMatch(/\/AdminCP\/(Default|$)/);
  });

  test('TC-043: Cross-portal guard — MEMBER login ở /AdminCP/Login bị reject', async ({ page }) => {
    await page.goto('/AdminCP/Login');
    await page.fill('input[name="UserName"]', TestAccounts.member.user);
    await page.fill('input[name="Password"]', TestAccounts.member.pass);
    await page.click('button[type="submit"]');

    // Vẫn ở trang login + có error message
    await expect(page).toHaveURL(/\/AdminCP\/Login/);
    const html = await page.content();
    expect(html).toMatch(/không tồn tại|sai|không đúng|không có quyền/i);
  });

  test('TC-044: AdminCP — chưa login → redirect /AdminCP/Login', async ({ page }) => {
    const resp = await page.goto('/AdminCP/Default');
    // Có thể redirect hoặc trả về login page
    await expect(page).toHaveURL(/\/AdminCP\/Login/);
  });

  test('TC-056: Letan login truy cập /bac-si-portal → bị từ chối', async ({ page }) => {
    await loginStaff(page, 'letan');
    await page.goto('/bac-si-portal');
    // StaffAuthorize redirect RECEPTION sang portal đúng role (/le-tan).
    // Test pass nếu URL cuối KHÔNG còn ở /bac-si-portal (đã bị từ chối + redirect).
    const finalUrl = page.url();
    expect(finalUrl, `Letan không được vào /bac-si-portal — phải redirect /le-tan hoặc /AdminCP/Login`)
      .not.toMatch(/\/bac-si-portal($|\/(?!yeu-cau-doi-lich))/);
    expect(
      finalUrl.includes('/le-tan') || finalUrl.includes('/AdminCP/Login'),
      `Phải redirect tới /le-tan hoặc /AdminCP/Login, nhưng URL = ${finalUrl}`
    ).toBeTruthy();
  });
});
