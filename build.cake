//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=5.2.0"
#addin "nuget:?package=Cake.Docker&version=0.10.0"
#addin "nuget:?package=Cake.Incubator&version=5.1.0"

using Cake.Incubator.LoggingExtensions;

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

// This is the verbosity of the Cake logs - by defaulting to Normal we get better logging out of the gate
Context.Log.Verbosity = Argument("Verbosity", Verbosity.Normal);

var target = Argument("target", "Default");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
string semVer;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////
Setup(context =>
{
    var fromEnv = context.EnvironmentVariable("GitVersion.semVer");
    
    if (string.IsNullOrEmpty(fromEnv))
    {
        var tempPath = context.MakeAbsolute(context.Directory("./build")).Combine("temp");
        var logFilePath = tempPath.CombineWithFilePath("gitversion.log");

        try
        {
            var gitVersionInfo = GitVersion(new GitVersionSettings {
                OutputType = GitVersionOutput.Json,
                LogFilePath = logFilePath
            });
            semVer = gitVersionInfo.SemVer;
            Information("Building step-execution-container images v{0}", semVer);
            Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
            Verbose("GitVersion:\n{0}", gitVersionInfo.Dump());
        }
        finally
        {
            Information($"The GitVersion log file is available at {logFilePath}");

            if (BuildSystem.IsRunningOnTeamCity)
            {
                Information($"The GitVersion log file will be stored as a TeamCity build artifact called {logFilePath.GetFilename()} just in case you need to analyse it.");
                var buildSystem = BuildSystemAliases.BuildSystem(Context);
                buildSystem.TeamCity.PublishArtifacts(logFilePath.FullPath);
            }
        }
    }
    else
    {
        semVer = fromEnv;
        Information("Building step-execution-container images v{0}", semVer);
    }

    if (BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(semVer);
});

Teardown(context =>
{
    Information("Finished running tasks for build v{0}", semVer);
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    var tag = $"octopusdeploy/step-execution-container:{semVer}-ubuntu1804";
    DockerBuild(new DockerImageBuildSettings { Tag = new [] { tag } }, "ubuntu.18.04");
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
