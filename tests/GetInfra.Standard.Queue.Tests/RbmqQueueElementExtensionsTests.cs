using FluentAssertions;
using GetInfra.Standard.Queue.Extensions;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json;
using Xunit;

namespace GetInfra.Standard.Queue.Tests
{
    public class RbmqQueueElementExtensionsTests
    {
        [Fact]
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

            re.AutoDelete.Should().Be(e.AutoDelete);
            re.Exchange.Should().Be(e.Exchange);
            re.Bind.Should().Be(e.Bind);
            re.DeadLetters.Should().Be(e.DeadLetters);
            re.ExchangeType.Should().Be(e.ExchangeType);
            re.Host.Should().Be(e.Host);
            re.IsDurable.Should().Be(e.IsDurable);
            re.MessageLimit.Should().Be(e.MessageLimit);
            re.Name.Should().Be(e.Name);
            re.Password.Should().Be(e.Password);
            re.Port.Should().Be(e.Port);
            re.QoS.Should().Be(e.QoS);
            re.Queue.Should().Be(e.Queue);
            re.RoutingKey.Should().Be(e.RoutingKey);
            re.Username.Should().Be(e.Username);
            re.Vhost.Should().Be(e.Vhost);
        }
    }
}
