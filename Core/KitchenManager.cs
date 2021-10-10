using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Kitchen.Controllers;
using Kitchen.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kitchen.Core
{
     public class KitchenManager : IKitchenManager
     {
          private readonly ILogger<KitchenController> _logger;
          private readonly IMapper _mapper;
          private readonly IKitchenRequestHandler _kitchenRequestHandler;
          private List<KitchenFoodItem> _foodItems = new();
          public readonly int TIME_UNIT;
          private static Mutex foodListMutex = new();
          private static Mutex preparedOrdersMutex = new();
          private readonly FoodDictionary _foodDictionary = new();
          private ConcurrentDictionary<Guid, KitchenReturnOrder> _preparedOrders = new();

          public KitchenManager(IConfiguration configuration, ILogger<KitchenController> logger, IMapper mapper, IKitchenRequestHandler kitchenRequestHandler)
          {
               _logger = logger;
               _mapper = mapper;
               _kitchenRequestHandler = kitchenRequestHandler;
               TIME_UNIT = int.Parse(configuration["TIME_UNIT"]);
               var numberOfCooks = int.Parse(configuration["Cooks"]);
               var cookList = new CooksList().Cooks;

               for (var i = 1; i < numberOfCooks; i++)
               {
                    var i1 = i;
                    Task.Factory.StartNew(() => { ProcessFoodList(cookList[i1 - 1]); });
               }

          }
          public void AddOrderToPrepareList(HallOrder order)
          {
               _logger.LogInformation($"Order with Id {order.OrderId} received by the Kitchen Mediator.");

               foodListMutex.WaitOne();
               foreach (var foodItemId in order.Items)
               {
                    var tempItem = _mapper.Map<KitchenFoodItem>(_foodDictionary.GetFoodById(foodItemId));
                    tempItem.OrderId = order.OrderId;
                    _foodItems.Add(tempItem);
               }
               foodListMutex.ReleaseMutex();

               preparedOrdersMutex.WaitOne();
               var orderToAdd = _mapper.Map<KitchenReturnOrder>(order);
               orderToAdd.CookingDetails = new();
               _preparedOrders.TryAdd(order.OrderId, orderToAdd);
               preparedOrdersMutex.ReleaseMutex();

          }

          private void CheckOrderState()
          {
               preparedOrdersMutex.WaitOne();

               foreach (var keyPairOrder in _preparedOrders)
               {
                    var tempOrder = keyPairOrder.Value;
                    if (tempOrder.CookingDetails.Count == tempOrder.Items.Count)
                    {
                         _kitchenRequestHandler.PostReadyOrderToHall(tempOrder);
                         _logger.LogInformation($"Order with Id {tempOrder.OrderId} send to the Hall");
                         _preparedOrders.TryRemove(tempOrder.OrderId, out tempOrder);
                    }
               }

               preparedOrdersMutex.ReleaseMutex();
          }

          private void ProcessFoodList(Cook cook)
          {
               var aTimer = new System.Timers.Timer(2 * TIME_UNIT);
               aTimer.Elapsed += async (sender, e) => await TakeFood(cook); ;
               aTimer.AutoReset = true;
               aTimer.Enabled = true;
          }

          private Task TakeFood(Cook cook)
          {
               foodListMutex.WaitOne();

               if (_foodItems.Count <= 0)
               {
                    foodListMutex.ReleaseMutex();
                    return Task.CompletedTask;
               }

               var minDifference = 100;
               KitchenFoodItem bestFoodItemToPrepare = null;

               foreach (var foodItem in _foodItems)
               {
                    if (foodItem.Complexity > cook.Rank) continue;

                    var complexityDifference = cook.Rank - foodItem.Complexity;

                    if (complexityDifference >= minDifference) continue;
                    minDifference = complexityDifference;
                    bestFoodItemToPrepare = foodItem;
               }

               _foodItems.Remove(bestFoodItemToPrepare);

               foodListMutex.ReleaseMutex();
               if (bestFoodItemToPrepare == null) return Task.CompletedTask;

               _logger.LogInformation($"Cook with Id {cook.CookId} took {bestFoodItemToPrepare.Id} from order {bestFoodItemToPrepare.OrderId}");

               Thread.Sleep(bestFoodItemToPrepare.PreparationTime * TIME_UNIT);

               preparedOrdersMutex.WaitOne();

               var temp = _preparedOrders[bestFoodItemToPrepare.OrderId];
               temp.CookingDetails.Add(new CookingDetails()
               {
                    CookId = cook.CookId,
                    FoodId = bestFoodItemToPrepare.Id
               });

               _preparedOrders.TryUpdate(bestFoodItemToPrepare.OrderId, temp,
                    _preparedOrders[bestFoodItemToPrepare.OrderId]);

               preparedOrdersMutex.ReleaseMutex();

               CheckOrderState();

               return Task.CompletedTask;
          }
     }
}