
using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Frameworks;

namespace NuGet.Packaging.Tasks
{
    public class ApiIntersect : ToolTask
    {
        [Required]
        public ITaskItem Framework { get; set; }

        [Required]
        public ITaskItem[] IntersectionAssembly { get; set; }

        [Required]
        public ITaskItem OutputDirectory { get; set; }

        protected override string ToolName
        {
            get { return "ApiIntersect"; }
        }

        protected override string GenerateFullPathToTool()
        {
            return Path.Combine(ToolPath, ToolExe);
        }

        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();

            var nugetFramework = NuGetFramework.Parse(Framework.ItemSpec);

            builder.AppendSwitch("-o");
            string fullOutputPath = Path.Combine(OutputDirectory.ItemSpec, GetOutputDirectoryName(nugetFramework));
            builder.AppendFileNameIfNotNull(fullOutputPath);

            string frameworkPath = GetFrameworkPath(nugetFramework);
            builder.AppendSwitch("-f");
            builder.AppendFileNameIfNotNull(frameworkPath);

            foreach (var assembly in IntersectionAssembly.NullAsEmpty())
            {
                builder.AppendSwitch("-i");
                builder.AppendFileNameIfNotNull(assembly.ItemSpec);
            }

            return builder.ToString();
        }

        string GetFrameworkPath(NuGetFramework framework)
        {
            if (framework.IsPCL)
            {
                return GetPortableLibraryPath(framework);
            }

            throw new NotImplementedException("Only PCL profiles supported.");
        }

        string GetPortableLibraryPath(NuGetFramework framework)
        {
            return string.Format("/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/xbuild-frameworks/.NETPortable/v{0}.{1}/Profile/{2}/",
                framework.Version.Major,
                framework.Version.Minor,
                framework.Profile);
        }

        string GetOutputDirectoryName(NuGetFramework framework)
        {
            if (framework.IsPCL && framework.HasProfile)
            {
                return framework.Profile;
            }

            return framework.GetShortFolderName();
        }
    }
}
