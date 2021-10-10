using System.Collections.Generic;

namespace Kitchen.Core.Models
{
     public class Cook
     {
          public int CookId { get; set; }
          public int Rank { get; set; }
          public string Name { get; set; }
          public int Proficiency { get; set; }
          public string CatchPhrase { get; set; }
     }

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
                    Proficiency = 1
               },
               new Cook()
               {
                    CatchPhrase = "",
                    CookId = 2,
                    Name = "Gordon Ramsay",
                    Rank = 3,
                    Proficiency = 3
               },
          };
     }
}
