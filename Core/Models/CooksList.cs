using System.Collections.Generic;

namespace Kitchen.Core.Models
{
     public class CooksList
     {
          public List<Cook> Cooks = new()
          {
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 0,
                    Name = "John",
                    Rank = 1,
                    Proficiency = 2
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 1,
                    Name = "Bob",
                    Rank = 2,
                    Proficiency = 2
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 2,
                    Name = "Gordon Ramsay",
                    Rank = 3,
                    Proficiency = 4
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 3,
                    Name = "Bill Gates",
                    Rank = 3,
                    Proficiency = 4
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 4,
                    Name = "Will",
                    Rank = 2,
                    Proficiency = 3
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 5,
                    Name = "Bob",
                    Rank = 2,
                    Proficiency = 2
               },
          };
     }
}