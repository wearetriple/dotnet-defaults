# Directory.Packages.props

To centrally manage package versions we use Directory.Packages.props so all projects in our solution
use the same package version. The props file should be placed at the root of the solution, 
and, after adding all `PackageVersion` elements to it, `Version="xx"` attributes must be 
removed from all `PackageReference` tags in all csproj files.

Make sure `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` is included in 
Directory.Packages.props to opt into central package management.

See https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management.

## Pinning transitive packages

To fix package vulnerabilities in packages that we use, we can pin transitive package versions using
`GlobalPackageReference` in the Directory.Packages.props file. This includes the package and pins
it to a specific version. By using a version expression (e.g. `[6.0.0,8.0.11]`) we set these packages
to a certain minimum version, but allow for newer versions when needed. We should always set an explicit
version or a minimum & maximum version.

Make sure `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>` is
included in Directory.Packages.props to opt into central transitive package pinning.

See https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#transitive-pinning.

An example Directory.Packages.props can be found here:: https://github.com/wearetriple/dotnet-defaults/blob/main/Directory.Packages.props
