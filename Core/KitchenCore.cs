using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DataStructures;
using Kitchen.Controllers;
using Kitchen.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kitchen.Core
{
     public class KitchenCore : IKitchenCore
     {
          private readonly ILogger<KitchenController> _logger;
          private readonly IKitchenRequestHandler _kitchenRequestHandler;
          private readonly IMapper _mapper;
          public readonly int TIME_UNIT;
          private readonly int ORDER_LIST_SIZE;
          private static Mutex mut = new();
          private ConcurrentQueue<HallOrder> _orderList = new();
          private BlockingCollection<HallOrder> _orders; 

          public KitchenCore(ILogger<KitchenController> logger, IKitchenRequestHandler kitchenRequestHandler, IMapper mapper, IConfiguration configuration)
          {
               _logger = logger;
               _kitchenRequestHandler = kitchenRequestHandler;
               _mapper = mapper;

               ORDER_LIST_SIZE = int.Parse(configuration["OrderListSize"]);
               var numberOfCooks = int.Parse(configuration["Cooks"]);
               TIME_UNIT = int.Parse(configuration["TIME_UNIT"]);
               _orders = new BlockingCollection<HallOrder>(ORDER_LIST_SIZE);

               for (var i = 1; i < numberOfCooks; i++)
               {
                    Task.Factory.StartNew(() => { ProcessOrderList(i); });
               }

          }

          private void ProcessOrderList(int cookId)
          {
               var aTimer = new System.Timers.Timer( 0.5*TIME_UNIT);
               aTimer.Elapsed += (sender, e) => TakeOrder(cookId); ;
               aTimer.AutoReset = true;
               aTimer.Enabled = true;
          }

          private void TakeOrder(int cookId)
          {
               var r = new Random();
               mut.WaitOne();
               if (_orderList.TryDequeue(out var order))
               {
                    _logger.LogInformation($"Order with Id {order.OrderId} taken by Cook {cookId}.");

                    var currentKitchenOrder = _mapper.Map<KitchenReturnOrder>(order);

                    //currentKitchenOrder.CookId = cookId;

                    Thread.Sleep(r.Next(1, 5) * TIME_UNIT);

                    _kitchenRequestHandler.PostReadyOrderToHall(currentKitchenOrder);
               }
               mut.ReleaseMutex();

          }

          public void AddOrderToList(HallOrder order)
          {
               mut.WaitOne();

               _orderList.Enqueue(order);
               _logger.LogInformation($"Order with Id {order.OrderId} added to the Order List");

               mut.ReleaseMutex();

          }
     }
}
