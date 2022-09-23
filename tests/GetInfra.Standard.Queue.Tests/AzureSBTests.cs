using FluentAssertions;
using GetInfra.Standard.Queue.Implementations.ServiceBus;
using GetInfra.Standard.Queue.Model;
using GetInfra.Standard.Queue.Tests.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Standard.Queue.Tests
{
    public class AzureSBTests
    {
        IConfiguration _configuration;
        public AzureSBTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("test-settings.json")
                .Build();
        }

        [Fact]
        public async Task SanityTest()
        {
            // define publisher
            IQueuePublisher publisher = new AzureSBTopicPublisher(new LoggerFactory(),_configuration, new DefaultJsonSerializer(), "publisher");

            // enqueue
            var exception = await Record.ExceptionAsync(() => publisher.Enqueue(new QMessage() { Body = new DummyObject() { Id = 1, Name = "test" } }));

            exception.Should().BeNull();

           
        }


        [Fact]
        public async Task DequeueTest()
        {
            // define
            IQueueConsumer consumer = new AzureSBTopicConsumer(new LoggerFactory(), _configuration, new DefaultJsonSerializer(), "subscriber");
            CancellationTokenSource source = new CancellationTokenSource();

            // dequeue
            // TODO
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
            QMessage recievedMsg = null;
            QProperties oProperties = null;
            consumer.MessageRecieved += (m, o) =>
            {
                consumer.Unsubscribe();
                recievedMsg = o;
                oProperties = o.Properties;
                _autoResetEvent.Set();
            };

            consumer.Subscribe();


            Assert.True(_autoResetEvent.WaitOne(3000));
            Assert.NotNull(recievedMsg);


        }

        

    }
}
