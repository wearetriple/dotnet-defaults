# Standards

## Configuration

Configuration defaults; [Guide](./Configuration/Readme.md)

1. Styling by [.editorconfig](./Configuration/.editorconfig)
2. Styling errors by [Directory.Build.props](./Configuration/Directory.Build.props)
3. Shared versioning by [Directory.Packages.props](./Configuration/Directory.Packages.props)
4. Ignoring files for git by [.gitignore](./../.gitignore)

## Renovate

The auto dependency updater; [Guide](./Renovate/Readme.md)

1. Npm package for installing and versioning by [package.json](./Renovate/package.json)
2. Global configuration by [renovate-config.json](./Renovate/renovate-config.js)
3. Repository configuration by [renovate.json](./Renovate/renovate.json)
4. Azure DevOps pipeline by [renovate-pipeline.yml](./Renovate/renovate-pipeline.yml)