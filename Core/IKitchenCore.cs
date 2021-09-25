using System.Threading.Tasks;
using Hall.Core.Models;

namespace Kitchen.Core
{
     public interface IKitchenCore
     {
          void AddOrderToList(HallOrder order);
     }
}