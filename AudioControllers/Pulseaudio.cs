using System.Text.RegularExpressions;
using static AdMuter.Program;

namespace AdMuter.AudioControllers
{
    class Pulseaudio : IAudioController
    {
        private static readonly Regex extractVolumePattern = new Regex(".*\\[(?<volume>[0-9]+)%\\].*");

        internal static string getRunningSink()
        {
            return runBash(@"pactl list short | grep RUNNING | sed -e 's,^\([0-9][0-9]*\)[^0-9].*,\1,'").Replace("\n", "");
        }

        public string Sink { get; private set; }
        public Pulseaudio(string sink) => this.Sink = sink;
        public void Mute()
        {
            runBash($"pactl set-sink-mute {Sink} 1");
        }

        public void Unmute()
        {
            runBash($"pactl set-sink-mute {Sink} 0");
        }
        int IAudioController.GetVolumeLevel() => UnknownVolume; // this property isn't used here, but when using the Amixer package
        void IAudioController.Unmute(int originalVolumePercentage) => Unmute();
    }
}