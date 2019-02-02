var TwilioChatManager = function (options) {
    var token;

    $.ajax({
        url: options.urlTokenEndpoint,
        async: true,
        type: 'post',
        data: JSON.stringify({
            'Identity': options.identity,
            'DeviceId': 'browser'//endpointId
        }),
        contentType: "application/json",
        dataType: 'json',
        crossDomain: true,
        success: function (data) {
            token = data.token

            client = new Twilio.Chat.Client(token);

            client.on('channelJoined', function (channel) {
                channel.on('messageAdded', options.onNewMessage);
            });
        },
        error: function (xhr, ajaxOptions, thrownError) {
            console.log("TwilioChatManager: " + xhr.status);
            console.log("TwilioChatManager: " + thrownError);

            //rmarquis
            //alert(xhr.status);
            //alert(thrownError);
        }
    });

    var $popContainer = $('<div/>').attr("id", "popcontainer").addClass("pop-messages-container");

    $('body').append($popContainer);

    function deactivatechatcolor() {
        $popContainer.find('.mdbc-chat-title[data-channelid]').removeClass('currentactive-pop-message');
        $popContainer.find('.mdbc-chat-title[data-channelid]').addClass('common-pop-messages');
    }

    function activechatcolor(roomName) {
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').removeClass('inactive');
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').removeClass('common-pop-messages');
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').addClass('currentactive-pop-message');
        return;
    }

    function channelalreadyopen(roomName) {
        if ($popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').length > 0) {
            return true;
        }
        return false;
    }

    this.openConversation = function (roomName, roomFriendlyName, openByUser) {
        try {
            deactivatechatcolor();
            var $popMessages = "";
            if (channelalreadyopen(roomName)) {
                activechatcolor(roomName);
                $popMessages = $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').closest('.pop-messages');
            }
            else {
                if ($(".pop-messages", $popContainer).length > 3) {
                    $(".pop-messages:eq(0)", $popContainer).remove();
                }

                $popMessages = $('\
            <div class="pop-messages z-depth-1" >\
                <div class="mdbc-chat-title z-depth-1 currentactive-pop-message text-white" style="text-transform: capitalize;" data-channelid="' + roomName + '">' + roomFriendlyName + '\
                    <a class="close material-icons" data-dismiss="modal" aria-label="Close">close</a>\
                </div >\
                <div class="mdbc-chat-container" data-channelid="' + roomName + '">\
                    <div class="mdbc-chat-messages">' + loadingHtml() + '</div>\
                    <div class="mdbc-chat-input col-12 white mdbc-zindex-10">\
                        <div class="md-form input-group">\
                            <textarea class="form-control md-textarea" data-channelid="' + roomName + '" placeholder="Mensagem..." rows="1" data-max-rows="3" data-min-rows="1" aria-describedby="btnSend"></textarea>\
                            <a class="input-group-addon transparent waves-effect btnSend">\
                                <i class="material-icons">send</i>\
                            </a>\
                        </div>\
                    </div>\
                </div>\
            </div>');

                $popContainer.append($popMessages);
            }

            var client = new TwilioChat({
                elmChatContainer: $popMessages.find('.mdbc-chat-messages'),
                elmButtonSend: $popMessages.find('.mdbc-chat-input .btnSend'),
                elmChatInput: $popMessages.find('.mdbc-chat-input textarea'),
                identity: options.identity,
                urlTokenEndpoint: options.urlTokenEndpoint,
                roomName: roomName,
                onMemberUpdated: options.onMemberUpdated
            });

            if (openByUser != true) {
                //play sound when receive message from the other user
                audio.play();
            }

            $(".close", $popMessages).click(function (e) {
                delete client;
                $popMessages.remove();
                e.preventDefault();
            });

            $(".mdbc-chat-title", $popMessages).click(function (e) {
                $popMessages.toggleClass('inactive');
                e.preventDefault();
            });

            $('.mdbc-chat-container[data-channelid="' + roomName + '"]', $popContainer).click(function (e) {
                deactivatechatcolor();
                activechatcolor(roomName);
                e.preventDefault();
            });

            $('.md-textarea[data-channelid="' + roomName + '"]', $popContainer).click(function (e) {
                deactivatechatcolor();
                activechatcolor(roomName);
                e.preventDefault();
            });
        }
        catch (ex) {
            console.log('openConversation error:' + ex);
        }
    };
}
var audio = new Audio('/Content/Assets/audio/Pling.mp3');

