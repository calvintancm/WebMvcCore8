using Microsoft.AspNetCore.Identity;

namespace ptc_IGH_Sys.Models
{
    public class ApplicationUser : IdentityUser
    {
        // [AspNetUsers] table
        // IdentityUser already includes:
        // Id, Email, EmailConfirmed, PasswordHash, SecurityStamp,
        // PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled,
        // LockoutEnd, LockoutEnabled, AccessFailedCount, UserName
    }
}