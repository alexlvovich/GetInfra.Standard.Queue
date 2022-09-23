using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq
{
    public class RabbitMqConsumer : BaseRabbitMq, IQueueConsumer
    {
        private readonly JsonSerializerSettings _consumerSerializationSettings;
        private readonly QueueSettings _consumerSettings;
        private IConnection _consumerConn;
        private IModel _consumerChannel;
        private EventingBasicConsumer _consumer;
        private object _lockSubscriber = new object();
        public RabbitMqConsumer(ILoggerFactory loggerFactory, RbmqConfigurationElement settings) : base(loggerFactory, settings)
        {
            // default serialization settings
            this._consumerSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };


            _consumerSettings = new QueueSettings(settings);
            if (string.IsNullOrEmpty(_consumerSettings.Queue))
            {
                _consumerSettings.GenerateNewQueueName(false);
                _consumerSettings.GeneratedQueueName = true;
                _consumerSettings.Exclusive = true;
            }
            _consumerConn = GetConnection(_consumerSettings);
            _consumerChannel = Initialize(_consumerConn, _consumerSettings);
            _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;

        }

        public event Action<object, QMessage> MessageRecieved;

        public QMessage Dequeue<T>()
        {
            return Dequeue<T>(true);
        }

        public QMessage Dequeue<T>(bool ack = false)
        {
            QMessage msg = null;
            BasicGetResult result;
            try
            {
                //IBasicProperties basicProperties = ConsumerChannel.CreateBasicProperties();
                //basicProperties.Persistent = true;
                lock (_lockSubscriber)
                {
                    bool noAck = false;
                    // get message from queue
                    result = _consumerChannel.BasicGet(_consumerSettings.Queue, noAck);
                    if (result == null)
                    {
                        // No message available at this time.
                    }
                    else
                    {
                        IBasicProperties props = result.BasicProperties;

                        // get body
                        byte[] body = result.Body.ToArray();

                        var json = Encoding.UTF8.GetString(body);

                        json = json.Replace("\"$type\":\"Infrastructure.Queueing.Model.QMessage, Infrastructure.Queueing\",", "");
                        json = json.Replace("\"$type\":\"Infra.Quotes.Brige.Models.BridgeQuote, Infra.Quotes.Bridge\",", "");

                        msg = JsonConvert.DeserializeObject<QMessage>(json, _consumerSerializationSettings);
                        msg.DeliveryTag = result.DeliveryTag;
                        msg.Properties = new QProperties()
                        {
                            AppId = props.AppId,
                            ClusterId = props.ClusterId,
                            ContentEncoding = props.ContentEncoding,
                            ContentType = props.ContentType,
                            CorrelationId = props.CorrelationId,
                            DeliveryMode = props.DeliveryMode,
                            Expiration = props.Expiration,
                            MessageId = props.MessageId,
                            Priority = props.Priority,
                            ReplyTo = props.ReplyTo,
                            Type = props.Type,
                            UserId = props.UserId
                        };

                        if (ack)
                        {
                            _consumerChannel.BasicAck(result.DeliveryTag, false);
                        }

                    }
                }
            }
            catch (OperationInterruptedException ex)
            {
                _logger.LogCritical($"Dequeue Error {ex.Message},Inner Exception:{ex.InnerException}, Stack: {ex.StackTrace}");
                throw;
            }

            return msg;
        }

        public void Subscribe()
        {
            _logger.LogInformation("Subscribe: Starting...");

            Connect();
        }

        public void Unsubscribe()
        {
            if (_consumer != null)
                _consumer.Received -= (model, ea) => { };

            if (_consumerConn != null)
                _consumerConn.ConnectionShutdown -= (sender, e) => { };
        }

        private void ConsumerConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            CancellationTokenSource source = new CancellationTokenSource();
            if (e.Initiator == ShutdownInitiator.Application) return;

            _logger.LogInformation("OnShutDown: Consumer Connection broke!");

            CleanupConnection((IConnection)sender);

            var mres = new ManualResetEventSlim(false); // state is initially false

            while (!mres.Wait(3000)) // loop until state is true, checking every 3s
            {
                try
                {
                    _consumerChannel = null;
                    if (_consumerSettings.GeneratedQueueName)
                        _consumerSettings.GenerateNewQueueName(false);
                    _consumerConn = GetConnection(_consumerSettings);
                    if (_consumerConn == null) throw new Exception("Consumer connection is null");
                    _consumerChannel = Initialize(_consumerConn, _consumerSettings);

                    Subscribe();

                    _consumerConn.ConnectionShutdown += ConsumerConnectionShutdown;
                    _logger.LogInformation("Consumer Reconnected!");
                    mres.Set(); // state set to true - breaks out of loop
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consumer reconnect failed!, Error: {0}", ex.Message);
                }
            }
        }

        private void CleanupConnection(IConnection conn)
        {
            if (conn != null && conn.IsOpen)
            {
                conn.Close();
            }
        }


        private void Connect()
        {
            var logString = $"hostname: {_consumerSettings.Host}, username: {_consumerSettings.Username}, password: {_consumerSettings.Password}, exchangeName: {_consumerSettings.Exchange}, " +
                $"queueName: {_consumerSettings.Queue}, isDurable: {_consumerSettings.IsDurable}, isAutodelete: {_consumerSettings.AutoDelete}, routingKey: {_consumerSettings.RoutingKey}";


            _logger.LogInformation("Connect: Connecting for {0}", logString);

            _consumer = new EventingBasicConsumer(_consumerChannel);

            _consumer.Received += (model, ea) =>
            {
                try
                {
                    _logger.LogInformation("Consume: Calling handler for {0}", logString);

                    IBasicProperties props = ea.BasicProperties;

                    // get body
                    byte[] body = ea.Body.ToArray();

                    var json = Encoding.UTF8.GetString(body);

                    var msg = JsonConvert.DeserializeObject<QMessage>(json, _consumerSerializationSettings);

                    msg.Properties = new QProperties()
                    {
                        AppId = props.AppId,
                        ClusterId = props.ClusterId,
                        ContentEncoding = props.ContentEncoding,
                        ContentType = props.ContentType,
                        CorrelationId = props.CorrelationId,
                        DeliveryMode = props.DeliveryMode,
                        Expiration = props.Expiration,
                        MessageId = props.MessageId,
                        Priority = props.Priority,
                        ReplyTo = props.ReplyTo,
                        Type = props.Type,
                        UserId = props.UserId
                    };

                    if (MessageRecieved != null)
                        MessageRecieved(model, msg);

                    //callBack(model, msg); // return byte array
                    _consumerChannel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Consume: Acknowledged for {0}", logString);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Consume: Error {0}, Stack: {1}", ex.Message, ex.StackTrace);
                }

            };


            _consumerChannel.BasicConsume(queue: _consumerSettings.Queue, autoAck: false,
                consumer: _consumer);

            _logger.LogInformation("Consume: Connected for {0}", logString);
        }
    }
}
