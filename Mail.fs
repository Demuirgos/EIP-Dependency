module Dependency.Mail 

open System
open System.Net.Mail

type Config = {
    Server: string
    Sender: string
    Password: string
    Port: int
}

let sendMailMessage config email subject msg =
    printfn "Sending email to %s" email 
    let client = new SmtpClient(config.Server, config.Port)
    client.EnableSsl <- true
    client.Credentials <- Net.NetworkCredential(config.Sender, config.Password)
    client.SendCompleted |> Observable.add(fun e -> 
        let eventMsg = e.UserState :?> MailMessage
        if e.Cancelled then
            ("Mail message cancelled:\r\n" + eventMsg.Subject) |> Console.WriteLine
        if e.Error <> null then
            ("Sending mail failed for message:\r\n" + eventMsg.Subject + 
                ", reason:\r\n" + e.Error.ToString()) |> Console.WriteLine
        if eventMsg<>Unchecked.defaultof<MailMessage> then eventMsg.Dispose()
        if client<>Unchecked.defaultof<SmtpClient> then client.Dispose())

    fun () -> 
        async {
            let msg = new MailMessage(config.Sender, email, subject, msg)
            msg.IsBodyHtml <- true
            do client.SendAsync(msg, msg)
        } |> Async.Start

let getConfigFromFile filePath = 
    try 
        let fileContent = System.IO.File.ReadAllText(filePath)
        let configs = System.Text.Json.JsonSerializer.Deserialize<Config>(fileContent)
        Some (configs)
    with 
    | ex -> None

