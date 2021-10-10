using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kitchen.Controllers;
using Kitchen.Core.Models;
using Microsoft.Extensions.Logging;

namespace Kitchen.Core
{
     public class KitchenRequestHandler:IKitchenRequestHandler
     {
          private readonly ILogger<KitchenController> _logger;

          public KitchenRequestHandler(ILogger<KitchenController> logger)
          {
               _logger = logger;
          }

          public async Task<HttpResponseMessage> PostReadyOrderToHall(KitchenReturnOrder order)
          {
               var clientHandler = new HttpClientHandler
               {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
               };

               using var client = new HttpClient(clientHandler);
               var postTask = await client.PostAsJsonAsync("https://192.168.100.2:8083/api/hall/distribution", order);

               if (postTask.IsSuccessStatusCode)
               {
                    _logger.LogInformation(
                         $"Order with Id {order.OrderId} send to the hall");
               }
               else
               {
                    _logger.LogInformation("Unable to send the order to kitchen " + postTask.StatusCode);
               }

               return postTask;
          }
     }
}