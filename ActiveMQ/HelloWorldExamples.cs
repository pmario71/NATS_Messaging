using Apache.NMS;
using Apache.NMS.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActiveMQ
{
    [TestFixture]
    public class HelloWorldExamples
    {
        private const string connectionString = "localhost";
        private const string activeMQName = "activeMQ-instance";

        //[OneTimeSetUp]
        //public void OneTimeSetup()
        //{
        //    string args = $"run --name {activeMQName} -rm -P -d webcenter/activemq:latest";
        //    Process.Start(docker, args).WaitForExit();
        //}

        //[OneTimeTearDown]
        //public void OneTimeTeardown()
        //{
        //    Process.Start(docker, $"stop {activeMQName} ").WaitForExit();
        //}

        [Test]
        public void Test()
        {

            Uri connecturi = new Uri("activemq:tcp://localhost:61616");
            IConnectionFactory factory = new NMSConnectionFactory(connecturi);

            using (IConnection connection = factory.CreateConnection())
            using (ISession session = connection.CreateSession())
            {
                IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                Console.WriteLine("Using destination: " + destination);

                // Create a consumer and producer
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    // Start the connection so that messages will be processed.
                    connection.Start();
                    producer.DeliveryMode = MsgDeliveryMode.Persistent;

                    // Send a message
                    ITextMessage request = session.CreateTextMessage("Hello World!");
                    request.NMSCorrelationID = "abc";
                    request.Properties["NMSXGroupID"] = "cheese";
                    request.Properties["myHeader"] = "Cheddar";

                    producer.Send(request);

                    // Consume a message
                    ITextMessage message = consumer.Receive() as ITextMessage;
                    if (message == null)
                    {
                        Console.WriteLine("No message received!");
                    }
                    else
                    {
                        Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                        Console.WriteLine("Received message with text: " + message.Text);
                    }
                }

            }
        }

    }
}
