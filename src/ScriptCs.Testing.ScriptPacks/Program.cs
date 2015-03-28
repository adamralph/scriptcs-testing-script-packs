namespace ScriptCs.Testing.ScriptPacks
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using ScriptCs.Testing.ScriptPacks.Logging;

    internal static class Program
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            LogConfigurator.DoYourWorst();

            var scriptCsExe = ConfigurationManager.AppSettings["ScriptCsExe"];

            var namesAndTypes = GetScriptPackNamesAndTypes(args ?? new string[0]);
            log.InfoFormat(
                "Starting tests for {0} script packs...", namesAndTypes.Length.ToString(CultureInfo.InvariantCulture));

            var failedInstallations = new List<string>();
            var failedDirectoryRemovals = new List<string>();
            var failedRuns = new List<string>();
            foreach (var nameAndType in namesAndTypes)
            {
                if (!File.Exists(Path.Combine(nameAndType.Item1, "scriptcs_packages.config")))
                {
                    log.InfoFormat("Installing '{0}'...", nameAndType.Item1);
                    try
                    {
                        Install(nameAndType.Item1, scriptCsExe);
                    }
                    catch (Exception ex)
                    {
                        failedInstallations.Add(nameAndType.Item1);
                        log.ErrorException("Failed to install '{0}!.", ex, nameAndType.Item1);
                        log.InfoFormat("Removing directory '{0}'...", nameAndType.Item1);
                        try
                        {
                            FileSystem.EnsureDirectoryDeleted(nameAndType.Item1);
                        }
                        catch (Exception ex2)
                        {
                            log.ErrorException("Failed to remove directory '{0}!", ex2, nameAndType.Item1);
                            failedDirectoryRemovals.Add(nameAndType.Item1);
                        }

                        continue;
                    }
                }
                else
                {
                    log.InfoFormat("Skipping installation of '{0}' since directory already exists.", nameAndType.Item1);
                }

                log.InfoFormat("Running '{0}'...", nameAndType.Item1);
                try
                {
                    Run(nameAndType.Item1, nameAndType.Item2, scriptCsExe);
                }
                catch (Exception ex)
                {
                    failedRuns.Add(nameAndType.Item1);
                    log.ErrorException("Failed to run '{0}!.", ex, nameAndType.Item1);
                }
            }

            log.InfoFormat(
                "Finished tests for {0} script packs.", namesAndTypes.Length.ToString(CultureInfo.InvariantCulture));

            LogSummary(failedInstallations, failedDirectoryRemovals, failedRuns);

        }

        private static Tuple<string, string>[] GetScriptPackNamesAndTypes(IEnumerable<string> args)
        {
            var availableNamesAndTypes = Sanitize(File.ReadLines("packs.txt"))
                .Select(pack => pack
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(token => token != null)
                    .Select(token => token.Trim())
                    .ToArray())
                .Where(tokens => tokens.Any())
                .ToArray();

            var requiredNames = Sanitize(args).ToArray();
            if (requiredNames.Length == 0)
            {
                requiredNames = availableNamesAndTypes.Select(nameAndType => nameAndType[0]).ToArray();
            }

            foreach (var message in availableNamesAndTypes.Where(nameAndType => nameAndType.Length != 2)
                .Select(pack => string.Format(CultureInfo.InvariantCulture, "Type missing for pack '{0}'.", pack[0])))
            {
                throw new Exception(message);
            }

            var namesAndTypes = availableNamesAndTypes
                .Where(nameAndType => requiredNames.Contains(nameAndType[0], StringComparer.OrdinalIgnoreCase))
                .Select(nameAndType => Tuple.Create(nameAndType[0], nameAndType[1]))
                .ToArray();
            return namesAndTypes;
        }

        private static IEnumerable<string> Sanitize(IEnumerable<string> packs)
        {
            return packs
                .Where(pack => pack != null)
                .Select(pack => pack.Trim())
                .Where(pack => !pack.StartsWith("//", StringComparison.Ordinal));
        }

        private static void Install(string pack, string scriptCsExe)
        {
            FileSystem.EnsureDirectoryDeleted(pack);
            FileSystem.EnsureDirectoryCreated(pack);
            var info = new ProcessStartInfo
            {
                FileName = scriptCsExe,
                Arguments = string.Concat("-install ", pack, " -pre"),
                WorkingDirectory = pack,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            info.Run(string.Concat("packs-install.", pack, ".log"));
        }

        private static void Run(string pack, string type, string scriptCsExe)
        {
            var script = Path.Combine(pack, "start.csx");
            FileSystem.EnsureFileDeleted(script);

            using (var writer = new StreamWriter(script, true))
            {
                writer.WriteLine(string.Concat("var pack = Require<", type, ">();"));
                writer.WriteLine("Console.WriteLine(pack);");
            }

            var info = new ProcessStartInfo
            {
                FileName = scriptCsExe,
                Arguments = "start.csx -log debug",
                WorkingDirectory = pack,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            info.Run(string.Concat("packs-run.", pack, ".log"));
        }

        private static void LogSummary(
            IReadOnlyCollection<string> failedInstallations,
            IReadOnlyCollection<string> failedDirectoryRemovals,
            IReadOnlyCollection<string> failedRuns)
        {
            if (failedInstallations.Any())
            {
                log.InfoFormat(
                    "Summary: failed to install {0} packs - {1}",
                    failedInstallations.Count.ToString(CultureInfo.InvariantCulture),
                    string.Join(",", failedInstallations));
            }

            if (failedDirectoryRemovals.Any())
            {
                log.InfoFormat(
                    "Summary: failed to remove {0} directories - {1}",
                    failedDirectoryRemovals.Count.ToString(CultureInfo.InvariantCulture),
                    string.Join(",", failedDirectoryRemovals));
            }

            if (failedRuns.Any())
            {
                log.InfoFormat(
                    "Summary: failed to run {0} packs - {1}",
                    failedRuns.Count.ToString(CultureInfo.InvariantCulture),
                    string.Join(",", failedRuns));
            }
        }
    }
}
