namespace Functions

open System.Net.Http
open FSharp.Json
open Functions.Connection
open Functions.Http

[<AutoOpen>]
module Client =
        
    let invoke<'T> (name: string) (body: Map<string, obj> option) (connection: FunctionsConnection) : Result<'T, FunctionsError> =
        let client = new HttpClient()
        let parsedBody = 
            match body with
            | Some b -> b
            | _      -> Map []
        let content = new StringContent(Json.serialize(parsedBody))
        
        let response = connection |> post name content client
        deserializeResponse<'T> response