namespace Functions.Http

open System.Net.Http
open FSharp.Json
open Functions.Connection
open Functions.Common

[<AutoOpen>]
module Http =
    type FunctionsError = {
        message: string
        statusCode: System.Net.HttpStatusCode
    }
    
    let private getResponseBody (responseMessage: HttpResponseMessage): string = 
        responseMessage.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously        
            
    let deserializeResponse<'T> (response: Result<HttpResponseMessage, FunctionsError>): Result<'T, FunctionsError> =
        match response with
        | Ok r    -> Result.Ok (Json.deserialize<'T> (r |> getResponseBody))
        | Error e -> Result.Error e
        
    let deserializeEmptyResponse (response: Result<HttpResponseMessage, FunctionsError>): Result<unit, FunctionsError> =
        match response with
        | Ok _    -> Result.Ok ()
        | Error e -> Result.Error e
        
    let post (urlSuffix: string) (content: StringContent) (httpClient: HttpClient)
             (connection: FunctionsConnection): Result<HttpResponseMessage, FunctionsError> =
        try
            let result =
                task {
                    addBearerIfMissing connection.Headers
                    httpClient |> addRequestHeaders connection.Headers
                    
                    printfn $"{httpClient.DefaultRequestHeaders}"
                    
                    let response = httpClient.PostAsync($"{connection.Url}/{urlSuffix}", content)
                    return! response
                } |> Async.AwaitTask |> Async.RunSynchronously
            match result.StatusCode with
            | System.Net.HttpStatusCode.OK ->
                Result.Ok result
            | statusCode                   ->
                Result.Error { message = result |> getResponseBody
                               statusCode = statusCode }
        with e ->
            Result.Error { message = e.ToString()
                           statusCode = System.Net.HttpStatusCode.BadRequest }