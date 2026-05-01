$(function () {
    var userName = $('#hdUserName').val();
    var listPermission = [];
    $.ajax({
        url: '/AdminCP/Users/GetListPermission',
        method: 'POST',
        dataType: 'json',
        data: {
            userName: userName
        },
        success: function (response) {
            listPermission = response.permissions;
            if (listPermission.length > 0) {
                var sideBar = $(".nav.nav-list");
                sideBar.find("li").each(function () {
                    var per = $(this).data("permision");
                    console.log(per);
                    var li = $(this);
                    $.each(listPermission, function (i, value) {
                        if (value != per && per !="" ) {
                            li.remove();
                        }
                    })
                })
            }
        },
        error: function (err) {
            console.log(err);
        }
    })    
})