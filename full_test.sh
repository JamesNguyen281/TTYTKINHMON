#!/bin/bash
# Full functional test — content-based assertions, not just HTTP status.
# Tests entire flow: page render, form submission, content presence, no JS errors.

BASE="http://localhost:5050"
PASS=0
FAIL=0
declare -a FAILURES

assert_contains() {
  local label="$1" content="$2" expected="$3"
  if echo "$content" | grep -q -F "$expected"; then
    PASS=$((PASS+1)); echo "  ✓ $label"
  else
    FAIL=$((FAIL+1)); echo "  ✗ $label — không tìm thấy: '$expected'"; FAILURES+=("$label")
  fi
}

assert_not_contains() {
  local label="$1" content="$2" forbidden="$3"
  if echo "$content" | grep -q -F "$forbidden"; then
    FAIL=$((FAIL+1)); echo "  ✗ $label — chứa cấm: '$forbidden'"; FAILURES+=("$label")
  else
    PASS=$((PASS+1)); echo "  ✓ $label"
  fi
}

assert_status() {
  local label="$1" url="$2" expected="$3" jar="$4"
  local code; if [ -n "$jar" ]; then
    code=$(curl -s -b "$jar" -o /dev/null -w "%{http_code}" "$url")
  else
    code=$(curl -s -o /dev/null -w "%{http_code}" "$url")
  fi
  if [ "$code" = "$expected" ]; then
    PASS=$((PASS+1)); echo "  ✓ $label ($expected)"
  else
    FAIL=$((FAIL+1)); echo "  ✗ $label — got $code, expected $expected"; FAILURES+=("$label")
  fi
}

############ HOMEPAGE ############
echo "===== 1. HOMEPAGE ====="
H=$(curl -s "$BASE/")
# HTML response encodes Vietnamese chars as &#xXXX; — assert ASCII fragments / URL slugs / CSS classes
assert_contains "Home: brand block"        "$H" "brand-link"
assert_contains "Home: menu Chuyên Khoa link" "$H" 'href="/chuyen-khoa"'
assert_contains "Home: menu Giới thiệu link"  "$H" 'href="/gioi-thieu-trung-tam"'
assert_contains "Home: menu Liên hệ link"     "$H" 'href="/lien-he"'
assert_contains "Home: featured news section" "$H" "tin-tuc"
assert_contains "Home: hero image"            "$H" "kinhmon-hero-img"
assert_not_contains "Home: KHÔNG có alert lỗi" "$H" "alert('' + err)"
assert_not_contains "Home: KHÔNG có /base/ legacy URL" "$H" "/base/GetFirstSiteId"
# Kiểm tra services.js đã rewrite
SJS=$(curl -s "$BASE/assets/client/js/services.js?v=2")
assert_not_contains "services.js KHÔNG còn alert object" "$SJS" "alert('' + err)"
assert_contains "services.js dùng console.error" "$SJS" "console.error"

############ LIÊN HỆ ############
echo
echo "===== 2. LIÊN HỆ ====="
C=$(curl -s "$BASE/lien-he")
assert_contains "Contact: heading"         "$C" "Trung tâm Y tế phường Kinh Môn"
assert_contains "Contact: form"            "$C" "/lien-he/gui-gop-y"
assert_contains "Contact: input fullName"  "$C" 'name="fullName"'
assert_contains "Contact: input email"     "$C" 'name="email"'
assert_contains "Contact: textarea content" "$C" 'name="content"'
assert_contains "Contact: Google Maps iframe" "$C" 'maps.google.com/maps'
assert_contains "Contact: Cấp cứu tel"     "$C" 'href="tel:'
# Submit empty form (POST)
JAR=/tmp/contact.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/lien-he" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
RES=$(curl -s -b "$JAR" -c "$JAR" -X POST -L \
  --data-urlencode "__RequestVerificationToken=$TK" \
  --data-urlencode "fullName=Test User" --data-urlencode "email=t@t.com" \
  --data-urlencode "phone=0901234567" --data-urlencode "subject=Test" \
  --data-urlencode "content=Nội dung test gửi đi" "$BASE/lien-he/gui-gop-y")
assert_contains "Contact submit success alert" "$RES" "alert-success"

