#addin "nuget:?package=Cake.Transifex&version=0.2.0"

var configuration = Argument("configuration", "Release");
var target        = Argument("target", "Default");

Task("Clean")
  .Does(() =>
{
    DotNetBuild("./Hello-Transifex.sln", (conf) => conf.SetConfiguration(configuration).WithTarget("Clean"));
});

// Download all transifex Translations
// Probably should have a condition to only
// run on appveyor, and on certain branches
// or if the .transifexrc file exists in
// The user profile directory
Task("Download-Translations")
  .Does(() =>
{
    TransifexPull(new TransifexPullSettings
    {
        All = true,
        MinimumPercentage = 60,
        Mode = TransifexMode.OnlyReviewed
    });
});

Task("Upload-Translation-Source")
// Basic criteria to only upload only translation source if target is called directly, or we are on the develop branch and running on appveyor
  .WithCriteria(() => target == "Upload-Translation-Source" ||
                (BuildSystem.AppVeyor.IsRunningOnAppVeyor && BuildSystem.AppVeyor.Environment.Repository.Branch == "develop"))
  .Does(() =>
{
    TransifexPush(new TransifexPushSettings
    {
        UploadSourceFiles = true,
        UploadTranslations = false
    });
});

Task("Translation-Status")
  .IsDependentOn("Download-Translations")
  .Does(() =>
{
  TransifexStatus();
});

// This task can be used if you need to
// push out the source translation file
// to transifex (since transifex only auto-updates once per day)
Task("Upload-Translations")
  .Does(() =>
{
    TransifexPush(new TransifexPushSettings
    {
        UploadSourceFiles = false,
        UploadTranslations = true
    });
});

Task("Build")
  .IsDependentOn("Clean")
  .IsDependentOn("Upload-Translation-Source")
  .IsDependentOn("Translation-Status")
  .Does(() =>
{
    DotNetBuild("./Hello-Transifex.sln", (conf) => conf.SetConfiguration(configuration));
});

Task("Create-ZipFile")
  .IsDependentOn("Build")
  .Does(() =>
{
    Zip("./src/Hello-Transifex/bin/" + configuration, "./Hello-Transifex-bin.zip");
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);