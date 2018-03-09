#load "nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease"
#tool "nuget:?package=nuget.commandline&version=4.5.1"

Environment.SetVariableNames();

BuildParameters.SetParameters(
    context: Context,
    buildSystem: BuildSystem,
    sourceDirectoryPath: "./src",
    title: "Hello-Transifex",
    repositoryOwner: "AdmiringWorm",
    repositoryName: "Hello-Transifex",
    solutionFilePath: "./Hello-Transifex.sln"
);

Task("Create-ZipArchive")
    .IsDependentOn("Test")
    .Does(() => {
        var path = BuildParameters.Paths.Directories.PublishedApplications + "/Hello_Transifex";
        var outputPathDirectory = BuildParameters.Paths.Directories.Packages.Combine("Archives");
        var outputPath = outputPathDirectory.CombineWithFilePath("Hello-Transifex-bin.zip");
        EnsureDirectoryExists(outputPathDirectory);
        Zip(path, outputPath);
});

BuildParameters.Tasks.PackageTask.IsDependentOn("Create-ZipArchive");

BuildParameters.PrintParameters(Context);

Build.Run();