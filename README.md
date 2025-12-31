# gRPC Bidirectional Chat Application

A simple chat application built with gRPC and .NET 8.0 demonstrating bidirectional streaming communication between a server and multiple clients.

## Features

- **Bidirectional Streaming**: Clients can send and receive messages simultaneously using gRPC bidirectional streaming
- **User Management**: Server assigns unique `userId` to each client; clients specify their username (duplicates allowed)
- **Message Broadcasting**: All messages from any client are broadcast to all connected clients
- **Rich Message Format**: Messages include `userId`, `username`, `content`, `timestamp`, and `messageId`
- **Concurrent Client Handling**: Separate threads handle user input (sending) and message reception (receiving) on the client side
- **Centralized Package Management**: Uses `Directory.Packages.props` to manage NuGet package versions globally

## Project Structure

```
ChatApplication/
├── ChatApplication.sln                    # Solution file
├── Directory.Packages.props                # Centralized NuGet package versions
├── README.md                               # This file
├── proto/
│   └── chat.proto                          # gRPC service definition
└── src/
    ├── Server/
    │   ├── ChatServer.csproj              # Server project file
    │   ├── Program.cs                     # Server entry point
    │   ├── appsettings.json               # Server configuration
    │   ├── appsettings.Development.json   # Development configuration
    │   └── Services/
    │       └── ChatServiceImpl.cs          # gRPC service implementation
    └── Client/
        ├── ChatClient.csproj              # Client project file
        └── Program.cs                     # Client entry point and console UI
```

## Technology Stack

- **.NET 8.0** - Target framework
- **gRPC** - Communication protocol (version 2.60.0)
- **Protocol Buffers** - Message serialization (version 3.25.1)
- **ASP.NET Core** - Server hosting (Kestrel)
- **C# 8.0+** - Programming language

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later installed
- Windows, macOS, or Linux

### Building the Solution

```bash
cd ChatApplication
dotnet build
```

This command restores all dependencies and compiles both the server and client projects.

### Running the Server

```bash
dotnet run --project src/Server/ChatServer.csproj
```

The server will start and listen on `https://localhost:7000` for incoming gRPC connections.

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Running the Client

In a new terminal window:

```bash
dotnet run --project src/Client/ChatClient.csproj
```

The client will prompt you to enter a username:
```
Enter your username: Alice
```

Once connected, you'll see:
```
[Connected] Your userId: user_1
```

Now you can type messages and they will be sent to all connected clients.

### Multi-Client Testing

Open multiple terminals and run the client command in each. Each client will receive:
- A unique `userId` assigned by the server
- All messages from all connected clients in real-time

Example with 3 clients:

**Client 1 (Alice):**
```
Enter your username: Alice
[Connected] Your userId: user_1
Hello from Alice
[Alice (user_1)]: Hello from Alice
[Bob (user_2)]: Hi Alice!
[Charlie (user_3)]: Hey everyone!
```

**Client 2 (Bob):**
```
Enter your username: Bob
[Connected] Your userId: user_2
[Alice (user_1)]: Hello from Alice
Hi Alice!
[Bob (user_2)]: Hi Alice!
[Charlie (user_3)]: Hey everyone!
```

**Client 3 (Charlie):**
```
Enter your username: Charlie
[Connected] Your userId: user_3
[Alice (user_1)]: Hello from Alice
[Bob (user_2)]: Hi Alice!
Hey everyone!
[Charlie (user_3)]: Hey everyone!
```

## How It Works

### Server Architecture

The `ChatServiceImpl` class implements the gRPC `ChatService`:

1. **Connection Management**: When a client connects, the server assigns a unique `userId` and registers the client's response stream
2. **Message Handling**: Listens for incoming messages from each connected client
3. **Message Broadcasting**: When a message arrives, it's enriched with `messageId` and `timestamp`, then sent to all connected clients
4. **Cleanup**: When a client disconnects, it's removed from the connected clients list

### Client Architecture

The client implements a two-threaded design:

1. **Send Thread**: Continuously reads user input from the console and sends messages to the server
2. **Receive Thread**: Listens for incoming messages from the server and displays them on the console

This allows users to type and receive messages concurrently without blocking.

## Message Format

Messages are defined in `proto/chat.proto` as `ChatMessage`:

```protobuf
message ChatMessage {
  string userId = 1;           // Server-assigned unique user ID
  string userName = 2;         // User-specified username (can be duplicated)
  string content = 3;          // Message content
  int64 timestamp = 4;         // Unix timestamp (set by server)
  string messageId = 5;        // Unique message ID (UUID, set by server)
}
```

## API Definition

The gRPC service is defined in `proto/chat.proto`:

```protobuf
service ChatService {
  rpc Chat (stream ChatMessage) returns (stream ChatMessage);
}
```

This bidirectional streaming RPC allows clients to send multiple messages and receive multiple messages in a single connection.

## Future Enhancements

- User authentication
- Private messaging (direct messages between users)
- Chat rooms/channels
- Message history/persistence
- User list broadcast
- Typing indicators
- Message encryption
- Graceful disconnect handling with notifications
- Connection timeout handling

## Troubleshooting

### Port Already in Use
If port 7000 is already in use, modify the server's `appsettings.json` to use a different port, and update the client's connection URL accordingly.

### SSL/TLS Certificate Issues
The application uses HTTPS by default. If you encounter certificate issues during development, you may need to trust the development certificate:

```bash
dotnet dev-certs https --trust
```

### Client Connection Refused
Ensure the server is running before starting the client. The client attempts to connect to `https://localhost:7000`.

## License

This project is provided as-is for educational purposes.
