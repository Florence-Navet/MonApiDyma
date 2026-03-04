using Microsoft.AspNetCore.Identity;

namespace HoteIdentiyServer.Models
{
   public class ApplicationUser : IdentityUser<int>
   {
      public string Fonction { get; set; } = "";
   }
}
