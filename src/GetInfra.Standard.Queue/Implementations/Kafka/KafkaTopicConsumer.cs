using Confluent.Kafka;
using GetInfra.Standard.Queue.Implementations.Kafka.Config;
using GetInfra.Standard.Queue.Model;
using System;
using System.Threading;

namespace GetInfra.Standard.Queue.Implementations.Kafka
{
    public class KafkaTopicConsumer : BaseTopicConsumer, ITopicConsumer
    {
        public readonly ConsumerConfig _config;
        private readonly IJsonSerializer _serializer;
        public readonly KafkaConsumerConfig _consumerConfig;
        public readonly string _name;
        public override string Name { get => _name; }

        public event Action<object, QMessage> MessageRecieved;

        public KafkaTopicConsumer(IJsonSerializer serializer, KafkaConsumerConfig consumerConfig)
        {
            _consumerConfig = consumerConfig;
            _serializer = serializer;
            _name = _consumerConfig.Name;
            _config = new ConsumerConfig
            {
                BootstrapServers = _consumerConfig.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                GroupId = _consumerConfig.GroupId
            };
        }

        public QMessage Consume<T>()
        {
            return Consume<T>(true);
        }

        public QMessage Consume<T>(bool ack = false)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
            {
                consumer.Subscribe(_consumerConfig.Topic);

                var cancelToken = new CancellationTokenSource();

                var consumeResult = consumer.Consume(cancelToken.Token);

                var msg = consumeResult.Message;

                consumer.Close();
                
                var ourMsg = _serializer.Deserialize<QMessage>(msg.Value);

                //ourMsg.Properties = new QProperties() {  } = consumeResult.Topic

                return ourMsg;
            }
        }

        public void Subscribe(CancellationToken token)
        {
            using (var consumer = new ConsumerBuilder<Ignore, string>(_config).Build())
            {
                consumer.Subscribe(_consumerConfig.Topic);

                while (!token.IsCancellationRequested)
                {
                    // handle consumed message.
                    var consumeResult = consumer.Consume(token);
                    var ourMsg = _serializer.Deserialize<QMessage>(consumeResult.Message.Value);
                    if (MessageRecieved != null)
                        MessageRecieved(_consumerConfig.Topic, ourMsg);
                }

                consumer.Close();
            }
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}
