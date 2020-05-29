// using System.Text.RegularExpressions;
// using static AdMuter.Program;

// namespace AdMuter.AudioControllers
// {
//     class Amixer : IAudioController
//     {
//         private static readonly Regex extractVolumePattern = new Regex(".*\\[(?<volume>[0-9]+)%\\].*");
//         public int GetVolumeLevel()
//         {
//             string output = Program.runBash("amixer -c 0 get Master playback");

//             Match match = extractVolumePattern.Match(output);
//             string? volume = match?.Groups["volume"].Value;
//             if (volume != null && int.TryParse(volume, out int result))
//                 return result;
//             return UnknownVolume;
//         }
//         public void Mute()
//         {
//             runBash("amixer -c 0 set Master playback 0%");
//         }

//         public void Unmute(int volumePercentage)
//         {
//             runBash($"amixer -c 0 set Master playback {volumePercentage}%");
//         }
//     }
// }