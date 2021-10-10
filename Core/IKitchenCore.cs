using System.Threading.Tasks;
using Kitchen.Core.Models;

namespace Kitchen.Core
{
     public interface IKitchenCore
     {
          void AddOrderToList(HallOrder order);
     }
}