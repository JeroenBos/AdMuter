using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdMuter
{
    class Program
    {
        const int UnknownVolume = -1;
        static bool mutedByMe => originalVolumne == UnknownVolume;
        static int originalVolumne = UnknownVolume;
        async static Task Main(string[] args)
        {
            while (true)
            {
                if (adsArePlaying() && !mutedByMe)
                {
                    originalVolumne = GetVolumeLevel();
                    if (originalVolumne != UnknownVolume)
                    {
                        Mute();
                    }
                }
                else if (!adsArePlaying() && mutedByMe)
                {
                    Unmute(originalVolumne);
                }
                await Task.Delay(100);
            }
        }

        static bool adsArePlaying()
        {
            string result = runBash("wmctrl -l | grep Chrome | grep bash");
            return !string.IsNullOrWhiteSpace(result);
        }

        private static readonly Regex extractVolumnePattern = new Regex(".*\\[(?<volume>[0-9]+)%\\].*");
        public static int GetVolumeLevel()
        {
            string output = runBash("amixer -c 0 get Master playback");

            Match match = extractVolumnePattern.Match(output);
            string? volume = match?.Groups["volume"].Value;
            if (volume != null && int.TryParse(volume, out int result))
                return result;
            return UnknownVolume;
        }
        public static void Mute()
        {
            runBash("amixer -c 0 set Master playback 0%");
        }

        public static void Unmute(int volumePercentage)
        {
            runBash($"amixer -c 0 set Master playback {volumePercentage}%");
        }

        public static string runBash(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
}
