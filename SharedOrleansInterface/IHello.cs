using System.Threading.Tasks;

namespace SharedOrleansInterface
{
    public interface IHello : Orleans.IGrainWithIntegerKey
    {
        Task<string> SayHello(string greeting);
    }
}