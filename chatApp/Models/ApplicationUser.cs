using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace chatApp.Models
{
    public class ApplicationUser : IdentityUser
    {
   
        public string? FullName { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")]  
        public string? AvatarUrl { get; set; } = "/images/default_user.png";
        public string? Gender { get; set; }

        public ICollection<Message>? SentMessages { get; set; }
        public ICollection<Message>? ReceivedMessages { get; set; }


    }
}
