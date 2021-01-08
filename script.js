
var websocketURL = "ws://localhost:8080/websocket";
var output;

function initiateWebSocket(){
    output = document.getElementById("output");
    
    websocket = new WebSocket(websocketURL);

    websocket.onopen = function(evt) { onOpen(evt) };
    websocket.onclose = function(evt) { onClose(evt) };
    websocket.onmessage = function(evt) { onMessage(evt) };
    websocket.onerror = function(evt) { onError(evt) };
}

function onOpen(evt){
    writeToScreen("CONNECTED");
    doSend("Send Data for socket");
}

function onClose(evt){
    writeToScreen("DISCONNECTED");
}

function onMessage(evt){
    writeToScreen(''+evt.data);
    //websocket.close();
}

function onError(evt){
    writeToScreen('' + evt.data);
}

function doSend(message){
    writeToScreen("SENT: " + message); 
    websocket.send(message);
}

function writeToScreen(message){
    var pre = document.createElement("tarea1");
    pre.style.wordWrap = "break-word";
    pre.innerHTML = message;
    console.log("Message: "+message);
    output.appendChild(pre);
}

var usernameGlobal;
var passwordGlobal;

$( document ).ready(function() {
    if(localStorage.getItem("loginUNAME")){
        $('#submitBtn3').text("LogOut "+localStorage.getItem("loginUNAME"));
                $('#submitBtn3').css('display','block');
                $('#forms').css('display','none');
                $('#fetchRow').css('display','block');
                $('#hashtagRow').css('display','block');
                $('#postRow').css('display','block');
                $('#subscribeRow').css('display','block');
                $('#retweetRow').css('display','block');
                fetchtweets();
    }
});

$('#submitBtn1').click(function() {
    // $('#errorMsg1').css('display','block');
    //             setTimeout(function(){ $('#errorMsg1').css('display','none'); }, 2000);

    var username = $('#exampleInputEmail1').val();
    var password = $('#exampleInputPassword1').val();

    usernameGlobal = username;
    passwordGlobal = password;
    
    var cred = {
        'username': $('#exampleInputEmail1').val(),
        'password': $('#exampleInputPassword1').val()
    }

    if(username && password){

        $.ajax({
            type : "POST",
            url :  "http://127.0.0.1:8080/register",
            
            dataType: "json",
            data: JSON.stringify({
                'username': $('#exampleInputEmail1').val(),
                'password': $('#exampleInputPassword1').val()
            }),
            
            //data : JSON.stringify(item),
            success : function(jsonResponse) {
                console.log("Got Register resposne");
                $('#errorMsg1').text("User registered successfully!!!!");
                $('#errorMsg1').css('display','block');
                setTimeout(function(){ $('#errorMsg1').css('display','none'); }, 2000);
            }
            
        });
        console.log("Register request sent");

        console.log(cred);
    } else {
        $('#errorMsg1').text("Enter both email and password.........");
        $('#errorMsg1').css('display','block');
        setTimeout(function(){ $('#errorMsg1').css('display','none'); }, 2000);
    }

});

$('#submitBtn2').click(function() {

    // $('#submitBtn3').text("LogOut "+$('#exampleInputEmail2').val());
    //             $('#submitBtn3').css('display','block');
    //             $('#forms').css('display','none');
    //             $('#fetchRow').css('display','block');
    //             $('#hashtagRow').css('display','block');

    var email1 = $('#exampleInputEmail2').val();
    var password1 = $('#exampleInputPassword2').val();

    var cred = {
        'username': $('#exampleInputEmail2').val(),
        'password': $('#exampleInputPassword2').val()
    }

    if(email1 && password1){


        $.ajax({
            type : "POST",
            url :  "http://127.0.0.1:8080/login",
            dataType: "json",
            data: JSON.stringify({
                'username': $('#exampleInputEmail2').val(),
                'password': $('#exampleInputPassword2').val()
            }),
            success: function(data){
                if(data != "unsuccessful"){
                    $('#submitBtn3').text("LogOut "+$('#exampleInputEmail2').val());
                    $('#submitBtn3').css('display','block');
                    $('#forms').css('display','none');
                    $('#fetchRow').css('display','block');
                    $('#hashtagRow').css('display','block');
                    $('#postRow').css('display','block');
                    $('#subscribeRow').css('display','block');
                    $('#retweetRow').css('display','block');
                    localStorage.setItem("loginUNAME",$('#exampleInputEmail2').val());
                    fetchtweets();
                } else {
                    $('#errorMsg3').text(data);
                    $('#errorMsg3').css('display','block');
                    setTimeout(function(){ $('#errorMsg3').css('display','none'); }, 2000);
                }
                
            },
            error : function (error) {
                $('#errorMsg3').text("Error while logging in.");
                $('#errorMsg3').css('display','block');
                setTimeout(function(){ $('#errorMsg3').css('display','none'); }, 2000);
            }
        });
    } else {
        $('#errorMsg3').text("Enter both email and password.........");
        $('#errorMsg3').css('display','block');
        setTimeout(function(){ $('#errorMsg3').css('display','none'); }, 2000);
    }

});

