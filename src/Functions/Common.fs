namespace Functions

open System
open System.Net.Http.Headers

[<AutoOpen>]
module Common =
    let internal addRequestHeaders (headers: Map<string, string>) (httpRequestHeaders: HttpRequestHeaders): unit =
        headers |> Seq.iter (fun (KeyValue(k, v)) -> httpRequestHeaders.Add(k, v))
    
    let internal addBearerIfMissing (headers: Map<string, string>) =
        let apiKey =
            match headers.TryGetValue "apiKey" with
            | true, key -> key
            | _         -> raise (Exception "Missing apiKey")
        
        match headers.ContainsKey "Authorization" with
        | true  -> headers
        | false -> headers |> Map.add "Authorization" $"Bearer {apiKey}"