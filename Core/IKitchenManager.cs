using Kitchen.Core.Models;

namespace Kitchen.Core
{
     public interface IKitchenManager
     {
          void AddOrderToPrepareList(HallOrder order);
     }
}
