using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Redis.Tests.Channels;

public class ChannelTests
{
    private readonly ITestOutputHelper _output;

    public ChannelTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task PubSub()
    {
        // Given
    
        var channel = Channel.CreateUnbounded<string>();

        await channel.Writer.WriteAsync("hello 1");

        // When
        int messageCount = 0;

        var t = Task.Run(async () =>
        {
            // Simulate subscribing to a channel
            await foreach (var message in channel.Reader.ReadAllAsync())
            {
                _output.WriteLine($"Received message: {message}");
                messageCount++;
            }
        });

        // Simulate publishing a message
        await channel.Writer.WriteAsync("hello 2");
        channel.Writer.Complete();
        await Task.Yield();
        await t;


        // Then
        Assert.Equal(2 , messageCount);

    }

}
