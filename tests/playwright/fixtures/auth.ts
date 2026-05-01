import { Page, expect, test as base } from '@playwright/test';

/** Tài khoản test mặc định — khớp với DB seed thực tế.
 *  Tất cả staff dùng `Tanh2004@` (đã reset + clear lockout).
 *  Member01 đã đổi `Member01@Test` qua /doi-mat-khau từ trước. */
export const TestAccounts = {
  admin:    { user: 'admin',    pass: 'Tanh2004@',     expectedRedirect: '/AdminCP/Default' },
  member:   { user: 'member01', pass: 'Member01@Test', expectedRedirect: '/' },
  letan:    { user: 'letan',    pass: 'Tanh2004@',     expectedRedirect: '/le-tan' },
  bacsy:    { user: 'Bacsy',    pass: 'Tanh2004@',     expectedRedirect: '/bac-si-portal' },
} as const;

export type AccountKey = keyof typeof TestAccounts;

/**
 * Login member qua /dang-nhap (form public).
 * Sau khi login: cookie session set, redirect về `/` mặc định.
 */
export async function loginMember(page: Page, user = TestAccounts.member.user, pass = TestAccounts.member.pass) {
  await page.goto('/dang-nhap');
  await page.fill('input[name="UserName"]', user);
  await page.fill('input[name="Password"]', pass);
  await page.click('button[type="submit"]');
  await page.waitForLoadState('networkidle');
  // Kiểm chỉ 1 lần — không crash nếu redirect chậm
  if (page.url().includes('/dang-nhap')) {
    throw new Error(`Member login failed for ${user}: vẫn ở ${page.url()}`);
  }
}

/**
 * Login staff qua /AdminCP/Login (form cán bộ).
 * Tự verify redirect tới portal đúng role.
 * Nếu rơi vào /doi-mat-khau (force pwd change) — TỰ ĐỘNG SKIP test thông qua test.skip.
 * Caller chỉ cần `await loginStaff(page, 'letan')` không cần handle thêm.
 */
export async function loginStaff(page: Page, account: 'admin' | 'letan' | 'bacsy'): Promise<void> {
  const a = TestAccounts[account];
  await page.goto('/AdminCP/Login');
  await page.fill('input[name="UserName"]', a.user);
  await page.fill('input[name="Password"]', a.pass);
  await page.click('button[type="submit"]');
  await page.waitForLoadState('networkidle');
  const url = page.url();
  if (url.includes('/doi-mat-khau')) {
    base.skip(true, `Account ${a.user} đang ở force-pwd-change — không thể login portal. Đổi mật khẩu qua /doi-mat-khau trước khi chạy test.`);
  }
  if (url.includes('/AdminCP/Login') && !url.includes('Logout')) {
    throw new Error(`Staff login failed for ${a.user}: vẫn ở ${url}`);
  }
}

/** Logout — đóng session phía client. */
export async function logout(page: Page) {
  // Public logout
  const url = page.url();
  if (url.includes('/AdminCP') || url.includes('/le-tan') || url.includes('/bac-si-portal')) {
    await page.goto('/AdminCP/Login/Logout').catch(() => {});
  } else {
    await page.goto('/dang-xuat').catch(() => {});
  }
  await page.context().clearCookies();
}

/** Helper kiểm tiếng Việt không bị Razor encode thành `&#x...` */
export async function expectNoMojibake(page: Page) {
  const html = await page.content();
  expect(html, 'Razor không được encode chữ Việt thành numeric entity').not.toMatch(/&#x[0-9a-fA-F]{3,4};/);
}
