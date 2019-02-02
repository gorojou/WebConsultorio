$.connection.hub.logging = true;

$.connection.hub.start()
    .done(function () {
        //  console.log("99999999999999999999");
        var currentdate = new Date();
        $.connection.wmHub.server.announce()
    })
    .fail(function () { alert("error server") });

$.connection.hub.error(function (error) {
    console.log('SignalR error: ' + error)
});

$.connection.wmHub.client.announce = function () {
               updateRate = 1000
                $.ajax({
                    cache: false,
                    type: "GET",
                    url: "Home/GetConsultationDashBoard",
                    data: {}, 
                    success: function (data) {

                        $("#result").empty();
                        $("#result").html(data);

                    },
                    error: function (xhr) {
                      
                    }
                });

                $.ajax({
                    cache: false,
                    type: "GET",
                    url: "Home/GetConsultationDashBoardPendentes",
                    data: {},
                    success: function (data) {

                        $("#result1").empty();
                        $("#result1").html(data);

                    },
                    error: function (xhr) {

                    }
                });




}



function updateSignalR() {
    $.connection.wmHub.server.announce();
}