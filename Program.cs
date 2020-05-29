using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using AdMuter.AudioControllers;

namespace AdMuter
{
    class Program
    {
        internal const int UnknownVolume = -1;
        static string phraseToGrep = "No trigger";

        async static Task Main(string[] args)
        {
            IMuteStateTracker? currentDevice = null;
            for (int i = 0; ; i++)
            {
                if ((i % 100) == 0)
                {
                    // at the start and subsequently every 10 seconds, read config file:
                    readTrigger();
                    // and check if the current device is still the running one
                    updateCurrentDevice(ref currentDevice);
                }
                log();

                if (currentDevice != null)
                {
                    if (adsArePlaying())
                    {
                        currentDevice.Mute();
                    }
                    else if (!adsArePlaying())
                    {
                        currentDevice.Unmute();
                    }
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
            Console.WriteLine($"Reread config: phraseToGrep='{phraseToGrep}'");
        }

        static bool logTrace
        {
            get
            {
                string log_all_greps = ConfigurationManager.AppSettings["log_level"];
                return log_all_greps?.ToLower() == "trace";
            }
        }
        static bool adsArePlaying()
        {
            string result = runBash($"wmctrl -l | grep Chrome | egrep -i '{phraseToGrep}'");
            return !string.IsNullOrWhiteSpace(result);
        }
        static string previous_command_output = "";

        static void log()
        {
            if (!logTrace)
                return;

            string result = runBash($"wmctrl -l | grep Chrome");
            if (logTrace && previous_command_output != result)
            {
                Console.WriteLine(result);
                previous_command_output = result;
            }
        }

        private static void updateCurrentDevice(ref IMuteStateTracker? device)
        {
            string sink = Pulseaudio.getRunningSink();
            if (device == null)
            {
                if (!string.IsNullOrWhiteSpace(sink))
                {
                    device = new AudioControllerStateTracker(new Pulseaudio(sink));
                }
            }
            else if (device.Controller is Pulseaudio p)
            {
                if (p.Sink != sink)
                {
                    device.Dispose();
                    device = new AudioControllerStateTracker(new Pulseaudio(sink));
                }
            }
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