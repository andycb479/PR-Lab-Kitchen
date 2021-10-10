using AutoMapper;

namespace Kitchen.Core.Models.Mapping
{
     public class Order : Profile
     {
          public Order()
          {
               CreateMap<HallOrder, KitchenReturnOrder>();
          }
     }
}
