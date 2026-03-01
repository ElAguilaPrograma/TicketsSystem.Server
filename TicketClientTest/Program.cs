using Microsoft.AspNetCore.SignalR.Client;
using System.Runtime.CompilerServices;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("1.- to test control panel, 2.- to test change status");
        string? optionStr = Console.ReadLine() ?? string.Empty;
        int option = int.Parse(optionStr);

        if (option == 1)
        { 
            Program program = new Program();
            await program.ControlPanel();
        }
        else
        {
            await TicketStatusChanged();
        }
    }

    private async Task ControlPanel()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7121/tickethub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMWZjNDI0MC05ZWViLTQxMWYtYTk0MC1hODBkYzlkMjU0ZWUiLCJlbWFpbCI6InBydWViYTNAZ21haWwuY29tIiwianRpIjoiZDU1OWQzNjctOWU4ZS00MzMzLTg5YjEtNzJmNjM0YjhmZjc0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NzI5NDc2NTAsImlzcyI6IlRpY2tldHNTeXN0ZW1BcGkiLCJhdWQiOiJUaWNrZXRzU3lzdGVtVXNlcnMifQ.-vSNe8v9l84cvgk4vnIUnSvz2to6oYxwtfDf_Ase2xw");
            })
            .Build();

        connection.On<object>("ReceiveNewTicket", (ticket) =>
        {
            Console.WriteLine($"Nuevo Ticket Recibido: {ticket}");
        });

        await connection.StartAsync();
        Console.WriteLine("Conectado al Hub. Esperando Tickets....");
        Console.ReadLine();
    }

    private async static Task TicketStatusChanged()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7121/tickethub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMWZjNDI0MC05ZWViLTQxMWYtYTk0MC1hODBkYzlkMjU0ZWUiLCJlbWFpbCI6InBydWViYTNAZ21haWwuY29tIiwianRpIjoiZDU1OWQzNjctOWU4ZS00MzMzLTg5YjEtNzJmNjM0YjhmZjc0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJleHAiOjE3NzI5NDc2NTAsImlzcyI6IlRpY2tldHNTeXN0ZW1BcGkiLCJhdWQiOiJUaWNrZXRzU3lzdGVtVXNlcnMifQ.-vSNe8v9l84cvgk4vnIUnSvz2to6oYxwtfDf_Ase2xw");
            })
            .Build();

        connection.On<object>("ReceiveNewTicketStatusChange", (ticket) =>
        {
            Console.WriteLine($"Se cambio el estado del ticket: {ticket}");
        });

        await connection.StartAsync();
        Console.WriteLine("Conectado al Hub. Esperando Cambios en el status de tickets....");
        Console.ReadLine();
    }
}