using System;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using NATS.Client.Core;
using NATS.Net;
using Xunit.Abstractions;

namespace Redis.Tests;

public class NATSIntegrationTests : IClassFixture<DABContext>
{
    private readonly DABContext _dAB;
    private readonly ITestOutputHelper _output;

    public NATSIntegrationTests(DABContext dAB, ITestOutputHelper output)
    {
        _dAB = dAB;
        _output = output;
    }

    [Fact]
    public async Task PubSub()
    {
        // Given
        var client = _dAB.NatsClient;
        INatsConnection conn = client.Connection;

        await conn.PublishAsync("greet.joe", "Hey Joe!");

        int i = 0;
        CancellationTokenSource cts = new();

        var t = Task.Run(async () =>
        {
            _output.WriteLine("Starting to listen for messages...");

            // ensure a new client and connection is used
            await foreach (var msg in _dAB.NatsClient.Connection.SubscribeAsync<string>("greet.*", cancellationToken: cts.Token)
                    .ConfigureAwait(false))
            {
                // _output.WriteLine($"Received message: {msg.Subject} / {msg.Data!}");
                i++;

                if (msg.Subject == "greet.jeff")
                {
                    break; // stop after receiving the message for Joe
                }
            }
        }, cts.Token); 

        await conn.PublishAsync("greet.jeff", "Hello Jeff!");


        // wait until all messages are processed
        await Task.Yield(); // Yield to allow the async task to run
        await Task.Delay(1000); // Wait a bit to ensure the message is processed
        
        if (!t.IsCompleted)
        {
            cts.Cancel(); // Cancel the task if it is still running
            _output.WriteLine("Task was cancelled due to timeout.");
        }
        else
        {
            _output.WriteLine("Task completed successfully.");
        }


        Assert.Equal(2, i);
    }
}

// public record Msg(string msg);

