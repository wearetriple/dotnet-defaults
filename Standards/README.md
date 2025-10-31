# Standards

## Configuration

Configuration defaults; [Guide](./Configuration/README.md)

1. Styling by [.editorconfig](./Configuration/.editorconfig)
2. Styling errors by [Directory.Build.props](./Configuration/Directory.Build.props)
3. Shared versioning by [Directory.Packages.props](./Configuration/Directory.Packages.props)
4. Ignoring files for git by [.gitignore](./../.gitignore)

## Renovate

The auto dependency updater; [Guide](./Renovate/README.md)

1. Npm package for installing and versioning by [package.json](./Renovate/package.json)
2. Global configuration by [renovate-config.json](./Renovate/renovate-config.js)
3. Repository configuration by [renovate.json](./Renovate/renovate.json)
4. Azure DevOps pipeline by [renovate-pipeline.yml](./Renovate/renovate-pipeline.yml)

## .NET version policy

From .NET 9 on, STS versions will be supported for 2 years. Overlapping entirely with the LTS version from a year earlier.
Read: <https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core>

This means when a new .NET version is released (typically mid-November) we can always use the latest version.

### Policy

- Wait for 1 month before releasing your application on the new version
- On **LTS releases** a migration is required thus needs to be planned & executed
  - .NET team informs devs, PMs and TechOps on it's release
- On **STS releases** a migration is optional and should be evaluated on necessity
- For new projects always use latest stable version (STS or LTS)
