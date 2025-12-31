using ChatApplication.Protos;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApplication.Services;

public class ChatServiceImpl : ChatService.ChatServiceBase
{
    private static readonly ConcurrentDictionary<string, IServerStreamWriter<ChatMessage>> ConnectedClients = new();
    private static int UserIdCounter = 0;
    private static readonly object UserIdLock = new();

    public override async Task Chat(IAsyncStreamReader<ChatMessage> requestStream, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
    {
        string assignedUserId = GenerateUserId();
        
        // Register the client
        ConnectedClients.TryAdd(assignedUserId, responseStream);
        Console.WriteLine($"Client connected with userId: {assignedUserId}");

        try
        {
            // Read incoming messages from the client
            await foreach (var message in requestStream.ReadAllAsync())
            {
                // Set the userId for the message
                message.UserId = assignedUserId;
                message.MessageId = Guid.NewGuid().ToString();
                message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                Console.WriteLine($"Message from {message.UserName} ({message.UserId}): {message.Content}");

                // Broadcast the message to all connected clients
                await BroadcastMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Client {assignedUserId} disconnected");
        }
        finally
        {
            // Remove the client from the connected list
            ConnectedClients.TryRemove(assignedUserId, out _);
            Console.WriteLine($"Client {assignedUserId} removed from chat");
        }
    }

    private string GenerateUserId()
    {
        lock (UserIdLock)
        {
            return $"user_{++UserIdCounter}";
        }
    }

    private async Task BroadcastMessageAsync(ChatMessage message)
    {
        var tasks = new List<Task>();

        foreach (var (userId, writer) in ConnectedClients)
        {
            tasks.Add(writer.WriteAsync(message));
        }

        await Task.WhenAll(tasks);
    }
}
