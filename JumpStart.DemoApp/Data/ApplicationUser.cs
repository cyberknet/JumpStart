using JumpStart.Data;
using JumpStart.Data.Advanced;
using Microsoft.AspNetCore.Identity;

namespace JumpStart.DemoApp.Data;

/// <summary>
/// Application user entity with Guid identifier.
/// Implements both Identity and JumpStart IUser interfaces.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>, IUser<Guid>
{
    // IUser.Id is satisfied by IdentityUser<Guid>.Id
    
    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the user joined.
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
}
