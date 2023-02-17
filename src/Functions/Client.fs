namespace Functions

open System.Net.Http
open FSharp.Json
open Functions.Connection
open Functions.Http

[<AutoOpen>]
module Client =
    let invokeRaw (name: string) (body: Map<string, obj> option) (connection: FunctionsConnection): Result<HttpResponseMessage, FunctionsError> =
        let client = connection.HttpClient
        let parsedBody = 
            match body with
            | Some b -> b
            | _      -> Map []
        let content = new StringContent(Json.serialize(parsedBody))
        
        connection |> post name content client
        
    let rec invoke<'T> (name: string) (body: Map<string, obj> option) (connection: FunctionsConnection): Result<'T, FunctionsError> =
        let response = connection |> invokeRaw name body
        deserializeResponse<'T> response