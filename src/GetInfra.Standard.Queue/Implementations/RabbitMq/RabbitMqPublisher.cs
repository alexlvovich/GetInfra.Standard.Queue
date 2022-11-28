using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq
{
    public class RabbitMqPublisher : BaseRabbitMq, IQueuePublisher
    {
        private readonly QueueSettings _publisherSettings;
        private JsonSerializerSettings _publisherSerializationSettings;
        private IConnection _publishConn;
        private IModel _publishChannel;
        private object _lockPublisher = new object();
        public JsonSerializerSettings PublisherSerializationSettings
        {
            get
            {
                return _publisherSerializationSettings;
            }
            set
            {
                _publisherSerializationSettings = value;
            }
        }

        public RabbitMqPublisher(ILoggerFactory loggerFactory, RbmqConfigurationElement settings) : base(loggerFactory, settings)
        {
            // default serialization settings
            this._publisherSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };


            _publisherSettings = new QueueSettings(settings);
            if (string.IsNullOrEmpty(_publisherSettings.Queue))
            {
                _publisherSettings.GenerateNewQueueName(true);
                _publisherSettings.GeneratedQueueName = true;
                _publisherSettings.Exclusive = true;
            }
            

           

        }


        public void EnsureConnection()
        {
            if (_publishConn == null || !_publishConn.IsOpen)
            {
                _publishConn = GetConnection(_publisherSettings);
                _publishChannel = Initialize(_publishConn, _publisherSettings);

                _publishConn.ConnectionShutdown += PublisherConnectionShutdown;
            }
        }
        public async Task Enqueue(QMessage msg)
        {
            
            
            try
            {
                EnsureConnection();

                IBasicProperties basicProperties = _publishChannel.CreateBasicProperties();

                if (msg.Properties != null)
                {
                    basicProperties.Persistent = msg.Properties.Persistent;
                    if (!string.IsNullOrEmpty(msg.Properties.AppId))
                        basicProperties.AppId = msg.Properties.AppId;
                    if (!string.IsNullOrEmpty(msg.Properties.ClusterId))
                        basicProperties.ClusterId = msg.Properties.ClusterId;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentEncoding))
                        basicProperties.ContentEncoding = msg.Properties.ContentEncoding;
                    if (!string.IsNullOrEmpty(msg.Properties.ContentType))
                        basicProperties.ContentType = msg.Properties.ContentType;
                    if (!string.IsNullOrEmpty(msg.Properties.CorrelationId))
                        basicProperties.CorrelationId = msg.Properties.CorrelationId;
                    if (msg.Properties.DeliveryMode != 0)
                        basicProperties.DeliveryMode = msg.Properties.DeliveryMode;
                    if (!string.IsNullOrEmpty(msg.Properties.Expiration))
                        basicProperties.Expiration = msg.Properties.Expiration;
                    if (!string.IsNullOrEmpty(msg.Properties.MessageId))
                        basicProperties.MessageId = msg.Properties.MessageId;
                    if (msg.Properties.Priority != 0)
                        basicProperties.Priority = msg.Properties.Priority;
                    if (!string.IsNullOrEmpty(msg.Properties.ReplyTo))
                        basicProperties.ReplyTo = msg.Properties.ReplyTo;
                    if (!string.IsNullOrEmpty(msg.Properties.Type))
                        basicProperties.Type = msg.Properties.Type;
                    if (!string.IsNullOrEmpty(msg.Properties.UserId))
                        basicProperties.UserId = msg.Properties.UserId;
                }

                // dead letters support
                //if (_settings.RetryDelay > 0)
                //{
                //    basicProperties.Headers.Add("x-delay", _settings.RetryDelay);
                //}

                lock (_lockPublisher)
                {
                    var jsonified = JsonConvert.SerializeObject(msg, Formatting.None, _publisherSerializationSettings);
                    var messageBuffer = Encoding.UTF8.GetBytes(jsonified);

                    _publishChannel.BasicPublish(_publisherSettings.Exchange, _publisherSettings.RoutingKey, basicProperties, messageBuffer);

                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Enqueue Error" + ex.Message + "Inner Exception:" + ex.InnerException);
                throw;
            }
        }

       

        private void PublisherConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("Publisher connection broke!");

            var mres = new ManualResetEventSlim(false); // state is initially false

            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _publishChannel = null;
                    if (_publisherSettings.GeneratedQueueName)
                        _publisherSettings.GenerateNewQueueName(true);
                    _publishConn = GetConnection(_publisherSettings);
                    if (_publishConn == null) throw new Exception("Publisher connection is null");
                    _publishChannel = Initialize(_publishConn, _publisherSettings);
                    _publishConn.ConnectionShutdown += PublisherConnectionShutdown;

                    _logger.LogInformation("Publisher reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Publisher reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }
    }
}
