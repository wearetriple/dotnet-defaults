# OpenApi docs

ASP.NET Core generates OpenApi docs using the 
[OpenApi](https://www.nuget.org/packages/Microsoft.AspNetCore.OpenApi) nuget package.
See [the docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi)
for more information.

## Correctly handling enums in OpenApi

Enums are documented as integers by default, without listing the possible values.
To fix this and make the documentation way more useful, create the following transformer:

```c#
public sealed class StringEnumOpenApiSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (!underlying.IsEnum)
        {
            return Task.CompletedTask;
        }

        var names = GetSerializedEnumNames(underlying);

        schema.Type = JsonSchemaType.String;
        schema.Enum = names.Select(static name => (JsonNode)JsonValue.Create(name)!).ToList();

        return Task.CompletedTask;
    }

    private static IEnumerable<string> GetSerializedEnumNames(Type enumType)
    {
        var namingPolicy = JsonNamingPolicy.CamelCase;

        foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attribute = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            yield return namingPolicy.ConvertName(attribute?.Name ?? field.Name);
        }
    }
}
```

Add the transformer to the OpenApi configuration via:

```c#
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer<StringEnumOpenApiSchemaTransformer>();
});
```

Make sure all enums are also serialized to and from strings, with the correct casing
by adding a `JsonStringEnumConverter` to the general json setup.

For minimal APIs do:

```c#
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
```

For Controller based APIs do:

```c#
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });
```

## Automatically generate docs on build

The nuget package Microsoft.Extensions.ApiDescription.Server allows for generating
docs on build. This is useful when the Api is exposed via Api Management, and the
deployment process requires an artifact containing the OpenApi spec.

Add the following items to `Directory.Build.props` so each project that depends on
`Microsoft.Extensions.ApiDescription.Server` will build its OpenApi spec in `bin/openapi-v1.json`.
As of April 2026, Api Management does not support the features in OpenApi v3.1, so
version 3.0 must be forced.

```xml
<!-- Opt-in to OpenApi generation on build (applies when project has Microsoft.Extensions.ApiDescription.Server dependency) -->
<PropertyGroup>
  <OpenApiGenerateDocuments Condition="'$(Configuration)' == 'Debug'">true</OpenApiGenerateDocuments>
  <OpenApiGenerateDocuments Condition="'$(Configuration)' == 'Release'">false</OpenApiGenerateDocuments>
  <!-- Output to bin so its ignored by git by default -->
  <OpenApiDocumentsDirectory>$(MSBuildProjectDirectory)/bin/</OpenApiDocumentsDirectory>
  <!-- Use the same name for every project so the CI tooling can use the same name everywhere -->
  <!-- Generate OpenApi3_0 since Api Management cannot handle 3.1 yet -->
  <OpenApiGenerateDocumentsOptions>--file-name openapi-v1 --openapi-version OpenApi3_0</OpenApiGenerateDocumentsOptions>
</PropertyGroup>
```

Since the generator runs your application, make sure that any authentication middleware
is disabled when the document is generated. This can be done by checking if the entry
assembly is `GetDocument.Insider`. Make sure this check does not lead to an authentication-bypass
vulnerability.

If the generation of OpenApi docs becomes too slow, the `Condition` in `OpenApiGenerateDocuments`
can for example be updated to `'$(MSBuildProjectName)' == '$(OPENAPI_PROJECT)'`.
When the environment variable `OPENAPI_PROJECT` is equal to full project name, only
then the OpenApi docs are generated. The OpenApi build pipeline becomes:

```yaml
parameters:
  - name: ProjectName
    type: string
    default: []
  - name: PublishBuildArtifacts
    type: boolean
    default: true

jobs:
  - ${{ if parameters.PublishBuildArtifacts }}:
    - job: build_openapi
      displayName: OpenApi ${{ parameters.ProjectName }}
      dependsOn: []
      steps:
        - task: PowerShell@2
          displayName: Build project
          inputs:
            targetType: "inline"
            script: |
              $projectName = "./src/${{ parameters.ProjectName }}/${{ parameters.projectName }}.csproj"
              $openApiSourceFile = "./src/${{ parameters.ProjectName }}/bin/openapi-v1.json"
              $openApiDestinationFile = "$(Build.ArtifactStagingDirectory)/openapi-v1.json"

              $env:OPENAPI_PROJECT = "${{ parameters.ProjectName }}"

              dotnet build $projectName -c Debug

              Copy-Item -Path $openApiSourceFile -Destination $openApiDestinationFile
        - task: PublishBuildArtifacts@1
          displayName: "Upload artifacts"
          inputs:
            pathToPublish: "$(Build.ArtifactStagingDirectory)/"
            artifactName: "openapi"
```
