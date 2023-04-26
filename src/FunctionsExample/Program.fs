open Functions.Connection
open Functions.Client

let apiKey = "<api-key>"
let connection = functionsConnection {
    url "https://<project-id>.functions.supabase.co"
    headers (Map [ ("apiKey", apiKey) ] )
}

type Response = {
    message: string
}

let result = connection
             |> invoke<Response> "test" (Some (Map ["name", "your-name"]))
             |> Async.RunSynchronously

match result with
| Ok    r -> printfn $"{r}"
| Error e -> printfn $"{e}"

