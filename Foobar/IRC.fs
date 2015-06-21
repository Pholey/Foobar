module Foobar.IRC

open System
open System.IO
open Foobar.Python


let server = "irc.freenode.net"
let port  = 6667
let nick = "Phobot"
let channel = "#arrowlang"

let popPrefix (msg: string) =
    msg.Split [|' '|]
    |> Array.toList
    |> List.tail
    |> fun x -> String.Join(" ",  x)
    
let evalMsg (msg: string) = (popPrefix >> cast >> python.Execute) msg :?> string

// active pattern for matching commands
let (|Prefix|_|) (p:string) (s:string) =
  if s.StartsWith( p ) then
    Some( s.Substring(p.Length) )
   else
    None

let login (writer: StreamWriter) =
    // identify with the server and join a room
    writer.WriteLine( sprintf "USER %s d d :%s\r\n" nick nick )
    writer.AutoFlush <- true
    writer.WriteLine( sprintf "NICK %s\r\n" nick )
    
// Give the server a chance to breath before shoving more crud at it
let rec waitForJoin (reader: StreamReader) (writer: StreamWriter) =
  let line = reader.ReadLine()
  let ready = line.Contains "End of /MOTD command."

  if not ready then 
    Console.WriteLine line
    waitForJoin reader writer
  else
    writer.WriteLine( sprintf "JOIN %s\r\n" channel )

let parsePrivmsg ( line : string ) =
  let delimiter = channel + " :"
  line.[line.IndexOf(delimiter) + delimiter.Length..line.Length - 1]

// To keep our connection alive
let sendPing ( writer : StreamWriter ) =
  writer.WriteLine( sprintf "PONG %s\r\n" server)

let sendPrivmsg ( writer : StreamWriter ) ( phrase : string ) =
  writer.WriteLine( sprintf "PRIVMSG %s %s\r\n" channel phrase )


let runBot (reader: StreamReader) (writer: StreamWriter) =
    login writer
    waitForJoin reader writer
        
    // main loop
    while( reader.EndOfStream = false ) do
      let line = reader.ReadLine()

      if (line.Contains("PING")) then
        sendPing writer

      let (msg:string) = parsePrivmsg line

      match msg with
      | Prefix "!version" rest -> sendPrivmsg writer (sprintf "%s on %A" nick Environment.Version)
      | Prefix "!date" rest -> sendPrivmsg writer (sprintf "%A" System.DateTime.Now)
      | Prefix "!python" rest -> sendPrivmsg writer (sprintf "%A" (evalMsg msg))
      | _ -> Console.WriteLine(msg)
