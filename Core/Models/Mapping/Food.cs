using AutoMapper;

namespace Kitchen.Core.Models.Mapping
{
     public class Food : Profile
     {
          public Food()
          {
               CreateMap<Models.Food, KitchenFoodItem>();
               CreateMap<KitchenFoodItem, Models.Food>();
          }
     }
}
