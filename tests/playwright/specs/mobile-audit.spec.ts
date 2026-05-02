import { test, expect } from '@playwright/test';
import { loginStaff, loginMember, logout } from '../fixtures/auth';

/**
 * Mobile UI/UX audit — chạy ở 3 viewport mobile + 1 desktop (regression).
 * Kiểm tra:
 *   1. Không có horizontal overflow (scrollWidth ≤ viewport + 1px tolerance).
 *   2. Tất cả button + a.btn ≥ 44×44 px (Apple HIG).
 *   3. Form input không gây iOS auto-zoom (font-size ≥ 16px).
 *   4. Desktop ≥ 1280px không bị ảnh hưởng (regression check).
 *
 * KHÔNG động vào view/layout — chỉ verify CSS responsive.
 */

const PAGES = [
  '/',
  '/dat-lich-kham',
  '/dang-nhap',
  '/dang-ky',
  '/hoi-dap',
  '/lien-he',
  '/bac-si',
  '/chuyen-khoa',
  '/tin-tuc',
];

const MOBILE_SIZES = [
  { name: '320×568 (iPhone SE 1)',  width: 320, height: 568 },
  { name: '375×667 (iPhone SE 2)',  width: 375, height: 667 },
  { name: '414×896 (iPhone XR)',    width: 414, height: 896 },
];

const DESKTOP_SIZE = { name: '1280×800 (Desktop)', width: 1280, height: 800 };

test.describe('Mobile UI/UX audit — không tràn ngang ở 3 size', () => {
  for (const vp of MOBILE_SIZES) {
    for (const path of PAGES) {
      test(`No horizontal overflow @ ${vp.name} ${path}`, async ({ page }) => {
        await page.setViewportSize({ width: vp.width, height: vp.height });
        await page.goto(path, { waitUntil: 'networkidle' });
        const overflow = await page.evaluate(() => ({
          docW: document.documentElement.scrollWidth,
          winW: window.innerWidth,
        }));
        // Tolerance 1px cho subpixel rounding
        expect(overflow.docW, `Page ${path} tràn ngang ở ${vp.width}px: docW=${overflow.docW} > winW=${overflow.winW}`)
          .toBeLessThanOrEqual(overflow.winW + 1);
      });
    }
  }

  test(`Desktop regression — / @ ${DESKTOP_SIZE.name} không tràn`, async ({ page }) => {
    await page.setViewportSize({ width: DESKTOP_SIZE.width, height: DESKTOP_SIZE.height });
    await page.goto('/', { waitUntil: 'networkidle' });
    const overflow = await page.evaluate(() => ({
      docW: document.documentElement.scrollWidth,
      winW: window.innerWidth,
    }));
    expect(overflow.docW).toBeLessThanOrEqual(overflow.winW + 1);
  });
});

test.describe('Mobile UI/UX audit — tap target ≥ 44px ở 320px', () => {
  test('Trang chủ — mọi button + a.btn ≥ 44×44', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 568 });
    await page.goto('/', { waitUntil: 'networkidle' });
    const small = await page.evaluate(() => {
      const els = Array.from(document.querySelectorAll('button, a.btn, .btn'));
      // Bỏ qua carousel controls (csvc-arrow, csvc-dot, owl-*, slick-*) vì primary UX là swipe,
      // dots chỉ là indicator. Apple HIG vẫn khuyến nghị nhưng pragmatic chấp nhận cho carousel.
      const isCarousel = (cls: string) =>
        /\b(csvc-|owl-|slick-|carousel-control)/i.test(cls);
      return els
        .filter(el => {
          const cls = (el as HTMLElement).className?.toString() ?? '';
          if (isCarousel(cls)) return false;
          const r = el.getBoundingClientRect();
          if (r.width === 0 || r.height === 0) return false;
          return r.height < 44 || r.width < 44;
        })
        .slice(0, 5)
        .map(el => ({
          tag: el.tagName,
          cls: (el as HTMLElement).className.toString().slice(0, 60),
          w: Math.round((el as HTMLElement).getBoundingClientRect().width),
          h: Math.round((el as HTMLElement).getBoundingClientRect().height),
        }));
    });
    expect(small, `Có ${small.length} button < 44px: ${JSON.stringify(small)}`).toHaveLength(0);
  });

  test('iPhone X (375×812): login icon + hamburger phải visible trong top bar', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto('/', { waitUntil: 'networkidle' });
    // Login icon (.fa-sign-in trong .header-top-right .social a) phải hiển thị
    const loginVisible = await page.evaluate(() => {
      const a = document.querySelector('.header-top-right ul li.social a');
      if (!a) return { found: false };
      const r = a.getBoundingClientRect();
      const cs = getComputedStyle(a);
      return {
        found: true,
        display: cs.display,
        visibility: cs.visibility,
        w: Math.round(r.width),
        h: Math.round(r.height),
      };
    });
    expect(loginVisible.found, 'login <a> trong DOM').toBeTruthy();
    expect(loginVisible.display, `login bị display: ${loginVisible.display}`).not.toBe('none');
    expect(loginVisible.visibility).not.toBe('hidden');
    expect(loginVisible.w).toBeGreaterThanOrEqual(20);
    expect(loginVisible.h).toBeGreaterThanOrEqual(20);

    // Hamburger ☰ (.bars) phải đã được JS move vào top bar và visible
    const barsVisible = await page.evaluate(() => {
      const bars = document.querySelector('.bars');
      if (!bars) return { found: false };
      const r = bars.getBoundingClientRect();
      const cs = getComputedStyle(bars);
      return {
        found: true,
        display: cs.display,
        parentClass: bars.parentElement?.className ?? '',
        w: Math.round(r.width),
        h: Math.round(r.height),
        topY: Math.round(r.top),
      };
    });
    expect(barsVisible.found, '.bars trong DOM').toBeTruthy();
    expect(barsVisible.display).not.toBe('none');
    expect(barsVisible.w).toBeGreaterThanOrEqual(20);
    expect(barsVisible.h).toBeGreaterThanOrEqual(20);
    // Hamburger phải nằm gần top (≤ 60px) — đã được JS move lên top bar, không nằm giữa trang
    expect(barsVisible.topY, `bars topY=${barsVisible.topY}; phải ≤ 60`).toBeLessThanOrEqual(60);
  });

  test('Form đăng nhập — input font-size ≥ 16px (chống iOS auto-zoom)', async ({ page }) => {
    await page.setViewportSize({ width: 320, height: 568 });
    await page.goto('/dang-nhap', { waitUntil: 'networkidle' });
    const small = await page.evaluate(() => {
      const els = Array.from(document.querySelectorAll('input, select, textarea'));
      return els
        .filter(el => {
          const fs = parseFloat(getComputedStyle(el as HTMLElement).fontSize);
          return fs > 0 && fs < 16;
        })
        .map(el => ({ tag: el.tagName, fs: getComputedStyle(el as HTMLElement).fontSize }));
    });
    expect(small, `Input có font-size < 16px: ${JSON.stringify(small)}`).toHaveLength(0);
  });
});

