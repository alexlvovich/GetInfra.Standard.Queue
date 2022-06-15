using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Model;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq
{
    public class BaseRabbitMq
    {
        public ILogger _logger;


        public BaseRabbitMq(ILoggerFactory loggerFactory, RbmqConfigurationElement settings)
        {
            _logger = loggerFactory.CreateLogger<RbmqQueue>();
            // default serialization settings
        }
        public IConnection GetConnection(QueueSettings config)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    UserName = config.Username,
                    Password = config.Password,
                    VirtualHost = "/",
                    HostName = config.Host,
                    Port = config.Port != 0 ? config.Port : AmqpTcpEndpoint.UseDefaultPort
                };

                factory.AutomaticRecoveryEnabled = false; // automaticRecoveryEnabled;

                // VHost
                if (!string.IsNullOrEmpty(config.Vhost))
                    factory.VirtualHost = config.Vhost;

                return factory.CreateConnection();
            }
            catch (Exception ex)
            {
                _logger.LogCritical("GetConnection Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                Thread.Sleep(1000);
                this.GetConnection(config);
            }
            return null;
        }

        public void Bind(IModel channel, string queue, string ex, string routingKey)
        {
            channel.QueueBind(queue, ex, routingKey);
        }

        public void DeclareExchange(IModel channel, QueueSettings settings, string exchnageName)
        {
            channel.ExchangeDeclare(exchnageName, settings.ExchangeType, settings.IsDurable, false, null);
        }

        public void DeclareQueue(IModel channel, QueueSettings settings, string queueName, IDictionary<string, object> queueArgs = null, bool exclusive = false)
        {
            if (settings.Exclusive)
                settings.AutoDelete = true;

            channel.QueueDeclare(
                queue: queueName,
                durable: settings.IsDurable,
                exclusive: exclusive,
                autoDelete: settings.AutoDelete,
                arguments: queueArgs);
        }

        public IModel Initialize(IConnection conn, QueueSettings settings)
        {

            // create our channels
            var channel = conn.CreateModel();

            // args
            Dictionary<String, Object> args = new Dictionary<string, object>();

            if (settings.DeadLetters)
            {
                //dead letter Exchange
                string deadLetterEx = $"{settings.Queue}.dead-letter-ex";
                DeclareExchange(channel, settings, deadLetterEx);

                // dead letter queue
                string deadLetterQ = $".dead-letter-q";
                DeclareQueue(channel, settings, deadLetterQ, null);
                Bind(channel, deadLetterQ, deadLetterEx, settings.RoutingKey);

                args.Add("x-dead-letter-exchange", deadLetterEx);
                args.Add("x-dead-letter-routing-key", settings.RoutingKey);

            }
            // create ex
            DeclareExchange(channel, settings, settings.Exchange);

            // create Queue


            //if (_settings.RetryDelay > 0)
            //{
            //    // c.ExchangeDeclare(delayedExchange, "x-delayed-message", true, true, CreateProperty("x-delayed-type", "direct")


            //    string delayedExchange = $"{_settings.Queue}.delayed";
            //    args.Add("x-dead-letter-exchange", delayedExchange ?? "");
            //}
            if (settings.QoS > 0)
                channel.BasicQos(0, settings.QoS, false);


            DeclareQueue(channel, settings, settings.Queue, args, settings.Exclusive);
            // bind
            Bind(channel, settings.Queue, settings.Exchange, settings.RoutingKey);

            return channel;
        }
    }
}
