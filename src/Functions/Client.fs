namespace Functions

open System.Net.Http
open System.Text
open FSharp.Json
open Functions.Connection
open Functions.Http

/// Contains all functions needed for invoking [Supabase function](https://supabase.com/docs/guides/functions)
[<AutoOpen>]
module Client =
    /// Invokes edge function with given name and optional body and returns result in 
    let invokeRaw (name: string) (body: Map<string, obj> option)
                  (connection: FunctionsConnection): Async<Result<HttpResponseMessage, FunctionsError>> =
        let requestBody = (Map[], body) ||> Option.defaultValue
        let content = new StringContent(Json.serialize requestBody, Encoding.UTF8, "application/json")
        
        post name content connection
        
    /// Invokes edge function with given name and optional body and returns result deserialized to given `'T` type
    let rec invoke<'T> (name: string) (body: Map<string, obj> option)
                       (connection: FunctionsConnection): Async<Result<'T, FunctionsError>> =
        async {
            let! response = invokeRaw name body connection
            return deserializeResponse<'T> response
        }
    
    /// Updates Bearer token in connection Header and returns new FunctionsConnection
    let updateBearer (bearer: string) (connection: FunctionsConnection): FunctionsConnection =
        let formattedBearer = $"Bearer {bearer}"
        let headers =
            connection.Headers |> Map.change "Authorization" (fun authorization ->
                match authorization with | Some _ | None -> Some formattedBearer
            )
        { connection with Headers = headers }