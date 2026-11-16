using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;

public sealed class TcpClientService : IDisposable
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _passphrase;

    private TcpClient? _client;
    private NetworkStream? _stream;

    public TcpClientService(string host, int port, string passphrase)
    {
        _host = host;
        _port = port;
        _passphrase = passphrase;
    }
    public async Task ConnectAsync()
    {
        if (_client != null)
            return;

        _client = new TcpClient();

        try
        {
            await _client.ConnectAsync(_host, _port);
            _stream = _client.GetStream();

            Console.WriteLine($"Connected to {_host}:{_port}");
        }
        catch
        {
            _client.Dispose();
            _client = null;
            throw;
        }
    }
    public async Task SendRequestAndReceiveAsync(string request)
    {
        if (_stream == null)
            throw new InvalidOperationException("Client is not connected.");

        try
        {
            byte[] encryptedRequest = AesEncryption.Encrypt(request, _passphrase);

            await MessageFraming.WriteAsync(_stream, encryptedRequest, CancellationToken.None);

            Console.WriteLine("Request sent. Waiting for response...\n");

            int receivedCount = 0;

            while (true)
            {
                byte[] encryptedResponse = await MessageFraming.ReadAsync(_stream, CancellationToken.None);

                string response = AesEncryption.Decrypt(encryptedResponse, _passphrase);

                if (response == "__END__")
                    break;

                receivedCount++;
                Console.WriteLine($"Response {receivedCount}: {response}");
            }

            if (receivedCount == 0)
                Console.WriteLine("No data found.");
        }
        catch (EndOfStreamException)
        {
            Console.WriteLine("Server disconnected.");
            Dispose();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Connection error: {ex.Message}");
            Dispose();
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine($"Decryption failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _client?.Dispose();

        _stream = null;
        _client = null;
    }

}
