namespace Functions

open System.Net
open System.Net.Http
open FSharp.Json
open Functions.Connection
open Functions.Common

/// Contains functions for performing http request and serialization/deserialization of data
module Http =
    /// Represents error base error type for this library
    type FunctionsError = {
        message: string
        statusCode: HttpStatusCode option
    }
    
    /// Parses HttpResponseMessage to it's string form
    let private getResponseBody (responseMessage: HttpResponseMessage): string = 
        responseMessage.Content.ReadAsStringAsync()
        |> Async.AwaitTask
        |> Async.RunSynchronously        
            
    /// Deserializes given response
    let deserializeResponse<'T> (response: Result<HttpResponseMessage, FunctionsError>): Result<'T, FunctionsError> =        
        try
            match response with
            | Ok r    -> Result.Ok (Json.deserialize<'T> (getResponseBody r))
            | Error e -> Result.Error e
        with e -> Error { message = e.Message ; statusCode = None }
        
    /// Deserializes empty (unit) response
    let deserializeEmptyResponse (response: Result<HttpResponseMessage, FunctionsError>): Result<unit, FunctionsError> =
        match response with
        | Ok _    -> Result.Ok ()
        | Error e -> Result.Error e
   
    /// Constructs HttpRequestMessage with given method and url
    let private getRequestMessage (httpMethod: HttpMethod) (url: string) (urlSuffix: string): HttpRequestMessage =
        new HttpRequestMessage(httpMethod, $"{url}/{urlSuffix}")
    
    /// Performs http POST request
    let post (urlSuffix: string) (content: StringContent)
             (connection: FunctionsConnection): Async<Result<HttpResponseMessage, FunctionsError>> =
        async {
            try
                let httpClient = connection.HttpClient
                
                let requestMessage = getRequestMessage HttpMethod.Post connection.Url urlSuffix
                requestMessage.Content <- content
                requestMessage.Headers |> addRequestHeaders (connection.Headers |> addBearerIfMissing)
                
                let! response = httpClient.SendAsync(requestMessage) |> Async.AwaitTask                    
                    
                match response.StatusCode with
                | HttpStatusCode.OK -> return Result.Ok response
                | statusCode -> return Result.Error { message = getResponseBody response ; statusCode = Some statusCode }
            with e -> return Result.Error { message = e.ToString() ; statusCode = None }
        }