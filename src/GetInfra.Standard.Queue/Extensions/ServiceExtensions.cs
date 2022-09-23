using GetInfra.Standard.Queue.Implementations.Kafka;
using GetInfra.Standard.Queue.Implementations.Kafka.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GetInfra.Standard.Queue.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureKafkaConsumer(this IServiceCollection services, IConfiguration configuration, string configName)
        {
            var config = new KafkaConsumerConfig();
            config.BootstrapServers = configuration.GetValue<string>($"Kafka:{configName}:BootstrapServers");
            config.GroupId = configuration.GetValue<string>($"Kafka:{configName}:GroupId");
            config.Name = configName;
            config.Topic = configuration.GetValue<string>($"Kafka:{configName}:Topic");


            services.AddSingleton<ITopicConsumer>(s => new KafkaTopicConsumer(new DefaultJsonSerializer(), config));
        }

        public static void ConfigureKafkaPublisher(this IServiceCollection services, IConfiguration configuration, string configName)
        {
            var config = new KafkaPublisherConfig();
            config.BootstrapServers = configuration.GetValue<string>($"Kafka:{configName}:BootstrapServers");
            config.Name = configName;
            config.Topic = configuration.GetValue<string>($"Kafka:{configName}:Topic");


            services.AddSingleton<ITopicPublisher>(s => new KafkaTopicPublisher(new DefaultJsonSerializer(), config));
        }
    }
}
