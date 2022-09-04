using FluentAssertions;
using GetInfra.Standard.Queue.Implementations.Kafka;
using GetInfra.Standard.Queue.Implementations.Kafka.Config;
using GetInfra.Standard.Queue.Model;
using GetInfra.Standard.Queue.Tests.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Standard.Queue.Tests
{
    public class KafkaTests
    {
        IConfiguration _configuration;
        public KafkaTests()
        {
            //_configuration = new ConfigurationBuilder()
            //    .AddJsonFile("test-settings.json")
            //    .Build();
        }

        [Fact]
        public async Task ProduceTest()
        {
            // define publisher
            ITopicPublisher publisher = new KafkaTopicPublisher(new DefaultJsonSerializer(), new KafkaPublisherConfig() { BootstrapServers = "localhost:29092,localhost:39092", Topic = "weblog" });

            // enqueue
            var exception = await Record.ExceptionAsync(() => publisher.Produce(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } }));

            exception.Should().BeNull();


        }


        [Fact]
        public async Task DequeueTest()
        {
            ITopicPublisher publisher = new KafkaTopicPublisher(new DefaultJsonSerializer(), new KafkaPublisherConfig() { BootstrapServers = "localhost:29092,localhost:39092", Topic = "weblog" });
            await publisher.Produce(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } });
            // define
            ITopicConsumer consumer = new KafkaTopicConsumer(new DefaultJsonSerializer(), new KafkaConsumerConfig() { BootstrapServers= "localhost:29092,localhost:39092", GroupId = "consumer_group", Topic = "weblog" });

            // dequeue
            var obj = consumer.Consume<DummyObject>();
            // TODO
            obj.Should().NotBeNull();


        }
    }
}
