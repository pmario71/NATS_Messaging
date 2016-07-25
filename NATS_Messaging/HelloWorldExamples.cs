using NATS.Client;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NATS_Client
{
    [TestFixture]
    public class HelloWorldExamples
    {
        private const int nrOfIterations = 300;
        
        [Test]
        public void HelloWorld()
        {
            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            var receiver = new Receiver(waitHandle);

            receiver.CaptureThreadID();
            Console.WriteLine($"ThreadContext Main: {Thread.CurrentThread.ManagedThreadId}");

            // Create a new connection factory to create
            // a connection.
            ConnectionFactory connectionFactory = new ConnectionFactory();

            // Creates a live connection to the default
            // NATS Server running locally
            Options opts = ConnectionFactory.GetDefaultOptions();
            Console.WriteLine(opts.Url);

            using (IConnection connection = connectionFactory.CreateConnection())
            {

                // Alternatively, create an asynchronous subscriber on subject foo, 
                // assign a message handler, then start the subscriber.   When
                // multicasting delegates, this allows all message handlers
                // to be setup before messages start arriving.
                IAsyncSubscription sAsync = connection.SubscribeAsync("foo");
                sAsync.MessageHandler += receiver.MessageHandler;
                sAsync.Start();

                var sw = Stopwatch.StartNew();

                Console.WriteLine("Start publishing ...");
                for (int i = 0; i < nrOfIterations; i++)
                {
                    connection.Publish("foo", Encoding.UTF8.GetBytes("hello world " + i.ToString()));
                }

                connection.Publish("foo", Receiver.EndMark);
                Console.WriteLine("Publish finished!");

                sw.Stop();
                Console.WriteLine("Sending {0} messages took: {1} ms", nrOfIterations, sw.ElapsedMilliseconds);
                Console.WriteLine("Message deliverd on {0} threads.", receiver.Threads.Length);

                if (!waitHandle.WaitOne(10000))
                {
                    Console.WriteLine("Not all messages were delivered within 5s!");
                    Assert.Fail($"Number of messages processd: {receiver.Counter} / {nrOfIterations}");
                }

                // Unsubscribing
                //sAsync.Unsubscribe();

                //connection.Flush();

                //// Publish requests to the given reply subject:
                //connection.Publish("foo", "bar", Encoding.UTF8.GetBytes("help!"));

                //// Sends a request (internally creates an inbox) and Auto-Unsubscribe the
                //// internal subscriber, which means that the subscriber is unsubscribed
                //// when receiving the first response from potentially many repliers.
                //// This call will wait for the reply for up to 1000 milliseconds (1 second).
                //m = connection.Request("foo", Encoding.UTF8.GetBytes("help"), 1000);

                // Closing a connection
            }
        }
    }
}
