using Newtonsoft.Json;

namespace GetInfra.Standard.Queue.Model
{
    public class QMessage
    {
        public object Body { get; set; }

        public QProperties Properties { get; set; }

        public ulong DeliveryTag { get; set; }
    }
}
