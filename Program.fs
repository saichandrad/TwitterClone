open System
open System.Text.RegularExpressions
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.Writers
open Newtonsoft.Json
open Suave.Sockets
open Suave.Sockets.Control
open Suave.WebSocket
type Message = {
    username: string
    password: string
    tweet: string
    tag: string
    retweetid: string
    subscribeTo: string
    follow: string
}

//Declare Variables
let mutable userRegistration = Map.empty<string,string> // We store the username with a value of 1 to check if logged in users are registered or not
let mutable currentSession = Map.empty<string, int> // Keeps the users in the current session
let mutable tweetsMap = Map.empty<string,list<string>> // key userid and value is tweet
let mutable followers=Map.empty<string,list<string>> // key userid and value is list of user id's
let mutable userToTweets = Map.empty<string, list<string>> //user to list of tweet ids
let mutable tweetList = [||]
let mutable hashtagTweetsMap = Map.empty<string,list<string>>
let mutable hashtagUsersMap = Map.empty<string,list<string>>  // key userid and value hashtag
let mutable subscribedTo=Map.empty<string,list<string>> // key userid and value is list of user id's

let getString (rawForm: byte[]) =
    System.Text.Encoding.UTF8.GetString(rawForm)

let fromJson<'a> json =
    JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

let setCORSHeader = 
    setHeader "Access-Control-Allow-Origin" "*"
    >=> setHeader  "Access-Control-Allow-Headers" "Content-Type"

let ws (webSocket : WebSocket) (context: HttpContext) =
  socket {
    let mutable loop = true
    while loop do
      let! msg = webSocket.read()
      match msg with
      | (Text, data, true) ->
        let str = UTF8.toString data
        let response = sprintf "response to %s" str
        let byteResponse =
          response
          |> System.Text.Encoding.ASCII.GetBytes
          |> ByteSegment
        do! webSocket.send Text byteResponse true
      | (Close, _, _) ->
        let emptyResponse = [||] |> ByteSegment
        do! webSocket.send Close emptyResponse true
        loop <- false
      | _ -> ()
    }

let getInfo (f:Map<string,list<string>>) st =
    let mutable lst = []
    if f.ContainsKey(st) then
        lst <- f.Item(st)
    lst

let GetTweetswithHashTag hashTag =
    let tag = hashTag.tag
    (getInfo(hashtagTweetsMap) tag)

let ReTweet reTweet =
    let username = reTweet.username
    let tweetID = reTweet.retweetid
    printfn "username %s tweet id: %s" username tweetID
    let mutable arr = (getInfo(userToTweets) username) @ [(tweetList.[int tweetID])]
    userToTweets <- userToTweets.Add(username, arr)
    if followers.ContainsKey(username) then
        for f in followers.Item(username) do
            let mutable arr = (getInfo(userToTweets) f) @ [(tweetList.[int tweetID])]
            userToTweets.Add(f, arr)

let SubscribeTo user =
    let userA = user.username 
    let userB = user.subscribeTo
    let mutable arr = (getInfo(followers) userB) 
    if not (List.exists ((=) userA) arr) then
        arr <- arr @ [userA]
    followers <- followers.Add(userB, arr)
    getInfo(followers) userB

let Tweet user =
    let username = user.username
    let tweetstring = user.tweet
    let tweetID = "tweet_" + (string tweetList.Length)
    tweetList <- Array.append tweetList [|tweetstring|]
    let mutable arr = (getInfo(tweetsMap) tweetID) @ [username]
//    if tweetsMap.ContainsKey(tweetID) then
//        arr <- tweetsMap.Item(tweetID)
//    arr <- arr @ [username]
    tweetsMap <- tweetsMap.Add(tweetID, arr)
    let mutable arr1 = (getInfo(userToTweets) username) @ [tweetstring] 
