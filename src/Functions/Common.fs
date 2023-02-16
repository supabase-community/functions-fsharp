namespace Functions.Common

open System
open System.Collections.Generic
open System.Net.Http

[<AutoOpen>]
module Common =    
    let private addRequestHeader (key: string) (value: string) (client: HttpClient): unit =
        client.DefaultRequestHeaders.Add(key, value)
    
    let internal addRequestHeaders (headers: IDictionary<string, string>) (client: HttpClient): unit =
        headers |> Seq.iter (fun (KeyValue(k, v)) -> client |> addRequestHeader k v)
        
    let internal addBearerIfMissing (headers: IDictionary<string, string>) =
        let apiKey =
            match headers.TryGetValue "apiKey" with
            | true, key -> key
            | _         -> raise (Exception "Missing apiKey")
        
        match headers.ContainsKey "Authorization" with
        | true  -> () 
        | false -> headers.Add("Authorization", $"Bearer {apiKey}")