# Per-repo configuration consumed by the canonical pack.ps1.
# Only this file may differ between repos; pack.ps1 itself is byte-identical everywhere.
@{
    # Projects to pack, in dependency order (least-dependent first).
    Projects = @(
        @{ Path = 'Models/Models.csproj';                 Name = 'Cerebellum.BlazorBlocks.Models' }
        @{ Path = 'Data/Data.csproj';                     Name = 'Cerebellum.BlazorBlocks.Data' }
        @{ Path = 'Data.Postgres/Data.Postgres.csproj';   Name = 'Cerebellum.BlazorBlocks.Data.Postgres' }
        @{ Path = 'API/API.csproj';                       Name = 'Cerebellum.BlazorBlocks.Api' }
        @{ Path = 'Web/Web.csproj';                       Name = 'Cerebellum.BlazorBlocks.Web' }
    )
    PushSymbols = $false
}
