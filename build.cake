//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
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
string tag;

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
            if (IsRunningOnUnix())
            {
                using(var process = StartAndReturnProcess("xmlstarlet", new ProcessSettings{ Arguments = "edit -O --inplace --update \"//dllmap[@os='linux']/@target\" --value \"/lib64/libgit2.so.26\" tools/GitVersion.CommandLine.4.0.0/tools/LibGit2Sharp.dll.config" }))
                {
                    process.WaitForExit();
                    // This should output 0 as valid arguments supplied
                    Information("Exit code: {0}", process.GetExitCode());
                }
            }

            var gitVersionInfo = GitVersion(new GitVersionSettings {
                NoFetch = true,
                OutputType = GitVersionOutput.Json,
                LogFilePath = logFilePath
            });
            Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
            Verbose("GitVersion:\n{0}", gitVersionInfo.Dump());
            semVer = gitVersionInfo.SemVer;
            Information("Building step-execution-container images v{0}", semVer);
        }
        catch (Exception e)
        {
            Information(e);
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
    tag = $"octopusdeploy/step-execution-container:{semVer}-ubuntu1804";
    DockerBuild(new DockerImageBuildSettings { Tag = new [] { tag } }, "ubuntu.18.04");
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var currentDirectory = MakeAbsolute(Directory("./"));
    if (IsRunningOnUnix())
    {
        try
        {
            using(var process = StartAndReturnProcess("docker", new ProcessSettings{ Arguments = $"run -v {currentDirectory}:/app {tag} /bin/bash -c 'cd app/ubuntu.18.04 && ./scripts/run_tests_during_build.sh'" }))
            {
                process.WaitForExit();
                // This should output 0 as valid arguments supplied
                Information("Exit code: {0}", process.GetExitCode());
                if (process.GetExitCode() > 0)
                {
                    throw new Exception("Tests exited with exit code grater than 0");
                }
            }
        }
        catch (Exception e)
        {
            Information(e);
            throw; // rethrow the exception so cake will fail
        }
    }
});

Task("Push")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() =>
{
    try
    {
        Information("Releasing image " + tag + " to Docker Hub");
         using(var process = StartAndReturnProcess("docker", new ProcessSettings{ Arguments = $"push {tag}" }))
        {
            process.WaitForExit();
            // This should output 0 as valid arguments supplied
            Information("Exit code: {0}", process.GetExitCode());
            if (process.GetExitCode() > 0)
            {
                throw new Exception("Pushing docker image failed");
            }
        }
    } catch (Exception e)
    {
        Information(e);
        throw; // rethrow the exception so cake will fail
    }
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
