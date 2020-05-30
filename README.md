# AdMuter
Automatically mutes system sound when it's detected that Spotify is playing an ad, and unmutes afterwards.


My setup: Ubuntu 18.04, and I play spotify in chrome, and it must be the active tab for it to work, which is not so much of a problem if you put that chrome in a different workspace.

To start it in the background I do:

    nohup $HOME/git/AdMuter/bin/Debug/netcoreapp3.1/AdMuter &>/dev/null &


