/* services.js — JS client cho public site.
   Logic UI: chuyển ngôn ngữ, doctor detail modal (/Home/DoctorDetail), contact form binding.
   alert() được thay bằng console.error() để không popup phá UX. */
$(document).ready(function () {

    /* ============== Khởi tạo nhãn EN/VI từ session backend ============== */
    // Lần đầu: hỏi server xem locate hiện tại là gì → set label nút sang ngôn ngữ ĐỐI LẬP
    $.getJSON('/base/GetSessionLocate')
        .done(function (res) {
            var cur = (res && res.locate) ? res.locate : 'vi';
            try { localStorage.setItem('lang', cur); } catch (e) {}
            $('.change_language').text(cur === 'en' ? 'VI' : 'EN');
        })
        .fail(function () {
            // fallback đọc localStorage
            var cur; try { cur = localStorage.getItem('lang'); } catch (e) { cur = null; }
            $('.change_language').text(cur === 'en' ? 'VI' : 'EN');
        });

    /* ============== Đổi ngôn ngữ — POST /base/ChangeCulture ============== */
    $(document).on('click', '.change_language', function (e) {
        e.preventDefault();
        var cur = ($(this).text() || '').trim();
        // Nếu nút hiện "EN" → đang là vi, switch sang en. Ngược lại.
        var newLang = (cur === 'EN') ? 'en' : 'vi';
        $.ajax({
            url: '/base/ChangeCulture',
            method: 'POST',
            data: { locate: newLang },
            success: function () {
                try { localStorage.setItem('lang', newLang); } catch (err) {}
                window.location.reload();
            },
            error: function (err) { console.error('ChangeCulture failed:', err); }
        });
    });

    /* ============== Validate email ============== */
    var error    = $('.error-text');
    var regEmail = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

    /* ============== Form gửi mail (legacy /base/sendmail).
       Trang Liên hệ mới đã có form server-side POST /lien-he/gui-gop-y → bỏ qua nếu không có DOM. */
    $('.btn-contact').on('click', function () {
        var fullName    = $('#fullname').val();
        var phone       = $('#phone').val();
        var remark      = $('#remark').val();
        var description = $('#description').val();
        var email       = $('#email').val();

        if (!fullName)            { error.text('Vui lòng nhập tên của bạn');     $('#fullname').focus();    return false; }
        if (!phone)               { error.text('Vui lòng nhập số điện thoại');   $('#phone').focus();       return false; }
        if (!remark)              { error.text('Vui lòng nhập ý kiến của bạn');                              return false; }
        if (!description)         { error.text('Vui lòng nhập nội dung');         $('#description').focus(); return false; }
        if (!email)               { error.text('Vui lòng nhập email');            $('#email').focus();       return false; }
        if (!email.match(regEmail)) { error.text('Email chưa đúng định dạng');    $('#email').focus();       return false; }

        $.ajax({
            url: '/lien-he/gui-gop-y',
            method: 'POST',
            data: { fullName: fullName, phone: phone, email: email, subject: remark, content: description },
            success: function () {
                error.css('color', 'green');
                error.text('Cảm ơn bạn đã gửi phản hồi. Chúng tôi sẽ liên hệ lại sớm nhất.');
                $('#fullname').val(''); $('#phone').val(''); $('#remark').val('');
                $('#description').val(''); $('#email').val('');
            },
            error: function (err) { console.error('contact form error:', err); error.text('Lỗi gửi góp ý — vui lòng thử lại.'); }
        });
    });
    $('#fullname,#phone,#remark,#description,#email').on('keyup', function () { error.text(''); });

    /* ============== Doctor detail modal — gọi endpoint /Home/DoctorDetail ============== */
    $(document).on('click', '.btn-doctor-detail', function () {
        var id = $(this).data('id');
        if (!id) return;
        $.getJSON('/Home/DoctorDetail?id=' + encodeURIComponent(id), function (d) {
            var modal = $('#detaildoctor1, #doctor-detail');
            if (modal.length === 0) return;
            if (!d || !d.id) {
                console.warn('Doctor not found id=' + id);
                return;
            }
            modal.find('.name-doctor').text(d.name || '');
            modal.find('img').attr('src', d.image_path ? '/' + String(d.image_path).replace(/^\//, '') : '/assets/admin/images/user_none.jpg');
            modal.find('.department').text(d.department_name || d.specially || '');

            // Build lưới lịch trực 7 cột × 2 hàng (T2-CN × Sáng/Chiều)
            function buildScheduleGrid(schedules, cycleLabel, weekDates) {
                if (!schedules || schedules.length === 0) {
                    return '<p style="color:#6c757d; font-style:italic;"><i class="fa fa-info-circle"></i> Chưa có lịch trực cho tuần ' + (cycleLabel || '') + '.</p>';
                }
                // weekday: 1=CN, 2=T2..7=T7 → reorder T2-CN
                var orderedWd = [2, 3, 4, 5, 6, 7, 1];
                var wdShort = { 1: 'CN', 2: 'T2', 3: 'T3', 4: 'T4', 5: 'T5', 6: 'T6', 7: 'T7' };
                // Map weekday → dd/MM của tuần đại diện (lấy từ server)
                var dateByWd = {};
                if (weekDates && weekDates.length) {
                    weekDates.forEach(function (w) { dateByWd[w.weekday] = w.date; });
                }
                var byKey = {};
                schedules.forEach(function (s) { byKey[s.weekday + '|' + s.session] = s; });
                var html = '<div class="km-sched-grid">';
                html += '<div class="km-sched-cell km-sched-h-buoi">Buổi</div>';
                orderedWd.forEach(function (wd) {
                    var d = dateByWd[wd];
                    html += '<div class="km-sched-cell km-sched-h-day">'
                          + '<div class="wd-name">' + wdShort[wd] + '</div>'
                          + (d ? '<div class="wd-date">' + d + '</div>' : '')
                          + '</div>';
                });
                ['morning', 'afternoon'].forEach(function (sess) {
                    var sessLbl = sess === 'morning' ? '☀ Sáng' : '🌙 Chiều';
                    var sessClass = sess === 'morning' ? 'morning' : 'afternoon';
                    html += '<div class="km-sched-cell km-sched-h-sess">' + sessLbl + '</div>';
                    orderedWd.forEach(function (wd) {
                        var slot = byKey[wd + '|' + sess];
                        if (slot) {
                            html += '<div class="km-sched-cell"><div class="km-sched-block ' + sessClass + '">'
                                  + '<div class="rm">' + (slot.room || '-') + '</div>'
                                  + '<div class="qt">' + slot.max_patients + ' BN</div>'
                                  + '</div></div>';
                        } else {
                            html += '<div class="km-sched-cell"><span class="km-sched-empty">—</span></div>';
                        }
                    });
                });
                html += '</div>';
                return html;
            }

            var line = modal.find('.linemodal,.doctormodal-right');
            if (line.length) {
                var infoHtml = '<h3>' + (d.name || '') + '</h3>' +
                    (d.position       ? '<p><strong>' + d.position + '</strong></p>' : '') +
                    (d.specially      ? '<p><i class="fa fa-stethoscope"></i> ' + d.specially + '</p>' : '') +
                    (d.quantification ? '<p><strong>Bằng cấp:</strong> ' + d.quantification + '</p>' : '') +
                    (d.experiences   ? '<p><strong>Kinh nghiệm:</strong> ' + d.experiences + '</p>' : '') +
                    (d.timetable     ? '<p><strong>Lịch khám (ghi chú):</strong> ' + d.timetable + '</p>' : '');
                var schedHtml;
                if (d.is_management) {
                    schedHtml = '<div class="km-sched-section km-sched-mgmt">'
                              + '<h4><i class="fa fa-briefcase"></i> Lịch làm việc</h4>'
                              + '<p class="km-mgmt-note"><i class="fa fa-info-circle"></i> '
                              + (d.management_note || 'Vai trò quản lý — trực hành chính trong giờ làm việc.')
                              + '</p></div>';
                } else {
                    var schedHeading = d.week_label
                        ? 'Lịch trực ' + d.week_label.toLowerCase()
                        : 'Lịch trực ' + (d.cycle_label || '');
                    schedHtml = '<div class="km-sched-section"><h4><i class="fa fa-calendar-check-o"></i> ' + schedHeading + '</h4>'
                              + buildScheduleGrid(d.schedules, d.cycle_label, d.week_dates) + '</div>';
                }
                line.html(infoHtml + schedHtml);
            }
            // Reset scroll về đầu cho cả hai cột — tránh hiện tượng modal mở ra đã ở vị trí cuối
            // do người dùng vuốt ở bác sĩ trước đó (DOM cũ vẫn giữ scrollTop cho đến khi reset).
            modal.find('.km-doc-scroll, .km-doc-left').each(function () { this.scrollTop = 0; });
            modal.modal('show');
        }).fail(function (err) { console.error('DoctorDetail fail:', err); });
    });

    // Khi modal hiển thị xong (Bootstrap đã tính lại layout) — đảm bảo scroll thực sự ở đầu.
    $(document).on('shown.bs.modal', '#detaildoctor1, #doctor-detail', function () {
        $(this).find('.km-doc-scroll, .km-doc-left').each(function () { this.scrollTop = 0; });
    });
    // Khi modal đóng — reset luôn để lần mở lại không kế thừa state cũ trên một số trình duyệt.
    $(document).on('hidden.bs.modal', '#detaildoctor1, #doctor-detail', function () {
        $(this).find('.km-doc-scroll, .km-doc-left').each(function () { this.scrollTop = 0; });
    });

    /* ============== Comment trên bài tin (legacy /home/comment).
       Web mới có Comments table; nếu form không tồn tại thì không bind. */
    $('.btn-comment').on('click', function () {
        var $err   = $('.error');
        var name   = $('#username').val();
        var email  = $('#email').val();
        var msg    = $('#message').val();
        var newId  = $('#newID').val();
        if (!name)               { $err.text('Vui lòng nhập họ tên');           $('#username').focus(); return; }
        if (!email)              { $err.text('Vui lòng nhập email');            $('#email').focus();    return; }
        if (!msg)                { $err.text('Vui lòng nhập nội dung tin nhắn'); $('#message').focus();  return; }
        if (!email.match(regEmail)) { $err.text('Email chưa đúng định dạng');     $('#email').focus();    return; }
        $.ajax({
            url: '/home/comment',
            method: 'POST',
            data: { new_id: newId, user_name: name, email: email, message: msg },
            success: function () {
                $err.css('color', 'green');
                $err.text('Cảm ơn bạn đã phản hồi.');
                $('#username').val(''); $('#email').val(''); $('#message').val('');
            },
            error: function (err) { console.error('comment error:', err); $err.text('Lỗi gửi bình luận.'); }
        });
    });
    $('#username,#message').on('keyup', function () { $('.error').text(''); });
});
