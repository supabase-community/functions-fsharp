namespace Functions.Connection

open System.Collections.Generic


[<AutoOpen>]
module Connection =
    type FunctionsConnection = {
        Url: string
        Headers: IDictionary<string, string>
        // Headers: seq<string * string>
        
    }
    
    type FunctionsConnectionBuilder() =
        member _.Yield _ =
            {   Url = ""
                Headers =  Dictionary() }
       
        [<CustomOperation("url")>]
        member _.Url(connection, url) =
            { connection with Url = url }
        
        [<CustomOperation("headers")>]
        member _.Headers(connection, headers) =
            { connection with Headers = headers }
            
    let functionsConnection = FunctionsConnectionBuilder()