//    if userToTweets.ContainsKey(username) then
//        arr1 <- userToTweets.Item(username)
//    arr1<- arr1 @ [tweetstring]
    userToTweets <- userToTweets.Add(username, arr1)
    let mentionrgx = new Regex("@+[a-zA-Z0-9(_)]{1,}")
    let mutable mlist = mentionrgx.Matches tweetstring
    for i in 0..mlist.Count-1 do
        if hashtagTweetsMap.ContainsKey (mlist.Item(i).Value) then
            let mutable arr = hashtagTweetsMap.Item(mlist.Item(i).Value)
            arr <- arr @ [(string tweetstring)]
            hashtagTweetsMap <- hashtagTweetsMap.Add(mlist.Item(i).Value, arr)        
        else
            let mutable arr = []
            arr <- arr @ [(string tweetstring)]
            hashtagTweetsMap <- hashtagTweetsMap.Add(mlist.Item(i).Value, arr)
    let hastagrgx = new Regex("#+[a-zA-Z0-9(_)]{1,}")
    let  hlist = hastagrgx.Matches tweetstring    
    for i in 0..hlist.Count-1 do
        if hashtagTweetsMap.ContainsKey (hlist.Item(i).Value) then
            let mutable arr = hashtagTweetsMap.Item(hlist.Item(i).Value)
            arr <- arr @ [(string tweetstring)]
            hashtagTweetsMap <- hashtagTweetsMap.Add(hlist.Item(i).Value, arr)        
        else
            let mutable arr = []
            arr <- arr @ [(string tweetstring)]
            hashtagTweetsMap <- hashtagTweetsMap.Add(hlist.Item(i).Value, arr)
    
        
    if followers.ContainsKey(username) then
        let lst = followers.Item(username)
        for f in lst do
            let mutable g = []
            if userToTweets.ContainsKey(f) then
                g <- userToTweets.Item(f)
            g <- g@[(tweetList.[int tweetID])]
            userToTweets <- userToTweets.Add(f, g)
    printfn "this is hashtag to tweets map: %A" hashtagTweetsMap
    string(user) + " Successfully tweeted " + (tweetstring) 

let RegisterNewUser registerNewUser = 
    let username = registerNewUser.username
    let password = registerNewUser.password
    userRegistration <- userRegistration.Add(username,password)
    printfn "User %s registered %s" username (userRegistration |> string)
    "Successfully Registered new User: "+username
    "new user " + (username) + "registered successfully"
    
let LoginUser loginUser =
    let username = loginUser.username
    let password = loginUser.password
    let mutable flag = false
    let mutable s = "Username and password don't match"
    if userRegistration.ContainsKey(username) then
        if currentSession.ContainsKey((username |> string)) then
            s <- "User reconnected"
            flag <- true
        elif userRegistration.Item(username) = password then
            currentSession <- currentSession.Add((username |> string), 1)
            s <- "User logged in successfully"
            flag <- true
    printfn "Successfully Logged In: %s " (currentSession |> string)
    s <- "unsuccessful"
    if flag then
        s <- "successful"
    s

let LogoutUser logoutUser = 
        let mutable s = "User already logged out!"
        let username = logoutUser.username
        printfn "Loggin out User: %s" username
        if currentSession.ContainsKey(username) then
            currentSession.Remove(username)
            s <- "User logged out successfully"
        s

let GetMyTweets(info)=
    getInfo(userToTweets) info.username
   
let getTweetsWithHashtag =
    request (fun r ->
            r.rawForm
            |> getString
            |> fromJson<Message>
            |> GetTweetswithHashTag
            |> JsonConvert.SerializeObject
            |> OK)
        >=> setCORSHeader
        >=> setMimeType "application/json"

let getMyTweets =
    request (fun r ->
            r.rawForm
            |> getString
            |> fromJson<Message>
            |> GetMyTweets
            |> JsonConvert.SerializeObject
            |> OK)
        >=> setCORSHeader
        >=> setMimeType "application/json"

