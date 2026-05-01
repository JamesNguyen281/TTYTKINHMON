import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright config cho TTYT Kinh Môn — ASP.NET Core 8 ở port 5050.
 *
 * Yêu cầu trước khi chạy:
 *   1. SQL Server Express up và DB ttytlp đã restore.
 *   2. Server Kestrel đang chạy: cd WebsiteCore/src/WebsiteCore.Web && dotnet run --urls http://localhost:5050
 *   3. Tài khoản test seed sẵn (xem docs/05_TestCases.md phụ lục Test Data).
 */
export default defineConfig({
  testDir: './specs',
  fullyParallel: false,         // workflow tests share DB state — chạy tuần tự
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,                    // 1 worker để tránh race trên DB shared
  reporter: [
    ['html', { open: 'never' }],
    ['list'],
  ],

  use: {
    baseURL: process.env.BASE_URL || 'http://localhost:5050',
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    actionTimeout: 10_000,
    navigationTimeout: 30_000,
    extraHTTPHeaders: {
      'Accept-Language': 'vi-VN,vi;q=0.9',
    },
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    // Smoke trên Firefox + Webkit, tuỳ chọn — bật khi cần
    // {
    //   name: 'firefox',
    //   use: { ...devices['Desktop Firefox'] },
    // },
    // {
    //   name: 'webkit',
    //   use: { ...devices['Desktop Safari'] },
    // },
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
      testMatch: /.*\.mobile\.spec\.ts/,
    },
  ],

  // Tự khởi server nếu chưa chạy. Comment lại nếu user start tay.
  // webServer: {
  //   command: 'dotnet run --project ../../WebsiteCore/src/WebsiteCore.Web --urls http://localhost:5050',
  //   url: 'http://localhost:5050',
  //   reuseExistingServer: !process.env.CI,
  //   timeout: 120_000,
  // },
});