var TwilioChat = function (options) {
    var $istoday = false;
    var $lastdate = '';
    var $chatContainer = options.elmChatContainer;
    var $input = options.elmChatInput;
    var $btnSend = options.elmButtonSend;

    // Our interface to the Chat service
    var chatClient;

    var currentChannel;

    var accessManager;

    $input.prop('disabled', true);

    //formatDate
    function formatDate(date) {
        try {
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            var strTime = hours + ':' + minutes + ' ' + ampm;

            var year = date.getFullYear();
            year = year.toString().substr(-2);

            return date.getMonth() + 1 + "/" + date.getDate() + "/" + year + " " + strTime;
        }
        catch (ex) {
            console.log('formatDate error:' + ex);
        }
    }
    //formatDateTime
    function formatDateTime(date, now) {
        try {
            var strTime = '';
            var ddnow = now.getDate(); var dddate = date.getDate();
            ddnow = ddnow < 10 ? '0' + ddnow : ddnow; dddate = dddate < 10 ? '0' + dddate : dddate;
            var mmnow = now.getMonth() + 1; var mmdate = date.getMonth() + 1; //January is 0!
            mmnow = mmnow < 10 ? '0' + mmnow : mmnow; mmdate = mmdate < 10 ? '0' + mmdate : mmdate;
            var yyyynow = now.getFullYear(); var yyyydate = date.getFullYear();

            if (ddnow + '/' + mmnow + '/' + yyyynow != dddate + '/' + mmdate + '/' + yyyydate) {
                strTime = dddate + '/' + mmdate + '/' + yyyydate;
                if (($lastdate == '') || ($lastdate != strTime)) {
                    $chatContainer.append('<br style="line-height:1vh;"/>');
                    $chatContainer.append('<div class="strikeit"><span>' + strTime + '</span></div>');
                    $lastdate = strTime;
                }
            }
            else {
                if ($istoday == false) {
                    $chatContainer.append('<br style="line-height:1vh;"/>');
                    $chatContainer.append('<div class="strikeit"><span>' + 'Hoje' + '</span></div>');
                    $istoday = true;
                }
            }
            var hours = date.getHours();
            var minutes = date.getMinutes();
            var ampm = hours >= 12 ? 'pm' : 'am';
            hours = hours % 12;
            hours = hours ? hours : 12; // the hour '0' should be '12'
            minutes = minutes < 10 ? '0' + minutes : minutes;
            strTime = hours + ':' + minutes + ' ' + ampm;
            return strTime;
        }
        catch (ex) {
            console.log('formatDateTime error:' + ex);
        }
    }
    //GetCurrentChannelId
    function GetCurrentChannelId() {
        try {
            var channelid = currentChannel.uniqueName;
            return channelid;
        }
        catch (ex) {
            console.log('GetCurrentChannelId error:' + ex);
        }
    }
    // Helper function to print info messages to the chat window
    function print(infoMessage, asHtml) {
        try {
            var $msg = $('<div class="info">');
            if (asHtml) {
                $msg.html(infoMessage);
            } else {
                $msg.text(infoMessage);
            }
            $chatContainer.append($msg);
        }
        catch (ex) {
            console.log('print error:' + ex);
        }
    }

    var uniquemessagehelper = [];
    // Helper function to print chat message to the chat window

    function clearArray(array) {
        try {
            while (array.length) {
                array.pop();
            }
        }
        catch (ex) {
            console.log('clearArray error:' + ex);
        }
    }

    function printInfoforErrorinChat() {
        try {
            var $errormessage = '<div id="messageEmpty" class="row" style="position: absolute;top: 40%;left:15%;right:15%">\
                                    <div class="offset-1 col-10 text-center">\
                                       <svg width="70px" height="70px">﻿<path fill="#41ABE9" fill-opacity="1" stroke-width="0.2" stroke-linejoin="round" d="M 15.8333,41.3605L 19.1921,38.0017L 24.79,43.5996L 30.3879,38.0017L 33.7467,41.3605L 28.1488,46.9584L 33.7467,52.5563L 30.3879,55.9151L 24.79,50.3171L 19.1921,55.9151L 15.8333,52.5563L 21.4313,46.9584L 15.8333,41.3605 Z M 49.0833,33.25C 53.4555,33.25 57,36.7945 57,41.1667C 57,45.5389 53.4045,48.9999 49,49L 31.75,49L 29.8154,46.9584L 35.4133,41.3605L 30.3879,36.4283L 24.5867,42.2296L 22.3442,39.7371C 23.25,37 25.2617,34.9376 27.4553,34.8389C 28.7579,31.1462 32.2783,28.4999 36.4167,28.4999C 40.3459,28.4999 43.7179,30.8853 45.1637,34.2869C 46.3193,33.627 47.6573,33.25 49.0833,33.25 Z "></path></svg>\
                                        <br/>\
                                        Houve um erro na conexão com o bate-papo<br/>\
                                    </div>\
                                 </div>';
            $chatContainer.append($errormessage);
        }
        catch (ex) {
            console.log('printInfoforErrorinChat error:' + ex);
        }
    }

    function printInfoforEmptyMessages() {
        try {
            var $emptymessage = '<div id="messageEmpty" class="row" style="position: absolute;top: 40%;left:15%;right:15%">\
                                    <div class="offset-1 col-10 text-center">\
                                        <svg width="70px" height="70px" viewBox="0 0 70 70">\
                                            <g id="Symbols" stroke="none" stroke-width="1" fill="none" fill-rule="evenodd">\
                                                <g id="EmptyStatesEAlteracoes" transform="translate(-1779.000000, -371.000000)" fill="#41ABE9">\
                                                    <path d="M1845.5035,385.0035 L1838.5,385.0035 L1838.5,416.5035 L1792.9965,416.5035 L1792.9965,423.5035 C1792.9965,425.4355 1794.5715,427.0035 1796.5,427.0035 L1834.9965,427.0035 L1849,441.0035 L1849,388.5035 C1849,386.568 1847.432,385.0035 1845.5035,385.0035 M1828.0035,371 L1782.5,371 C1780.568,371 1779,372.568 1779,374.5035 L1779,423.5035 L1792.9965,409.5035 L1828.0035,409.5035 C1829.932,409.5035 1831.5,407.9355 1831.5,406 L1831.5,374.5035 C1831.5,372.568 1829.932,371 1828.0035,371" id="Fill-6-Copy"></path>\
                                                </g>\
                                            </g>\
                                        </svg>\
                                        <br/>\
                                        Ainda não trocou mensagens com <br/>\
                                        este paciente\
                                    </div>\
                                 </div>';
            $chatContainer.append($emptymessage);
        }
        catch (ex) {
            console.log('printInfoforEmptyMessages error:' + ex);
        }
    }

    function printMessage(message, beepifnew) {
        try {
            if ($('#messageEmpty').length) {
                $('#messageEmpty').remove();
            }
            var sid = message.sid;
            if ($.inArray(sid, uniquemessagehelper) == -1) {
                uniquemessagehelper.push(sid);
                var timestamp = message.timestamp;
                var author = message.author;
                var body = message.body;
                var $message = '';
                if (author === options.identity) {
                    $message = $('<div class="messageme">').text(body);
                }
                else {
                    $message = $('<div class="message">').text(body);
                    if (beepifnew == true) {
                        //play sound when receive message from the other user
                        audio.play();
                    }
                }
                if ($('#typingIndicator').length > 0) $('#typingIndicator').remove();
                var date = formatDateTime(timestamp, new Date());
                var $datetime = $('<div class="datetimeformessages">').text(date);
                var $nextline = $('<div style="clear:both">').text('');
                $chatContainer.append('<br style="line-height:1vh;"/>');
                $chatContainer.append($message);
                $chatContainer.append($datetime);
                $chatContainer.append($nextline);
                $chatContainer.scrollTop($chatContainer[0].scrollHeight);
            }
        }
        catch (ex) {
            console.log('printMessage error:' + ex);
        }
    }

    function updateTypingIndicator(member, isTyping) {
        try {
            var $message;
            if (isTyping) {
                if ($('#typingIndicator').length > 0) return;

                $message = $('<div class="message" id="typingIndicator" title="' + member.identity + ' is typing..."><i class="material-icons">more_horiz</i></div>');
                $chatContainer.append($message);
                $chatContainer.scrollTop($chatContainer[0].scrollHeight);
            }
            else {
                $('#typingIndicator').remove();
            }
        }
        catch (ex) {
            console.log('updateTypingIndicator error:' + ex);
        }
    }

    function initChat(token) {
        try {
            // Initialize the Chat client
            chatClient = new Twilio.Chat.Client(token);
            chatClient.getSubscribedChannels().then(createOrJoinchannel);

            accessManager = new Twilio.AccessManager(token);
            accessManager.on('tokenUpdated', am => chatClient.updateToken(am.token));
            accessManager.on('tokenExpired', () => {
                $.ajax({
                    url: options.urlTokenEndpoint,
                    async: true,
                    type: 'post',
                    data: JSON.stringify({
                        'Identity': options.identity,
                        'DeviceId': 'browser'
                    }),
                    contentType: "application/json",
                    dataType: 'json',
                    crossDomain: true,
                    success: function (data) {
                        accessManager.updateToken(data.token);
                    },
                    error: function (xhr, ajaxOptions, thrownError) {
                        console.log('initChat error');
                        console.log("initChat: " + xhr.status);
                        console.log("initChat: " + thrownError);
                        //rmarquis
                        //alert(xhr.status);
                        //alert(thrownError);
                    }
                });
            });
        }
        catch (ex) {
            console.log('initChat error:' + ex);
        }
    }

    if (!options.token) {
        try {
            // Get an access token for the current user, passing a username (identity)
            // and a device ID - for browser-based apps, we'll always just use the
            // value "browser"
            $.ajax({
                url: options.urlTokenEndpoint,
                async: true,
                type: 'post',
                data: JSON.stringify({
                    'Identity': options.identity,
                    'DeviceId': 'browser'//endpointId
                }),
                contentType: "application/json",
                dataType: 'json',
                crossDomain: true,
                success: function (data) {
                    initChat(data.token);
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    console.log('!options.token error');
                    console.log("if (!options.token): " + xhr.status);
                    console.log("if (!options.token): " + thrownError);
                    //rmarquis
                    //alert(xhr.status);
                    //alert(thrownError);
                }
            });
        }
        catch (ex) {
            console.log('!options.token error:' + ex);
        }
    }
    else {
        initChat(options.token);
    }

    function createOrJoinchannel() {
        //try {
            // Get the general chat channel, which is where all the messages are
            // sent in this simple application

            chatClient.getChannelByUniqueName(options.roomName)
                .then(function (channel) {
                    currentChannel = channel;
                    currentChannel.getMessages().then(function (messages) {
                        $chatContainer.html('');
                        var totalMessages = messages.items.length,
                            message;
                        clearArray(uniquemessagehelper);
                        $istoday = false;
                        $lastdate = '';
                        if (totalMessages > 0) {
                            for (i = 0; i < totalMessages; i++) {
                                message = messages.items[i];
                                printMessage(message, false);
                            }
                        }
                        else {
                            printInfoforEmptyMessages();
                        }
                        updateLastConsumptionIndex();
                        $input.prop('disabled', false);
                    });
                    setupChannel();
                }).catch(function (err) {
                    // If it doesn't exist, let's create it
                    console.log(err);
                    if (err.body.code == 50400) {
                        //User not member of channel
                        //try to fix this
                        $chatContainer.html('');
                        printInfoforErrorinChat();
                        fixChatMembers(options.identity, options.roomName);
                    } else {
                        try {
                            printInfoforEmptyMessages();
                            chatClient.createChannel({
                                uniqueName: options.roomName,
                                friendlyName: options.roomFriendlyName,
                                isPrivate: true,
                            })
                                .then(function (channel) {
                                    channel.add(options.chatMembers.id).then(function () { });
                                    currentChannel = channel;
                                    $chatContainer.html('');
                                    printInfoforEmptyMessages();
                                    $input.prop('disabled', false);
                                    setupChannel();

                                }).catch(function (err) {
                                    console.log('on catch');
                                    $chatContainer.html('');
                                    printInfoforErrorinChat();
                                    $('div.mdbc-chat-messages div.loader-container').html('');
                                    $input.prop('disabled', false);
                                    setupChannel();
                                });
                        }
                        catch (ex) {
                            console.log(ex);
                            printInfoforErrorinChat();
                        }
                        finally {
                            $('div.mdbc-chat-messages div.loader-container').html('');
                            $input.prop('disabled', false);
                            setupChannel();
                        }
                    }
                });
        //}
        //catch (ex) {
        //    console.log('createOrJoinchannel error:' + ex);
        //}
    }

    // Set up channel after it has been found
    function setupChannel() {
        try {
            // Join the general channel
            currentChannel.join().then(function (channel) {
                //When chat is ready
            });

            //for (var memberId in options.chatMembers) {
            //    if (options.chatMembers.hasOwnProperty(memberId)) {
            //        currentChannel.add(memberId);
            //    }
            //}

            //set up the listener for the typing started Channel event
            currentChannel.on('typingStarted', function (member) {
                //process the member to show typing
                updateTypingIndicator(member, true);
            });

            //set  the listener for the typing ended Channel event
            currentChannel.on('typingEnded', function (member) {
                //process the member to stop showing typing
                updateTypingIndicator(member, false);
            });

            // Listen for new messages sent to the channel
            currentChannel.on('messageAdded', function (message) {
                printMessage(message, true);

                if (message.author === options.identity) {
                    updateLastConsumptionIndex();
                }
            });

            currentChannel.on('memberJoined', function (member) {
                member.getUser().then(user => {
                    if (options.chatMembers && options.chatMembers.hasOwnProperty(user.identity) && options.chatMembers[user.identity]) {
                        if (user.online) {
                            $(options.chatMembers[user.identity]).removeClass('offline').addClass('online');
                        }
                        else {
                            $(options.chatMembers[user.identity]).removeClass('online').addClass('offline');
                        }
                    }
                    user.on('updated', (evnt) => {
                        if (options.chatMembers && options.chatMembers.hasOwnProperty(user.identity) && options.chatMembers[user.identity]) {
                            if (user.online) {
                                $(options.chatMembers[user.identity]).removeClass('offline').addClass('online');
                            }
                            else {
                                $(options.chatMembers[user.identity]).removeClass('online').addClass('offline');
                            }
                        }
                    });
                });
            });

            currentChannel.getMembers().then(members => {
                members.forEach(member => {
                    member.getUser().then(user => {
                        if (options.chatMembers && options.chatMembers.hasOwnProperty(user.identity) && options.chatMembers[user.identity]) {
                            if (user.online) {
                                $(options.chatMembers[user.identity]).removeClass('offline').addClass('online');
                            }
                            else {
                                $(options.chatMembers[user.identity]).removeClass('online').addClass('offline');
                            }
                        }
                        user.on('updated', (evnt) => {
                            if (options.chatMembers && options.chatMembers.hasOwnProperty(user.identity) && options.chatMembers[user.identity]) {
                                if (user.online) {
                                    $(options.chatMembers[user.identity]).removeClass('offline').addClass('online');
                                }
                                else {
                                    $(options.chatMembers[user.identity]).removeClass('online').addClass('offline');
                                }
                            }
                        });
                    });
                });
            });

            currentChannel.on('memberUpdated', function (member) {
                //note this method would use the provided information
                //to render this to the user in some way
                options.onMemberUpdated && options.onMemberUpdated();
            });
        }
        catch (ex) {
            console.log('setupChannel error:' + ex);
        }
    }

    function fixChatMembers(memberIdenty, roomName) {
        $.ajax({
            url: 'FixChatMember',
            async: true,
            type: 'post',
            data: JSON.stringify({
                'Identity': memberIdenty,
                'RoomName': roomName
            }),
            contentType: "application/json",
            dataType: 'json',
            success: function (data) {
                if (data.IsValid == true) {
                    createOrJoinchannel();
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
            }
        });
    }

    // Send a new message to the general channel

    $input.on('keydown', function (e) {
        try {
            if (e.keyCode == 13) {
                e.preventDefault();
                if ($input.val() === '') return;
                currentChannel.sendMessage($input.val())
                $input.val('').triggerHandler('input');
                $input.val('');
            }
            //else send the Typing Indicator signal
            else {
                currentChannel.typing();
            }
        }
        catch (ex) {
            console.log('onkeydown error:' + ex);
        }
    });

    $input.on('focus', updateLastConsumptionIndex);

    $btnSend.on('click', function (e) {
        try {
            if ($input.val() === '') return;
            currentChannel.sendMessage($input.val())
            $input.triggerHandler('input');
            $input.val('');
        }
        catch (ex) {
            console.log('onclick error:' + ex);
        }
    });

    window.addEventListener('beforeunload', leaveRoomIfJoined);

    function leaveRoomIfJoined() {
        try {
            if (currentChannel) {
                currentChannel.disconnect();
            }
        }
        catch (ex) {
            console.log('leaveRoomIfJoined error:' + ex);
        }
    }

    function updateLastConsumptionIndex() {
        try {
            PutLastChatMessageDateTime();
            currentChannel.getMessages().then(function (messages) {
                //determine the newest message index
                var newestMessageIndex = messages.items.length ?
                    messages.items[messages.items.length - 1].index : 0;
                //check if we we need to set the consumption horizon
                if (currentChannel.lastConsumedMessageIndex !== newestMessageIndex) {
                    currentChannel.updateLastConsumedMessageIndex(newestMessageIndex);
                }
            });
        }
        catch (ex) {
            console.log('updateLastConsumptionIndex error:' + ex);
        }
    }

    function PutLastChatMessageDateTime() {
        try {
            /*Here call the webservice api to update the last message datetime*/
            jQuery.support.cors = true;
            var channelid = GetCurrentChannelId();
            var labeloflastchatmessagedatetime = "lcmdt_" + channelid;
            var now = formatDate(new Date());
            $.ajax({
                url: '/Home/PutLastChatMessageDateTime',
                type: 'GET',
                data: { 'CHANNEL_ID': channelid },
                success: function (data) {
                },
                error: function (xhr) {
                }
            });
            $('ul#conversationsList li.active-notification[data-channelid="' + channelid + '"] .textofuser div').html(now);
            $('ul#conversationsList li.active-notification[data-channelid="' + channelid + '"]').prependTo('ul#conversationsList');
        }
        catch (ex) {
            console.log('PutLastChatMessageDateTime error:' + ex);
        }
    }

    this.disconnect = function () {
        try {
            leaveRoomIfJoined();
        }
        catch (ex) {
            console.log('disconnect error:' + ex);
        }
    }
};