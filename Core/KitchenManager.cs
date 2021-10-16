using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
          private readonly int TIME_UNIT;
          private static Mutex foodListMutex = new();
          private static Mutex preparedOrdersMutex = new();
          private readonly FoodDictionary _foodDictionary = new();
          private int[] _cookCookingStack;
          private ConcurrentDictionary<Guid, KitchenReturnOrder> _preparedOrders = new();
          private SemaphoreSlim _stovesSemaphore;
          private SemaphoreSlim _ovenSemaphore;

          public KitchenManager(IConfiguration configuration, ILogger<KitchenController> logger, IMapper mapper, IKitchenRequestHandler kitchenRequestHandler)
          {
               _logger = logger;
               _mapper = mapper;
               _kitchenRequestHandler = kitchenRequestHandler;
               TIME_UNIT = int.Parse(configuration["TIME_UNIT"]);
               var numberOfCooks = int.Parse(configuration["Cooks"]);
               var cookList = new CooksList().Cooks;
               _cookCookingStack = new int[numberOfCooks];

               var stovesCount = int.Parse(configuration["Stove"]);
               var ovensCount = int.Parse(configuration["Oven"]);
               _stovesSemaphore = new SemaphoreSlim(stovesCount, stovesCount);
               _ovenSemaphore = new SemaphoreSlim(ovensCount, ovensCount);

               for (var i = 0; i < numberOfCooks; i++)
               {
                    var i1 = i;
                    Task.Factory.StartNew(() => { ProcessFoodList(cookList[i1]); });
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
                         _preparedOrders.TryRemove(tempOrder.OrderId, out tempOrder);
                    }
               }

               preparedOrdersMutex.ReleaseMutex();
          }

          private void ProcessFoodList(Cook cook)
          {
               var aTimer = new System.Timers.Timer(2 * TIME_UNIT);
               aTimer.Elapsed += async (sender, e) => await TakeFood(cook, aTimer); ;
               aTimer.AutoReset = true;
               aTimer.Enabled = true;
          }

          private Task TakeFood(Cook cook, System.Timers.Timer timer)
          {
               foodListMutex.WaitOne();

               if (_foodItems.Count <= 0)
               {
                    foodListMutex.ReleaseMutex();
                    return Task.CompletedTask;
               }

               timer.Stop();
               _cookCookingStack[cook.CookId]++;

               if (_cookCookingStack[cook.CookId] < cook.Proficiency)
               {
                    timer.Start();
               }

               var bestFoodItemToPrepare = _foodItems.FirstOrDefault(f => f.Complexity <= cook.Rank);

               _foodItems.Remove(bestFoodItemToPrepare);
               foodListMutex.ReleaseMutex();

               if (bestFoodItemToPrepare == null) return Task.CompletedTask;

               var foodCookingTool = bestFoodItemToPrepare.CookingTool;
               switch (foodCookingTool)
               {
                    case CookingTool.Oven:
                         _ovenSemaphore.Wait();
                         break;
                    case CookingTool.Stove:
                         _stovesSemaphore.Wait();
                         break;
                    case CookingTool.None:
                         break;
                    default:  
                         throw new ArgumentOutOfRangeException();
               }

               _logger.LogInformation(
                    $"[COOK-ID:{cook.CookId}-TOOK] {bestFoodItemToPrepare.Id}:{bestFoodItemToPrepare.CookingTool} from order {bestFoodItemToPrepare.OrderId}. " +
                    $"Remaining tools. Stove:{_stovesSemaphore.CurrentCount} - Oven:{_ovenSemaphore.CurrentCount}");

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

               switch (foodCookingTool)
               {
                    case CookingTool.Oven:
                         _ovenSemaphore.Release();
                         break;
                    case CookingTool.Stove:
                         _stovesSemaphore.Release();
                         break;
                    case CookingTool.None:
                         break;
                    default:
                         throw new ArgumentOutOfRangeException();
               }
               _logger.LogInformation(
                    $"[COOK-ID:{cook.CookId}-PREPARED] {bestFoodItemToPrepare.Id}:{bestFoodItemToPrepare.CookingTool} from order {bestFoodItemToPrepare.OrderId}. " +
                    $"Remaining tools. Stove:{_stovesSemaphore.CurrentCount} - Oven:{_ovenSemaphore.CurrentCount}");
               _cookCookingStack[cook.CookId]--;

               timer.Start();

               CheckOrderState();

               return Task.CompletedTask;
          }
     }
}

// foreach (var foodItem in _foodItems)
// {
//      if (foodItem.Complexity > cook.Rank) continue;
//
//      var itemCookingTool = foodItem.CookingTool;
//
//      var complexityDifference = cook.Rank - foodItem.Complexity;
//      var foodIndex = _foodItems.IndexOf(foodItem);
//
//      if (complexityDifference <= minDifference && )
//      {
//           minDifference = complexityDifference;
//           bestFoodItemToPrepare = foodItem;
//      }

// }