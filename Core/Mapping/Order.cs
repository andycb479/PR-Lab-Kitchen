using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hall.Core.Models;

namespace Kitchen.Core.Mapping
{
     public class Order : Profile
     {
          public Order()
          {
               CreateMap<HallOrder, KitchenOrder>();
          }
     }
}
