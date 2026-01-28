using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace JumpStart.Authorization;

public class EntityPolicyProvider : IAuthorizationPolicyProvider
{
    public static string PolicyName = "EntityPolicy";
    private readonly DefaultAuthorizationPolicyProvider FallbackPolicyProvider;

    public EntityPolicyProvider(IOptions<AuthorizationOptions> options) => FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);


    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // If the policy isn't found, check if it's one of our dynamic ones
        return await FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    // Default implementations...
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => FallbackPolicyProvider.GetFallbackPolicyAsync();
}
