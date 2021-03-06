using System.Threading.Tasks;
using Kitchen.Core;
using Kitchen.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kitchen.Controllers
{

     [Route("api/[controller]")]
     [ApiController]
     public class KitchenController : ControllerBase
     {
          private readonly ILogger<KitchenController> _logger;
          private readonly IKitchenCore _kitchenCore;

          public KitchenController(ILogger<KitchenController> logger, IKitchenCore kitchenCore)
          {
               _logger = logger;
               _kitchenCore = kitchenCore;
          }

          [HttpPost("order")]
          public ActionResult ReceivedOrder([FromBody] HallOrder order)
          {
               _logger.LogInformation($"Order with Id {order.OrderId} received by the kitchen");
               Task.Factory.StartNew(() =>
               {
                    _kitchenCore.AddOrderToList(order);

               });
               
               return Ok();
          }
     }
}