let reTweet =
    request (fun r ->
            r.rawForm
            |> getString
            |> fromJson<Message>
            |> ReTweet
            |> JsonConvert.SerializeObject
            |> OK)
        >=> setCORSHeader
        >=> setMimeType "application/json"

let subscribeTo =
    request (fun r ->
        r.rawForm
        |> getString
        |> fromJson<Message>
        |> SubscribeTo
        |> JsonConvert.SerializeObject
        |> OK)
    >=> setCORSHeader
    >=> setMimeType "application/json"

let tweet = 
    request (fun r ->
        r.rawForm
        |> getString
        |> fromJson<Message>
        |> Tweet
        |> JsonConvert.SerializeObject
        |> OK)
    >=> setCORSHeader
    >=> setMimeType "application/json"

let registerUser = 
    printfn "Inside Request"
    request (fun r ->
        printfn "Inside Req"
        r.rawForm
        |> getString
        |> fromJson<Message>
        |> RegisterNewUser
        |> JsonConvert.SerializeObject
        |> OK)
    >=> setCORSHeader
    >=> setMimeType "application/json"

let loginUser = 
    request (fun r ->
        printfn "Attempting to Login User"
        r.rawForm
        |> getString
        |> fromJson<Message>
        |> LoginUser
        |> JsonConvert.SerializeObject
        |> OK
    )
    >=> setCORSHeader
    >=> setMimeType "application/json"

let logoutUser = 
    request (fun r ->
        printfn "Attempting to Logout User"
        r.rawForm
        |> getString
        |> fromJson<Message>
        |> LogoutUser
        |> JsonConvert.SerializeObject
        |> OK)
    >=> setCORSHeader
    >=> setMimeType "application/json"

let fetchTweetsThroughSocket (webSocket : WebSocket) (context: HttpContext) =
    socket {
        let mutable loop = true
        while loop do
          let! msg = webSocket.read()
          match msg with
          | (Text, data, true) ->
            let str = UTF8.toString data
            let response = sprintf "Tweets: %s" (tweetsMap |> string)
            printfn "The String is: %s" str
            let byteResponse =
              response
              |> System.Text.Encoding.ASCII.GetBytes
              |> ByteSegment
            do! webSocket.send Text byteResponse true
          | (Close, _, _) ->
            let emptyResponse = [||] |> ByteSegment
            do! webSocket.send Close emptyResponse true
            loop <- false
          | _ -> ()
    }

let handleWebsocketErrors (webSocket : WebSocket) (context: HttpContext) = 
   let exampleDisposableResource = { new IDisposable with member __.Dispose() = printfn "Resource needed by websocket connection disposed" }
   let websocketWorkflow = ws webSocket context
   async {
    let! successOrError = websocketWorkflow
    match successOrError with
    | Choice1Of2() -> ()
    | Choice2Of2(error) ->
        printfn "Error: [%A]" error
        exampleDisposableResource.Dispose()
    return successOrError
   }

let app =
    choose
        [ 
          path "/websocket" >=>  handShake fetchTweetsThroughSocket
          path "/websocketWithSubprotocol" >=> handShakeWithSubprotocol (chooseSubprotocol "test") fetchTweetsThroughSocket
          path "/websocketWithError" >=> handShake handleWebsocketErrors
          
          GET >=> choose
           [
              path "/" >=> OK "TwitterEngine"
           ]
          POST >=> choose
            [ 
                path "/register"  >=> registerUser
                path "/login"  >=> loginUser
                path "/logout"  >=> logoutUser
                path "/getmytweets"  >=> getMyTweets
                path "/tweet" >=> tweet
                path "/retweet" >=> reTweet
                path "/SubscribeTo" >=> subscribeTo
                path "/getTweetsWithHashtag" >=> getTweetsWithHashtag
            ] 
        ]

[<EntryPoint>]
let main argv =
    startWebServer defaultConfig app
    0