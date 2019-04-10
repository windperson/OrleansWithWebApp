using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharedOrleansInterface
{
    public interface IHelloArchive : Orleans.IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);

        Task<IEnumerable<string>> GetGreetings();
    }
}