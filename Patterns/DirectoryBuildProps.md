# Directory.Build.props

In addition to `.editorconfig` for aligning our coding styles we use Directory.Build.props to force
additional rules through analyzer packages. The props file will add additional settings to each csproj
to avoid having to include packages manually. This file should be put in the root of all (non-test) projects
and should be automatically be picked up by IDEs.

Our default Directory.Build.props can be found here: https://github.com/wearetriple/dotnet-defaults/blob/main/Directory.Build.props
