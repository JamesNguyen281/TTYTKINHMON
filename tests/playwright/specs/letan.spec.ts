import { test, expect } from '@playwright/test';
import { loginStaff, logout } from '../fixtures/auth';

/**
 * Letan portal spec — TC-021, TC-022, TC-024, TC-025.
 * Yêu cầu: tài khoản letan01 đã seed; DB có ít nhất 1 lịch pending.
 */

test.describe('UC21 — Lễ tân duyệt lịch', () => {
  test.beforeEach(async ({ page }) => {
    await loginStaff(page, 'letan');
  });

  test.afterEach(async ({ page }) => {
    await logout(page);
  });

  test('TC-021: Danh sách lịch pending hiển thị', async ({ page }) => {
    const resp = await page.goto('/le-tan/lich-hen?status=pending');
    expect(resp?.status()).toBe(200);

    // Phải có table hoặc danh sách
    const html = await page.content();
    expect(html).toMatch(/pending|chờ duyệt|Mã|Bệnh nhân/i);
  });

  test('TC-022 (UI smoke): Form duyệt lịch pending xuất hiện', async ({ page }) => {
    await page.goto('/le-tan/lich-hen?status=pending');

    // Kiểm có nút Duyệt / Từ chối / Detail
    const buttons = page.locator('a, button');
    const text = (await buttons.allTextContents()).join(' ');
    expect(text).toMatch(/Duyệt|Chi tiết|Xem|Cập nhật/i);
  });

  test('TC-025 (UI smoke): Trang check-in load OK', async ({ page }) => {
    const resp = await page.goto('/le-tan/check-in');
    expect(resp?.status()).toBe(200);

    // Có input cho mã booking
    const input = page.locator('input[type="text"], input[name*="ode" i]');
    expect(await input.count()).toBeGreaterThanOrEqual(1);
  });
});

test.describe('UC31 — Bác sĩ portal', () => {
  test.beforeEach(async ({ page }) => {
    await loginStaff(page, 'bacsy');
  });

  test.afterEach(async ({ page }) => {
    await logout(page);
  });

  test('TC-031 (UI smoke): /bac-si-portal/benh-nhan-hom-nay load OK', async ({ page }) => {
    const resp = await page.goto('/bac-si-portal/benh-nhan-hom-nay');
    expect(resp?.status()).toBe(200);
  });

  test('Bác sĩ portal — index trả 200', async ({ page }) => {
    const resp = await page.goto('/bac-si-portal');
    expect(resp?.status()).toBe(200);
  });
});
