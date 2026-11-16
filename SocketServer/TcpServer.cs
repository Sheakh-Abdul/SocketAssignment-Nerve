using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

public sealed class TcpServer
{
    private readonly IPAddress _ip;
    private readonly int _port;
    private readonly string _passphrase;
    private readonly ConcurrentDictionary<int, byte> _activeClients = new();
    private int _nextClientId;

    public TcpServer(IPAddress ip, int port, string passphrase)
    {
        _ip = ip;
        _port = port;
        _passphrase = passphrase;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(_ip, _port);
        listener.Start();
        Console.WriteLine($"Server listening on {_ip}:{_port}");
        Console.WriteLine("Press Ctrl+C to stop. Waiting for clients...\n");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
                int clientId = Interlocked.Increment(ref _nextClientId);
                _activeClients.TryAdd(clientId, 0);
                Console.WriteLine($"[Client {clientId}] Connected: {client.Client.RemoteEndPoint} | Active: {_activeClients.Count}");
                _ = HandleClientAsync(client, clientId, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // exit ctrl+c 
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested)
        {
            // interrupted during shutdown.
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("Server stopped.");
        }
    }


    private async Task HandleClientAsync(TcpClient client, int clientId, CancellationToken serverToken)
    {
        using (client)
        using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serverToken))
        {
            try
            {
                NetworkStream stream = client.GetStream();

                // Keep processing requests until client disconnects.
                while (!linkedCts.Token.IsCancellationRequested)
                {
                    byte[] encryptedRequest;

                    try
                    {
                        encryptedRequest = await MessageFraming.ReadAsync(stream, linkedCts.Token);
                    }
                    catch (EndOfStreamException)
                    {
                        // Client disconnected gracefully.
                        break;
                    }

                    string request = AesEncryption.Decrypt(encryptedRequest, _passphrase);

                    Console.WriteLine($"[Client {clientId}] Request: {request}");

                    int? responseCount = DataRepository.GetResponseCount(request);

                    if (responseCount is null or <= 0)
                    {
                        await SendEncryptedAsync(stream, "EMPTY", linkedCts.Token);

                        Console.WriteLine($"[Client {clientId}] Sent EMPTY.");
                    }
                    else
                    {
                        for (int i = 0; i < responseCount.Value; i++)
                        {
                            string timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");

                            await SendEncryptedAsync(stream, timestamp, linkedCts.Token);

                            Console.WriteLine($"[Client {clientId}] Sent: {timestamp} ({i + 1}/{responseCount.Value})");

                            if (i < responseCount.Value - 1)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(1), linkedCts.Token);
                            }
                        }
                    }

                    // Signals completion of this request's response batch.
                    await SendEncryptedAsync(stream, "__END__", linkedCts.Token);
                }
            }
            catch (OperationCanceledException)
                when (serverToken.IsCancellationRequested)
            {
                // Server shutdown.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {clientId}] Error: {ex.Message}");
            }
            finally
            {
                _activeClients.TryRemove(clientId, out _);

                Console.WriteLine($"[Client {clientId}] Connection closed | Active: {_activeClients.Count}");
            }
        }
    }


    private async Task SendEncryptedAsync(NetworkStream stream, string message, CancellationToken token)
    {
        byte[] encrypted = AesEncryption.Encrypt(message, _passphrase);
        await MessageFraming.WriteAsync(stream, encrypted, token);
    }
}
