using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;

namespace AdMuter
{
    class Program
    {
        const int UnknownVolume = -1;
        static bool mutedByMe = false;
        static int originalVolume = UnknownVolume;
        static string phraseToGrep = "No trigger";
        async static Task Main(string[] args)
        {
            for (int i = 0; ; i++)
            {
                // at the start and subsequently every 10 seconds, read config file:
                if ((i % 100) == 0)
                {
                    readTrigger();
                }

                if (adsArePlaying() && !mutedByMe)
                {
                    originalVolume = GetVolumeLevel();
                    if (originalVolume != UnknownVolume)
                    {
                        Mute();
                        mutedByMe = true;
                    }
                }
                else if (!adsArePlaying() && mutedByMe && originalVolume != UnknownVolume)
                {
                    Unmute(originalVolume);
                    mutedByMe = false;
                }
                await Task.Delay(100);
            }
        }

        static void readTrigger()
        {
            ConfigurationManager.RefreshSection("appSettings");
            phraseToGrep = ConfigurationManager.AppSettings["phraseToGrep"];
            if (phraseToGrep == null)
            {
                Console.WriteLine("Phrase not found");
                phraseToGrep = "No trigger";
            }
            else if (phraseToGrep.Contains("'"))
            {
                Console.WriteLine("The trigger may not contain single quotes");
                phraseToGrep = "No trigger";
            }
            Console.WriteLine("Reread: " + phraseToGrep);
        }

        static bool adsArePlaying()
        {
            string result = runBash($"wmctrl -l | grep Chrome | egrep -i '{phraseToGrep}'");
            return !string.IsNullOrWhiteSpace(result);
        }

        private static readonly Regex extractVolumePattern = new Regex(".*\\[(?<volume>[0-9]+)%\\].*");
        public static int GetVolumeLevel()
        {
            string output = runBash("amixer -c 0 get Master playback");

            Match match = extractVolumePattern.Match(output);
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
