import { test, expect } from '@playwright/test';

/**
 * Đo độ mượt navigation giữa các trang public + capture logo header
 * trên mobile sau lần sửa CSS brand-logo.
 */

const PUBLIC_PAGES = [
  { path: '/',                label: 'home' },
  { path: '/bac-si',          label: 'bac-si' },
  { path: '/chuyen-khoa',     label: 'chuyen-khoa' },
  { path: '/tin-tuc',         label: 'tin-tuc' },
  { path: '/hoi-dap',         label: 'hoi-dap' },
  { path: '/lien-he',         label: 'lien-he' },
  { path: '/dat-lich-kham',   label: 'dat-lich-kham' },
];

interface NavTiming {
  ttfb: number;
  domContentLoaded: number;
  loadEvent: number;
  fcp: number | null;
}

async function measure(page: import('@playwright/test').Page, url: string): Promise<NavTiming> {
  await page.goto(url, { waitUntil: 'load' });
  return await page.evaluate<NavTiming>(() => {
    const nav = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming | undefined;
    const fcpEntry = performance.getEntriesByName('first-contentful-paint')[0];
    return {
      ttfb: nav ? Math.round(nav.responseStart - nav.requestStart) : 0,
      domContentLoaded: nav ? Math.round(nav.domContentLoadedEventEnd - nav.startTime) : 0,
      loadEvent: nav ? Math.round(nav.loadEventEnd - nav.startTime) : 0,
      fcp: fcpEntry ? Math.round(fcpEntry.startTime) : null,
    };
  });
}

test.describe('Navigation smoothness — mobile iPhone X', () => {
  test.use({ viewport: { width: 375, height: 812 } });

  test('all public pages load fast + no overflow + logo visible', async ({ page }) => {
    const report: Array<{ label: string } & NavTiming> = [];
    for (const p of PUBLIC_PAGES) {
      const timing = await measure(page, `http://localhost:5050${p.path}`);
      report.push({ label: p.label, ...timing });

      // Mỗi trang phải xuất hiện logo trong header
      const logo = page.locator('.brand-logo-img').first();
      await expect(logo).toBeVisible();

      // Không tràn ngang — body width ≤ window width
      const overflow = await page.evaluate(() => {
        return Math.max(document.body.scrollWidth, document.documentElement.scrollWidth)
             - window.innerWidth;
      });
      expect(overflow, `${p.label} overflow ngang`).toBeLessThanOrEqual(2);

      // Đo kích thước logo thực tế trên mobile
      const logoBox = await logo.boundingBox();
      expect(logoBox?.height ?? 0, `${p.label} logo height < 60px nghĩa là quá bé`).toBeGreaterThan(60);
    }

    console.log('Navigation timing report (mobile 375):');
    report.forEach(r => {
      console.log(`  ${r.label.padEnd(15)} TTFB ${r.ttfb}ms | DOM ${r.domContentLoaded}ms | load ${r.loadEvent}ms | FCP ${r.fcp ?? '?'}ms`);
    });

    // Average load time gắt: ≤ 2.5s
    const avgLoad = report.reduce((s, r) => s + r.loadEvent, 0) / report.length;
    console.log(`Average load: ${Math.round(avgLoad)}ms`);
    expect(avgLoad).toBeLessThan(2500);
  });

  test('snapshot logo header trên iPhone X', async ({ page }) => {
    await page.goto('http://localhost:5050/', { waitUntil: 'networkidle' });
    await page.waitForTimeout(500);
    await page.locator('header').first().screenshot({ path: 'screenshots/logo-mobile-iphone-x.png' });
  });
});

test.describe('Navigation smoothness — desktop 1366', () => {
  test.use({ viewport: { width: 1366, height: 768 } });

  test('average page load < 2s on desktop', async ({ page }) => {
    const report: Array<{ label: string } & NavTiming> = [];
    for (const p of PUBLIC_PAGES) {
      report.push({ label: p.label, ...(await measure(page, `http://localhost:5050${p.path}`)) });
    }
    console.log('Navigation timing report (desktop 1366):');
    report.forEach(r => {
      console.log(`  ${r.label.padEnd(15)} TTFB ${r.ttfb}ms | DOM ${r.domContentLoaded}ms | load ${r.loadEvent}ms | FCP ${r.fcp ?? '?'}ms`);
    });
    const avgLoad = report.reduce((s, r) => s + r.loadEvent, 0) / report.length;
    console.log(`Average load: ${Math.round(avgLoad)}ms`);
    expect(avgLoad).toBeLessThan(2000);
  });
});