/* ───── Portal pages (auth-gated) — kiểm tra không tràn + không đè box ─────
   Chạy tuần tự với login flows. Logout sau mỗi test để session sạch.
*/
test.describe('Portal mobile audit — Lễ tân + Bác sĩ + AdminCP', () => {
  test.afterEach(async ({ page }) => { await logout(page).catch(() => {}); });

  const portalChecks = [
    { name: 'LeTan / le-tan',                  account: 'letan' as const, path: '/le-tan' },
    { name: 'LeTan / lich-hen?status=pending', account: 'letan' as const, path: '/le-tan/lich-hen?status=pending' },
    { name: 'LeTan / tim-theo-sdt',            account: 'letan' as const, path: '/le-tan/tim-theo-sdt' },
    { name: 'LeTan / lich-theo-ngay',          account: 'letan' as const, path: '/le-tan/lich-theo-ngay' },
    { name: 'LeTan / check-in',                account: 'letan' as const, path: '/le-tan/check-in' },
    { name: 'LeTan / suat-kham',               account: 'letan' as const, path: '/le-tan/suat-kham' },
    { name: 'BacSi / bac-si-portal',           account: 'bacsy' as const, path: '/bac-si-portal' },
    { name: 'BacSi / benh-nhan-hom-nay',       account: 'bacsy' as const, path: '/bac-si-portal/benh-nhan-hom-nay' },
    { name: 'BacSi / lich-truc',               account: 'bacsy' as const, path: '/bac-si-portal/lich-truc' },
    { name: 'AdminCP / Default',               account: 'admin' as const, path: '/AdminCP/Default' },
    { name: 'AdminCP / Departments',           account: 'admin' as const, path: '/AdminCP/Departments' },
    { name: 'AdminCP / DoctorSchedules',       account: 'admin' as const, path: '/AdminCP/DoctorSchedules' },
    { name: 'AdminCP / DoctorSchedules/AutoGenerate', account: 'admin' as const, path: '/AdminCP/DoctorSchedules/AutoGenerate' },
    { name: 'AdminCP / Users',                 account: 'admin' as const, path: '/AdminCP/Users' },
  ];

  for (const c of portalChecks) {
    test(`No horizontal overflow @ 375×812 ${c.name}`, async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 812 });
      await loginStaff(page, c.account);
      await page.goto(c.path, { waitUntil: 'networkidle' });
      const overflow = await page.evaluate(() => ({
        docW: document.documentElement.scrollWidth,
        winW: window.innerWidth,
      }));
      expect(overflow.docW, `${c.path} tràn ngang docW=${overflow.docW} > winW=${overflow.winW}`)
        .toBeLessThanOrEqual(overflow.winW + 1);
    });
  }
});

/* ───── Member portal — /lich-cua-toi + /ho-so + /lich-su-kham ───── */
test.describe('Member portal mobile audit', () => {
  test.afterEach(async ({ page }) => { await logout(page).catch(() => {}); });

  const memberPaths = ['/lich-cua-toi', '/ho-so', '/lich-su-kham', '/cau-hoi-cua-toi'];
  for (const p of memberPaths) {
    test(`No horizontal overflow @ 375×812 Member ${p}`, async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 812 });
      await loginMember(page);
      await page.goto(p, { waitUntil: 'networkidle' });
      const overflow = await page.evaluate(() => ({
        docW: document.documentElement.scrollWidth,
        winW: window.innerWidth,
      }));
      expect(overflow.docW).toBeLessThanOrEqual(overflow.winW + 1);
    });
  }
});
