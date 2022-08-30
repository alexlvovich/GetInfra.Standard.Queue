using GetInfra.Standard.Queue.Model;
using System;
using System.Threading;

namespace GetInfra.Standard.Queue
{
    public interface ITopicConsumer
    {
        event Action<object, QMessage> MessageRecieved;

        void Subscribe(CancellationToken token);

        QMessage Consume<T>();

        QMessage Consume<T>(bool ack = false);
    }
}
