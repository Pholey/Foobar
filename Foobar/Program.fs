// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.
module Foobar.Main

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Collections.Generic
open System.Text.RegularExpressions
open Foobar.Python

let server = "irc.freenode.net"
let port  = 6667
let channel = "#arrowlang"
let nick = "Phobot"

let login (writer: StreamWriter) =
    // identify with the server and join a room
    writer.WriteLine( sprintf "USER %s d d :%s\r\n" nick nick )
    writer.AutoFlush <- true
    writer.WriteLine( sprintf "NICK %s\r\n" nick )

// some flow control stuff
let irc_ping ( writer : StreamWriter ) =
  writer.WriteLine( sprintf "PONG %s\r\n" server)

let irc_privmsg ( writer : StreamWriter ) ( phrase : string ) =
  writer.WriteLine( sprintf "PRIVMSG %s %s\r\n" channel phrase )

let irc_get_msg ( line : string ) =
  let delimiter = channel + " :"
  line.[line.IndexOf(delimiter) + delimiter.Length..line.Length - 1]

let popPrefix (msg: string) = 
    msg.Split [|' '|] 
    |> Array.toList
    |> List.tail
    |> fun x -> String.Join(" ",  x)

let evalMsg (msg: string) = (popPrefix >> cast >> python.Execute) msg :?> string

let runBot (reader: StreamReader) (writer: StreamWriter) =
    // main loop
    while( reader.EndOfStream = false ) do
      let line = reader.ReadLine()
      // Console.WriteLine( line )
      if (line.Contains("PING")) then
        irc_ping writer

      if (line.Contains("End of /MOTD command.")) then
        // Note to self, listen to server's bullshit, THEN join.
        writer.WriteLine( sprintf "JOIN %s\r\n" channel )

      // active pattern for matching commands
      let (|Prefix|_|) (p:string) (s:string) =
        if s.StartsWith( p ) then
          Some( s.Substring(p.Length))
         else
          None
      let (msg:string) = irc_get_msg line

      match msg with
      | Prefix "!version" rest -> irc_privmsg writer (sprintf "%s on %A" nick Environment.Version)
      | Prefix "!date" rest -> irc_privmsg writer (sprintf "%A" System.DateTime.Now)
      | Prefix "!python" rest -> irc_privmsg writer (sprintf "%A" (evalMsg msg))
      | _ -> Console.WriteLine(msg)

[<EntryPoint>]
let main args =
    // establish a connection to the server
    let irc_client = new TcpClient();
    irc_client.Connect( server, port )

    // get the input and output streams
    let irc_reader = new StreamReader( irc_client.GetStream() )
    let irc_writer = new StreamWriter( irc_client.GetStream() )

    // Send all of our information to the server
    login irc_writer
    runBot irc_reader irc_writer
    0
    