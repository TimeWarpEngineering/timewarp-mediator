# 30-second script — not recorded

There is a sharp before/after: `AddMediator` plus `AddOpenBehavior`, then
`AddGeneratedMediator` plus `[assembly: MediatorBehavior]`. This file is the script only.

No mp4. This machine has neither an ElevenLabs key nor a local voice (`espeak`, `espeak-ng`,
`piper`, `festival` are absent). The task allows the clip only with one of those. Do not
invent a voiceover, and do not post a silent file as if it were the launch video. The
follow-up blog is the concrete example until a voice exists.

Read time is about 28 seconds at a calm pace. On screen, show the two snippets from
`documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-follow-up.md`, then the card
`documentation/posts/assets/timewarp-mediator-14-card.png`.

## Words

Thirteen was a rename. Fourteen is another dispatcher.

This is the reflection host. `AddMediator` scans the assembly. `AddOpenBehavior` closes
`LoggingBehavior` at runtime. That path is not trim-safe.

This is 14.0. The behavior class is the same. Registration moves to
`MediatorBehavior`, and the host calls `AddGeneratedMediator`. A missing handler fails
the build.

On our comparison baseline, generated Send is 29.9 nanoseconds and 96 bytes. Reflection
Send on the same Ping is 69.0 nanoseconds and 224 bytes. martinothamar is still faster.
14.0 is not a drop-in. The migration guide is in the repository.
