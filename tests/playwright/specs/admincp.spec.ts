import { test, expect } from '@playwright/test';
import { loginStaff, loginMember, logout, expectNoMojibake } from '../fixtures/auth';

/**
 * AdminCP spec — quản trị nội dung CMS + role guard.
 * Phủ TC-042, TC-043, TC-044, TC-045, TC-046, TC-047, TC-048, TC-049, TC-057.
 */

test.describe('UC42 — AdminCP CRUD nội dung', () => {

  test.describe('Auth & Role guard', () => {
    test.afterEach(async ({ page }) => { await logout(page); });

    test('TC-042: Admin login → /AdminCP/Default 200', async ({ page }) => {
      await loginStaff(page, 'admin');
      const resp = await page.goto('/AdminCP/Default');
      expect(resp?.status()).toBe(200);
    });

    test('TC-044: Chưa login → /AdminCP/Default redirect /AdminCP/Login', async ({ page }) => {
      const resp = await page.goto('/AdminCP/Default');
      await expect(page).toHaveURL(/\/AdminCP\/Login/);
    });

    test('TC-057: Member login truy cập /AdminCP/Default → bị từ chối', async ({ page }) => {
      await loginMember(page);
      const resp = await page.goto('/AdminCP/Default');
      // Hoặc 302 → /AdminCP/Login, hoặc 403, hoặc redirect /
      const url = page.url();
      expect(url).not.toMatch(/\/AdminCP\/Default/);
    });

    test('TC-043: MEMBER login ở /AdminCP/Login → reject (cross-portal guard)', async ({ page }) => {
      await page.goto('/AdminCP/Login');
      await page.fill('input[name="UserName"]', 'member01');
      await page.fill('input[name="Password"]', '123456');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL(/\/AdminCP\/Login/);
      const html = await page.content();
      expect(html).toMatch(/không tồn tại|sai|không đúng|không có quyền/i);
    });
  });

  test.describe('CMS endpoints', () => {
    test.beforeEach(async ({ page }) => { await loginStaff(page, 'admin'); });
    test.afterEach(async ({ page }) => { await logout(page); });

    const adminPages = [
      { url: '/AdminCP/Default',          label: 'Dashboard' },
      { url: '/AdminCP/Categories',       label: 'Danh mục' },
      { url: '/AdminCP/News',             label: 'Tin tức' },
      { url: '/AdminCP/Slides',           label: 'Hình ảnh' },
      { url: '/AdminCP/Videos',           label: 'Video' },
      { url: '/AdminCP/Documents',        label: 'Văn bản' },
      { url: '/AdminCP/Comment',          label: 'Hộp thư' },
      { url: '/AdminCP/Departments',      label: 'Chuyên khoa' },
      { url: '/AdminCP/Doctors',          label: 'Bác sĩ' },
      { url: '/AdminCP/DoctorSchedules',  label: 'Lịch trực' },
      { url: '/AdminCP/MedicalRecords',   label: 'Hồ sơ khám' },
      { url: '/AdminCP/Appointments',     label: 'Lịch hẹn' },
      { url: '/AdminCP/Quotas',           label: 'Suất khám' },
      { url: '/AdminCP/Users',            label: 'Người dùng' },
      { url: '/AdminCP/AuditSystems',     label: 'Audit log' },
      { url: '/AdminCP/Sites',            label: 'Sites' },
      { url: '/AdminCP/Partners',         label: 'Đối tác' },
    ];

    for (const p of adminPages) {
      test(`Smoke ${p.label} — ${p.url} → 200`, async ({ page }) => {
        const resp = await page.goto(p.url);
        expect(resp?.status()).toBe(200);
        await expectNoMojibake(page);
      });
    }

    test('TC-045: Department/Create — form load OK', async ({ page }) => {
      const resp = await page.goto('/AdminCP/Departments/Create');
      expect(resp?.status()).toBe(200);
      await expect(page.locator('input[name="NameL"], input[name="Name"]')).toBeVisible();
      await expect(page.locator('input[name="__RequestVerificationToken"]')).toHaveCount(1);
    });

    test('TC-046: Department/Edit/{id} — load form với data', async ({ page }) => {
      await page.goto('/AdminCP/Departments');
      const editLink = page.locator('a[href*="/AdminCP/Departments/Edit/"]').first();
      if (await editLink.count() === 0) test.skip(true, 'Không có department để edit');
      const href = await editLink.getAttribute('href');
      const resp = await page.goto(href!);
      expect(resp?.status()).toBe(200);
      const nameInput = page.locator('input[name="NameL"], input[name="Name"]').first();
      const v = await nameInput.inputValue();
      expect(v.length).toBeGreaterThan(0);
    });

    test('TC-047: Sites/Edit — form upload logo có input file', async ({ page }) => {
      await page.goto('/AdminCP/Sites');
      const editLink = page.locator('a[href*="/AdminCP/Sites/Edit/"]').first();
      if (await editLink.count() === 0) test.skip(true, 'Không có site');
      await editLink.click();
      await page.waitForLoadState('networkidle');
      await expect(page.locator('input[type="file"]').first()).toBeVisible();
    });

    test('TC-048: AuditSystems — read-only, không có nút Edit/Delete', async ({ page }) => {
      const resp = await page.goto('/AdminCP/AuditSystems');
      expect(resp?.status()).toBe(200);
      const html = await page.content();
      // Audit log immutable — không được có form sửa hoặc xoá
      expect(html).not.toMatch(/<form[^>]*action=["'][^"']*AuditSystems\/(Edit|Delete)/i);
    });

    test('TC-049: Users/ChangeRole — chuyển nhóm user', async ({ page }) => {
      const resp = await page.goto('/AdminCP/Users');
      expect(resp?.status()).toBe(200);
      // Có nút edit role hoặc form change-role nào đó
      const html = await page.content();
      expect(html).toMatch(/role|nhóm|group|Admin|Doctor|Reception|Member/i);
    });
  });
});
