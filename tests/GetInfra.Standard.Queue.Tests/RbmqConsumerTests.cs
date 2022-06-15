using FluentAssertions;
using GetInfra.Standard.Queue.Implementations.RabbitMq;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Model;
using GetInfra.Standard.Queue.Tests.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Standard.Queue.Tests
{
    public class RbmqConsumerTests
    {
        [Fact]
        public async Task DequeueTest()
        {
            var config = new RbmqConfigurationElement()
            {
                Host = "localhost",
                Port = 49154,
                Username = "tester",
                Password = "P@ssw0rd",
                Exchange = "exchange.dev.direct",
                ExchangeType = "direct",
                Queue = "queue-for-test"
            };

            // define publisher
            IQueuePublisher publisher = new RabbitMqPublisher(new LoggerFactory(), config);

            // enqueue
            var exception = await Record.ExceptionAsync(() => publisher.Enqueue(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } }));


            exception.Should().BeNull();

            IQueueConsumer consumer = new RabbitMqConsumer(new LoggerFactory(), config);

            var response = consumer.Dequeue<DummyObject>();

            response.Should().NotBeNull();

        }
    }
}
