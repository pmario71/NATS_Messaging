using NATS.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NATS_Client
{
    class Receiver
    {
        private HashSet<int> _threadID = new HashSet<int>();
        private int _counter;
        private EventWaitHandle _handle;

        public Receiver(EventWaitHandle handle)
        {
            Contract.Requires<ArgumentNullException>(handle != null);
            _handle = handle;
        }

        public void MessageHandler(object sender, MsgHandlerEventArgs e)
        {
            //Console.WriteLine($"ThreadContext MessageHandler: {Thread.CurrentThread.ManagedThreadId}");

            CaptureThreadID();
            if ((++_counter) % 100 == 0)
            {
                Console.WriteLine("Received messages: {0}", _counter);
            }
            //Console.WriteLine("  > Message received:  ", e.Message.Subject);

            if (ByteArrayHelper.ByteArrayCompare(e.Message.Data, EndMark))
            {
                Console.WriteLine("All messages received!");
                _handle.Set();
            }
        }

        public static readonly byte[] EndMark = new byte[] { 0 };

        public int[] Threads
        {
            get
            {
                return _threadID.ToArray();
            }
        }

        public int Counter
        {
            get
            {
                return _counter;
            }
        }

        public void CaptureThreadID(string context = null)
        {
            var threadID = Thread.CurrentThread.ManagedThreadId;

            if (!_threadID.Contains(threadID))
                _threadID.Add(threadID);
        }
    }
}
