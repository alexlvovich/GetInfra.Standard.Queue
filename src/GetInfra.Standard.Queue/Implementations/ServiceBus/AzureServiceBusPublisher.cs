using System;
using System.Text;
using System.Threading.Tasks;
using GetInfra.Standard.Queue.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetInfra.Standard.Queue.Implementations.ServiceBus
{
    public class AzureServiceBusPublisher : IQueuePublisher
    {
        public ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly QueueClient _client;
        private readonly IJsonSerializer _serializer;

        public AzureServiceBusPublisher(ILoggerFactory loggerFactory, IConfiguration configuration, IJsonSerializer serializer, string publisherName)
        {
            _logger = loggerFactory.CreateLogger<AzureServiceBusPublisher>();
            _serializer = serializer;
            _configuration = configuration;

            if (publisherName == null)
            {
                _logger.LogError("publisher name not specified");
                throw new ArgumentException("publisher name not specified");
            }

            var publisher = (ServiceBusConfig)_configuration.GetSection("AzureServiceBus:" + publisherName);
            if (publisher == null)
            {
                _logger.LogError("publisher configuration not found");
                throw new Exception("publisher configuration not found");
            }

            var conSting = new ServiceBusConnectionStringBuilder(publisher.Endpoint, publisher.EntityPath, publisher.SasKeyName, publisher.SasKey);

            _client = new QueueClient(conSting);
        }

        public async Task Enqueue(QMessage msg)
        {
            var jsonified = _serializer.Serialize(msg);

            var messageBuffer = Encoding.UTF8.GetBytes(jsonified);
            await _client.SendAsync(new Message(messageBuffer));

        }

       
    }
}
