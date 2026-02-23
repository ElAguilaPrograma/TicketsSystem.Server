using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:7121/tickethub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMWZjNDI0MC05ZWViLTQxMWYtYTk0MC1hODBkYzlkMjU0ZWUiLCJlbWFpbCI6InBydWViYTNAZ21haWwuY29tIiwianRpIjoiMTU3NzMwYjItYWJiMi00ZDA4LWIwMDMtYzQ4MmU5YTA3MTEzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NzI0MjkyMTksImlzcyI6IlRpY2tldHNTeXN0ZW1BcGkiLCJhdWQiOiJUaWNrZXRzU3lzdGVtVXNlcnMifQ.Q7J9P-O4TAxpmbkBIqDBXqu-dKAkEH-BjE0F5IaPMCQ");
    })
    .Build();

connection.On<object>("ReceiveNewTicket", (ticket) =>
{
    Console.WriteLine($"Nuevo Ticket Recibido: {ticket}");
});

await connection.StartAsync();
Console.WriteLine("Conectado al Hub. Esperando Tickets....");
Console.ReadLine();