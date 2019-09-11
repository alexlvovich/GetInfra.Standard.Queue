using GetInfra.Standard.Queue.Extensions;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json;
using NUnit.Framework;

namespace GetInfra.Standard.Queue.Tests
{
    public class RbmqQueueElementExtensionsTests
    {
        [Test]
        public void ConvertToRbmqConfigurationElement_Test()
        {
            var re = new RbmqQueueElement() {
                AutoDelete = true,
                Bind = false,
                DeadLetters= false,
                Exchange = "TestEX",
                ExchangeType = "direct",
                Host = "127.0.0.1",
                IsDurable = true,
                MessageLimit = 5,
                Name = "test",
                Password = "pwd",
                Port = 15678,
                QoS = 1024,
                Queue = "TheQueue",
                RoutingKey ="TheKey",
                Username = "Usr",
                Vhost = "/"
            };


            var e = re.ToRbmqConfigurationElement();

            Assert.AreEqual(re.AutoDelete, e.AutoDelete);
            Assert.AreEqual(re.Exchange, e.Exchange);
            Assert.AreEqual(re.Bind, e.Bind);
            Assert.AreEqual(re.DeadLetters, e.DeadLetters);
            Assert.AreEqual(re.ExchangeType, e.ExchangeType);
            Assert.AreEqual(re.Host, e.Host);
            Assert.AreEqual(re.IsDurable, e.IsDurable);
            Assert.AreEqual(re.MessageLimit, e.MessageLimit);
            Assert.AreEqual(re.Name, e.Name);
            Assert.AreEqual(re.Password, e.Password);
            Assert.AreEqual(re.Port, e.Port);
            Assert.AreEqual(re.QoS, e.QoS);
            Assert.AreEqual(re.Queue, e.Queue);
            Assert.AreEqual(re.RoutingKey, e.RoutingKey);
            Assert.AreEqual(re.Username, e.Username);
            Assert.AreEqual(re.Vhost, e.Vhost);
        }
    }
}
