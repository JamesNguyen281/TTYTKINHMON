$(document).ready(function() {
    // Global broken-image fallback. Khi <img> nào fail load, swap sang placeholder.
    // Áp dụng cho cả ảnh src="/" (image_path NULL trong DB) lẫn 404.
    var FALLBACK_IMG = '/assets/admin/images/none.png';
    $(document).on('error', 'img', function () {
        if (this.src && this.src.indexOf(FALLBACK_IMG) === -1) { this.src = FALLBACK_IMG; }
    });
    $('img').each(function () {
        var s = this.getAttribute('src');
        if (!s || s === '/' || s === '' || s.indexOf('//assets/') === 0) {
            this.src = FALLBACK_IMG;
        }
    });


   //  var offset = 220; 
   //  var duration = 500; 
   //  $(window).scroll(function() { 
   //      if ($(this).scrollTop() > offset) { 
   //         $('.back-to-top').fadeIn(duration); 
   //      } else { 
   //         $('.back-to-top').fadeOut(duration); 
   //      } 
   //  });
   //  $('.back-to-top').click(function(event) { 
   //     event.preventDefault(); 
   //     $('html, body').animate({ 
   //         scrollTop: 0 
   //     }, duration); 
   //     return false; 
   // }); 

    $(".slide-index").owlCarousel({
        items: 1,
        margin: 0,
        autoplay: true,
		loop:true,
        autoplayTimeout:15000,
        animateOut: 'fadeOut',
        autoplayHoverPause:false,
        nav: false,
        dots: false,
        navText: [ 
            '<i class="fa fa-chevron-left" aria-hidden="true"></i>', 
            '<i class="fa fa-chevron-right" aria-hidden="true"></i>' 
        ],
    });
    $(".slide-st2").owlCarousel({
        speed: 'slow',
        items: 1,
        margin: 0,
		loop:true,
        autoplay: true,
        autoplayTimeout: 15000,
        animateOut: 'fadeOut',
        autoplayHoverPause:false,
        nav: false,
        dots: true,
        navText: [ 
            '<i class="fa fa-chevron-left" aria-hidden="true"></i>', 
            '<i class="fa fa-chevron-right" aria-hidden="true"></i>' 
        ],
    });
    $(".header-nav .bars").off("click.mobnav").on("click.mobnav", function(e){
        e.preventDefault();
        $("#nav.menu_list, .nav-header .menu_list").toggleClass("open");
    });
    // Mobile (≤1199px): tap parent li có .sub_menu → toggle .sub-open thay vì navigate.
    // Chỉ hoạt động khi menu đang ở chế độ mobile (.menu_list.open).
    $(document).on("click.subnav", ".nav-header .menu_list.open > li.col-li-2 > a", function(e){
        var $li = $(this).parent("li");
        if ($li.find("> .sub_menu").length === 0) return; // không có sub thì để link đi bình thường
        if (window.matchMedia("(max-width: 1199px)").matches) {
            e.preventDefault();
            $li.toggleClass("sub-open").siblings(".sub-open").removeClass("sub-open");
        }
    });
    // .slide-st5 / .slide-list — Init thông minh: tránh Owl loop:true duplicate item.
    // Quy tắc: tối đa 4 card hiển thị 1 lần; nếu có >4 bác sĩ → carousel cuộn; nếu ≤4 → hiện tĩnh, ko duplicate.
    $(".slide-st5,.slide-list").each(function () {
        var $el = $(this);
        // Skip nếu marker .km-static-grid — đã có CSS flex-grid riêng, không cần owl
        if ($el.hasClass('km-static-grid')) return;
        var n = $el.children().length;
        var hasMore = n > 4;               // có cần cuộn không
        $el.owlCarousel({
            speed: 'slow',
            items: 4,                      // luôn 4 card width — items thiếu thì để trống bên phải
            margin: 25,
            loop: hasMore,                 // chỉ loop khi đủ item để cuộn
            autoplay: hasMore,
            autoplayTimeout: 15000,
            animateOut: hasMore ? 'fadeOut' : null,
            autoplayHoverPause: false,
            nav: hasMore,                  // ẩn prev/next khi không có gì để cuộn
            dots: false,
            navText: [
                '<i class="fa fa-angle-left" aria-hidden="true"></i>',
                '<i class="fa fa-angle-right" aria-hidden="true"></i>'
            ],
            responsive: {
                0:    { items: 1, nav: hasMore, loop: hasMore },
                768:  { items: Math.min(2, n || 1), nav: false, loop: hasMore },
                1000: { items: 4, nav: hasMore, loop: hasMore }
            }
        });
    });
    $(".slide-list-pgd").owlCarousel({
        speed: 'slow',
        items: 2,
        margin: 25,
		loop:true,
        autoplay: true,
        autoplayTimeout: 15000,
        animateOut: 'fadeOut',
        autoplayHoverPause:false,
        nav: true,
        dots: false,
        navText: [ 
            '<i class="fa fa-angle-left" aria-hidden="true"></i>', 
            '<i class="fa fa-angle-right" aria-hidden="true"></i>' 
        ],
        responsive:{
            0:{
                items:1,
                nav:true
            },
            768:{
                items:2,
                nav:false
            },
            1000:{
                items:2,
                nav:true,
                loop:false
            }
        },
    }); 
    // .slide-st7 KHÔNG dùng owl-carousel — template gốc dùng CSS grid với
    // .col-5 (width: 20% desktop, 50% mobile). Đoạn này từng được uncommented
    // và gây lỗi đè layout. Để nguyên grid CSS như template Long Phú gốc.

    $('.tab-js').each(function() { 
        var $active, $content, $links = $(this).find('a'); 
        $active = $($links.filter('[href="' + location.hash + '"]')[0] || $links[0]); 
        $active.parent().addClass('active'); 
        $content = $($active[0].hash); 
        $links.not($active).each(function() { 
            $(this.hash).hide(); 
        }); 
        $(this).on('click', 'a', function(e) { 
            $active.parent().removeClass('active'); 
            $content.hide(); 
            $active = $(this); 
            $content = $(this.hash); 
            $active.parent().addClass('active'); 
            $content.show(); 
            e.preventDefault(); 
        }); 
    }); 

    $(".view-more").click(function(){
        $(".item-detail-cs").addClass("show");
    });

    // ── Mobile header layout: dịch hamburger ☰ vào top bar khi ≤ 991px ──
    // Vì CSS-only position: absolute bị specificity của style.css cũ override,
    // ta dùng JS DOM-move để đảm bảo vị trí. Desktop ≥ 992px hoàn toàn không
    // chạm vào (early-return). Resize window thì auto-restore.
    var $bars = $(".header-nav .bars");
    var $topBarUl = $(".header-top-right ul");
    var $navRow = $(".header-nav .row");
    if ($bars.length && $topBarUl.length) {
        var $mobileBarsLi = $('<li class="mobile-bars-li" style="margin-left:auto;list-style:none;"></li>');
        function syncHeader() {
            var isMobile = window.matchMedia("(max-width: 991px)").matches;
            var inTopBar = $bars.parent().hasClass("mobile-bars-li");
            if (isMobile && !inTopBar) {
                $mobileBarsLi.append($bars);
                $topBarUl.append($mobileBarsLi);
            } else if (!isMobile && inTopBar) {
                $navRow.append($bars);
                $mobileBarsLi.detach();
            }
        }
        syncHeader();
        $(window).on("resize", syncHeader);
    }
});
