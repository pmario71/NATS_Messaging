using NATS.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NATS_Client
{
    class Program
    {
        private const int nrOfIterations = 300;
        private static EventWaitHandle _handle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static readonly byte[] endMark = new byte[] { 0 };

        static void Main(string[] args_in)
        {
            CaptureThreadID();

            // Create a new connection factory to create
            // a connection.
            ConnectionFactory connectionFactory = new ConnectionFactory();

            // Creates a live connection to the default
            // NATS Server running locally
            Options opts = ConnectionFactory.GetDefaultOptions();
            Console.WriteLine(opts.Url);

            IConnection connection = connectionFactory.CreateConnection();

            // Alternatively, create an asynchronous subscriber on subject foo, 
            // assign a message handler, then start the subscriber.   When
            // multicasting delegates, this allows all message handlers
            // to be setup before messages start arriving.
            IAsyncSubscription sAsync = connection.SubscribeAsync("foo");
            sAsync.MessageHandler += MessageHandler;
            sAsync.Start();

            var sw = Stopwatch.StartNew();

            Console.WriteLine("Start publishing ...");
            for (int i = 0; i < nrOfIterations; i++)
            {
                connection.Publish("foo", Encoding.UTF8.GetBytes("hello world " + i.ToString()));
            }

            connection.Publish("foo", endMark);
            Console.WriteLine("Publish finished!");

            sw.Stop();
            Console.WriteLine("Sending {0} messages took: {1} ms", nrOfIterations, sw.ElapsedMilliseconds);
            Console.WriteLine("Message deliverd on {0} threads.", _threadID.Count);

            // Unsubscribing
            sAsync.Unsubscribe();

            connection.Flush();

            //// Publish requests to the given reply subject:
            //connection.Publish("foo", "bar", Encoding.UTF8.GetBytes("help!"));

            //// Sends a request (internally creates an inbox) and Auto-Unsubscribe the
            //// internal subscriber, which means that the subscriber is unsubscribed
            //// when receiving the first response from potentially many repliers.
            //// This call will wait for the reply for up to 1000 milliseconds (1 second).
            //m = connection.Request("foo", Encoding.UTF8.GetBytes("help"), 1000);

            // Closing a connection
            connection.Close();
        }

        static HashSet<int> _threadID = new HashSet<int>();
        private static int counter;

        private static void MessageHandler(object sender, MsgHandlerEventArgs e)
        {
            CaptureThreadID();
            if ((++counter) % 100 == 0)
            {
                Console.WriteLine("Received messages: {0}", counter);
            }
            //Console.WriteLine("  > Message received:  ", e.Message.Subject);

            if (ByteArrayHelper.ByteArrayCompare(e.Message.Data, endMark))
            {
                Console.WriteLine("All messages received!");
                //_handle.Set();
            }
        }

        private static void CaptureThreadID()
        {
            var threadID = Thread.CurrentThread.ManagedThreadId;

            if (!_threadID.Contains(threadID))
                _threadID.Add(threadID);
        }
    }
}
