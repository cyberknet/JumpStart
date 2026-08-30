# ADR-016: Configurable JWT Token Options

**Status:** Accepted

**Date:** 2026-08-30

**Decision Makers:** JumpStart Core Team

## Context

`JwtTokenService` (see [ADR-004](004-jwt-authentication.md)) took a raw `IConfiguration` in its
constructor and indexed it with four string literals hardcoded directly in `GenerateToken`'s method
body: `"JwtSettings:SecretKey"`, `"JwtSettings:Issuer"`, `"JwtSettings:Audience"`,
`"JwtSettings:ExpirationMinutes"`.

This surfaced as a real friction point in a consuming application (RustArchon) that wanted every one
of its own `.env`/Docker Compose variable names to reach the containers under that exact same name,
with no `Section__Key`-style rename step in between - e.g. `RUSTARCHON_INTERNAL_API_KEY` staying
`RUSTARCHON_INTERNAL_API_KEY` all the way from `.env` to the bound C# property. Every other setting in
that app could be restructured to work this way except the JWT secret, because `JwtTokenService`
itself dictated the literal config path - the *consuming app* had no say in it at all, short of
forking JumpStart or accepting the awkward asymmetry.

More generally: hardcoding a config path inside a reusable service, rather than accepting a
strongly-typed settings object via dependency injection, means every consumer is locked into the
exact same configuration shape the library's author happened to pick. That's backwards - the
framework shouldn't need to know or care whether its settings arrive via a nested JSON section, a flat
environment variable, Azure Key Vault, or anything else.

## Decision

`JwtTokenService` now depends on `IOptions<JwtTokenOptions>` instead of `IConfiguration`:

```csharp
public class JwtTokenOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtTokenOptions _options;

    public JwtTokenService(IOptions<JwtTokenOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    // GenerateToken reads _options.SecretKey/.Issuer/.Audience/.ExpirationMinutes -
    // no IConfiguration, no string literals, anywhere in this class.
}
```

A new extension method, `AddJwtTokenService()`, registers the service **and** a default binding of
`JwtTokenOptions` from the `"JwtSettings"` configuration section - so every existing consumer
(including `JumpStart.DemoApp`) keeps working with zero changes, using the exact same
`appsettings.json` shape as before:

```csharp
public static IServiceCollection AddJwtTokenService(this IServiceCollection services)
{
    services.TryAddScoped<IJwtTokenService, JwtTokenService>();
    services.AddOptions<JwtTokenOptions>().BindConfiguration("JwtSettings");
    return services;
}
```

`RegisterTokenExchangeServices` (the internal method `AddJumpStart(options => options.RegisterTokenController = true)` calls) now calls `AddJwtTokenService()` instead of registering `IJwtTokenService`/`JwtTokenService` directly, so the API-side default binding comes along for free too.

An app that wants any one of the four values sourced from somewhere other than a nested
`"JwtSettings"` section adds its own `PostConfigure` call, after `AddJwtTokenService()` (or
`AddJumpStart`, which calls it internally) or before - order doesn't matter, since `PostConfigure`
delegates always run after every `Configure` delegate regardless of registration order:

```csharp
builder.Services.AddJwtTokenService();
builder.Services.PostConfigure<JwtTokenOptions>(options =>
    options.SecretKey = builder.Configuration["RUSTARCHON_JWT_SECRET_KEY"] ?? options.SecretKey);
```

This is exactly the pattern the RCON pipeline's `InvitationCodeOptions`/`InternalApiKeyOptions` in
RustArchon.Api already use for their own flat-key config values - `JwtTokenOptions` now works the
same way, just supplied by the *host app*, not baked into JumpStart.

## Consequences

### Positive Consequences

- **No forced config shape.** Any consuming app can source the JWT secret (or issuer/audience/
  expiration) from wherever its own config conventions demand - a flat env var, a secrets manager, a
  nested section - without forking JumpStart or accepting an awkward rename step somewhere else.
- **Backward compatible.** `AddJwtTokenService()`'s default `"JwtSettings"` section binding means
  every existing consumer (including the demo app) needs no changes at all after upgrading.
- **Testability.** `JwtTokenServiceTests` now constructs `JwtTokenOptions` directly via
  `Options.Create(...)` instead of building a throwaway `IConfiguration` with `AddInMemoryCollection` -
  less test scaffolding for the same coverage.
- **Consistent with the rest of .NET.** `IOptions<T>` is the standard configuration pattern; this
  brings `JwtTokenService` in line with how virtually every other configurable service in the
  ecosystem (and now, every other JumpStart service) is built.

### Negative Consequences

- **Breaking change to `JwtTokenService`'s constructor.** Any code directly instantiating
  `new JwtTokenService(configuration)` (rather than resolving `IJwtTokenService` via DI) breaks. No
  such usage existed outside the framework's own test suite, which has been updated.
- **One more concept to explain.** Consumers who want a non-default config source now need to
  understand `Configure`/`PostConfigure` ordering semantics, rather than just knowing "it reads
  `JwtSettings:SecretKey`." Documented in `JwtTokenOptions`'s remarks and in
  [authentication.md](../../authentication.md).

### Neutral Consequences

- `IJwtTokenService`'s public interface is unchanged - this only affects how `JwtTokenService` itself
  is constructed and registered, not how consumers call `GenerateToken`.

## Alternatives Considered

### 1. Keep `IConfiguration`, make the section name a constructor parameter

```csharp
public JwtTokenService(IConfiguration configuration, string sectionName = "JwtSettings")
```

**Why Rejected:** Still couples the service to `IConfiguration`'s string-indexing API and to a single
section covering all four values together - doesn't let a consumer source, say, just the secret key
from a different place than the issuer/audience, and doesn't compose with DI registration the way
`IOptions<T>` does (no `Configure`/`PostConfigure` layering, no testability via `Options.Create`).

### 2. Leave `JwtTokenService` alone; have consumers duplicate the secret into two config paths

E.g. RustArchon could keep `JwtSettings:SecretKey` for JumpStart and separately read
`RUSTARCHON_JWT_SECRET_KEY` into it via `IConfiguration`'s in-memory provider tricks, or duplicate the
value under both names in `.env`.

**Why Rejected:** Doesn't fix the underlying problem (JumpStart still owns a config shape it
shouldn't), and duplicating one secret under two names is itself the kind of indirection this whole
change exists to remove - a secret that can drift out of sync between its two copies is worse than
one that never had two names to begin with.

## References

- [ADR-004: JWT Authentication](004-jwt-authentication.md) - original `JwtTokenService` design
- [ADR-013: JWT Token Exchange](013-jwt-token-exchange.md)
- [Microsoft: Options pattern in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/options)

## Related Documentation

- [Authentication Setup](../../authentication.md)
