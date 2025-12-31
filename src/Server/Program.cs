using ChatApplication.Services;
using ChatApplication.Protos;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddGrpc();
builder.Services.AddSingleton<ChatServiceImpl>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<ChatServiceImpl>();
app.MapGet("/", () => "Use a gRPC client to communicate with this server.");

app.Run();
