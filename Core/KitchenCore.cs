using AutoMapper;
using Kitchen.Controllers;
using Kitchen.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Kitchen.Core
{
     public class KitchenCore : IKitchenCore
     {
          private readonly ILogger<KitchenController> _logger;
          private readonly IKitchenRequestHandler _kitchenRequestHandler;
          private readonly EventHandler OrderFromHallReceived;
          private readonly IMapper _mapper;
          private readonly IKitchenManager _kitchenManager;
          public readonly int TIME_UNIT;
          private readonly int ORDER_LIST_SIZE;
          private static Mutex mut = new();
          private BlockingCollection<HallOrder> _ordersList;

          public KitchenCore(ILogger<KitchenController> logger, IKitchenRequestHandler kitchenRequestHandler, IMapper mapper, IConfiguration configuration, IKitchenManager kitchenManager)
          {
               _logger = logger;
               _kitchenRequestHandler = kitchenRequestHandler;
               _mapper = mapper;
               _kitchenManager = kitchenManager;
               ORDER_LIST_SIZE = int.Parse(configuration["OrderListSize"]);
               TIME_UNIT = int.Parse(configuration["TIME_UNIT"]);
               _ordersList = new BlockingCollection<HallOrder>(ORDER_LIST_SIZE);
               OrderFromHallReceived += SendOrderToMediator;
          }

          private void SendOrderToMediator(object? sender, EventArgs e)
          {
               mut.WaitOne();

               if (_ordersList.TryTake(out var order))
               {
                    _kitchenManager.AddOrderToPrepareList(order);
               }

               mut.ReleaseMutex();
          }

          public void AddOrderToList(HallOrder order)
          {
               mut.WaitOne();

               _ordersList.TryAdd(order);
               _ordersList = new BlockingCollection<HallOrder>(
                    new ConcurrentQueue<HallOrder>(_ordersList.OrderByDescending(o => o.Priority)), ORDER_LIST_SIZE);
               _logger.LogInformation($"Order with Id {order.OrderId} added to the Order List");
               OrderFromHallReceived?.Invoke(this, EventArgs.Empty);

               mut.ReleaseMutex();

          }
     }
}
