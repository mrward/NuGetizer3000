﻿
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Packaging.Tasks.Tests.Infrastructure;
using Xunit;

namespace NuGet.Packaging.Tasks.Tests
{
    public class BuildNuProjTests : BuildProjectTests
    {
        [Fact]
        public async Task BuildNuProj_OneNuGetFile()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuProj_OneNuGetFile");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "bin", "Debug", "NuGetPackage.1.0.0.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("NuGetPackage", identity.Id);
                Assert.Equal("1.0.0", identity.Version.ToString());
                Assert.Equal("NuGetPackage", nuspecReader.GetId());
                Assert.Equal("1.0.0", nuspecReader.GetVersion().ToString());
                Assert.Equal("author", nuspecReader.GetMetadataValue("authors"));
                Assert.Equal("owner", nuspecReader.GetMetadataValue("owners"));
                Assert.Equal("Description", nuspecReader.GetMetadataValue("description"));
                Assert.Equal("false", nuspecReader.GetMetadataValue("requireLicenseAcceptance"));
                Assert.Equal("NuGetPackage", nuspecReader.GetMetadataValue("title"));

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/readme.txt", frameworkSpecificGroup.Items);
                Assert.Equal(1, frameworkSpecificGroup.Items.Count());
            }
        }

        [Fact]
        public async Task BuildNuProj_PackageReference()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuProj_PackageReference");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "bin", "Debug", "NuGetPackage.1.0.0.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());

                var dependencyGroup = nuspecReader.GetDependencyGroups().Single();
                Assert.Equal("net45", dependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = dependencyGroup.Packages.Single();
                Assert.Equal("Newtonsoft.Json", packageDependency.Id);
                Assert.Equal("[8.0.3, )", packageDependency.VersionRange.ToString());
            }
        }

        [Fact]
        public async Task BuildNuProj_ProjectReference()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuProj_ProjectReference");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "bin", "Debug", "NuGetPackage.1.0.0.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("NuGetPackage", identity.Id);
                Assert.Equal("1.0.0", identity.Version.ToString());
                Assert.Equal("NuGetPackage", nuspecReader.GetId());
                Assert.Equal("1.0.0", nuspecReader.GetVersion().ToString());

                var packageDependencyGroup = reader.GetPackageDependencies().Single();
                Assert.Equal("net45", packageDependencyGroup.TargetFramework.GetShortFolderName());
                var packageDependency = packageDependencyGroup.Packages.Single(item => item.Id == "Mono.Addins");
                Assert.Equal("[1.2.0, )", packageDependency.VersionRange.ToString());
                Assert.Equal(1, packageDependencyGroup.Packages.Count());

                var frameworkSpecificGroup = reader.GetLibItems().Single();
                Assert.Equal("net45", frameworkSpecificGroup.TargetFramework.GetShortFolderName());
                Assert.Contains("lib/net45/Library.dll", frameworkSpecificGroup.Items);
                Assert.Equal(1, frameworkSpecificGroup.Items.Count());
            }
        }

        [Fact]
        public async Task BuildNuProj_PackageVersion_DeterminedAtBuildTime()
        {
            string sourceSolutionPath = Assets.GetScenarioSolutionPath("BuildNuProj_PackageVersion_IsDeterminedAtBuildTime");
            string solutionPath = CopySolutionToTempDirectory(sourceSolutionPath);

            await NuGetRunner.RestorePackagesAsync(tempSolutionDirectory);

            await MSBuildRunner.RebuildAsync(solutionPath, buildNuGet: true);

            string packagePath = Path.Combine(tempSolutionDirectory, "bin", "Debug", "NuGetPackage.1.2.3.nupkg");

            using (var reader = new PackageArchiveReader(File.OpenRead(packagePath))) {
                var nuspecReader = new NuspecReader(reader.GetNuspec());
                var identity = reader.GetIdentity();
                Assert.Equal("NuGetPackage", identity.Id);
                Assert.Equal("1.2.3", identity.Version.ToString());
                Assert.Equal("NuGetPackage", nuspecReader.GetId());
                Assert.Equal("1.2.3", nuspecReader.GetVersion().ToString());
            }
        }
    }
}
