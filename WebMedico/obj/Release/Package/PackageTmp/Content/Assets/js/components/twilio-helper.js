function isOnline(baseUrl) {
    var result = [];
    $('.userid').each(function () {
        result.push($(this).val());
    });
    var result2 = unique(result);
    $.each(result2, function (i, e) {
        $.ajax({
            url: baseUrl+"/api/Twilio/GetUserResource?userIdentity=utn" + encodeURIComponent(e),
            async: true,
            type: 'get',
            dataType: 'json',
            crossDomain: true,
            success: function (data) {
                processDataOnline(data, e)
            },
            error: function (xhr, ajaxOptions, thrownError) {
            }
        });
    });
    setTimeout(function () { isOnline(); }, 60000);
}

function processDataOnline(data, user) {
    var rows = $('input[value="' + user + '"]').get();

    $.each(rows, function (i, e) {
        if (data.is_online)
            $(e).prev(".fa-circle").removeClass("online").removeClass("offline").addClass("online");
        else
            $(e).prev(".fa-circle").removeClass("online").removeClass("offline").addClass("offline");
    });
}

function unique(list) {
    var result = [];
    $.each(list, function (i, e) {
        if ($.inArray(e, result) == -1)
            result.push(e);
    });
    return result;
}