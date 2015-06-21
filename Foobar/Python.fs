module Foobar.Python

open System
open IronPython.Hosting
open Microsoft.FSharp.Reflection


let python = Python.CreateEngine() // create Python script engine with default settings
let cast (code: string) =  "str(" + code + ")"

let (?) (o : obj) m : 'Result = 
    if FSharpType.IsFunction typeof<'Result>    
    then 
        // if it was function call then we need to take requested callable member from instance
        let func = python.Operations.GetMember(o, m)
        let domain, _ = FSharpType.GetFunctionElements(typeof<'Result>)
        let getArgs = 
            if domain = typeof<unit> then fun _ -> [||]
            elif FSharpType.IsTuple domain then fun a -> FSharpValue.GetTupleFields(a)
            else fun a -> [|a|]

        downcast FSharpValue.MakeFunction(typeof<'Result>, fun args -> 
            python.Operations.Invoke(func, getArgs(args))
            )
    else 
        downcast python.Operations.GetMember(o, m)

let (?<-) (o : obj) m v = 
    python.Operations.SetMember(o, m, v)


// let script = "
// def identity(x):
//     return x

// identity([1,3,5])
// "

// let a = python.Execute(script);
// a?y <- 500
// a?append(8) 