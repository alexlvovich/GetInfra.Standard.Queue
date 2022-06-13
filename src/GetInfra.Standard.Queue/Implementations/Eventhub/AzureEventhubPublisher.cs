using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GetInfra.Standard.Queue.Model;

namespace GetInfra.Standard.Queue.Implementations.Eventhub
{
    public class AzureEventhubPublisher : IQueuePublisher
    {
        /// <summary>
        ///  sdfdsfsd
        /// </summary>
        ///private readonly EventHubClient _client;

        public Task Enqueue(QMessage msg)
        {
            throw new NotImplementedException();
        }
    }
}
