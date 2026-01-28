using JumpStart.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Repositories;

using JumpStart.Authorization;
using Microsoft.AspNetCore.Authorization;

// Implement IAuthorizeData so the framework recognizes this as an authorization trigger
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class EntityAuthorizeAttribute : Attribute, IAuthorizeData
{
    public string Action { get; }

    // This MUST be public and have { get; set; } to satisfy IAuthorizeData
    public string? Policy { get; set; }

    public string? Roles { get; set; }
    public string? AuthenticationSchemes { get; set; }

    public EntityAuthorizeAttribute(string action)
    {
        Action = action;
        // Assign the static string you created
        Policy = EntityPolicyProvider.PolicyName;
    }
}
