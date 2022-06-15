using GetInfra.Standard.Queue.Model;
using System;

namespace GetInfra.Standard.Queue
{
    public interface IQueueConsumer
    {
        event Action<object, QMessage> MessageRecieved;

        void Subscribe();

        void Unsubscribe();

        QMessage Dequeue<T>();

        QMessage Dequeue<T>(bool ack = false);
    }
}
