using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hall.Core.Models;

namespace Kitchen.Core
{
     public interface IKitchenRequestHandler
     {
          Task<HttpResponseMessage> PostReadyOrderToHall(KitchenOrder order);
     }
}
