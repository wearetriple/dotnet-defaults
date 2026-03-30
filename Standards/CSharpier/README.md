# CSharpier

To align our code formatting we should [use CSharpier](https://csharpier.com/), 
and accept the way this tool formats the code.

## Installation

CSharpier is defined as a **local .NET tool** in this repo (recommended so everyone 
uses the same version).

From the **repository root**, restore the tool once (or when the manifest changes):

```shell
dotnet tool restore --tool-manifest .config/dotnet-tools.json
```

After that, you can run `csharpier` or `dotnet csharpier` from the repo root.

To install CSharpier **globally** instead (any directory):

```shell
dotnet tool install -g csharpier
```

## Using CSharpier

From the **repository root**:

1. **Format the entire codebase:**

   ```shell
   csharpier format .
   ```

2. **Format a specific folder or file:**

   ```shell
   csharpier format src/Client.Api
   csharpier format path/to/File.cs
   ```

3. **Check only (no changes)** — reports if any files would be reformatted (same as CI):

   ```shell
   dotnet csharpier check .
   ```

## Format on save in Visual Studio

To have CSharpier format files automatically when you save (or when you change a 
file), use the official extension:

1. In Visual Studio 2022, go to **Extensions** → **Manage Extensions**.
2. Search for **CSharpier** and install the [CSharpier extension](https://marketplace.visualstudio.com/items?itemName=csharpier.CSharpier).
3. Restart Visual Studio if prompted.
4. Ensure CSharpier is available as a local tool in this repo (run `dotnet tool restore` 
from the repo root as in [Installation](#installation)). The extension will use that 
so formatting stays consistent with the repo and CI.

After that, saving a `.cs` file will run CSharpier on it. You can also trigger formatting 
via the command palette or the format shortcut.

## Format on save in Rider

To have CSharpier format files automatically when you save (or when you change a file), 
use the official extension:

1. In Rider, go to **Settings** → **Plugins**.
2. Search for **CSharpier** and install the [CSharpier plugin](https://plugins.jetbrains.com/plugin/18243-csharpier).
3. Restart Rider if prompted.
4. Ensure CSharpier is available as a local tool in this repo (run `dotnet tool restore` 
from the repo root as in [Installation](#installation)). The extension will use that 
so formatting stays consistent with the repo and CI.
5. Enable `Run on save` by navigating **Settings** → **Tools** → **CSharpier**

After that, saving a `.cs` file will run CSharpier on it. You can also trigger formatting 
via the command palette or the format shortcut.

## Pull request checks

The PR pipeline runs `dotnet csharpier check .`. If any file would be changed by 
formatting, the pipeline fails and the PR cannot merge until you run `csharpier format .` 
and push.

> NOTE: CSharpier also checks XML files, so this check might trigger some false
negatives. Make sure to ignore the XML files it should not check.

## Ignoring files

Files and folders can be excluded via [`.csharpierignore`](.csharpierignore) at the 
repository root (gitignore-style patterns).

## Example Azure DevOps PR validation yaml

```yaml
# PR verification: code formatting (CSharpier).

trigger: none

pool:
  vmImage: 'ubuntu-latest'

stages:
  - stage: verify_formatting
    jobs:
      - job: verify_formatting
        displayName: Verify formatting
        steps:
          - task: UseDotNet@2
            displayName: "Use .NET SDK"
            inputs:
              packageType: "sdk"
              version: "10.0.x"
            retryCountOnTaskFailure: 1

          - script: |
              dotnet tool restore --tool-manifest .config/dotnet-tools.json
              dotnet csharpier check .
            displayName: "Verify code is formatted with CSharpier"
            workingDirectory: $(Build.SourcesDirectory)
```
