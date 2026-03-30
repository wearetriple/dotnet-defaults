# Configuration

## .editorconfig

To align our coding style we use a `.editorconfig` file that specifies our default coding style. This
file should be put in the root of all of our projects, and should automatically be picked up by IDEs.

File: [.editorconfig](.editorconfig)

Documentation: 
- https://editorconfig.org/
- https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/code-style-rule-options

## Directory.Packages.props

To centrally manage package versions we use Directory.Packages.props so all projects in our solution
use the same package version. The props file should be placed at the root of the solution,
and, after adding all `PackageVersion` elements to it, `Version="xx"` attributes must be
removed from all `PackageReference` tags in all csproj files.

Make sure `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` is included in
Directory.Packages.props to opt into central package management.

File: [Directory.Packages.props](Directory.Packages.props)

Documentation: https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management

### Pinning transitive packages

To fix package vulnerabilities in packages that we use, we can pin transitive package versions using
`GlobalPackageReference` in the Directory.Packages.props file. This includes the package and pins
it to a specific version. By using a version expression (e.g. `[6.0.0,8.0.11]`) we set these packages
to a certain minimum version, but allow for newer versions when needed. We should always set an explicit
version or a minimum & maximum version.

Make sure `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>` is
included in Directory.Packages.props to opt into central transitive package pinning.

See https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#transitive-pinning.

## Directory.Build.props

In addition to `.editorconfig` for aligning our coding styles we use Directory.Build.props to force
additional rules through analyzer packages. The props file will add additional settings to each csproj
to avoid having to include packages manually. This file should be put in the root of all (non-test) projects
and should be automatically be picked up by IDEs.

It adds:
- Enables nullability;
- Shared warnings as errors setup;
- Default analyzers;

File: [Directory.Build.props](Directory.Build.props)

Documentation: https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory

### Directory.Test.props

Directory.Build.props references Directory.Test.props when the project file ends with `.Tests`. This
props file configures Rider to use the new Microsoft Testing Platform Runner, and disables any
errors about using banned symbols. Tests are allowed to use any banned symbol, as those might
be needed to setup or run a test.

File: [Directory.Test.props](Directory.Test.props)

## Banned Symbols

Banned Symbols allow for preventing common made mistakes and codify alternatives. For example, it is
easy to miss `DateTime.Now` in a PR. Banned Symbols help with disallowing `DateTime.Now` and point
developers to `DateTime.UtcNow`. It also helps with preventing developers using a default feature,
while the project might have an extension that wraps that feature. For example, our default
[Options](../../Patterns/Options.md) way-of-working adds the `AddConfiguration()` extension method
that should always be used, instead of the default `AddOptions`.

Each of our projects should have it own Banned Symbol list, and should be expanded with project
specific rules and conventions. 

Files: [Banned Symbols](BannedSymbols.txt) and [Directory.Build.props](Directory.Build.props)

## .gitignore

Shared configuration for ignoring files for git.
To be placed in root of the solution.

File: [.gitignore](./../../.gitignore)

Documentation: https://git-scm.com/docs/gitignore