$('#submitBtn3').click(function () {

    // $('#exampleInputEmail2').val("");
    //         $('#exampleInputPassword2').val("");
    //         $('#exampleInputEmail1').val("");
    //         $('#exampleInputPassword1').val("");
    //         //setTimeout(function(){ $('#errorMsg2').css('display','none'); }, 2000);
    //         $('#submitBtn3').text("LogOut");
    //         $('#submitBtn3').css('display','none');
    //         $('#forms').css('display','block');
    //         $('#fetchRow').css('display','none');
    //         $('#hashtagRow').css('display','none');

    $.ajax({
        type : "POST",
        url :  "http://127.0.0.1:8080/logout",
        dataType: "json",
        data: JSON.stringify({
            'username': $('#exampleInputEmail2').val()
        }),
        success: function(data){
            // $('#submitBtn3').attr('disabled','true');
            // $('#submitBtn2').removeAttr("disabled");
            // $('#errorMsg2').text("User logged out successfully!!!!");
            // $('#errorMsg2').css('display','block');
            $('#exampleInputEmail2').val("");
            $('#exampleInputPassword2').val("");
            $('#exampleInputEmail1').val("");
            $('#exampleInputPassword1').val("");
            //setTimeout(function(){ $('#errorMsg2').css('display','none'); }, 2000);
            $('#submitBtn3').text("LogOut");
            $('#submitBtn3').css('display','none');
            $('#forms').css('display','block');
            $('#fetchRow').css('display','none');
            $('#hashtagRow').css('display','none');
            $('#postRow').css('display','none');
            $('#subscribeRow').css('display','none');
            $('#retweetRow').css('display','none');
            localStorage.removeItem("loginUNAME")
        }
    });
});

function fetchtweets() {

    $.ajax({
        type : "POST",
        url: "http://127.0.0.1:8080/getmytweets",
        dataType: "json",
        data: JSON.stringify({
            'username': $('#exampleInputEmail2').val()
        }),
        success: function(data){
            $('#tarea1').val(data);
        },
        error : function (error) {
            $('#errorMsg2').text("Error while fetching data.");
            $('#errorMsg2').css('display','block');
            setTimeout(function(){ $('#errorMsg2').css('display','none'); }, 2000);
        }
    });

}

$('#hashtagTweets').click(function () {
    var hashtag = $('#hashtagInput').val();

    if(hashtag){
        $.ajax({
            type: "POST",
            url: "http://127.0.0.1:8080/getTweetsWithHashtag",
            dataType: "json",
            data: JSON.stringify({
                'tag': hashtag,
                'username': localStorage.getItem("loginUNAME")
            }),
            success: function(data){
                $('#tarea2').val(data);
            },
            error : function (error) {
                $('#errorMsg4').text("Error while getting results for hashtag....");
                $('#errorMsg4').css('display','block');
                setTimeout(function(){ $('#errorMsg4').css('display','none'); }, 2000);
            }
        });
    } else {
        $('#errorMsg4').text("Please enter hashtag.");
        $('#errorMsg4').css('display','block');
        setTimeout(function(){ $('#errorMsg4').css('display','none'); }, 2000);
    }
    
});

$('#submitBtn4').click(function () {
    var tweet = $('#tweetInput').val();

    if(tweet){
        $.ajax({
            type : "POST",
            url :  "http://127.0.0.1:8080/tweet",

            dataType: "json",
            data: JSON.stringify({
                'tweet': tweet,
                'username':localStorage.getItem("loginUNAME")
            }),
            success: function(data){
                //alert("Posted successfully....")
            },
            error : function (error) {
                $('#errorMsg5').text("Error while posting tweet....");
                $('#errorMsg5').css('display','block');
                setTimeout(function(){ $('#errorMsg5').css('display','none'); }, 2000);
            }
        });
        fetchtweets();
    } else {
        $('#errorMsg5').text("Enter tweet.....");
        $('#errorMsg5').css('display','block');
        setTimeout(function(){ $('#errorMsg5').css('display','none'); }, 2000);
    }
    
});

$('#submitBtn5').click(function () {
    var user = $('#subsInput').val();
    var currUser = localStorage.getItem("loginUNAME");

    if(user && currUser){
        $.ajax({
            type : "POST",
            url :  "http://127.0.0.1:8080/SubscribeTo",
            
            dataType: "json",
            data: JSON.stringify({
                'username': currUser,
                'subscribeTo' : user
            }),
            success: function(data){
                //alert("Subscribed successfully....")
                $('#errorMsg6').text("Subscribed successfully...");
                $('#errorMsg6').css('display','block');
                setTimeout(function(){ $('#errorMsg6').css('display','none'); }, 2000);
            },
            error : function (error) {
                //alert("Error while subscribing tweet....")
                $('#errorMsg6').text("Error while subscribing tweet.....");
                $('#errorMsg6').css('display','block');
                setTimeout(function(){ $('#errorMsg6').css('display','none'); }, 2000);
            }
        });
    } else {
        $('#errorMsg6').text("Enter user to be subscribed to.....");
        $('#errorMsg6').css('display','block');
        setTimeout(function(){ $('#errorMsg6').css('display','none'); }, 2000);
    }
    
});

$('#submitBtn6').click(function () {
    var retweet = $('#retweetInput').val();
    var currUser = localStorage.getItem("loginUNAME");

    if(retweet && currUser){
        $.ajax({
            type : "POST",
            url :  "http://127.0.0.1:8080/retweet",
            
            dataType: "json",
            data: JSON.stringify({
                'username': currUser,
                'retweet' : retweet
            }),
            success: function(data){
                //alert("Subscribed successfully....")
                $('#errorMsg7').text("Retweeted successfully...");
                $('#errorMsg7').css('display','block');
                setTimeout(function(){ $('#errorMsg7').css('display','none'); }, 2000);
            },
            error : function (error) {
                //alert("Error while subscribing tweet....")
                $('#errorMsg7').text("Error while  retweet.....");
                $('#errorMsg7').css('display','block');
                setTimeout(function(){ $('#errorMsg7').css('display','none'); }, 2000);
            }
        });
    } else {
        $('#errorMsg6').text("Enter tweet.....");
        $('#errorMsg6').css('display','block');
        setTimeout(function(){ $('#errorMsg6').css('display','none'); }, 2000);
    }
    
});