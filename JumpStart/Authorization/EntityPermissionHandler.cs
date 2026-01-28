using JumpStart.Data;
using JumpStart.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace JumpStart.Authorization;

public class EntityPermissionHandler : AuthorizationHandler<EntityPermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EntityPermissionRequirement requirement)
    {
        // 1. Get the Endpoint/Action context
        if (context.Resource is HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (actionDescriptor != null)
            {
                // 2. Look for our custom attribute
                var attr = actionDescriptor.MethodInfo.GetCustomAttribute<EntityAuthorizeAttribute>()
                           ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<EntityAuthorizeAttribute>();

                if (attr != null)
                {
                    // 3. Find TEntity by looking at the base type of the controller/repository
                    // Assuming your controller/repo is: Repository<TEntity>
                    var entityType = GetEntityType(actionDescriptor.ControllerTypeInfo);

                    if (entityType != null)
                    {
                        // 4. Format the string: "Product.Get"
                        var requiredPolicy = $"{entityType.Name}.{attr.Action}";

                        // 5. Check the user's claims
                        if (context.User.HasClaim("Permission", requiredPolicy))
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    private Type? GetEntityType(Type type)
    {
        // Walk up the inheritance chain to find the generic TEntity
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType)
            {
                // Returns the first generic argument (TEntity)
                return type.GetGenericArguments()[0];
            }
            type = type.BaseType!;
        }
        return null;
    }
}