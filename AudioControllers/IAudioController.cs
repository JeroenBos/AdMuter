
namespace AdMuter.AudioControllers
{
    interface IAudioController
    {
        int GetVolumeLevel();
        void Mute();
        void Unmute(int originalVolumePercentage);
    }
}