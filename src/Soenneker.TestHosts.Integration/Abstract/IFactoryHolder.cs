using System.Threading.Tasks;

namespace Soenneker.TestHosts.Integration.Abstract;

internal interface IFactoryHolder
{
    ValueTask DisposeIfCreated();
}