namespace Functions

open System.Net.Http

[<AutoOpen>]
module Connection =
    type FunctionsConnection = {
        Url: string
        Headers: Map<string, string>
        HttpClient: HttpClient
    }
    
    type FunctionsConnectionBuilder() =
        member _.Yield _ =
            {   Url = ""
                Headers = Map []
                HttpClient = new HttpClient() }
       
        [<CustomOperation("url")>]
        member _.Url(connection, url) =
            { connection with Url = url }
        
        [<CustomOperation("headers")>]
        member _.Headers(connection, headers) =
            { connection with Headers = headers }
            
        [<CustomOperation("httpClient")>]
        member _.HttpClient(connection, httpClient) =
            { connection with HttpClient = httpClient }
            
    let functionsConnection = FunctionsConnectionBuilder()