using System.Diagnostics;

namespace DynamixelServo.Quadruped.WebInterface.VideoStreaming
{
    public class BashCommand
    {
        private const char CommandSeparator = '\n';

        private readonly string _command;

        private BashCommand(string command)
        {
            _command = command;
        }

        public static BashCommand Command(string command)
        {
            return new BashCommand(command);
        }

        public BashCommand AddCommand(string command)
        {
            return new BashCommand(_command + CommandSeparator + command);
        }

        public string Execute()
        {
            var excaped = Escape(_command);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{excaped}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        private string Escape(string command)
        {
            return command.Replace("\"", "\\\"");
        }
    }
}
