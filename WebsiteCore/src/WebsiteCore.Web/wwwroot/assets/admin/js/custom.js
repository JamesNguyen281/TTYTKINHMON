/*
*
*Author : TBD
*
* Copyright Laplo company
*/
$(document).ready(function () {
    $('.select2').select2();

    $('.focus').focus();
    $(".nav-tabs li").on("click", function () {
        setTimeout(() => {
            $('.focus').focus();
        },0)
    });
    var url = window.location.href;
    var activePage = '/AdminCP/' + url.split('/AdminCP/')[1];

    if (activePage.indexOf('?') > 0) {

        activePage = activePage.split('?');
        if (activePage[1].indexOf('&') > 0) {
            a = activePage[1].split('&');
            if (a[0] == "type=complete") {
                activePage = activePage[0] + "?" + a[0];
            } else if (a[0] == "type=waiting") {
                activePage = activePage[0] + "?" + a[0];
            } else if (a[0] == "type=no") {
                activePage = activePage[0] + "?" + a[0];
            } else if (a[0] == "type=delete") {
                activePage = activePage[0] + "?" + a[0];
            } else {
                activePage = activePage[0];
            }
        } else {
            if (activePage[1] == "type=complete") {
                activePage = activePage[0] + "?" + activePage[1];
            } else if (activePage[1] == "type=waiting") {
                activePage = activePage[0] + "?" + activePage[1];
            } else if (activePage[1] == "type=no") {
                activePage = activePage[0] + "?" + activePage[1];
            } else if (activePage[1] == "type=delete") {
                activePage = activePage[0] + "?" + activePage[1];
            } else {
                activePage = activePage[0];
            }
        }
    } else {
        if (activePage.indexOf('/Create') > 0) {
            activePage = activePage.split('/Create')[0];
            if (activePage == '/AdminCP/News') {
                activePage += '?type=waiting';
            }
        } else if (activePage.indexOf('/Edit') > 0) {
            activePage = activePage.split('/Edit')[0];
            if (activePage == '/AdminCP/News') {
                activePage += '?type=waiting';
            }
        }
        if (window.location.pathname == '/AdminCP/News') {
            activePage = '/AdminCP/News?type=waiting';
        }
    }
    $('.nav-list li a').each(function () {
        var currentPage = '/AdminCP/' + this.href.split('/AdminCP/')[1];
        if (activePage == currentPage) {
            $(this).parent().addClass('active');
            $(this).parent('li').parent('ul').parent('li').addClass('menu-open');
            $(this).parent('li').parent('ul').css("display", " block");
        }
    });
    /***********    Change language ****************/
    $(window).load(function () {
        $.ajax({
            url: '/AdminCP/Base/GetCurrentCulture',
            method: 'GET',
            dataType: 'json',
            success: function (res) {
                if (res.located == 'vi') {
                    $('#img_language').attr('src', '../../../assets/img/en.png');
                } else if (res.located == 'en') {
                    $('#img_language').attr('src', '../../../assets/img/vn.png');
                }
            }
        })
    })
    $('#changed_language').click(function () {
        $.ajax({
            url: '/AdminCP/Base/GetCurrentCulture',
            method: 'GET',
            dataType: 'json',
            success: function (res) {
                if (res.located == 'vi') {
                    $('#img_language').attr('src', '../../../assets/img/vn.png');
                    ChangeCulture('en');
                } else if (res.located == 'en') {
                    $('#img_language').attr('src', '../../../assets/img/en.png');
                    ChangeCulture('vi');
                }
            }
        })
    })

    function ChangeCulture(language) {
        $.ajax({
            url: '/AdminCP/Base/ChangeCulture',
            method: 'POST',
            dataType: 'json',
            data: { dllCulture: language },
            success: function (data) {
                if (data) {
                    window.location.reload();
                }
            }
        })
    }
});
