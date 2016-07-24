using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reddis
{
    [TestFixture]
    public class HelloWorldExamples
    {
        private const string connectionString = "localhost";
        private const string redisName = "redis-instance";
        string docker = "docker";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string args = $"run --name {redisName} -P -d redis";
            Process.Start(docker, args).WaitForExit();
        }

        [OneTimeTearDown]
        public void OneTimeTeardown()
        {
            Process.Start(docker, $"stop {redisName} ").WaitForExit();
            Process.Start(docker, $"rm {redisName} ").WaitForExit();
        }

        [Test]
        public async Task HelloWorld_of_key_value_store()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);

            var db = redis.GetDatabase();

            const string key = "TestKey";
            const string testValue = "TestValue";

            await db.StringSetAsync(key, testValue);

            string result = await db.StringGetAsync(key);

            Assert.AreEqual(testValue, result);
        }

        [Test]
        public async Task HelloWorld_of_PubSub()
        {
            bool messageDelivered = false;
            const string channelName = "messages";

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connectionString);
            var sub = redis.GetSubscriber();

            sub.Subscribe(channelName, (channel, message) => 
            {
                Console.WriteLine((string)message);
                messageDelivered = true;
            });

            await sub.PublishAsync(channelName, "hello");

            Assert.IsTrue(messageDelivered);
        }
    }
}
