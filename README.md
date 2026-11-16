# TCP Socket Programming Assignment (.NET 8)

This solution contains two independent console applications:

- **SocketServer**: Accepts multiple TCP client connections concurrently, decrypts incoming requests, performs collection lookups, and sends encrypted timestamp responses.
- **SocketClient**: Connects to the server, sends encrypted requests, receives encrypted responses, decrypts them, and displays the results.

The application supports persistent TCP connections, allowing clients to send multiple requests without reconnecting after every request.

## Requirements

- .NET 8 SDK
- Visual Studio 2022 (or VS Code)

## Run

Open `SocketAssignment.sln` in Visual Studio, or use two terminals from the solution folder.

**Terminal 1 — Start the server:**

```bash
dotnet run --project SocketServer
```

The server listens on `0.0.0.0` (all network interfaces) and port `5000` by default.

For another computer on the same network, allow the port through the server machine's firewall and use its LAN IP address in the client configuration.

**Terminal 2 — Start a client:**

```bash
dotnet run --project SocketClient
```

Use server IP `127.0.0.1` when the client and server are running on the same machine.

Keep the passphrase identical in the server and every client. The default demo passphrase is `SocketAssignment@2026`.

To test multiple clients, start the client command in multiple terminals/windows. Only one server instance is needed. Each client maintains its own TCP connection and can send multiple requests independently while the server handles other clients concurrently.

## Request Format and Expected Behavior

Send a request in `SetName-ItemName` format, for example:

```text
SetA-Two
```

| Request | Expected server behavior |
|---|---|
| `SetA-Two` | Sends 2 local timestamps, one second apart |
| `SetB-Four` | Sends 4 local timestamps, one second apart |
| `SetE-Nine` | Sends 9 local timestamps, one second apart |
| `SetA-Unknown` | Sends `EMPTY` |
| `Unknown-Two` | Sends `EMPTY` |
| Malformed request | Sends `EMPTY` |

Collection used by the server:

```text
SetA: One=1, Two=2
SetB: Three=3, Four=4
SetC: Five=5, Six=6
SetD: Seven=7, Eight=8
SetE: Nine=9, Ten=10
```

The first timestamp is sent immediately; subsequent timestamps are sent after one-second delays.

Timestamp format: `dd-MM-yyyy HH:mm:ss`.

After sending all responses for a request, the server sends an encrypted `__END__` marker to indicate that the response batch is complete.

For invalid requests, the server sends an encrypted `EMPTY` response followed by the `__END__` marker.

The client waits for this marker before accepting the next request. Enter `exit` to close the client connection.

## Implementation Notes

- TCP uses a four-byte, big-endian length prefix so reads are not dependent on TCP packet boundaries.
- Each client connection is handled independently using asynchronous I/O.
- Client connections remain open until the client disconnects or the server shuts down.
- The server uses concurrent client handling to serve multiple connected clients.
- Every application message is encrypted using AES-256-GCM. A fresh random 96-bit nonce and 128-bit authentication tag are used for each message.
- The AES key is derived from the shared passphrase using SHA-256 for this self-contained assignment demo.
- The passphrase must match on both sides. The default is for local testing only; choose a different value for assessment demonstrations.
- Maximum framed payload is limited to 1 MiB.

## Suggested Test Checklist

1. Run one server and one client; test `SetA-Two`.
2. Send multiple valid requests from the same client without reconnecting.
3. Test all valid collection pairs and confirm response counts.
4. Test invalid set/item names and malformed input; confirm `EMPTY`.
5. Confirm the client accepts the next request after receiving `__END__`.
6. Run at least three client instances at the same time.
7. Enter a different passphrase on one side and confirm decryption fails.
8. Disconnect one client during communication and confirm the server remains available for other clients.
9. Enter `exit` and confirm the client disconnects gracefully.

## Security Note

This is an educational assignment implementation. In a production system, provision keys securely rather than 
asking users to type a shared passphrase. Use a proper password-based key derivation function when deriving keys 
from passwords and use TLS for transport security and peer authentication.