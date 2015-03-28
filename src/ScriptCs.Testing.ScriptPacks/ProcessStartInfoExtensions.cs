namespace ScriptCs.Testing.ScriptPacks
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public static class ProcessStartInfoExtensions
    {
        public static void Run(this ProcessStartInfo info, string outputFile)
        {
            var output = new StringBuilder();
            int exitCode;
            using (var process = new Process())
            {
                process.StartInfo = info;
                process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data);
                process.ErrorDataReceived += (sender, e) => output.AppendLine(e.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (!process.WaitForExit(30000))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        throw new TimeoutException(
                            "The process took too longer than 30 seconds to exit and killing the process failed.", ex);
                    }

                    throw new TimeoutException("The process took longer than 30 seconds to exit.");
                }

                using (var writer = new StreamWriter(outputFile, true))
                {
                    writer.WriteLine(output.ToString());
                    writer.Flush();
                }

                exitCode = process.ExitCode;
            }

            if (exitCode != 0)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The process exited with code {0}.",
                    exitCode.ToString(CultureInfo.InvariantCulture));

                throw new Exception(message);
            }
        }
    }
}
