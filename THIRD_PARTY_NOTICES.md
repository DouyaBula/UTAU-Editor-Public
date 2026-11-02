# Third-party notices

UTAU Editor is an unofficial, non-commercial derivative of [osu!lazer](https://github.com/ppy/osu). Original copyright headers and the MIT [LICENCE](LICENCE) are retained. No affiliation with ppy is implied; the osu! and ppy names/logos are not licensed by the MIT code licence.

| Component | Attribution / terms |
| --- | --- |
| osu!lazer / osu!framework | ppy Pty Ltd and contributors, MIT. [Source](https://github.com/ppy/osu) / [framework](https://github.com/ppy/osu-framework). |
| osu!resources | ppy and resource contributors. Mostly [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/), with separate font licences. [Resource licence](https://github.com/ppy/osu-resources/blob/master/LICENCE.md). Game assets are used through the upstream resource package. |
| Noto Sans Math | Copyright 2022 The Noto Project Authors; SIL Open Font License 1.1. [Full licence](osu.Game.Rulesets.Osu/Edit/Resources/Fonts/NotoSansMath-OFL.txt); portable packages include `licences/NotoSansMath-OFL.txt`. |
| BASS / BASSmix / BASS_FX | Un4seen Developments and component authors. [BASS licensing](https://www.un4seen.com/bass.html); free for qualifying non-commercial use, not covered by this project's MIT licence. |
| FFmpeg libraries | FFmpeg contributors. [Licence and source](https://ffmpeg.org/legal.html); native builds are supplied by [osu-framework](https://github.com/ppy/osu-framework/tree/master/native). |
| Other managed/native dependencies | Supplied through NuGet under each package's licence. Versioned references are in project files; release archives include `dependencies.json` with the resolved direct/transitive package list. Native package build sources are maintained upstream. |

The optional pose companion uses [MediaPipe](https://github.com/google-ai-edge/mediapipe) and OpenCV with their respective licences. Pose model bundles are downloaded separately from Google's model storage; no model, user video or pose export is shipped here. Check the model terms before redistributing them.

Inspiration: [Mapping Tools](https://github.com/OliBomby/Mapping_Tools) for reusable patterns; [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier) for local-strain presentation. This fork does not bundle their applications.
