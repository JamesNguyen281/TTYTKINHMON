# Playwright E2E Suite — TTYT Kinh Môn

Bộ kịch bản kiểm thử tự động bằng **Playwright + TypeScript** cho website TTYT Kinh Môn (ASP.NET Core 8 — port 5050). Phục vụ đồ án tốt nghiệp K63 ĐH GTVT — đề tài *"Nghiên cứu, xây dựng website y tế và triển khai kiểm thử tự động bằng Playwright"*.

## Yêu cầu môi trường

- Node.js ≥ 18
- SQL Server Express với DB `ttytlp` đã restore
- Backend đang chạy ở `http://localhost:5050` (`dotnet run`)
- 4 tài khoản test: `admin`, `letan` (RECEPTION), `Bacsy` (DOCTOR), `member01` (MEMBER) với pass `123456`

## Cài đặt

```bash
cd tests/playwright
npm install
npx playwright install chromium
```

## Chạy test

| Lệnh | Mô tả |
|---|---|
| `npm test` | Chạy toàn bộ ~80+ test cases |
| `npm run test:headed` | Có hiển thị browser |
| `npm run test:ui-mode` | Mở Playwright UI Mode (time-travel debug) |
| `npm run report` | Mở báo cáo HTML sau khi chạy |
| `npm run codegen` | Sinh test code từ thao tác browser |

### Chạy theo nhóm

| Lệnh | Spec | Phủ TC |
|---|---|---|
| `npm run test:smoke` | smoke.spec.ts | TC-001, 002, 002b, 050, 052, 054 |
| `npm run test:auth` | auth.spec.ts | TC-005, 006, 020, 030, 042-44, 056-57 |
| `npm run test:register` | register.spec.ts | TC-002, 003, 004 |
| `npm run test:appointment` | appointment.spec.ts | TC-007 → 016 |
| `npm run test:myaccount` | myaccount.spec.ts | TC-017, 018, 019 |
| `npm run test:letan` | letan.spec.ts + letan-flow.spec.ts | TC-021 → 029 |
| `npm run test:bacsi` | bacsi.spec.ts | TC-031 → 035 |
| `npm run test:qna` | qna.spec.ts | TC-036 → 041, 075 |
| `npm run test:admincp` | admincp.spec.ts | TC-042 → 049, 057 |
| `npm run test:ui` | ui.spec.ts | TC-051, 053, 076 → 083 |
| `npm run test:security` | security.spec.ts | TC-055, 061 → 075 |

### Chạy theo loại kiểm thử (đề cương)

| Lệnh | Mô tả | Specs |
|---|---|---|
| `npm run test:ui-only` | **UI Testing** — giao diện | smoke + ui |
| `npm run test:functional` | **Functional Testing** — nghiệp vụ | auth + register + appointment + myaccount + letan + letan-flow + bacsi + qna + admincp |
| `npm run test:regression` | **Regression Testing** — bảo mật | security |

## Cấu trúc

```
tests/playwright/
├── package.json               ← scripts npm
├── playwright.config.ts       ← cấu hình runner (port 5050, vi-VN, Asia/Ho_Chi_Minh)
├── tsconfig.json
├── README.md                  ← file này
├── fixtures/
│   └── auth.ts                ← TestAccounts, loginMember, loginStaff, logout, expectNoMojibake
└── specs/
    ├── smoke.spec.ts          ← Public site + mojibake + 404
    ├── auth.spec.ts           ← Đăng nhập / cross-portal guard
    ├── register.spec.ts       ← Đăng ký tài khoản
    ├── appointment.spec.ts    ← UC07 Đặt lịch khám
    ├── myaccount.spec.ts      ← Hồ sơ + đổi mật khẩu (member)
    ├── letan.spec.ts          ← UC21 Lễ tân (smoke)
    ├── letan-flow.spec.ts     ← UC21 state machine đầy đủ
    ├── bacsi.spec.ts          ← UC24/31 Bác sĩ
    ├── qna.spec.ts            ← UC Q&A
    ├── admincp.spec.ts        ← UC42 Admin CMS CRUD
    ├── ui.spec.ts             ← UI redesign (5-col grid, modal, top-bar)
    └── security.spec.ts       ← Regression bảo mật (CSRF/XSS/IDOR/lockout)
```

## Mapping 83 Test Case → spec file

| Nhóm | TC range | Spec | Số TC |
|---|---|---|---|
| Public / UI Testing | TC-001, 002, 002b, 050, 052, 054, 076-083 | smoke + ui | 14 |
| Authentication | TC-005, 006, 020, 030, 042-44, 056-57 | auth + admincp | 9 |
| Register | TC-002, 003, 004 | register | 3 |
| UC07 Đặt lịch | TC-007 → 016 | appointment | 10 |
| UC18-19 Hồ sơ + pass | TC-017, 018, 019 | myaccount | 3 |
| UC21 Lễ tân | TC-021 → 029 | letan + letan-flow | 9 |
| UC24/31 Bác sĩ | TC-031 → 035 | bacsi | 5 |
| UC Q&A | TC-036 → 041, 075 | qna | 7 |
| UC42 Admin CMS | TC-045 → 049, 058-60 | admincp | 8 |
| UI redesign | TC-051, 053, 076 → 083 | ui | 10 |
| Security regression | TC-055, 061 → 075 | security | 16 |

## Tài khoản test

| Account | Username | Password | Group |
|---|---|---|---|
| Admin  | `admin` | `123456` | ADMIN |
| Lễ tân | `letan` | `123456` | RECEPTION |
| Bác sĩ | `Bacsy` | `123456` | DOCTOR |
| Bệnh nhân | `member01` | `123456` | MEMBER |

> **Lưu ý**: nếu staff account đang ở trạng thái force-pwd-change (đang dùng mật khẩu mặc định), test login có thể redirect `/doi-mat-khau` thay vì portal — phải đổi pass trước khi chạy.

## Test results

Sau khi chạy `npm test`, Playwright tạo:
- `playwright-report/index.html` — báo cáo HTML đầy đủ
- `test-results/<spec>/<test>/trace.zip` — Trace Viewer cho test fail (mở bằng `npx playwright show-trace`)
- Video / screenshot cho test fail

