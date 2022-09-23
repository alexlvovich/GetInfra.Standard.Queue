using Confluent.Kafka;
using GetInfra.Standard.Queue.Implementations.Kafka.Config;
using GetInfra.Standard.Queue.Model;
using System.Net;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue.Implementations.Kafka
{
    public class KafkaTopicPublisher : ITopicPublisher
    {
        private readonly ProducerConfig _config;
        private readonly IJsonSerializer _serializer;
        private readonly KafkaPublisherConfig _kafkaPublisherConfig;

        public KafkaTopicPublisher(IJsonSerializer serializer, KafkaPublisherConfig kafkaPublisherConfig)
        {
            _serializer = serializer;
            _kafkaPublisherConfig = kafkaPublisherConfig;
            _config = new ProducerConfig
            {
                BootstrapServers = _kafkaPublisherConfig.BootstrapServers,
                ClientId = Dns.GetHostName(),
            };
        }
        public async Task Produce(QMessage msg)
        {
            var jsonified = _serializer.Serialize(msg);

            using (var producer = new ProducerBuilder<Null, string>(_config).Build())
            {
                await producer.ProduceAsync(_kafkaPublisherConfig.Topic, new Message<Null, string> { Value = jsonified });
            }
        }
    }
}
