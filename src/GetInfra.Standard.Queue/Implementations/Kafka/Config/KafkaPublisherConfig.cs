using System;
using System.Collections.Generic;
using System.Text;

namespace GetInfra.Standard.Queue.Implementations.Kafka.Config
{
    public class KafkaPublisherConfig
    {
        public string BootstrapServers { get; set; }
        public string Topic { get; set; }

        public string Name { get; set; }
    }
}
