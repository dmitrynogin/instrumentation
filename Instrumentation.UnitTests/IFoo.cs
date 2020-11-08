using System.Threading.Tasks;

namespace Instrumentation
{
    public interface IFoo
    {
        void Bar();
        Task BarAsync();
        Task<int> IntAsync();
    }     
}