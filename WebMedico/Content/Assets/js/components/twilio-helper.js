function isOnline(baseUrl) {
    var result = [];
    $('.userid').each(function () {
        result.push("utn"+$(this).val());
    });
    var result2 = unique(result);
    
    var myJSON = JSON.stringify(result2);
    $.ajax({
        url: baseUrl + "/api/Twilio/UsersResource",
        data: myJSON,
        async: true,
        type: 'post',
        contentType: "application/json; charset=utf-8",
        dataType: 'json',
        crossDomain: true,
        success: function (data) {
            process(data)
        },
        error: function (xhr, ajaxOptions, thrownError) {
        }
    });
    setTimeout(function () { isOnline(baseUrl); }, 60000);
}

function process(data)
{
    $.each(data, function (index, obj) {
        var rows = $('input[value="' + obj.identity.slice(3) + '"]').get();
        $.each(rows, function (i, e) {
            if (obj.is_online)
                $(e).prev(".fa-circle").removeClass("online").removeClass("offline").addClass("online");
            else
                $(e).prev(".fa-circle").removeClass("online").removeClass("offline").addClass("offline");
        });
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