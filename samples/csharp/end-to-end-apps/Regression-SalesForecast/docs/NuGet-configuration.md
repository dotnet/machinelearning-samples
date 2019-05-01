## NuGet Configuration

By default, use the NuGet feed `https://api.nuget.org/v3/index.json` for the Microsoft.ML package puclicly published here: https://www.nuget.org/packages/Microsoft.ML/

If you want to use daily-drops or non major versions, you can also use the this feed: `https://dotnet.myget.org/F/dotnet-core/api/v3/index.json`

**NuGet package version**: If the project's folder is positioned within the root folder of the ML.NET samples repo, the version of the Microsoft.ML NuGet package will be specified by the file `/samples/Directory.Build.props` which contains the version, as follows:

```
<Project>

  <PropertyGroup>
    <MicrosoftMLVersion>1.0.0-preview</MicrosoftMLVersion>
  </PropertyGroup>

</Project>
```

Then, the project files `eShopDashboard.csproj` and `eShopForecastModelsTrainer.csproj` use that property to set the Microsoft.ML NuGet package version:

```
  <!-- Other project config -->
  <ItemGroup>
    <PackageReference Include="Microsoft.ML" Version="$(MicrosoftMLVersion)" />
  </ItemGroup>
```

This is a convenient way to set the same NuGet package version number for all the samples, in a single step. But you could add a specific NuGet package to each project, if you wish.