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
            alert(xhr.status);
            alert(thrownError);
        }
    }); 

    var $popContainer = $('<div/>').attr("id", "popcontainer").addClass("pop-messages-container");

    $('body').append($popContainer);

    function deactivatechatcolor () {
        $popContainer.find('.mdbc-chat-title[data-channelid]').removeClass('currentactive-pop-message');
        $popContainer.find('.mdbc-chat-title[data-channelid]').addClass('common-pop-messages');
    }

    function activechatcolor (roomName) {
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').removeClass('inactive');
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').removeClass('common-pop-messages');
        $popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').addClass('currentactive-pop-message');
        return; 
    }

    function channelalreadyopen (roomName) {
        if ($popContainer.find('.mdbc-chat-title[data-channelid="' + roomName + '"]').length > 0) {
            return true;
        }
        return false;
    }

    this.openConversation = function (roomName, roomFriendlyName ,openByUser) {
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
    //formatDateTime
    function formatDateTime(date, now) {
        var strTime = '';
        var ddnow = now.getDate(); var dddate = date.getDate();
        ddnow = ddnow < 10 ? '0' + ddnow : ddnow; dddate = dddate < 10 ? '0' + dddate : dddate;
        var mmnow = now.getMonth() + 1; var mmdate = date.getMonth() + 1; //January is 0!
        mmnow = mmnow < 10 ? '0' + mmnow : mmnow; mmdate = mmdate < 10 ? '0' + mmdate : mmdate;
        var yyyynow = now.getFullYear(); var yyyydate = date.getFullYear();

        if (ddnow + '/' + mmnow + '/' + yyyynow != dddate + '/' + mmdate + '/' + yyyydate) {
            strTime = dddate + '/' + mmdate + '/' + yyyydate;
            if (($lastdate == '') || ($lastdate != strTime))
            {
                $chatContainer.append('<br style="line-height:1vh;"/>');
                $chatContainer.append('<div class="strikeit"><span>' + strTime + '</span></div>');
                $lastdate = strTime;
            }
        }
        else
        {
            if ($istoday == false) {
                $chatContainer.append('<br style="line-height:1vh;"/>');
                $chatContainer.append('<div class="strikeit"><span>' + 'Hoy' + '</span></div>');
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
    //GetCurrentChannelId
    function GetCurrentChannelId() {
        var channelid = currentChannel.uniqueName;
        return channelid;
    }
    // Helper function to print info messages to the chat window
    function print(infoMessage, asHtml) {
        var $msg = $('<div class="info">');
        if (asHtml) {
            $msg.html(infoMessage);
        } else {
            $msg.text(infoMessage);
        }
        $chatContainer.append($msg);
    }

    var uniquemessagehelper = [];
    // Helper function to print chat message to the chat window

    function clearArray(array) {
        while (array.length) {
            array.pop();
        }
    }
    
    function printMessage(message, beepifnew) {
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

    function updateTypingIndicator(member, isTyping) {
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

    function initChat(token) {
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
                    alert(xhr.status);
                    alert(thrownError);
                }
            });
        });
    }

    if (!options.token) {
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
                alert(xhr.status);
                alert(thrownError);
            }
        });
    }
    else
    {
        initChat(options.token);
    }

    function createOrJoinchannel() {
        // Get the general chat channel, which is where all the messages are
        // sent in this simple application

        var promise = chatClient.getChannelByUniqueName(options.roomName);
        promise.then(function (channel) {
            currentChannel = channel;
                currentChannel.getMessages().then(function (messages) {
                    $chatContainer.html('');
                    var totalMessages = messages.items.length,
                        message;
                    clearArray(uniquemessagehelper);
                    $istoday = false;
                    $lastdate = '';
                    for (i = 0; i < totalMessages; i++) {
                        message = messages.items[i];
                        printMessage(message,false);
                    }
                    updateLastConsumptionIndex();
                    $input.prop('disabled', false);
                });

                setupChannel();
            
        }).catch(function (err) {
            // If it doesn't exist, let's create it
            chatClient.createChannel({
                uniqueName: options.roomName,
                friendlyName: options.roomFriendlyName,
                isPrivate: true,
            }).then(function (channel) {
                channel.add(options.chatMembers.id).then(function () {
                });
                currentChannel = channel;
                $chatContainer.html('');
                $input.prop('disabled', false);
                setupChannel();
            });
        });
    }

    // Set up channel after it has been found
    function setupChannel() {
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

                printMessage(message,true);

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

    // Send a new message to the general channel

    $input.on('keydown', function (e) {
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
    });

    $input.on('focus', updateLastConsumptionIndex);

    $btnSend.on('click', function (e) {
        if ($input.val() === '') return;
        currentChannel.sendMessage($input.val())
        $input.triggerHandler('input');
        $input.val('');
    });

    window.addEventListener('beforeunload', leaveRoomIfJoined);

    function leaveRoomIfJoined() {
        if (currentChannel) {
            currentChannel.disconnect();
        }
    }

    function updateLastConsumptionIndex() {
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

    function PutLastChatMessageDateTime() {
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

    this.disconnect = function () {
        leaveRoomIfJoined();
    }
};