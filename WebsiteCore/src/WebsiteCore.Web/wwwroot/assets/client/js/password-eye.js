/* password-eye.js — auto-attach eye toggle to all <input type="password"> */
(function () {
    'use strict';
    var SVG_EYE = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>';
    var SVG_EYE_OFF = '<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>';

    function ensureStyles() {
        if (document.getElementById('km-pwd-eye-style')) return;
        var s = document.createElement('style');
        s.id = 'km-pwd-eye-style';
        s.textContent =
            '.km-pwd-wrap { position: relative; display: block; }' +
            '.km-pwd-wrap input { padding-right: 38px !important; }' +
            '.km-pwd-eye { position: absolute; top: 50%; right: 8px; transform: translateY(-50%);' +
            '  background: transparent; border: 0; padding: 4px; cursor: pointer; color: #6c757d;' +
            '  display: inline-flex; align-items: center; justify-content: center; line-height: 0;' +
            '  border-radius: 3px; transition: color 0.15s, background 0.15s; z-index: 5; }' +
            '.km-pwd-eye:hover { color: #0a3d62; background: rgba(10,61,98,0.08); }' +
            '.km-pwd-eye:focus { outline: 0; color: #0a3d62; }' +
            '.km-pwd-eye svg { display: block; }' +
            // Variant cho Bootstrap 3 .input-group (display:table) — KHÔNG bọc input
            // mà set position:relative cho .input-group + dùng absolute với offset chừa addon phải nếu có
            '.input-group.km-pwd-host { position: relative; }' +
            '.input-group.km-pwd-host > input.form-control { padding-right: 38px !important; }' +
            '.input-group.km-pwd-host > .km-pwd-eye { right: 8px; }';
        document.head.appendChild(s);
    }

    function makeButton(input) {
        var btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'km-pwd-eye';
        btn.setAttribute('aria-label', 'Hiện/ẩn mật khẩu');
        btn.setAttribute('tabindex', '-1');
        btn.innerHTML = SVG_EYE;
        btn.title = 'Hiện mật khẩu';
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            if (input.type === 'password') {
                input.type = 'text';
                btn.innerHTML = SVG_EYE_OFF;
                btn.title = 'Ẩn mật khẩu';
            } else {
                input.type = 'password';
                btn.innerHTML = SVG_EYE;
                btn.title = 'Hiện mật khẩu';
            }
        });
        return btn;
    }

    function attach(input) {
        if (!input || input.dataset.kmEye === '1') return;
        if (input.type !== 'password') return;
        // Skip hidden / disabled fields
        if (input.disabled || input.readOnly) return;

        input.dataset.kmEye = '1';
        var parent = input.parentNode;

        // Trường hợp 1: input nằm trong Bootstrap 3 .input-group (display:table) —
        // bọc thêm <span> sẽ vỡ table layout. Append button trực tiếp vào .input-group
        // và set position:relative cho host.
        if (parent && parent.classList && parent.classList.contains('input-group')) {
            parent.classList.add('km-pwd-host');
            parent.appendChild(makeButton(input));
            return;
        }

        // Trường hợp 2: tái dùng wrap đã có
        var wrap;
        if (parent && parent.classList && parent.classList.contains('km-pwd-wrap')) {
            wrap = parent;
        } else {
            wrap = document.createElement('span');
            wrap.className = 'km-pwd-wrap';
            parent.insertBefore(wrap, input);
            wrap.appendChild(input);
        }
        wrap.appendChild(makeButton(input));
    }

    function scan(root) {
        var inputs = (root || document).querySelectorAll('input[type="password"]');
        for (var i = 0; i < inputs.length; i++) attach(inputs[i]);
    }

    function init() {
        ensureStyles();
        scan(document);
        // Quan sát DOM cho input password thêm sau (modal, ajax, ...)
        if (typeof MutationObserver === 'function') {
            var obs = new MutationObserver(function (mutations) {
                for (var m = 0; m < mutations.length; m++) {
                    var nodes = mutations[m].addedNodes;
                    for (var n = 0; n < nodes.length; n++) {
                        var el = nodes[n];
                        if (!el || el.nodeType !== 1) continue;
                        if (el.matches && el.matches('input[type="password"]')) attach(el);
                        else scan(el);
                    }
                }
            });
            obs.observe(document.body, { childList: true, subtree: true });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
