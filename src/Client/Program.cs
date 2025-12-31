using ChatApplication.Protos;
using Grpc.Net.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

Console.Write("Enter your username: ");
string? userName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(userName))
{
    userName = "Anonymous";
}

// Create gRPC channel
using var channel = GrpcChannel.ForAddress("https://localhost:7000");
var client = new ChatService.ChatServiceClient(channel);

// Create bidirectional streaming call
using var call = client.Chat();

string? assignedUserId = null;

// Task for sending messages (user input thread)
var sendTask = Task.Run(async () =>
{
    try
    {
        while (true)
        {
            string? messageContent = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(messageContent))
                continue;

            var message = new ChatMessage
            {
                UserName = userName,
                Content = messageContent
            };

            await call.RequestStream.WriteAsync(message);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error sending message: {ex.Message}");
    }
});

// Task for receiving messages (receive thread)
var receiveTask = Task.Run(async () =>
{
    try
    {
        while (await call.ResponseStream.MoveNext(CancellationToken.None))
        {
            var message = call.ResponseStream.Current;
            if (assignedUserId == null)
            {
                assignedUserId = message.UserId;
                Console.WriteLine($"\n[Connected] Your userId: {assignedUserId}\n");
            }

            Console.WriteLine($"[{message.UserName} ({message.UserId})]: {message.Content}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error receiving message: {ex.Message}");
    }
});

// Wait for either task to complete
await Task.WhenAny(sendTask, receiveTask);
Console.WriteLine("Disconnected from chat");
