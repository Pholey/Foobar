module Foobar.Mongo

open System
open MongoDB.Bson
open MongoDB.Driver
open System.Threading
open System.Threading.Tasks

type Person = { 
    Name: BsonString;
}

// Helper to await Tasks using async and void the result 
// with proper error/cancellation propogation
let (~%%) (task : Task) : Async<unit> =
    Async.FromContinuations(fun (cont, econt, ccont) ->
        task.ContinueWith(fun (task: Task) ->
            if task.IsFaulted then econt task.Exception
            elif task.IsCanceled then ccont (OperationCanceledException())
            else cont ()) |> ignore)

let connectionString = "mongodb://127.0.0.1:27017"
let client = new MongoClient(connectionString)
let db = client.GetDatabase("TEST");
let collection = db.GetCollection<Person> "people"


let createPerson (input: Person) = %% collection.InsertOneAsync input
    



let logAsync =
    let name = BsonString "Gpp"
    createPerson { Name = name }
     