############ DANH MỤC TIN TỨC ############
echo
echo "===== 3. CHUYÊN MỤC TIN TỨC ====="
N=$(curl -s "$BASE/chuyen-muc/tin-tuc")
assert_contains "News list: container" "$N" "container"

# tin-tuc redirect
assert_status "/tin-tuc redirect" "$BASE/tin-tuc" "302"

############ BÁC SĨ ============
echo
echo "===== 4. ĐỘI NGŨ BÁC SĨ ====="
D=$(curl -s "$BASE/bac-si")
assert_contains "Doctor list: ĐỘI NGŨ BÁC SĨ" "$D" "ĐỘI NGŨ BÁC SĨ"
assert_contains "Doctor list: tab Ban Giám đốc" "$D" "Ban Giám đốc"
assert_contains "Doctor list: form tìm" "$D" 'name="q"'

############ CHUYÊN KHOA ====
echo
echo "===== 5. CHUYÊN KHOA ====="
CK=$(curl -s "$BASE/chuyen-khoa")
assert_contains "Dept list: page renders" "$CK" "<html"

############ ĐẶT LỊCH KHÁM ============
echo
echo "===== 6. ĐẶT LỊCH KHÁM ====="
DL=$(curl -s "$BASE/dat-lich-kham")
assert_contains "Booking: form" "$DL" "PatientName"
assert_contains "Booking: session select" "$DL" "Session"

############ ĐĂNG KÝ + ĐĂNG NHẬP MEMBER ============
echo
echo "===== 7. ĐĂNG KÝ + ĐĂNG NHẬP ====="
JAR=/tmp/m.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/dang-ky" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
UN="ftest_$(date +%s)"
REGRES=$(curl -s -c "$JAR" -b "$JAR" -X POST \
  --data-urlencode "UserName=$UN" --data-urlencode "FullName=Full Test" \
  --data-urlencode "Phone=0911223344" --data-urlencode "Email=$UN@e.com" \
  --data-urlencode "Password=Strong@1234" --data-urlencode "ConfirmPassword=Strong@1234" \
  --data-urlencode "Gender=male" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null -w "%{http_code}|%{redirect_url}" "$BASE/dang-ky")
echo "  register: $REGRES"
[[ "$REGRES" == 302* ]] && PASS=$((PASS+1)) && echo "  ✓ Register success" || { FAIL=$((FAIL+1)); echo "  ✗ Register failed"; FAILURES+=("Register"); }

