open System.Collections.Generic
open System.Net.Http
open Functions.Connection
open Functions.Client

let apiKey = "<api-key>"
let connection = functionsConnection {
    url "https://<project-id>.functions.supabase.co"
    headers (Dictionary(
        dict [
            ("apiKey", apiKey)
            // ("Authorization", $"Bearer {<bearer-token>}")
            // There is no need to specify Bearer token if it is same as apiKey
            // the library is going to handle it for you
            ]
        )
    )
    httpClient (new HttpClient())
}

type TestResponse = {
    message: string
}

let result = connection |> invoke<TestResponse> "test" (Some (Map ["name", "Your beautiful name"]))

match result with
| Ok    r -> printfn $"{r}"
| Error e -> printfn $"{e}"

