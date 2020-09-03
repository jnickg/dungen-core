# Contributing to DunGen
This document gives guidance and instruction to those looking to contribute to the DunGen project.

# Development
## Environment
* Visual Studio 2019 Community is recommended. We plan to support VS Code and other IDEs eventually, but today we do not.
* We use .NET Core 2.1 as our target framework
* We rely on NuGet to integrate third-party packages
## Building
In the solution folder, run the commands below:
```
dotnet build --configuration Release
```
## Testing
We use MSTest style unit tests, and the Microsoft .NET Test SDK.

Running unit tests is as simple as:
```
dotnet test --verbosity normal
```
## Debug & Test Tools
The `dungen-cli` project is designed to be a quick way to test new functionality as it's added, such as algorithms or design changes. It uses a predicate menu system, with help text built in. Append `-h` to any predicate to see its description.

One common workflow, for example:
```
> dungeon create -w
> generator runs add 0
> generator go
> dungeon render -s testRender.bmp
> dungeon save -n testDungeon.dgm
```
This will create a 51x51 dungeon, add the first-listed algorithm, run that algorithm on the whole dungeon, then render the generated dungeon to an image (`testRender.bmp` if you couldn't guess), and save it to the file `testDungeon.dgm`.
# Collaboration
## Issues & Milestones
We track progress using Github's [Issues](https://github.com/jnickg/dungen-core/issues) and [Milestones](https://github.com/jnickg/dungen-core/milestones) tools. If you have an issue of your own you'd like to raise, feel free to file one and we will respond promptly.

Commits that relate directly to an Issue should call them out (e.g. `fixes #37` or `related to #42`) as a bullet-point in the description.
## Pull Requests
All updates to `master` go through code review via pull request (PR), if just ritualistically. During code review, reviewers are expected to review any design issues, as well as code quality, before approving. If they are unsure they will loop in an arbiter to make a call.

The latest commit in a PR will automatically be validated using _Github workflows_. Currently this checks the build, and runs unit tests.

PR description should explicitly call out which Issues are closed by that PR, if any.
## Coding Style
We "use" the canonical [CoreFX C# Coding Style](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md) guide for coding convention. The word "use" is in quotes because it's not enforced at this point. That said, messy and inconsistently styled code will likely not make it past code review.