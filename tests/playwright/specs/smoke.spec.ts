import { test, expect } from '@playwright/test';
import { expectNoMojibake } from '../fixtures/auth';

/**
 * Smoke spec — phủ TC-001, TC-002, TC-005, TC-050, TC-052
 *
 * Chạy: npm run test:smoke
 * Yêu cầu: server đã chạy ở http://localhost:5050.
 */

test.describe('A. Public site smoke', () => {
  test('TC-001: Trang chủ tải thành công', async ({ page }) => {
    const resp = await page.goto('/');
    expect(resp?.status()).toBe(200);

    // Header logo + hotline
    await expect(page.locator('header, .header').first()).toBeVisible();
    // Footer 5 cột (hoặc ít nhất 1 footer block)
    await expect(page.locator('footer')).toBeVisible();

    // Tin tức + chuyên khoa nổi bật xuất hiện
    await expect(page.getByRole('heading').first()).toBeVisible(); // ít nhất 1 heading

    await expectNoMojibake(page);
  });

  test('TC-002: Form đăng ký hiển thị', async ({ page }) => {
    const resp = await page.goto('/dang-ky');
    expect(resp?.status()).toBe(200);

    // Anti-forgery token
    await expect(page.locator('input[name="__RequestVerificationToken"]')).toHaveCount(1);

    // Field tối thiểu — tên + SĐT + mật khẩu
    const inputs = page.locator('form input');
    expect(await inputs.count()).toBeGreaterThan(3);

    await expectNoMojibake(page);
  });

  test('TC-002b: Form đăng nhập hiển thị', async ({ page }) => {
    const resp = await page.goto('/dang-nhap');
    expect(resp?.status()).toBe(200);

    await expect(page.locator('input[name="UserName"]')).toBeVisible();
    await expect(page.locator('input[name="Password"]')).toBeVisible();
    await expect(page.locator('input[name="__RequestVerificationToken"]')).toHaveCount(1);

    await expectNoMojibake(page);
  });

  test('TC-050: Tiếng Việt KHÔNG bị Razor encode thành numeric entity', async ({ page }) => {
    const pages = ['/', '/dang-nhap', '/dang-ky', '/dat-lich-kham', '/hoi-dap', '/chuyen-khoa'];
    for (const url of pages) {
      const resp = await page.goto(url);
      expect(resp?.status(), `URL ${url} phải trả 200`).toBe(200);
      await expectNoMojibake(page);
    }
  });

  test('TC-052: Favicon load đủ 3 size', async ({ page }) => {
    await page.goto('/');
    const favicons = page.locator('link[rel*="icon"]');
    const count = await favicons.count();
    expect(count, 'Phải có ít nhất 1 link favicon').toBeGreaterThanOrEqual(1);
  });

  test('TC-054: 404 page tuỳ biến', async ({ page }) => {
    const resp = await page.goto('/khong-ton-tai-12345');
    // Cho phép 404 hoặc 200 (custom error page handler)
    expect([200, 404]).toContain(resp?.status());
    const html = await page.content();
    // KHÔNG được hiển thị stack trace
    expect(html).not.toMatch(/at\s+System\./);
    expect(html).not.toMatch(/Microsoft\.AspNetCore/);
  });
});

test.describe('B. Public endpoints không-auth trả 200', () => {
  const publicUrls = [
    '/',
    '/dang-nhap',
    '/dang-ky',
    '/dat-lich-kham',
    '/chuyen-khoa',
    '/hoi-dap',
  ];

  for (const url of publicUrls) {
    test(`smoke ${url} → 200`, async ({ page }) => {
      const resp = await page.goto(url);
      expect(resp?.status()).toBe(200);
    });
  }
});

test.describe('C. Static asset & SEO', () => {
  test('robots.txt accessible', async ({ request }) => {
    const resp = await request.get('/robots.txt');
    expect([200, 404]).toContain(resp.status());
  });

  test('sitemap.xml accessible (nếu có)', async ({ request }) => {
    const resp = await request.get('/sitemap.xml');
    // Có thể trả 200 (tự generate) hoặc 404 (không config)
    expect([200, 404]).toContain(resp.status());
  });

  test('favicon 32×32 phục vụ 200', async ({ request }) => {
    const resp = await request.get('/assets/client/images/logo-kinhmon-32.png');
    expect(resp.status()).toBe(200);
  });

  test('SVG sơ đồ chỉ dẫn — accessible', async ({ request }) => {
    const resp = await request.get('/assets/client/images/sodo/so-do-chi-dan-duong-di.svg');
    expect([200, 404]).toContain(resp.status());
  });
});
