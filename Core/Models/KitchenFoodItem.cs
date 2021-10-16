using System;

namespace Kitchen.Core.Models
{
     public class KitchenFoodItem
     {
          public Guid OrderId { get; set; }
          public int Id { get; set; }
          public int Complexity { get; set; }
          public string Name { get; set; }
          public CookingTool CookingTool { get; set; }
          public int PreparationTime { get; set; }
     }
}