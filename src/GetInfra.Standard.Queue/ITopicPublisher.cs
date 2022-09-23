using GetInfra.Standard.Queue.Model;
using System.Threading.Tasks;

namespace GetInfra.Standard.Queue
{
    public interface ITopicPublisher
    {
        Task Produce(QMessage msg);
    }
}
