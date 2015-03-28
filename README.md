# scriptcs script pack testing app

This is a console application designed to run smoke tests against all [Script Packs](https://github.com/scriptcs/scriptcs/wiki/Script-Packs) published on NuGet, ahead of a scriptcs release.

The list of Script Packs is maintained in `packs.txt` in the project source folder. Each entry contains two comma delimited items - the name of the script pack and the name of the type which should be used in `Require<T>()` when running the test. An item in this list can be ignored by prefixing the line with `//`.

The list was seeded by running `nuget list scriptcs -pre` and then (painstakingly) curating the output.

If your Script Pack is not in the list, please send a PR which adds it. This will ensure that the scriptcs core team will run the tests against your Script Pack before a release to ensure it is still compatible.

### Test details

It should be noted that, in it's current form, this app only performs an *extremely* basic test. The following steps are performed for each script pack.

* A directory is created for the script pack in the project output folder and is used as the working directory for all further steps.
* The script pack is installed by executing `scriptcs -install {Script Pack name} -pre`. Note that if the latest version of a Script Pack is pre-release, that version will be used.
* A `start.csx` file is created containing the following script, with `T` replaced with the name of the type provided in `packs.txt`:

```c#
var pack = Require<T>();
Console.WriteLine(pack);
```

* The script is run by executing `scriptcs start.csx -log debug`.

### Building the app

* Command line: Run `build.cmd` (Windows) or `build.sh` (*nix).
* Visual Studio\*: build like any other solution.

\* *Substitute 'Visual Studio with your IDE/posh text editor of choice*

### Running the app

First, change the `ScriptCsExe` configuration item to point to your copy of scriptcs.exe.

* Command line: edit `packs.config`
* Visual Studio: edit `App.config`

To run the tests for all script packs:

* Command line: navigate to the project output folder and execute `packs` (Windows) or `mono packs` (*nix)
* Visual Studio: brutalise the appropriate shortcut key with your favourite finger, e.g. <kbd>F5</kbd> in Visual Studio.

To run the tests for specific script packs, provide the names of the script packs as arguments (case-insensitive):

* Command line: `packs scriptcs.nancy scriptcs.adder` (Windows) or `mono packs scriptcs.nancy scriptcs.adder` (*nix)
* Visual Studio: set the arguments in the Debug tab of the project properties.

If there are any failures, you will see them both in the console output and in the log file `packs.log`. For more detail about a particular failure, see `packs-install.{Script Pack name}.log` (for installation failures) or `packs-run.{Script Pack name}.log` (for script running failures).
