namespace Functions

open System
open System.Net.Http.Headers

/// Contains helper functions for another modules
[<AutoOpen>]
module Common =
    /// Adds HttpRequestHeaders to given headers Map
    let internal addRequestHeaders (headers: Map<string, string>) (httpRequestHeaders: HttpRequestHeaders): unit =
        headers |> Seq.iter (fun (KeyValue(k, v)) -> httpRequestHeaders.Add(k, v))
    
    /// Adds apiKey to headers Map as Authorization
    let internal addBearerIfMissing (headers: Map<string, string>) =
        let apiKey =
            match headers.TryGetValue "apiKey" with
            | true, key -> key
            | _         -> raise (Exception "Missing apiKey")
        
        match headers.ContainsKey "Authorization" with
        | true  -> headers
        | false -> headers |> Map.add "Authorization" $"Bearer {apiKey}"
        
    /// Updates Bearer token in connection Header and returns new FunctionsConnection
    let updateBearer (bearer: string) (connection: FunctionsConnection): FunctionsConnection =
        let formattedBearer = $"Bearer {bearer}"
        let headers =
            connection.Headers |> Map.change "Authorization" (fun authorization ->
                match authorization with | Some _ | None -> Some formattedBearer
            )
        { connection with Headers = headers }