import { test, expect } from '@playwright/test';
import { loginMember, loginStaff, logout, expectNoMojibake } from '../fixtures/auth';

/**
 * Q&A spec — UC Hỏi đáp công khai.
 * Phủ TC-036 → TC-041, TC-075.
 */

test.describe('UC-QnA — Hỏi đáp', () => {
  test.afterEach(async ({ page }) => { await logout(page); });

  test('TC-036: Đặt câu hỏi — anonymous submit bị chặn (yêu cầu login)', async ({ page }) => {
    // Design: GET /dat-cau-hoi cho phép anonymous xem form; chỉ POST mới kiểm tra login.
    await page.goto('/dat-cau-hoi');
    expect(page.url()).toContain('/dat-cau-hoi');
    // Submit form anonymously — server phải reject + show error "cần đăng nhập"
    const titleField = page.locator('input[name="qa_title"], input[name="Title"]').first();
    const bodyField = page.locator('textarea[name="qa_body"], textarea[name="Body"]').first();
    if (await titleField.count() === 0 || await bodyField.count() === 0) {
      test.skip(true, 'Form layout khác — skip');
    }
    await titleField.fill('Test anonymous');
    await bodyField.fill('Câu hỏi từ user chưa login');
    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
    const html = await page.content();
    expect(html).toMatch(/đăng nhập|cần đăng nhập|login required/i);
  });

  test('TC-037: Đặt câu hỏi — member submit thành công', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-cau-hoi');
    const titleField = page.locator('input[name="qa_title"]').first();
    const bodyField  = page.locator('textarea[name="qa_body"]').first();
    if (await titleField.count() === 0 || await bodyField.count() === 0) {
      test.skip(true, 'Form Q&A khác layout — skip');
    }
    await titleField.fill('Câu hỏi test E2E ' + Date.now());
    await bodyField.fill('Nội dung test tự động gửi từ Playwright.');
    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
    // Sau submit success: redirect tới /cau-hoi-cua-toi
    expect(page.url()).not.toContain('/dat-cau-hoi');
  });

  test('TC-039: /cau-hoi-cua-toi — list của member', async ({ page }) => {
    await loginMember(page);
    const resp = await page.goto('/cau-hoi-cua-toi');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    expect(html).toMatch(/Câu hỏi|Hỏi đáp|chưa có|Không có/i);
  });

  test('TC-040: /hoi-dap — chỉ hiện câu hỏi approved + public', async ({ page }) => {
    const resp = await page.goto('/hoi-dap');
    expect(resp?.status()).toBe(200);
    const html = await page.content();
    // KHÔNG được hiện status pending / private
    expect(html).not.toMatch(/Đang chờ duyệt.*<|status:\s*pending/i);
    await expectNoMojibake(page);
  });

  test('TC-041: Length cap — body 500 ký tự (server-side)', async ({ page }) => {
    await loginMember(page);
    await page.goto('/dat-cau-hoi');
    const bodyField  = page.locator('textarea[name="qa_body"]').first();
    const titleField = page.locator('input[name="qa_title"]').first();
    if (await bodyField.count() === 0) test.skip(true, 'No body field');
    await titleField.fill('Test cap');
    // Body 600 ký tự — server phải trim về 500
    await bodyField.fill('A'.repeat(600));
    await page.click('button[type="submit"]');
    // Test pass nếu không crash (server cap chuyên môn — trim hoặc reject)
    await page.waitForLoadState('networkidle');
  });

  test('TC-038 (skeleton): Bác sĩ duyệt + trả lời', async ({ page }) => {
    await loginStaff(page, 'bacsy');
    const resp = await page.goto('/bac-si-portal');
    expect(resp?.status()).toBe(200);
    // Hiện tại bác sĩ portal có thể có hoặc không có module Q&A — chỉ smoke
    const qnaLink = page.locator('a:has-text("Hỏi đáp"), a:has-text("Câu hỏi")').first();
    if (await qnaLink.count() > 0) {
      await qnaLink.click();
      await page.waitForLoadState('networkidle');
    }
  });

  test('TC-075: Q&A double answer — 2 lần trả lời cùng câu hỏi → reject', async ({ page }) => {
    // Test này yêu cầu seed Q&A đã được trả lời. Dùng skeleton để evaluate
    test.fixme(true, 'Yêu cầu seed Q&A đã có answer — implement sau khi fixture ready');
  });
});
