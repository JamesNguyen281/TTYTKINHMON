import { test } from '@playwright/test';
import { loginStaff, loginMember } from '../fixtures/auth';
import * as path from 'path';

/**
 * Screenshot capture spec — chụp ảnh các trang chính ở mobile + desktop để
 * đính kèm báo cáo / slide thuyết trình. Output: `screenshots/{viewport}/{page}.png`.
 *
 * Chạy bằng:  npx playwright test specs/screenshots.spec.ts
 * Output mở:  screenshots/  hoặc playwright-report (nếu test fail).
 */

const OUT = path.resolve(__dirname, '..', 'screenshots');

const VIEWPORTS = [
  { name: 'desktop-1366',  width: 1366, height: 768 },
  { name: 'tablet-768',    width: 768,  height: 1024 },
  { name: 'mobile-iphone-x', width: 375, height: 812 },
  { name: 'mobile-se',     width: 320, height: 568 },
];

const PUBLIC_PAGES = [
  { slug: 'home',          path: '/' },
  { slug: 'dat-lich-kham', path: '/dat-lich-kham' },
  { slug: 'dang-nhap',     path: '/dang-nhap' },
  { slug: 'dang-ky',       path: '/dang-ky' },
  { slug: 'bac-si',        path: '/bac-si' },
  { slug: 'chuyen-khoa',   path: '/chuyen-khoa' },
  { slug: 'hoi-dap',       path: '/hoi-dap' },
  { slug: 'tin-tuc',       path: '/tin-tuc' },
  { slug: 'lien-he',       path: '/lien-he' },
];

test.describe('Screenshots — Public pages × 4 viewports', () => {
  for (const vp of VIEWPORTS) {
    for (const p of PUBLIC_PAGES) {
      test(`Public ${vp.name} ${p.slug}`, async ({ page }) => {
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto(p.path, { waitUntil: 'networkidle' });
        // Đợi animations + lazy images settle
        await page.waitForTimeout(500);
        await page.screenshot({
          path: path.join(OUT, vp.name, `public-${p.slug}.png`),
          fullPage: true,
        });
      });
    }
  }
});

test.describe('Screenshots — Portal pages × 4 viewports', () => {
  const PORTAL_PAGES: Array<{ slug: string; path: string; account: 'admin' | 'letan' | 'bacsy' }> = [
    { slug: 'le-tan-index',           path: '/le-tan',                        account: 'letan' },
    { slug: 'le-tan-lich-hen',        path: '/le-tan/lich-hen?status=pending', account: 'letan' },
    { slug: 'le-tan-tim-theo-sdt',    path: '/le-tan/tim-theo-sdt',           account: 'letan' },
    { slug: 'le-tan-lich-theo-ngay',  path: '/le-tan/lich-theo-ngay',         account: 'letan' },
    { slug: 'le-tan-check-in',        path: '/le-tan/check-in',               account: 'letan' },
    { slug: 'bac-si-portal',          path: '/bac-si-portal',                 account: 'bacsy' },
    { slug: 'bac-si-bn-hom-nay',      path: '/bac-si-portal/benh-nhan-hom-nay', account: 'bacsy' },
    { slug: 'admin-default',          path: '/AdminCP/Default',               account: 'admin' },
    { slug: 'admin-doctor-schedules', path: '/AdminCP/DoctorSchedules',       account: 'admin' },
    { slug: 'admin-auto-generate',    path: '/AdminCP/DoctorSchedules/AutoGenerate', account: 'admin' },
  ];

  for (const vp of VIEWPORTS) {
    for (const p of PORTAL_PAGES) {
      test(`Portal ${vp.name} ${p.slug}`, async ({ page }) => {
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await loginStaff(page, p.account);
        await page.goto(p.path, { waitUntil: 'networkidle' });
        await page.waitForTimeout(500);
        await page.screenshot({
          path: path.join(OUT, vp.name, `portal-${p.slug}.png`),
          fullPage: true,
        });
      });
    }
  }
});

test.describe('Screenshots — Member pages × 4 viewports', () => {
  const MEMBER_PAGES = [
    { slug: 'lich-cua-toi',  path: '/lich-cua-toi' },
    { slug: 'ho-so',         path: '/ho-so' },
    { slug: 'lich-su-kham',  path: '/lich-su-kham' },
  ];

  for (const vp of VIEWPORTS) {
    for (const p of MEMBER_PAGES) {
      test(`Member ${vp.name} ${p.slug}`, async ({ page }) => {
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await loginMember(page);
        await page.goto(p.path, { waitUntil: 'networkidle' });
        await page.waitForTimeout(500);
        await page.screenshot({
          path: path.join(OUT, vp.name, `member-${p.slug}.png`),
          fullPage: true,
        });
      });
    }
  }
});
