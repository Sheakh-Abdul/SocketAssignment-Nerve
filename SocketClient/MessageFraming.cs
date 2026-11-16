using System.Buffers.Binary;
using System.Net.Sockets;

public static class MessageFraming
{
    private const int MaxMessageSize = 1024 * 1024;

    public static async Task WriteAsync(NetworkStream stream, byte[] payload, CancellationToken token)
    {
        if (payload.Length > MaxMessageSize)
            throw new InvalidDataException("Message exceeds the 1 MB limit.");

        byte[] header = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(header, payload.Length);
        await stream.WriteAsync(header, token);
        await stream.WriteAsync(payload, token);
        await stream.FlushAsync(token);
    }

    public static async Task<byte[]> ReadAsync(NetworkStream stream, CancellationToken token)
    {
        byte[] header = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(header, token);
        int length = BinaryPrimitives.ReadInt32BigEndian(header);
        if (length < 0 || length > MaxMessageSize)
            throw new InvalidDataException($"Invalid message length: {length}.");

        byte[] payload = new byte[length];
        await stream.ReadExactlyAsync(payload, token);
        return payload;
    }
}
