var callstatus = {
    makingcall: 1,
    receivingcall: 2,
    finishingcorrectly: 3,
    rejectedbypatient: 4,
    patientdoesnottakethecall: 5,
    errorontwilio: 6
}
var TwilioVideo = function (options) {

    var startingcalltime;
    //var Video = require('twilio-video');
    const Video = Twilio.Video;
    var activeRoom;
    var previewTracks;
    var identity;
    var token;
    var utenteToken;

    var hasVideo = false;

    var waitingParticipant = true;

    this.RoomName = options.roomName;
    /*Added tp get the name of the patient*/
    var PatientName = options.relevantData.Name;
    // Attach the Tracks to the DOM.
    function attachTracks(tracks, container) {
        tracks.forEach(function (track) {
            if (container) {
                if (track.kind == "video") {
                    hasVideo && $('video', container).length === 0 && container.appendChild(track.attach());
                }
                else {
                    $('audio', container).length === 0 && container.appendChild(track.attach());
                }
            }
        });
    }

    // Attach the Participant's Tracks to the DOM.
    function attachParticipantTracks(participant, container) {
        var tracks = Array.from(participant.tracks.values());
        attachTracks(tracks, container);
    }

    // Attach the Participant's Tracks to the DOM.
    function reAttachParticipantTracks(participant, container) {
        var tracks = Array.from(participant.tracks.values());
        reAttachTracks(tracks, container);
    }

    // Detach the Tracks from the DOM.
    function detachTracks(tracks) {
        tracks.forEach(function (track) {
            track.detach().forEach(function (detachedElement) {
                detachedElement.remove();
            });
        });
    }

    function reAttachTracks(tracks, container) {
        tracks.forEach(function (track) {
            track.detach().forEach(function (detachedElement) {
                detachedElement.remove();
            });
        });

        tracks.forEach(function (track) {
            if (container) {
                if (track.kind == "video") {
                    hasVideo && $('video', container).length === 0 && container.appendChild(track.attach());
                }
                else {
                    $('audio', container).length === 0 && container.appendChild(track.attach());
                }
            }
        });
    }

    // Detach the Participant's Tracks from the DOM.
    function detachParticipantTracks(participant) {
        var tracks = Array.from(participant.tracks.values());
        detachTracks(tracks);
    }

    // Check for WebRTC
    if (!navigator.webkitGetUserMedia && !navigator.mozGetUserMedia) {
        alert('WebRTC is not available in your browser.');
    }

    // When we are about to transition away from this page, disconnect
    // from the room, if joined.
    window.addEventListener('beforeunload', leaveRoomIfJoined);

    function notificatePatientPromise() {
        return $.ajax({
            url: options.urlEndPoint + '/api/GenerateVideoChatToken',
            async: true,
            type: 'post',
            data: JSON.stringify({
                'Room': options.roomName,
                'Identity': options.relevantData.IdUtente
            }),
            contentType: "application/json",
            dataType: 'json',
            crossDomain: true
        })
            .then(function (data) {
                utenteToken = data.token;
                return $.ajax({
                    url: options.urlEndPoint + '/api/CallNotifications/Patient',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    type: 'POST',
                    dataType: 'json',
                    data: JSON.stringify({
                        idConsulta: options.relevantData.idConsulta,
                        RoomName: options.roomName,
                        Identify: options.relevantData.IdUtente,
                        VideoAccessToken: data.token,
                        NotificationType: 1,
                        Status: 1,
                        VideoEnabled: hasVideo
                    })
                });
            });
    }

    function initCallPromise() {
        startingcalltime = new Date().getTime();

        return $.ajax({
            url: options.urlEndPoint + '/api/GenerateVideoChatToken',
            async: true,
            type: 'post',
            data: JSON.stringify({
                'Room': options.roomName,
                'Identity': options.identity
            }),
            contentType: "application/json",
            dataType: 'json',
            crossDomain: true,
            error: function (xhr, ajaxOptions, thrownError) {
                alert(xhr.status);
                alert(thrownError);
            }
        });
    }

    // Bind button to join Room.
    document.getElementById(options.btnJoinCallId).onclick = function (ev) {
        ev.preventDefault();

        if ($(this).hasClass('disabled')) return;
        $('#' + options.btnJoinCallId).addClass('disabled');
        $('#' + options.btnJoinVideoId).addClass('disabled');
        hasVideo = true;
        videoCall = true;
        initCallPromise().
            then((data) => {
                notificatePatientPromise()
                    .then(() => {
                        var connectOptions = {
                            name: options.roomName,
                            video: true,
                            logLevel: 'debug',
                        };

                        if (previewTracks) {
                            connectOptions.tracks = previewTracks;
                        }
                        //hasVideo = false;
                        $('#' + options.btnJoinVideoId).text('videocam');
                        // Join the Room with the token from the server and the
                        // LocalParticipant's Tracks.
                        if (!activeRoom) {
                            Video.connect(data.token, connectOptions).then(roomJoined, function (error) {
                                alert('Could not connect to Twilio: ' + error.message);
                            });
                        }
                        $('#' + options.btnJoinVideoId).removeClass('disabled');
                    })
                    .fail(() => {
                        mdbcToast("Não foi possível notificar o paciente");
                        $('#' + options.btnJoinCallId).removeClass('disabled');
                        leaveRoomIfJoined(callstatus.errorontwilio);
                    });
            });
    };

    // Bind button to join Room.
    document.getElementById(options.btnJoinVideoId).onclick = function (ev) {
        ev.preventDefault();

        if ($(this).hasClass('disabled')) return;

        if ($(this).text() == 'videocam') {
            $(this).text('videocam_off');

            $(options.containerSelector).addClass('video');
            hasVideo = true;

            if (!activeRoom) {

                $('#' + options.btnJoinCallId).addClass('disabled');
                $('#' + options.btnJoinVideoId).addClass('disabled');

                initCallPromise().
                    then((data) => {
                        notificatePatientPromise()
                            .then(() => {
                                var connectOptions = {
                                    name: options.roomName,
                                    logLevel: 'debug'
                                };

                                if (previewTracks) {
                                    connectOptions.tracks = previewTracks;
                                }
                                $('#' + options.btnJoinCallId).addClass('disabled');
                                // Join the Room with the token from the server and the
                                // LocalParticipant's Tracks.
                                Video.connect(data.token, connectOptions).then(roomJoined, function (error) {
                                    alert('Could not connect to Twilio: ' + error.message);
                                });
                                $('#' + options.btnJoinVideoId).removeClass('disabled');
                            })
                            .fail(() => {
                                mdbcToast("Não foi possível notificar o paciente");
                                leaveRoomIfJoined(callstatus.errorontwilio);
                            });
                    });
            }
            else {
                var previewContainer = document.getElementById(options.localVideoId);
                attachParticipantTracks(room.localParticipant, previewContainer);

                // Attach the Tracks of the Room's Participants.
                room.participants.forEach(function (participant) {
                    previewContainer = document.getElementById(options.remoteVideoId);
                    attachParticipantTracks(participant, previewContainer);
                });
            }

        }
        else {
            $(this).text('videocam');

            $(options.containerSelector).removeClass('video');

            var tracks;

            room.participants.forEach(function (participant) {
                tracks = Array.from(participant.tracks.values()).filter((track) => {
                    return track.kind == 'video';
                });
                detachTracks(tracks);
            });
        }
    };

    document.getElementById(options.btnFullscreen).onclick = function () {
        if ($('i', this).text() == 'fullscreen') {
            $('i', this).text('fullscreen_exit');
            $(options.containerSelector).appendTo('#' + options.fullscreenContainerId);
            $('[href="#panelVideo"]').show().click();

            var previewContainer = document.getElementById(options.localVideoId);
            reAttachParticipantTracks(room.localParticipant, previewContainer);

            // Attach the Tracks of the Room's Participants.
            room.participants.forEach(function (participant) {
                var previewContainer = document.getElementById(options.remoteVideoId);
                reAttachParticipantTracks(participant, previewContainer);
            });
        }
        else {
            $('i', this).text('fullscreen');
            $('#' + options.fullscreenContainerId).children().prependTo('.mdbc-chat-container');
            $('[href="#panelSoap"]').click();
            $('[href="#panelVideo"]').hide();

            var previewContainer = document.getElementById(options.localVideoId);
            reAttachParticipantTracks(room.localParticipant, previewContainer);

            // Attach the Tracks of the Room's Participants.
            room.participants.forEach(function (participant) {
                var previewContainer = document.getElementById(options.remoteVideoId);
                reAttachParticipantTracks(participant, previewContainer);
            });
        }

    };

    // Bind button to leave Room.
    document.getElementById(options.btnLeaveId).onclick = function () {
        console.log('Leaving room...');
        leaveRoomIfJoined(callstatus.finishingcorrectly);
        $.ajax({
            //url: 'https://doctorwebservice.azurewebsites.net/api/CallNotifications/Patient',
            url: options.urlEndPoint + '/api/CallNotifications/Patient',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            type: 'POST',
            dataType: 'json',
            data: JSON.stringify({
                idConsulta: options.relevantData.idConsulta,
                RoomName: options.roomName,
                Identify: options.relevantData.IdUtente,
                VideoAccessToken: utenteToken,
                NotificationType: 1,
                Status: 3,
                VideoEnabled: hasVideo
            })
        });

    };

    // Successfully connected!
    function roomJoined(room) {
        window.room = activeRoom = room;
        $(options.containerSelector).addClass('active');
        hasVideo && $(options.containerSelector).addClass('video');
        $(options.loadingContainerSelector).show();
        waitingParticipant = true;
        //!waitingParticipant && $(options.loadingContainerSelector).hide();
        var count = 0,
            timer = setInterval(() => {
                if (count == 50) {
                    clearInterval(timer);
                    if (waitingParticipant) {
                        mdbcToast("O paciente não compareceu à chamada");
                        leaveRoomIfJoined(callstatus.patientdoesnottakethecall);
                        $.ajax({
                            //url: 'https://doctorwebservice.azurewebsites.net/api/CallNotifications/Patient',
                            url: options.urlEndPoint + '/api/CallNotifications/Patient',
                            headers: {
                                'Accept': 'application/json',
                                'Content-Type': 'application/json'
                            },
                            type: 'POST',
                            dataType: 'json',
                            data: JSON.stringify({
                                idConsulta: options.relevantData.idConsulta,
                                RoomName: options.roomName,
                                Identify: options.relevantData.IdUtente,
                                VideoAccessToken: utenteToken,
                                NotificationType: 1,
                                Status: 3,
                                VideoEnabled: hasVideo
                            })
                        });
                    }
                }
                count++;
            }, 1000);
        //log("Joined as '" + identity + "'");
        // Attach LocalParticipant's Tracks, if not already attached.
        var previewContainer = document.getElementById(options.localVideoId);
        if (!previewContainer.querySelector('video')) {
            attachParticipantTracks(room.localParticipant, previewContainer);

        }

        // Attach the Tracks of the Room's Participants.
        room.participants.forEach(function (participant) {
            var previewContainer = document.getElementById(options.remoteVideoId);
            attachParticipantTracks(participant, previewContainer);
        });

        // When a Participant joins the Room, log the event.
        room.on('participantConnected', function (participant) {
            //log("Joining: '" + participant.identity + "'");
            joiningRoomChatMsgManager(callstatus.makingcall);
            $(options.loadingContainerSelector).hide();
            waitingParticipant = false;
            clearInterval(timer);
        });

        // When a Participant adds a Track, attach it to the DOM.
        room.on('trackAdded', function (track, participant) {
            //log(participant.identity + " added track: " + track.kind);
            var previewContainer = document.getElementById(options.remoteVideoId);
            attachTracks([track], previewContainer);
        });

        // When a Participant removes a Track, detach it from the DOM.
        room.on('trackRemoved', function (track, participant) {
            //log(participant.identity + " removed track: " + track.kind);
            detachTracks([track]);
        });

        // When a Participant leaves the Room, detach its Tracks.
        room.on('participantDisconnected', function (participant) {
            //log("Participant '" + participant.identity + "' left the room");
            detachParticipantTracks(participant);

            if (room.participants.size === 0) leaveRoomIfJoined(callstatus.finishingcorrectly);
        });

        // Once the LocalParticipant leaves the room, detach the Tracks
        // of all Participants, including that of the LocalParticipant.
        room.on('disconnected', function () {
            //log('Left');
            if (previewTracks) {
                previewTracks.forEach(function (track) {
                    track.stop();
                });
            }
            detachParticipantTracks(room.localParticipant);
            room.participants.forEach(detachParticipantTracks);
            activeRoom = null;
            //document.getElementById('button-join').style.display = 'inline';
            //document.getElementById('button-leave').style.display = 'none';
        });
    }

    // Preview LocalParticipant's Tracks.
    //document.getElementById(options.btnPreview).onclick = function () {
    //    var localTracksPromise = previewTracks
    //        ? Promise.resolve(previewTracks)
    //        : Video.createLocalTracks();

    //    localTracksPromise.then(function (tracks) {
    //        window.previewTracks = previewTracks = tracks;
    //        var previewContainer = document.getElementById(options.remoteVideoId);
    //        if (!previewContainer.querySelector('video')) {
    //            attachTracks(tracks, previewContainer);
    //        }
    //    }, function (error) {
    //        console.error('Unable to access local media', error);
    //        log('Unable to access Camera and Microphone');
    //    });
    //};

    // Activity log.
    function log(message) {
        var logDiv = document.getElementById('log');
        logDiv.innerHTML += '<p>&gt;&nbsp;' + message + '</p>';
        logDiv.scrollTop = logDiv.scrollHeight;
    }

    // Leave Room.
    function PutMessageOnChatArea(statusofcall, textmsg) {
        var img = '';
        if (statusofcall == callstatus.finishingcorrectly) {
            img = 'finishingphonecall.png';
        } else if (statusofcall == callstatus.rejectedbypatient) {
            img = 'makingphonecallnotasnwer.png';
        } else if (statusofcall == callstatus.patientdoesnottakethecall) {
            img = 'makingphonecallnotasnwer.png';
        } else if (statusofcall == callstatus.errorontwilio) {
            img = 'exception.png';
        } else if (statusofcall == callstatus.makingcall) {
            img = 'makingphonecallsuccesfully.png';
        } else if (statusofcall == callstatus.receivingcall) {
            img = 'receivingphonecall.png';
        }
        var $msg = '<div style="padding-top:3px;padding-bottom:3px;"><div style="float:left;padding-right:7px"><img src="/Content/Images/' + img + '" width="20px"/></div><div style="font-size:0.8rem;">' + textmsg + '</div></div>';
        $('.mdbc-chat-messages').append($msg);
    }
    function GetMessageFromStatus(statusofcall) {
        /*Getting Duration of Call*/
        var finishingcalltime = new Date().getTime();
        var timelasting = finishingcalltime - startingcalltime;
        var date = new Date(timelasting);
        var hh = date.getUTCHours();
        var mm = date.getUTCMinutes();
        var ss = date.getSeconds();
        if (hh < 10) { hh = "0" + hh; }
        if (mm < 10) { mm = "0" + mm; }
        if (ss < 10) { ss = "0" + ss; }
        var t;
        if (hh > 0) { t = hh + ":" + mm + ":" + ss; }
        { t = mm + ":" + ss; }
        /*Establishing textmessage*/
        var msg = '';
        if (statusofcall == callstatus.makingcall) {
            msg = 'Llamada a <b>' + PatientName + '</b>';
        } else if (statusofcall == callstatus.rejectedbypatient) {
            msg = 'Llamada a <b>' + PatientName + '</b>, ocupado';
        } else if (statusofcall == callstatus.patientdoesnottakethecall) {
            msg = 'La llamada a <b>' + PatientName + '</b> no ha sido respondida';
        } else if (statusofcall == callstatus.receivingcall) {
            msg = 'Llamada de <b> ' + PatientName + '</b >';
        } else if (statusofcall == callstatus.errorontwilio) {
            msg = 'Lo sentimos pero Hubo algunos Problemas al realizar la Llamada';
        } else if (statusofcall == callstatus.finishingcorrectly) {
            msg = 'Llamada finalizada. Duración:' + t;
        } else {
            msg = '';
        }
        return msg;
    }

    function joiningRoomChatMsgManager(statusofcall) {
        var msg = GetMessageFromStatus(statusofcall);
        PutMessageOnChatArea(statusofcall, msg);
    }

    function leaveRoomIfJoined(statusofcall) {
        if (activeRoom) {
            $(options.containerSelector).removeClass('active').removeClass('video');
            $(options.loadingContainerSelector).hide();
            $('#' + options.btnJoinCallId).removeClass('disabled');
            $('#' + options.btnJoinVideoId).text('videocam');
            $('#' + options.fullscreenContainerId).children().prependTo('.mdbc-chat-container');
            $('[href="#panelSoap"]').click();
            $('[href="#panelVideo"]').hide();
            activeRoom.disconnect();
        }
        //$('#' + options.btnLeaveId).click();
        var msg = GetMessageFromStatus(statusofcall);
        /*Putting message into Chat*/
        PutMessageOnChatArea(statusofcall, msg);

    }

    this.disconnect = function () {
        var statusofcall = callstatus.finishingcorrectly;
        if (activeRoom) {
            mdbcToast("O paciente rejeitou a chamada");
            statusofcall = callstatus.rejectedbypatient;
        }
        leaveRoomIfJoined(statusofcall);
    }

}