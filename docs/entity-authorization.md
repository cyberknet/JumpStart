# Entity Authorization

Learn how JumpStart controls access to each entity type and action, and what you need to configure
before your API will actually respond to requests.

## Overview

Entity authorization answers one question for every request to an `ApiControllerBase`-derived
controller:

- **Does the current user have permission to perform this action on this entity type?**

> **⚠️ This is not opt-in.** Calling `AddJumpStart()` - which every JumpStart application must do -
> unconditionally registers a global authorization policy and requires a matching `Permission`
> claim on every `ApiControllerBase` action. There is currently no configuration option to disable
> or relax this. If you have never issued `Permission` claims to your users, **every request to
> every JumpStart-generated endpoint will return `403 Forbidden`** (or `401 Unauthorized` if the
> user isn't authenticated at all).

## Why Entity Authorization?

### Fine-Grained Access Control
Grant "can view Products but not delete them" without hand-writing a policy per controller.

### Consistency Across Entities
Every `ApiControllerBase`-derived controller gets the same enforcement automatically - there's no
way to accidentally ship an unprotected CRUD endpoint.

### Role Composition
Combine permissions freely per role (e.g. a "Support" role might get `Order.Get` and `Order.List`
but not `Order.Delete`) without writing custom `[Authorize(Policy = "...")]` combinations by hand.

## How It Works

### The Claim Format

Every action on `ApiControllerBase<TEntity, ...>` is decorated with `[EntityAuthorize(action: "...")]`:

| Method (base class) | Action  |
|----------------------|---------|
| `GetById`            | `Get`    |
| `GetAll`             | `List`   |
| `Create`             | `Create` |
| `Update`             | `Update` |
| `Delete`             | `Delete` |

At request time, `EntityPermissionHandler` resolves the entity type from the controller's own
generic base type (e.g. `ProductsController : ApiControllerBase<Product, ...>` → `Product`), builds
a policy string as `"{EntityName}.{Action}"`, and checks it against the current user's claims:

```csharp
context.User.HasClaim("Permission", "Product.Get")
```

So a user attempting `GET /api/products/{id}` needs a claim of type `Permission` with value
`Product.Get`. `GET /api/products` (list) needs `Product.List`. `POST /api/products` needs
`Product.Create`, and so on.

### What AddJumpStart Registers

This happens unconditionally, with no `JumpStartOptions` flag to control it:

```csharp
services.AddSingleton<IAuthorizationPolicyProvider, EntityPolicyProvider>();
services.AddScoped<IAuthorizationHandler, EntityPermissionHandler>();
services.AddAuthorization(options =>
{
    options.AddPolicy("EntityPolicy", policy => policy.AddRequirements(new EntityPermissionRequirement()));
});
```

Because `EntityAuthorizeAttribute` implements `IAuthorizeData` (the same interface `[Authorize]`
itself implements), it also enforces plain authentication - an anonymous request gets `401`, an
authenticated request missing the right `Permission` claim gets `403`.

## Quick Start

### 1. Issue Permission Claims When You Create the User's Identity

**JWT (Web API):**

```csharp
var token = _jwtTokenService.GenerateToken(
    user.NumericId,
    user.UserName,
    additionalClaims: new Dictionary<string, string>
    {
        ["Permission"] = "Product.Get" // only one value per key - see note below
    });
```

`IJwtTokenService.GenerateToken`'s `additionalClaims` is a flat `Dictionary<string, string>` - one
value per key - so it cannot add multiple `Permission` claims on its own. For more than one
permission, build the `ClaimsIdentity` directly instead (see [How-To: Secure
Endpoints](how-to/secure-endpoints.md) for the pattern):

```csharp
var identity = new ClaimsIdentity(authenticationType);
foreach (var permission in new[] { "Product.Get", "Product.List", "Product.Create" })
{
    identity.AddClaim(new Claim("Permission", permission));
}
```

**Cookie Authentication (Blazor Server / Identity):**

```csharp
var claims = new List<Claim>
{
    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
};
claims.AddRange(userPermissions.Select(p => new Claim("Permission", p)));

var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
await signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));
```

### 2. Verify

Call an endpoint without the claim first to confirm you get `403`, then add the claim and confirm
it succeeds - it's easy to assume authorization is "just working" because authentication succeeded.

## Protecting Custom Controller Actions

`[EntityAuthorize]` isn't limited to the inherited CRUD actions - apply it to your own custom
actions the same way:

```csharp
public class ProductsController : ApiControllerBase<Product, ProductDto, CreateProductDto, UpdateProductDto, IProductRepository>
{
    [HttpGet("featured")]
    [EntityAuthorize(action: "List")] // reuses the "Product.List" permission
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeatured()
    {
        var products = await _repository.GetFeaturedAsync();
        return Ok(_mapper.Map<IEnumerable<ProductDto>>(products));
    }
}
```

Entity type resolution walks up the controller's inheritance chain looking for the first generic
type argument, so this works on any controller derived from `ApiControllerBase<TEntity, ...>` -
custom actions don't need to specify the entity name themselves.

## Best Practices

### Do's ✅

- **Decide your permission model before writing controllers** - retrofitting claims onto existing
  users is more work than issuing them correctly from the start
- **Test the unauthenticated and under-permissioned cases**, not just the happy path
- **Use one permission per entity/action pair** (`Product.Get`, `Product.Delete`, etc.) rather than
  broader catch-all claims, to keep the model consistent with how the handler checks claims

### Don'ts ❌

- **Don't assume a working demo means authorization is configured** - a 401/403 on every call is
  the default until you issue `Permission` claims
- **Don't rely on `[AllowAnonymous]` to "turn off" entity authorization for a whole controller**
  without checking every custom action individually - it only affects actions you apply it to

## Troubleshooting

### Every Request Returns 403 Forbidden

**Problem:** All API calls fail with `403`, even for an authenticated user.

**Solutions:**
1. Confirm the user's claims actually include a `Permission` claim matching
   `"{EntityName}.{Action}"` for the endpoint being called (log `context.User.Claims` to check)
2. Confirm the claim type is exactly `"Permission"` (case-sensitive) and the value exactly matches
   `EntityName.Action` (e.g. `"Product.Get"`, not `"products.get"` or `"Product:Get"`)
3. Confirm the entity name matches the controller's generic `TEntity` argument's class name exactly

### Every Request Returns 401 Unauthorized

**Problem:** Requests fail before authorization is even evaluated.

**Solutions:**
1. This means authentication itself is failing - see [Authentication &
   Security](authentication.md) - `EntityAuthorize` piggybacks on the standard ASP.NET Core
   authentication pipeline, it doesn't replace it

### A Custom Action Isn't Protected

**Problem:** A hand-written controller action allows access without the expected permission.

**Solutions:**
1. Confirm you added `[EntityAuthorize(action: "...")]` to that specific action - it is not
   inherited automatically for actions you write yourself, only for the base class's own CRUD
   methods

## Next Steps

- **[Multi-Tenancy](multi-tenancy.md)** - Another automatic, always-on data-access concern to be
  aware of alongside entity authorization
- **[Authentication & Security](authentication.md)** - JWT and cookie authentication setup
- **[How-To: Secure Endpoints](how-to/secure-endpoints.md)** - Role-based and resource-based
  authorization patterns
- **[API Development](api-development.md)** - Build RESTful APIs with `ApiControllerBase`

---

**Questions?** See [FAQ](faq.md) or [open an issue](https://github.com/cyberknet/JumpStart/issues).