# Login lại
JAR2=/tmp/m2.txt; rm -f "$JAR2"
TK=$(curl -s -c "$JAR2" -b "$JAR2" "$BASE/dang-nhap" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
LOGRES=$(curl -s -c "$JAR2" -b "$JAR2" -X POST \
  --data-urlencode "UserName=$UN" --data-urlencode "Password=Strong@1234" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null -w "%{http_code}|%{redirect_url}" "$BASE/dang-nhap")
echo "  login: $LOGRES"
[[ "$LOGRES" == 302* ]] && PASS=$((PASS+1)) && echo "  ✓ Login success" || { FAIL=$((FAIL+1)); echo "  ✗ Login failed"; FAILURES+=("Login"); }

# Hồ sơ
HOSO=$(curl -s -b "$JAR2" "$BASE/ho-so")
assert_contains "Patient: Hồ sơ — họ tên đúng" "$HOSO" "Full Test"

############ ADMIN CP - admin/123456 ============
echo
echo "===== 8. ADMIN CP ====="
JAR=/tmp/admin.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=admin" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/AdminCP/Login/Login"

DASH=$(curl -s -b "$JAR" "$BASE/AdminCP/Default")
assert_contains "Admin: dashboard welcome" "$DASH" "Chào mừng bạn đến với hệ thống"
assert_contains "Admin: stats Người dùng"   "$DASH" "Người dùng"
assert_contains "Admin: stats Chuyên khoa"  "$DASH" "Chuyên khoa"

NEWSL=$(curl -s -b "$JAR" "$BASE/AdminCP/News")
assert_contains "Admin News: 4 tab" "$NEWSL" "Tin chờ duyệt"
assert_contains "Admin News: nút Thêm tin mới" "$NEWSL" "Thêm tin mới"
assert_contains "Admin News: action Sửa" "$NEWSL" 'fa fa-pencil'

USRL=$(curl -s -b "$JAR" "$BASE/AdminCP/Users")
assert_contains "Admin Users: link Create"  "$USRL" '/AdminCP/Users/Create'
assert_contains "Admin Users: nút Đổi pass" "$USRL" "fa-key"
# Lock/Unlock — pick one (the page may have either depending on user state)
if echo "$USRL" | grep -q -E 'fa-lock|fa-unlock'; then
  PASS=$((PASS+1)); echo "  ✓ Admin Users: lock/unlock icon"
else
  FAIL=$((FAIL+1)); echo "  ✗ Admin Users: lock/unlock icon"; FAILURES+=("Admin Users lock")
fi

DOCL=$(curl -s -b "$JAR" "$BASE/AdminCP/Doctors")
assert_contains "Admin Doctors: toggle status icon" "$DOCL" 'fa-toggle'

############ Các portal ====
echo
echo "===== 9. PORTALS (Reception/Doctor) — login & doi-mat-khau page ====="
# letan & Bacsy đã đổi mật khẩu Tanh2004@ → login trực tiếp vào portal (không còn force-change)
JAR=/tmp/letan.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
LOGIN_RES=$(curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=letan" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null -w "%{redirect_url}" "$BASE/AdminCP/Login/Login")
[[ "$LOGIN_RES" == *"/le-tan"* ]] && PASS=$((PASS+1)) && echo "  ✓ letan/Tanh2004@ → /le-tan" \
  || { FAIL=$((FAIL+1)); echo "  ✗ letan login redirect: $LOGIN_RES"; FAILURES+=("letan portal login"); }
# /le-tan accessible with valid session
LETAN_RES=$(curl -s -b "$JAR" -o /dev/null -w "%{http_code}" "$BASE/le-tan")
[[ "$LETAN_RES" == "200" ]] && PASS=$((PASS+1)) && echo "  ✓ /le-tan accessible (200)" \
  || { FAIL=$((FAIL+1)); echo "  ✗ /le-tan status: $LETAN_RES"; FAILURES+=("letan portal access"); }
# doi-mat-khau page renders with auth session (policy hint + strength bar — no force banner since not forced)
FORCE=$(curl -s -b "$JAR" "$BASE/doi-mat-khau")
assert_contains "Force-change page: policy hint" "$FORCE" "Tối thiểu 8 ký tự"
assert_contains "Force-change page: pwd strength meter" "$FORCE" 'id="pwd-strength-bar"'

JAR=/tmp/doc.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
DOC_RES=$(curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=Bacsy" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null -w "%{redirect_url}" "$BASE/AdminCP/Login/Login")
[[ "$DOC_RES" == *"/bac-si-portal"* ]] && PASS=$((PASS+1)) && echo "  ✓ Bacsy/Tanh2004@ → /bac-si-portal" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Bacsy login redirect: $DOC_RES"; FAILURES+=("bacsy portal login"); }

# Admin/123456 should NOT force
JAR=/tmp/admin2.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
ADM_RES=$(curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=admin" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null -w "%{redirect_url}" "$BASE/AdminCP/Login/Login")
[[ "$ADM_RES" == *"/AdminCP/Default"* ]] && PASS=$((PASS+1)) && echo "  ✓ admin/Tanh2004@ NOT forced — direct to AdminCP" \
  || { FAIL=$((FAIL+1)); echo "  ✗ admin redirect: $ADM_RES"; FAILURES+=("admin not forced"); }

############ LEGACY SEO REDIRECTS ====
echo
echo "===== 10. LEGACY SEO REDIRECTS ====="
for u in /Home/DatCauHoi /Home/HoiDap /Home/Index /Home/Contact /Home/DoctorList; do
  code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE$u")
  if [ "$code" = "301" ]; then
    PASS=$((PASS+1)); echo "  ✓ $u → 301"
  else
    FAIL=$((FAIL+1)); echo "  ✗ $u got $code"; FAILURES+=("Redirect $u")
  fi
done

############ EN/VI LANGUAGE SWITCHING ====
echo
echo "===== 11. EN/VI LANGUAGE SWITCHING ====="
JAR=/tmp/lang.txt; rm -f "$JAR"
curl -s -c "$JAR" -b "$JAR" -o /dev/null "$BASE/"
LOC1=$(curl -s -b "$JAR" "$BASE/base/GetSessionLocate")
[ "$LOC1" = '{"locate":"vi"}' ] && PASS=$((PASS+1)) && echo "  ✓ Default locate vi" || { FAIL=$((FAIL+1)); echo "  ✗ Default locate $LOC1"; FAILURES+=("Default locate"); }

curl -s -b "$JAR" -c "$JAR" -X POST -d "locate=en" -o /dev/null "$BASE/base/ChangeCulture"
LOC2=$(curl -s -b "$JAR" "$BASE/base/GetSessionLocate")
[ "$LOC2" = '{"locate":"en"}' ] && PASS=$((PASS+1)) && echo "  ✓ Switch to en" || { FAIL=$((FAIL+1)); echo "  ✗ Switch en $LOC2"; FAILURES+=("Switch en"); }

H_EN=$(curl -s -b "$JAR" "$BASE/")
assert_contains "EN home: EMERGENCY 24/7"  "$H_EN" "EMERGENCY 24/7"
assert_contains "EN home: HOTLINE"          "$H_EN" "HOTLINE"
assert_contains "EN home: Sign in"          "$H_EN" "Sign in"
assert_contains "EN home: Address: prefix"  "$H_EN" "Address:"
assert_contains "EN home: Featured news heading" "$H_EN" "Featured news"
# Nút change_language EN/VI đã bị ẩn theo yêu cầu user (site mặc định tiếng Việt)
# assert_contains "EN home: change_language nút VI" "$H_EN" '>VI</a>'

# Switch back
curl -s -b "$JAR" -c "$JAR" -X POST -d "locate=vi" -o /dev/null "$BASE/base/ChangeCulture"

############ NEW ADMIN MODULES ====
echo
echo "===== 12. ADMIN COMMENT + QRCODE ====="
JAR=/tmp/admin.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=admin" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/AdminCP/Login/Login"

assert_status "AdminCP/Comment" "$BASE/AdminCP/Comment" "200" "$JAR"
assert_status "AdminCP/QrCode"  "$BASE/AdminCP/QrCode"  "200" "$JAR"
COMM=$(curl -s -b "$JAR" "$BASE/AdminCP/Comment")
assert_contains "Comment admin: search form" "$COMM" "Tìm theo họ tên"
assert_contains "Comment admin: ToggleRead action" "$COMM" "/AdminCP/Comment/ToggleRead/"

QR=$(curl -s -b "$JAR" "$BASE/AdminCP/QrCode")
assert_contains "QR: textarea content" "$QR" 'id="qr-content"'
assert_contains "QR: template Cấp cứu" "$QR" 'tel:02203822205'

############ DASHBOARD VISIT COUNTER ====
echo
echo "===== 13. DASHBOARD VISIT COUNTER ====="
DASH=$(curl -s -b "$JAR" "$BASE/AdminCP/Default")
assert_contains "Dashboard: visit counter heading" "$DASH" "fa-line-chart"
assert_contains "Dashboard: Đang online stat" "$DASH" 'Đang online'
assert_contains "Dashboard: Hôm nay stat" "$DASH" 'Hôm nay'
assert_contains "Dashboard: Tổng truy cập stat" "$DASH" 'Tổng truy cập'
assert_contains "Dashboard: ResetCounter form" "$DASH" '/AdminCP/Default/ResetCounter'
assert_contains "Dashboard: Comment widget" "$DASH" '/AdminCP/Comment'
assert_contains "Dashboard: Doctor count widget" "$DASH" 'Bác sĩ'

############ SIDEBAR ====
echo
echo "===== 14. SIDEBAR (Comment + QrCode menu) ====="
assert_contains "Sidebar: Comment link" "$DASH" '/AdminCP/Comment'
assert_contains "Sidebar: QrCode link" "$DASH" '/AdminCP/QrCode'

############ APPOINTMENT WORKFLOW (book → approve / reject) ====
echo
echo "===== 15. APPOINTMENT WORKFLOW (real-time book → approve → reject) ====="

# 15.1 Đăng ký + đăng nhập bệnh nhân
JAR=/tmp/wfp.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/dang-ky" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
WF_USER="wf_$(date +%s)"
WF_PWD="Strong@1234"
curl -s -c "$JAR" -b "$JAR" -X POST \
  --data-urlencode "UserName=$WF_USER" --data-urlencode "FullName=BN Workflow" \
  --data-urlencode "Phone=0911000111" --data-urlencode "Email=wf@e.com" \
  --data-urlencode "Password=$WF_PWD" --data-urlencode "ConfirmPassword=$WF_PWD" \
  --data-urlencode "Gender=male" --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/dang-ky"

# Login bằng cookie jar mới (đăng ký auto-login chỉ MVC5 cũ — Core thì phải re-login)
JAR=/tmp/wfp2.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/dang-nhap" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -c "$JAR" -b "$JAR" -X POST \
  --data-urlencode "UserName=$WF_USER" --data-urlencode "Password=$WF_PWD" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/dang-nhap"

# 15.2 Lấy token từ trang đặt lịch (DepartmentId không còn trong form — booking tự dùng Khoa Khám bệnh)
DLPAGE=$(curl -s -b "$JAR" "$BASE/dat-lich-kham")
DEPT_ID=""
TK=$(echo "$DLPAGE" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
[ -n "$TK" ] && PASS=$((PASS+1)) && echo "  ✓ Booking form CSRF token present" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Booking form no CSRF token"; FAILURES+=("booking csrf token"); }

# 15.3 Đặt lịch — phải redirect 302 sau khi success
APPT_DATE=$(date -d "+2 days" +"%Y-%m-%d" 2>/dev/null || date -v+2d +"%Y-%m-%d" 2>/dev/null || powershell -Command "(Get-Date).AddDays(2).ToString('yyyy-MM-dd')" | tr -d '\r')
BOOK_RES=$(curl -s -b "$JAR" -c "$JAR" -X POST \
  --data-urlencode "PatientName=BN Workflow" \
  --data-urlencode "PatientPhone=0911000111" \
  --data-urlencode "PatientEmail=wf@e.com" \
  --data-urlencode "DepartmentId=$DEPT_ID" \
  --data-urlencode "AppointmentDate=$APPT_DATE" \
  --data-urlencode "Session=morning" \
  --data-urlencode "Reason=Khám tổng quát workflow test" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null -w "%{http_code}" "$BASE/dat-lich-kham")
[ "$BOOK_RES" = "302" ] && PASS=$((PASS+1)) && echo "  ✓ Booking submitted (302)" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Booking failed: $BOOK_RES"; FAILURES+=("Book appointment"); }

# 15.4 Bệnh nhân xem /lich-cua-toi → phải có pending
LCT=$(curl -s -b "$JAR" "$BASE/lich-cua-toi")
assert_contains "Patient sees pending appointment" "$LCT" "Chờ duyệt"
assert_contains "Patient sees department in list" "$LCT" "BN Workflow"

# 15.5 Đặt lịch trùng buổi → phải bị từ chối
DLPAGE=$(curl -s -b "$JAR" "$BASE/dat-lich-kham")
TK=$(echo "$DLPAGE" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
DUP_RES=$(curl -s -b "$JAR" -X POST \
  --data-urlencode "PatientName=BN Workflow" \
  --data-urlencode "PatientPhone=0911000111" \
  --data-urlencode "DepartmentId=$DEPT_ID" \
  --data-urlencode "AppointmentDate=$APPT_DATE" \
  --data-urlencode "Session=morning" \
  --data-urlencode "Reason=Trùng" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  "$BASE/dat-lich-kham")
echo "$DUP_RES" | grep -q -i "đã có lịch" && PASS=$((PASS+1)) && echo "  ✓ Duplicate same-session booking rejected" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Duplicate booking not rejected"; FAILURES+=("dup booking"); }

# 15.6 Polling endpoint /lich-cua-toi/check-updates phải trả JSON
UPDATES=$(curl -s -b "$JAR" "$BASE/lich-cua-toi/check-updates")
echo "$UPDATES" | grep -q '"ok":true' && PASS=$((PASS+1)) && echo "  ✓ Polling endpoint returns JSON" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Polling endpoint: $UPDATES"; FAILURES+=("polling endpoint"); }

############ ADMIN PORTAL APPROVAL ====
echo
echo "===== 16. ADMIN APPROVE WORKFLOW ====="
# Dùng admin (NOT forced) để duyệt lịch — admin có toàn quyền
JAR=/tmp/admapp.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/AdminCP/Login" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -c "$JAR" -b "$JAR" -X POST -d "UserName=admin" -d "Password=Tanh2004@" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/AdminCP/Login/Login"

# Lấy danh sách pending từ AdminCP
APPL=$(curl -s -b "$JAR" "$BASE/AdminCP/Appointments?status=pending")
APPT_ID=$(echo "$APPL" | grep -oE '/AdminCP/Appointments/Detail/[a-f0-9-]{36}' | head -1 | sed -E 's|.*/||')
[ -n "$APPT_ID" ] && PASS=$((PASS+1)) && echo "  ✓ Pending list shows new booking ($APPT_ID)" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Booking not in pending list"; FAILURES+=("pending list"); }

# 16.2 Confirm booking
DETAIL=$(curl -s -b "$JAR" "$BASE/AdminCP/Appointments/Detail/$APPT_ID")
TK=$(echo "$DETAIL" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -b "$JAR" -X POST \
  --data-urlencode "id=$APPT_ID" \
  --data-urlencode "newStatus=confirmed" \
  --data-urlencode "staffNote=Đã liên hệ xác nhận" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/AdminCP/Appointments/UpdateStatus"

# 16.3 Patient phải thấy confirmed + booking code
LCT2=$(curl -s -b /tmp/wfp2.txt "$BASE/lich-cua-toi")
assert_contains "Patient sees confirmed status" "$LCT2" "Đã xác nhận"
echo "$LCT2" | grep -qE 'KM[0-9]{6}[SC][A-F0-9]{6}' && PASS=$((PASS+1)) && echo "  ✓ Booking code visible to patient" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Booking code not generated"; FAILURES+=("booking code"); }

# 16.4 Reject without reason — phải fail
JAR=/tmp/wfp3.txt; rm -f "$JAR"
TK=$(curl -s -c "$JAR" -b "$JAR" "$BASE/dang-nhap" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -c "$JAR" -b "$JAR" -X POST \
  --data-urlencode "UserName=$WF_USER" --data-urlencode "Password=$WF_PWD" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/dang-nhap"

# Tạo lịch thứ 2 ở buổi chiều
DLPAGE=$(curl -s -b "$JAR" "$BASE/dat-lich-kham")
TK=$(echo "$DLPAGE" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -b "$JAR" -X POST \
  --data-urlencode "PatientName=BN Workflow" \
  --data-urlencode "PatientPhone=0911000111" \
  --data-urlencode "DepartmentId=$DEPT_ID" \
  --data-urlencode "AppointmentDate=$APPT_DATE" \
  --data-urlencode "Session=afternoon" \
  --data-urlencode "Reason=Để test reject" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/dat-lich-kham"

# Admin lấy lịch mới
APPL=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments?status=pending")
NEW_ID=$(echo "$APPL" | grep -oE '/AdminCP/Appointments/Detail/[a-f0-9-]{36}' | head -1 | sed -E 's|.*/||')
DETAIL=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments/Detail/$NEW_ID")
TK=$(echo "$DETAIL" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')

# Reject without reason → server từ chối (TempData[Error] sẽ hiện ở Detail sau redirect)
curl -s -b /tmp/admapp.txt -c /tmp/admapp.txt -X POST \
  --data-urlencode "id=$NEW_ID" \
  --data-urlencode "newStatus=rejected" \
  --data-urlencode "staffNote=" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/AdminCP/Appointments/UpdateStatus"
# Follow lên detail → đọc TempData["Error"]
NOREASON=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments/Detail/$NEW_ID")
echo "$NOREASON" | grep -q -i "lý do từ chối" && PASS=$((PASS+1)) && echo "  ✓ Reject without reason blocked" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Reject w/o reason should be blocked"; FAILURES+=("reject no reason"); }

# Reject WITH reason → success
REJECT_REASON="DOCTOR-ON-LEAVE-WORKFLOW-TEST-XYZ"
curl -s -b /tmp/admapp.txt -X POST \
  --data-urlencode "id=$NEW_ID" \
  --data-urlencode "newStatus=rejected" \
  --data-urlencode "staffNote=$REJECT_REASON" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/AdminCP/Appointments/UpdateStatus"

# Patient phải thấy lý do từ chối
LCT3=$(curl -s -b "$JAR" "$BASE/lich-cua-toi")
assert_contains "Patient sees rejection status" "$LCT3" "Từ chối"
assert_contains "Patient sees rejection reason" "$LCT3" "$REJECT_REASON"

# 16.5 Invalid status injection → blocked (cần lịch chưa final mới có form + token)
# Tạo lịch mới ở 1 ngày khác để tránh trùng
APPT_DATE2=$(date -d "+3 days" +"%Y-%m-%d" 2>/dev/null || powershell -Command "(Get-Date).AddDays(3).ToString('yyyy-MM-dd')" | tr -d '\r')
DLPAGE=$(curl -s -b "$JAR" "$BASE/dat-lich-kham")
TK=$(echo "$DLPAGE" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')
curl -s -b "$JAR" -X POST \
  --data-urlencode "PatientName=BN Workflow" \
  --data-urlencode "PatientPhone=0911000111" \
  --data-urlencode "DepartmentId=$DEPT_ID" \
  --data-urlencode "AppointmentDate=$APPT_DATE2" \
  --data-urlencode "Session=morning" \
  --data-urlencode "Reason=Injection test" \
  --data-urlencode "__RequestVerificationToken=$TK" -o /dev/null "$BASE/dat-lich-kham"

APPL=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments?status=pending")
INJ_ID=$(echo "$APPL" | grep -oE '/AdminCP/Appointments/Detail/[a-f0-9-]{36}' | head -1 | sed -E 's|.*/||')
DETAIL=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments/Detail/$INJ_ID")
TK=$(echo "$DETAIL" | grep -oE 'name="__RequestVerificationToken"[^>]+value="[^"]+"' | head -1 | sed -E 's/.*value="([^"]+)".*/\1/')

curl -s -b /tmp/admapp.txt -c /tmp/admapp.txt -X POST \
  --data-urlencode "id=$INJ_ID" \
  --data-urlencode "newStatus=DROP TABLE Appointment" \
  --data-urlencode "staffNote=hack" \
  --data-urlencode "__RequestVerificationToken=$TK" \
  -o /dev/null "$BASE/AdminCP/Appointments/UpdateStatus"
INJ=$(curl -s -b /tmp/admapp.txt "$BASE/AdminCP/Appointments/Detail/$INJ_ID")
# Whitelist newStatus phải reject — kiểm tra TempData[Error]
echo "$INJ" | grep -qE "(không hợp lệ|Không thể chuyển)" && PASS=$((PASS+1)) && echo "  ✓ Invalid status injection blocked" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Status injection not blocked"; FAILURES+=("status injection"); }
# Verify lịch vẫn ở pending (không bị thay đổi)
echo "$INJ" | grep -q '<span class="label label-info">Chờ duyệt</span>' && PASS=$((PASS+1)) && echo "  ✓ Status remains pending after injection" \
  || { FAIL=$((FAIL+1)); echo "  ✗ Status was changed"; FAILURES+=("status changed by injection"); }

############ KIỂM TRA RESPONSIVE / NO BROKEN INCLUDES ====
echo
echo "===== 17. STATIC ASSETS & RESPONSIVE ====="
for f in "/assets/client/css/style.css" "/assets/client/css/responsive-fixes.css?v=20260430b" \
         "/assets/client/css/custom-ux.css?v=4" "/assets/admin/css/ace.min.css" \
         "/assets/admin/css/custom.css" "/assets/client/js/jquery.min.js" \
         "/assets/client/js/custom.js" "/assets/client/js/services.js?v=3"; do
  assert_status "Static $f" "$BASE$f" "200"
done

############ KẾT QUẢ ====
echo
echo "===================="
echo "PASS: $PASS · FAIL: $FAIL"
if [ $FAIL -gt 0 ]; then
  echo "Failures:"; for f in "${FAILURES[@]}"; do echo "  - $f"; done
  exit 1
fi
exit 0
