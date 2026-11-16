using System.Net;

Console.WriteLine("=== TCP SOCKET SERVER ===");
Console.Write("IP to bind [0.0.0.0]: ");
string bindIpText = Console.ReadLine()?.Trim() ?? "";
if (string.IsNullOrWhiteSpace(bindIpText)) bindIpText = "0.0.0.0";

if (!IPAddress.TryParse(bindIpText, out IPAddress? bindIp))
{
    Console.WriteLine("Invalid IP address.");
    return;
}

Console.Write("Port [5000]: ");
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

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\nStopping server...");
};

var server = new TcpServer(bindIp, port, passphrase);
await server.RunAsync(cts.Token);
