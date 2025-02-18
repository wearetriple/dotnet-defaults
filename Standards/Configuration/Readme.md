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

### Pinning transitive packages

To fix package vulnerabilities in packages that we use, we can pin transitive package versions using
`GlobalPackageReference` in the Directory.Packages.props file. This includes the package and pins
it to a specific version. By using a version expression (e.g. `[6.0.0,8.0.11]`) we set these packages
to a certain minimum version, but allow for newer versions when needed. We should always set an explicit
version or a minimum & maximum version.

Make sure `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>` is
included in Directory.Packages.props to opt into central transitive package pinning.

See https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#transitive-pinning.

## .gitignore

File: [.gitignore](.gitignore)