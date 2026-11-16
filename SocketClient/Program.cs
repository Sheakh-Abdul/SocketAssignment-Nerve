Console.WriteLine("=== TCP SOCKET CLIENT ===");
Console.Write("Server IP [127.0.0.1]: ");
string host = Console.ReadLine()?.Trim() ?? "";
if (string.IsNullOrWhiteSpace(host)) host = "127.0.0.1";

Console.Write("Server port [5000]: ");
string portText = Console.ReadLine()?.Trim() ?? "";
int port = string.IsNullOrWhiteSpace(portText) ? 5000 :
    int.TryParse(portText, out int parsedPort) ? parsedPort : -1;
if (port is < 1 or > 65535)
{
    Console.WriteLine("Port must be between 1 and 65535.");
    return;
}

Console.Write("Shared passphrase [SocketAssignment@Nerve]: ");
string passphrase = Console.ReadLine() ?? "";
if (string.IsNullOrWhiteSpace(passphrase)) passphrase = "SocketAssignment@Nerve";

Console.Write("Enter request (e.g. SetA-Two): ");
string request = Console.ReadLine() ?? "";
if (string.IsNullOrWhiteSpace(request))
{
    Console.WriteLine("Request cannot be empty.");
    return;
}

using TcpClientService clientService = new TcpClientService(host, port, passphrase);

try
{
    await clientService.ConnectAsync();

    while (true)
    {
        Console.Write("\nEnter request (e.g. SetA-Two) or exit: ");
        string? userRequest = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(userRequest))
            continue;

        if (userRequest.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        await clientService.SendRequestAndReceiveAsync(userRequest);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
}

Console.WriteLine("Client disconnected.");
