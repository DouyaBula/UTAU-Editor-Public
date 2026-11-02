# Official lazer synchronization — 2026-10-19

Merged official `upstream/master` at `676f3b01f2` into UTAU_Editor.

- Updated to .NET 10 and database schema 52.
- Integrated official editor action bindings and placement state management.
- Preserved custom gameplay default keys, approach-rate shortcut, toolbox ordering identifiers, custom left toolbox groups, canvas zoom/pan, and difficulty timeline access.
- Adapted custom radio-button panels and slider toolboxes to the official editor APIs.
- Kept the custom project README and configuration entries alongside new official options.

Validation: desktop build succeeded; 50 focused pose, curve, gallery and difficulty sampling tests passed. No client was launched and no user database was modified. Official MessagePack 3.1.8 dependency reports NU1902 during restore/build.

SDK: .NET 10.0.100. Existing system SDK installations remain unchanged.
