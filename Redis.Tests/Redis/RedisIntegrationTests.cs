using StackExchange.Redis;

namespace Redis.Tests.Redis;

public class RedisIntegrationTests : IClassFixture<DABContext>
{
    private readonly DABContext _dAB;

    public RedisIntegrationTests(DABContext dAB)
    {
        _dAB = dAB;
    }

    [Fact]
    public async Task Set_and_read_key()
    {
        // Arrange
        var db = _dAB.Redis.GetDatabase();

        // Act
        const string key = "TestKey";
        const string testValue = "TestValue";

        await db.StringSetAsync(key, testValue);

        string? result = await db.StringGetAsync(key);

        Assert.Equal(testValue, result);
    }

    [Fact]
    public async Task PubSub()
    {
        // Given
        bool messageDelivered = false;
        var channelName = RedisChannel.Literal("messages");

        // When
        var sub = _dAB.Redis.GetSubscriber();

        sub.Subscribe(channelName, (channel, message) =>
        {
            // Console.WriteLine((string)message);
            messageDelivered = true;
        });

        await sub.PublishAsync(channelName, "hello");

        // Wait for a short time to allow the message to be processed
        await Task.Delay(100);

        // Then
        Assert.True(messageDelivered, "Message was not delivered to the subscriber.");
    }
}
