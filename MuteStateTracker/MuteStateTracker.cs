using System;
using AdMuter.AudioControllers;

namespace AdMuter
{
    interface IMuteStateTracker : IDisposable
    {
        IAudioController Controller { get; }

        /// <summary> Mutes the controller if it isn't muted already. </summary>
        void Mute();
        /// <summary> Unmutes the controller if it is muted. </summary>
        void Unmute();
    }

    class AudioControllerStateTracker : IMuteStateTracker
    {
        public IAudioController Controller { get; private set; }
        public AudioControllerStateTracker(IAudioController controller)
        {
            this.Controller = controller;
        }

        public bool IsMuted { get; private set; }
        // maybe if I need to reimplement Amixer I'll need to check that originalVolume != currentVolume again.
        // in which case this indirection of state tracker is kinda dumb maybe
        // it's already a bit over the top 
        private int originalVolume = Program.UnknownVolume;
        public void Mute()
        {
            if (!this.IsMuted)
            {
                this.originalVolume = this.Controller.GetVolumeLevel();
                this.Controller.Mute();
                this.IsMuted = true;
            }
        }
        public void Unmute()
        {
            if (this.IsMuted)
            {
                this.Controller.Unmute(this.originalVolume);
                this.IsMuted = false;
            }
        }

        void IDisposable.Dispose()
        {
            try
            {
                if (this.IsMuted)
                    this.Unmute();
            }
            catch { }
        }
    }
}