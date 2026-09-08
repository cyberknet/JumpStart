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
                // 2a. A directly-named permission takes precedence. RequirePermission is how an
                // endpoint that is not shaped like an entity - a report, a capability spanning
                // several tables - states its requirement without inventing a second mechanism.
                // Checked first because an endpoint carrying both is being explicit about which one
                // it means. See ADR-019.
                var named = actionDescriptor.MethodInfo.GetCustomAttribute<RequirePermissionAttribute>()
                            ?? actionDescriptor.ControllerTypeInfo.GetCustomAttribute<RequirePermissionAttribute>();

                if (named != null)
                {
                    if (context.User.HasClaim("Permission", named.Permission))
                    {
                        context.Succeed(requirement);
                    }

                    return Task.CompletedTask;
                }

                // 2b. Otherwise derive it from the controller's entity type, per ADR-011.
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