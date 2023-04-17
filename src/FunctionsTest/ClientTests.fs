module ClientTests

open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Functions
open Functions.Connection
open Functions.Http
open FsUnit.Xunit
open Moq
open Moq.Protected
open Xunit

let mockHttpMessageHandlerWithBody (response: string) (requestBody: string) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback(fun (req: HttpRequestMessage) (_: CancellationToken) -> 
                req.Content <- new StringContent(requestBody, Encoding.UTF8, "application/json")
            )
            .ReturnsAsync(
                new HttpResponseMessage(
                    HttpStatusCode.OK,
                    Content = new StringContent(response, Encoding.UTF8, "application/json")
                )
            )
            .Verifiable()
        mockHandler
        
let mockHttpMessageHandlerWithBodyFail (error: FunctionsError) (requestBody: string) =
        let mockHandler = Mock<HttpMessageHandler>(MockBehavior.Strict)
        let statusCode = if error.statusCode.IsSome then error.statusCode.Value else HttpStatusCode.BadRequest
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback(fun (req: HttpRequestMessage) (_: CancellationToken) -> 
                req.Content <- new StringContent(requestBody, Encoding.UTF8, "application/json")
            )
            .ReturnsAsync(
                new HttpResponseMessage(
                    statusCode,
                    Content = new StringContent(error.message, Encoding.UTF8, "application/json")
                )
            )
            .Verifiable()
        mockHandler
        
type CustomResponse = {
    message: string
}

[<Collection("invoke tests")>]
module InvokeTests =
    [<Fact>]
    let ``should return a given type <'T> of response when success`` () =
        // Arrange
        let expectedResponse = { message = "function invoked successfully" }
        let response ="""{"message":"function invoked successfully"}"""
        let requestBody = """{"name:"function-name"}"""
            
        let mockHandler = mockHttpMessageHandlerWithBody response requestBody 
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = functionsConnection {
            url "http://example.functions.supabase.co"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }

        // Act
        let result =
            connection
            |> Client.invoke<CustomResponse> "function-name" (Some (Map<string, obj>["name", "function-name"]))

        // Assert
        match result with
        | Ok r -> r |> should equal expectedResponse
        | Error err -> failwithf $"Expected Ok, but got Error: {err}"
        
        // Verify
        mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), 
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.AbsoluteUri = "http://example.functions.supabase.co/function-name" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>()
            )
            
    [<Fact>]
    let ``should return an error when API request fails`` () =
        // Arrange
        let expectedError = { message = "Bad Request"; statusCode = Some HttpStatusCode.BadRequest }
        let requestBody = """{"name:"function-name"}"""
            
        let mockHandler = mockHttpMessageHandlerWithBodyFail expectedError requestBody  
        let mockHttpClient = new HttpClient(mockHandler.Object)
        
        let connection = functionsConnection {
            url "http://example.functions.supabase.co"
            headers Map["apiKey", "exampleApiKey"]
            httpClient mockHttpClient
        }

        // Act
        let result =
            connection
            |> Client.invoke<CustomResponse> "function-name" (Some (Map<string, obj>["name", "function-name"]))

        // Assert
        match result with
        | Ok ok -> failwithf $"Expected Error, but got Ok: {ok}"
        | Error err -> err |> should equal expectedError
        
        // Verify
        mockHandler.Protected().Verify("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
        mockHandler.Protected()
            .Verify("SendAsync", Times.Once(), 
                ItExpr.Is<HttpRequestMessage>(fun req ->
                    req.Method = HttpMethod.Post &&
                    req.Headers.Contains("apiKey") &&
                    req.RequestUri.AbsoluteUri = "http://example.functions.supabase.co/function-name" &&
                    req.Content.ReadAsStringAsync().Result = requestBody),
                ItExpr.IsAny<CancellationToken>()
            )