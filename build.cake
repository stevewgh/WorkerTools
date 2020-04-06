//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=GitVersion.CommandLine&version=4.0.0"
#addin "nuget:?package=Cake.Docker&version=0.10.0"
#addin "nuget:?package=Cake.Incubator&version=5.1.0"

using Cake.Incubator.LoggingExtensions;

class OctopusDockerTag
{
    public string operatingSystem { get; set; }
    public string dockerNamespace { get; }
    public string completeTag { get; }
    public string version { get; set; }
    public string tag { get; set; }
    private GitVersion gitVersion;

    public OctopusDockerTag(GitVersion version, string operatingSystem) {
        this.gitVersion = version;
        this.operatingSystem = operatingSystem;
        this.version =  $"{gitVersion.Major}.{gitVersion.Minor}.{gitVersion.Patch}";
        this.tag = $"{version}-{operatingSystem}";
        this.dockerNamespace = "octopusdeploy/step-execution-container";
        this.completeTag = $"{this.dockerNamespace}:{this.version}-{this.operatingSystem}";
    }

    public string[] Tags() {
        return new string[] {
            this.CreateTag("latest"),
            this.CreateTag(this.version),
            this.CreateTag(this.gitVersion.Major.ToString()),
            this.CreateTag(new string[] { this.gitVersion.Major.ToString(), this.gitVersion.Minor.ToString() })
        };
    }

    private string CreateTag(string[] version) {
        return $"{this.dockerNamespace}:{string.Join(".", version)}-{this.operatingSystem}";
    }

    private string CreateTag(string version) {
        return $"{this.dockerNamespace}:{version}-{this.operatingSystem}";
    }
}

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

// This is the verbosity of the Cake logs - by defaulting to Normal we get better logging out of the gate
Context.Log.Verbosity = Argument("Verbosity", Verbosity.Normal);

var target = Argument("target", "Default");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
string operatingSystem;
GitVersion gitVersionInfo;
OctopusDockerTag dockerTag;

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
                operatingSystem = "ubuntu.18.04";

                using(var process = StartAndReturnProcess("xmlstarlet", new ProcessSettings{ Arguments = "edit -O --inplace --update \"//dllmap[@os='linux']/@target\" --value \"/lib64/libgit2.so.26\" tools/GitVersion.CommandLine.4.0.0/tools/LibGit2Sharp.dll.config" }))
                {
                    process.WaitForExit();
                    // This should output 0 as valid arguments supplied
                    Information("Exit code: {0}", process.GetExitCode());
                }
            }

            gitVersionInfo = GitVersion(new GitVersionSettings {
                NoFetch = true,
                OutputType = GitVersionOutput.Json,
                LogFilePath = logFilePath
            });
            Information("Informational Version {0}", gitVersionInfo.InformationalVersion);
            Verbose("GitVersion:\n{0}", gitVersionInfo.Dump());
            Information("Building step-execution-container images v{0}", gitVersionInfo);
            dockerTag = new OctopusDockerTag(gitVersionInfo, operatingSystem);
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
        Information("Building step-execution-container images v{0}", gitVersionInfo);
    }

    if (BuildSystem.IsRunningOnTeamCity)
        BuildSystem.TeamCity.SetBuildNumber(gitVersionInfo.SemVer);
});

Teardown(context =>
{
    Information("Finished running tasks for build v{0}", gitVersionInfo.SemVer);
});

//////////////////////////////////////////////////////////////////////
//  PRIVATE TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .Does(() =>
{
    Information("Tags to be built:");
    dockerTag.Tags().ToList().ForEach((tag) => Information(tag));
    DockerBuild(new DockerImageBuildSettings { Tag = dockerTag.Tags() }, dockerTag.operatingSystem);
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
            Information($"Running tests against {dockerTag.completeTag}");
            using(var process = StartAndReturnProcess("docker", new ProcessSettings{ Arguments = $"run -v {currentDirectory}:/app {dockerTag.completeTag} /bin/bash -c 'cd app/ubuntu.18.04 && ./scripts/run_tests_during_build.sh'" }))
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
        Information("Releasing image " + dockerTag.completeTag + " to Docker Hub");
         using(var process = StartAndReturnProcess("docker", new ProcessSettings{ Arguments = $"push {dockerTag.tag}" }))
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
