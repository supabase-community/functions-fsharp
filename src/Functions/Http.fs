namespace Functions.Http

open System.Net
open System.Net.Http
open FSharp.Json
open Functions.Connection
open Functions.Common

[<AutoOpen>]
module Http =
    type FunctionsError = {
        message: string
        statusCode: HttpStatusCode option
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
   
    let private getRequestMessage (httpMethod: HttpMethod) (url: string) (urlSuffix: string): HttpRequestMessage =
        new HttpRequestMessage(httpMethod, $"{url}/{urlSuffix}")
    
    let post (urlSuffix: string) (content: StringContent) (connection: FunctionsConnection): Result<HttpResponseMessage, FunctionsError> =
        try
            let httpClient = connection.HttpClient
            
            let requestMessage = getRequestMessage HttpMethod.Post connection.Url urlSuffix
            requestMessage.Content <- content
            requestMessage.Headers |> addRequestHeaders (connection.Headers |> addBearerIfMissing)
            
            let result =
                task {
                    let response = httpClient.SendAsync(requestMessage)                    
                    return! response
                } |> Async.AwaitTask |> Async.RunSynchronously
            match result.StatusCode with
            | HttpStatusCode.OK -> Result.Ok result
            | statusCode        ->
                Result.Error { message    = result |> getResponseBody
                               statusCode = Some statusCode }
        with e ->
            Result.Error { message    = e.ToString()
                           statusCode = None }