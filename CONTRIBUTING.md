# Contributing to JumpStart

Thanks for considering a contribution. JumpStart is a deliberately **opinionated** Blazor
framework — it prescribes concrete solutions (Guid-only entities, mandatory entity authorization,
DataAnnotations-only EF configuration) rather than exposing pluggable abstractions for every
choice. Keep that in mind before proposing a config flag or extension interface: see
[Design Philosophy](docs/architecture/index.md) and the RFC
[Decision Criteria](docs/architecture/rfc/index.md#decision-criteria) for when flexibility is
actually warranted versus when it just adds surface area.

## Ways to Contribute

- **Report bugs** — open a [GitHub Issue](https://github.com/cyberknet/JumpStart/issues) with
  repro steps, expected vs. actual behavior, and your .NET/JumpStart versions.
- **Suggest features** — start a
  [GitHub Discussion](https://github.com/cyberknet/JumpStart/discussions) first for anything
  non-trivial. See [Proposing a Design Change](#proposing-a-design-change) below — some features
  need an RFC before any code lands.
- **Submit pull requests** — bug fixes, docs, and small well-scoped features are welcome directly
  as PRs. For anything touching architecture, open an issue/discussion first so the direction is
  agreed before you invest the work.
- **Improve documentation** — the `docs/` folder (built with DocFX) is as much a part of the
  project as the code. Typos, unclear explanations, and missing how-tos are all fair game.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (17.14+) or another editor with C# 13 support

### Clone and Build

```bash
git clone https://github.com/cyberknet/JumpStart.git
cd JumpStart
dotnet restore JumpStart.slnx
dotnet build JumpStart.slnx
```

### Run the Tests

```bash
dotnet test JumpStart.slnx
```

All tests must pass before a PR will be reviewed. New features and bug fixes require accompanying
`JumpStart.Tests` coverage (xUnit, Moq, FluentAssertions, EF Core InMemory).

### Run the Demo Apps

The demo apps double as integration examples for repository, audit, and authentication behavior —
useful for confirming a change works end-to-end, not just in isolated unit tests.

```bash
# Terminal 1
cd JumpStart.DemoApp.Api && dotnet run

# Terminal 2
cd JumpStart.DemoApp && dotnet run
```

Then browse to `https://localhost:7099`. See [Samples](docs/samples.md) for what each demo
project demonstrates.

## Coding Standards

Full, binding conventions live in [`.github/copilot-instructions.md`](.github/copilot-instructions.md)
— read it before your first PR. Highlights:

- One class per file; PascalCase for types/members, camelCase for locals/parameters,
  `_camelCase` for private fields, `I`-prefixed interfaces.
- Primary constructors for DI wherever possible.
- Nullable Reference Types are on — no un-annotated nullability.
- EF Core entity configuration uses **DataAnnotations only**; Fluent API is reserved for query
  filters and seed data.
- Database access is repository-abstracted and confined to the API tier (Identity's own
  DbContext/managers are the sole exception).
- No abbreviations, no type info in variable names, no emojis in code or comments.
- Every public member gets XML doc comments (with `<example>` where useful) — these feed the
  DocFX-generated [API Reference](docs/api/index.html).
- Every `.cs`/`.razor` file starts with a copyright + GPL-3.0 header (see the template in
  `copilot-instructions.md`).

When modifying existing code, bring it up to these standards even if the gaps predate your change.

## Documentation

If your change affects public API surface or behavior, update the relevant page under `docs/` in
the same PR. To preview the generated site locally:

```bash
dotnet tool install -g docfx   # first time only
build-docs.cmd                 # or ./build-docs.sh on Linux/macOS
docfx serve _site
```

## Proposing a Design Change

JumpStart records architectural decisions formally so the reasoning survives past the PR that
made them:

- **[Request For Comments](docs/architecture/rfc/index.md)** — for a feature that's still an open design question:
  a new extension point, a side-effecting operation above `Repository<TEntity>`, an authorization
  shape that doesn't fit the existing `{EntityName}.{Action}` claim model, or anything future
  roadmap features will depend on. Check the RFC index's
  [Decision Criteria](docs/architecture/rfc/index.md#decision-criteria) — "this looks simple" has
  been wrong twice already (see RFC-001, RFC-002).
- **[Architecture Design Review](docs/architecture/adr/index.md)** — for a decision that's settled, whether it went
  through an RFC first or was small enough to decide directly in a PR. An ADR is closed and never
  edited in place; if a decision changes later, a new ADR supersedes it (see ADR-002 → ADR-009).

When in doubt, open a GitHub Discussion first — it's cheaper than either document and will tell
you which one you need, if any.

## Pull Requests

1. Fork the repo and branch from `main`.
2. Keep the PR scoped to one concern — separate refactors from behavior changes.
3. Include or update tests; `dotnet test JumpStart.slnx` must pass.
4. Update `docs/` and XML comments for any public-facing change.
5. Write a clear PR description: what changed and why (link the issue/discussion/RFC/ADR if one
   exists).
6. Be responsive during review — architectural feedback here is what keeps the framework
   opinionated instead of accumulating one-off escape hatches.

## License

JumpStart is licensed under [GPL-3.0](LICENSE.txt). By submitting a contribution, you agree it
will be licensed under the same terms. If you bring in third-party code or libraries, confirm
their license is GPL-3.0-compatible and note it in the PR description.

## Questions?

Open a [GitHub Discussion](https://github.com/cyberknet/JumpStart/discussions) or check the
[FAQ](docs/faq.md) / [Troubleshooting](docs/troubleshooting.md) guides.
