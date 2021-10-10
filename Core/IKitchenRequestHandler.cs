using Kitchen.Core.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kitchen.Core
{
     public interface IKitchenRequestHandler
     {
          Task<HttpResponseMessage> PostReadyOrderToHall(KitchenReturnOrder order);
     }
}
