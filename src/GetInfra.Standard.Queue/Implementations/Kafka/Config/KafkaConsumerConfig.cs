namespace GetInfra.Standard.Queue.Implementations.Kafka.Config
{
    public class KafkaConsumerConfig
    {
        public string BootstrapServers { get; set; }
        public string GroupId { get; set; }
        public string Topic { get; set; }

        public string Name { get; set; }
    }
}
