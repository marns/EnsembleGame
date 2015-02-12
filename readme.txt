README

Welcome to the Ensemble player!

It's currently a toy that plays some dancing animation (to the tempo set by Pitch!) and applies
some funky audio effects, based on an Ensemble API composition.

Special note: Notes may have a "time_float" property, which should be the elapsed time, in seconds,
since the beginning of the performance. If there is no time_float property, each note will be
automatically spaced apart by 1 second.

Configuration:

Locate the config.json file in the StreamingAssets folder.
On OSX, this folder is located within the .App package.

1. Set the Ensemble REST API URL, e.g. "http://soundserver.herokuapp.com/api/"
Must have a trailing forward slash.

2. Set the composition_id. This composition will be pulled from 
http://soundserver.herokuapp.com/api/Compositions/{id}/notes.
To use the most recent composition, set the value to -1.

3. Select the audio file to use. Only the first value is played.
To use a new audio file, place a song in .ogg format (you can convert from MP3 using
Audacity) in the Music directory within the StreamingAssets folder.

4. Map sensor input to effects. The sensor names are pulled from the API notes data.

The available audio effects and (loosely) recommended ranges are:
"Flange.Rate" [0, 20]
"LowPass.CutoffFreq" [1000, 5000]
"Pitch" [.3, 3]
PitchShifter.Pitch" [.5, 2]
"Flange.WetMix" [0, 1]

And the special effect that launches fireworks:
"Fireworks"