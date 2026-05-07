(function () {
    var STORAGE_KEY = 'admListLimit';
    var OPTS = [10, 20, 50, 100, 1000];
    var DEFAULT_LIMIT = 20;

    function getSavedLimit() {
        var saved = parseInt(localStorage.getItem(STORAGE_KEY), 10);
        if (isNaN(saved)) return DEFAULT_LIMIT;
        if (saved !== 0 && OPTS.indexOf(saved) === -1) return DEFAULT_LIMIT;
        return saved;
    }

    function setupTable(tbl) {
        if (tbl.dataset.admPaginate === 'off') return;
        if (tbl.closest('.adm-scroll')) return;
        if (tbl.closest('.list-scroll')) return;
        // Bo qua neu page da co server-side pagebar (controller paginate roi)
        var page = tbl.closest('.page-content');
        if (page && page.querySelector('.pagebar')) return;

        var tbody = tbl.querySelector('tbody');
        if (!tbody) return;
        var rows = Array.prototype.slice.call(tbody.children).filter(function (n) {
            return n.tagName === 'TR';
        });
        // Bang ngan thi khong can pagination — nguong = limit nho nhat
        if (rows.length <= OPTS[0]) return;

        // Wrap bang trong scroll box
        var scroll = document.createElement('div');
        scroll.className = 'adm-scroll';
        var parent = tbl.parentNode;
        // Neu table dang nam trong .table-responsive thi unwrap (de tranh nested scroll)
        if (parent && parent.classList && parent.classList.contains('table-responsive')) {
            scroll.appendChild(tbl);
            parent.parentNode.insertBefore(scroll, parent);
            parent.parentNode.removeChild(parent);
        } else {
            parent.insertBefore(scroll, tbl);
            scroll.appendChild(tbl);
        }

        // ===== Toolbar =====
        var bar = document.createElement('div');
        bar.className = 'adm-list-toolbar';

        // Left: limit selector
        var left = document.createElement('div');
        left.className = 'adm-list-left';

        var lbl = document.createElement('span');
        lbl.innerHTML = '<i class="fa fa-list"></i> Hiển thị';
        left.appendChild(lbl);

        var sel = document.createElement('select');
        sel.className = 'form-control input-sm';
        OPTS.forEach(function (v) {
            var o = document.createElement('option');
            o.value = String(v);
            o.text = String(v);
            sel.appendChild(o);
        });
        var allOpt = document.createElement('option');
        allOpt.value = '0';
        allOpt.text = 'Tất cả';
        sel.appendChild(allOpt);
        left.appendChild(sel);

        var info = document.createElement('span');
        info.className = 'adm-list-info';
        left.appendChild(info);

        bar.appendChild(left);

        // Right: prev / page indicator / next
        var right = document.createElement('div');
        right.className = 'adm-list-right';

        function makeBtn(html, title) {
            var b = document.createElement('button');
            b.type = 'button';
            b.className = 'btn btn-default btn-xs';
            b.innerHTML = html;
            b.title = title;
            return b;
        }
        var btnFirst = makeBtn('<i class="fa fa-angle-double-left"></i>', 'Trang đầu');
        var btnPrev  = makeBtn('<i class="fa fa-angle-left"></i>',        'Trước');
        var btnNext  = makeBtn('<i class="fa fa-angle-right"></i>',       'Sau');
        var btnLast  = makeBtn('<i class="fa fa-angle-double-right"></i>','Trang cuối');

        var pageInput = document.createElement('input');
        pageInput.type = 'number';
        pageInput.min = '1';
        pageInput.className = 'form-control input-sm adm-list-page-input';
        pageInput.title = 'Nhập trang rồi Enter';

        var ofTxt = document.createElement('span');
        ofTxt.className = 'adm-list-of';

        right.appendChild(btnFirst);
        right.appendChild(btnPrev);
        right.appendChild(document.createTextNode(' Trang '));
        right.appendChild(pageInput);
        right.appendChild(ofTxt);
        right.appendChild(btnNext);
        right.appendChild(btnLast);

        bar.appendChild(right);

        scroll.parentNode.insertBefore(bar, scroll);

        // ===== State =====
        var state = {
            limit: getSavedLimit(),
            page: 1,
            total: rows.length
        };

        function totalPages() {
            if (state.limit === 0) return 1;
            return Math.max(1, Math.ceil(state.total / state.limit));
        }

        function render() {
            var tp = totalPages();
            if (state.page < 1) state.page = 1;
            if (state.page > tp) state.page = tp;

            var start = state.limit === 0 ? 0 : (state.page - 1) * state.limit;
            var end   = state.limit === 0 ? state.total : start + state.limit;

            var shown = 0;
            rows.forEach(function (r, i) {
                var visible = (i >= start && i < end);
                r.style.display = visible ? '' : 'none';
                if (visible) shown++;
            });

            // Update toolbar
            sel.value = String(state.limit);
            pageInput.value = String(state.page);
            pageInput.max = String(tp);
            ofTxt.textContent = ' / ' + tp + ' ';
            info.textContent = state.limit === 0
                ? '— hiện tất cả ' + state.total
                : '— hiện ' + (shown === 0 ? 0 : (start + 1)) + '-' + (start + shown) + ' / ' + state.total;

            // Disable buttons at edges
            var atFirst = state.page <= 1;
            var atLast  = state.page >= tp;
            btnFirst.disabled = atFirst;
            btnPrev.disabled  = atFirst;
            btnNext.disabled  = atLast;
            btnLast.disabled  = atLast;

            // "Tat ca" -> hide page nav controls
            var hideNav = (state.limit === 0);
            right.style.visibility = hideNav ? 'hidden' : '';

            scroll.scrollTop = 0;
        }

        sel.addEventListener('change', function () {
            state.limit = parseInt(sel.value, 10);
            state.page = 1;
            try { localStorage.setItem(STORAGE_KEY, String(state.limit)); } catch (e) {}
            render();
        });
        btnFirst.addEventListener('click', function () { state.page = 1; render(); });
        btnPrev .addEventListener('click', function () { state.page--; render(); });
        btnNext .addEventListener('click', function () { state.page++; render(); });
        btnLast .addEventListener('click', function () { state.page = totalPages(); render(); });
        pageInput.addEventListener('change', function () {
            var v = parseInt(pageInput.value, 10);
            if (!isNaN(v)) { state.page = v; render(); }
        });
        pageInput.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') { e.preventDefault(); pageInput.blur(); }
        });

        render();
    }

    function init() {
        var tables = document.querySelectorAll('.main-content table.table');
        Array.prototype.forEach.call(tables, setupTable);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
