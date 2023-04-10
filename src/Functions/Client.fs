namespace Functions

open System.Net.Http
open System.Text
open FSharp.Json
open Functions.Connection
open Functions.Http

[<AutoOpen>]
module Client =
    let invokeRaw (name: string) (body: Map<string, obj> option)
                  (connection: FunctionsConnection): Result<HttpResponseMessage, FunctionsError> =
        let requestBody = (Map[], body) ||> Option.defaultValue
        let content = new StringContent(Json.serialize requestBody, Encoding.UTF8, "application/json")
        
        post name content connection
        
    let rec invoke<'T> (name: string) (body: Map<string, obj> option)
                       (connection: FunctionsConnection): Result<'T, FunctionsError> =
        let response = invokeRaw name body connection
        deserializeResponse<'T> response
    
    let updateBearer (bearer: string) (connection: FunctionsConnection): FunctionsConnection =
        let formattedBearer = $"Bearer {bearer}"
        let headers =
            match connection.Headers.ContainsKey "Authorization" with
            | true  ->
                connection.Headers |> Seq.map (fun (KeyValue (k, v)) -> if k = "Authorization" then (k, formattedBearer) else (k, v)) |> Map
            | false ->
                Map.add "Authorization" formattedBearer connection.Headers
        { connection with Headers = headers }