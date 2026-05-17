# BlazorBlocks

Shared Blazor and ASP.NET Core building blocks for [Cerebellum.NetBlocks](https://github.com/daniel-c-harvey/NetBlocks)-based applications. BlazorBlocks provides the data, API, and UI layers so you can wire up a full-stack application without rewriting the same scaffolding every time.

## Packages

### Cerebellum.BlazorBlocks.Data

EF Core data layer: base `DbContext` classes, entity models, and repository abstractions. Handles persistence plumbing for Cerebellum.NetBlocks-based apps.

```
dotnet add package Cerebellum.BlazorBlocks.Data
```

### Cerebellum.BlazorBlocks.Api

ASP.NET Core API shared layer: base controllers, endpoint wiring, and request/response plumbing. Reduces repetitive API scaffolding in Cerebellum.NetBlocks-based projects.

```
dotnet add package Cerebellum.BlazorBlocks.Api
```

### Cerebellum.BlazorBlocks.Web

MudBlazor-based Blazor UI components: entity management views, modals, and form scaffolding. Accelerates building admin and CRUD interfaces in Cerebellum.NetBlocks-based apps.

```
dotnet add package Cerebellum.BlazorBlocks.Web
```

## Prerequisites

- [Cerebellum.NetBlocks](https://github.com/daniel-c-harvey/NetBlocks) — the core domain and infrastructure library these packages extend
- .NET 10

## License

AGPL-3.0-or-later. See [LICENSE](https://github.com/daniel-c-harvey/BlazorBlocks/blob/main/LICENSE) for details.

## Source

https://github.com/daniel-c-harvey/BlazorBlocks
