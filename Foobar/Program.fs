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
open Foobar.IRC

[<EntryPoint>]
let main args =
    // establish a connection to the server
    let client = new TcpClient();
    client.Connect( server, port )

    // get the input and output streams
    let reader = new StreamReader( client.GetStream() )
    let writer = new StreamWriter( client.GetStream() )
    runBot reader writer
    
    0
