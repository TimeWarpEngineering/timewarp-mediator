# Marketing prep for the Mediator full version release

## Description

Prepare all marketing material for the TimeWarp Mediator full version release so everything is drafted and unpublished before release day. Masoko (Markets) owns the marketing pieces.

- **Working headline:** "TimeWarp Mediator 1.0: The Rewrite That Pays You Back" (Steven's phrasing, adapted from a voice note that said "Ganda"; set the real version number if the release isn't 1.0).
- **Breaking changes:** yes, a lot changed. The release notes and readme need a migration section.
- **Nothing gets posted publicly without Steven's OK.**

## Checklist

- [x] 1. Refresh the NuGet readme: one-line pitch, install line, ~10-line example, migration notes. MUST land before release (it ships inside the package).
- [x] 2. Release notes / GitHub Release draft, with a migration section for the breaking changes.
- [x] 3. Blog post draft: the hub the X and Nostr posts link to.
- [x] 4. X post draft linking to the blog.
- [x] 5. Nostr post draft linking to the blog.
- [x] 6. 1200x675 social card from the Nuru card pipeline, used on X, Nostr and as the blog OG image.
- [x] 7. (Optional) Short video (≤30s) only if there's a sharp before/after code story. Voiceover depends on ElevenLabs or the free local voice. Script only: the before/after is real, and this machine has no ElevenLabs key and no local TTS, so no mp4.
- [x] 8. Follow-up post with a concrete example, 2–5 days after release.
- [ ] Get Steven's OK before anything is posted publicly.

## Timing

1. Before release: everything drafted and unpublished (readme merged, since it ships in the package).
2. Release day, once NuGet is live and indexed (~15–60 min): publish the GitHub Release, then the blog, then X and Nostr around 8–10 PM Bangkok.
3. 2–5 days later: the follow-up post.

## Session

- Created: 1184111 (2026-10-12)
- Implementer: Grok session 01a13140-27e9-76b2-9cfd-0e3b7a04b48f (2026-10-13)

## Notes

Source: Steven's voice call plus Masoko (Markets) advice on 2026-10-12. Masoko asked for the release's headline change and confirmation of breaking changes (confirmed: yes).

The release is **14.0.0**, not 1.0. 13.0.0 was the MediatR rename. Headline used in the drafts: "TimeWarp Mediator 14.0: The Rewrite That Pays You Back".

There is no Nuru CLI that renders social cards. The Nuru 3.0 launch sized a 16:9 image and cropped it. This card is HTML captured by headless Chrome so the headline and the baseline figures stay exact. Numbers are the 2026-10-12 comparison baseline (commit `d478415`): generated Send 29.9 ns / 96 B, reflection Send 69.0 ns / 224 B, ratio 0.43. martinothamar's generator on that same run is 8.8 ns / 24 B. The drafts say so.

Nothing was posted. No `gh release create`, no `ganda post`, no FreezeTeam stub PR. `<Version>` is still `14.0.0-beta.4`. The packed readmes tell a consumer to install `--version 14.0.0`. The GitHub readme says that install fails until 14.0.0 is on nuget.org. Tag-day doc flips are listed in the release draft (`generated-vs-legacy.md` still calls 14.0.0 a beta, which is true until the tag).

## Learnings

## Results

Drafted the 14.0.0 launch pack and left it unpublished. The NuGet readmes are the piece that ships in the package. Posts, the GitHub Release body, and the card wait for Steven.

- **Version.** 14.0.0, not 1.0. `<Version>` is still `14.0.0-beta.4`. This task does not bump it and does not cut the release.
- **NuGet readmes.** `source/timewarp-mediator/readme.md`, `source/timewarp-mediator-contracts/readme.md`, `source/timewarp-mediator-generators/readme.md`, `source/timewarp-mediator-analyzers/readme.md`. Each opens with the package id, a one-line pitch, `dotnet add package … --version 14.0.0`, a short example, and migration notes with an absolute link. The repository `readme.md` uses the same pitch and says the 14.0.0 install fails until that version is on nuget.org.
- **GitHub Release draft.** `documentation/release-notes/github-release-v14.0.0-draft.md`, with the breaking changes and the migration steps. `documentation/release-notes/release-notes-v14.0.0.md` points at it. Not published.
- **Blog.** `documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-release.md`. Intended URL once the date replaces `xx`: `https://thefreezeteam.com/posts/steven-t-cramer/2026/xx/2026-xx-xx-timewarp-mediator-14-release`. The FreezeTeam stub was not opened.
- **X.** `documentation/posts/blips/2026-xx-xx-timewarp-mediator-14-release.md` (259 characters).
- **Nostr.** `documentation/posts/blips/2026-xx-xx-timewarp-mediator-14-release-nostr.md` (265 characters).
- **Card.** `documentation/posts/assets/timewarp-mediator-14-card.png`, 1200×675, from `timewarp-mediator-14-card.html`.
- **Follow-up.** `documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-follow-up.md` and `documentation/posts/blips/2026-xx-xx-timewarp-mediator-14-follow-up.md` (278 characters). Same logging behavior, reflection registration beside generated registration.
- **Video.** `documentation/posts/video-script.md` only. No mp4: no ElevenLabs and no local TTS on this machine.
- **Not done.** Steven's OK. Publish order after that: GitHub Release once nuget.org has indexed 14.0.0, then the blog, then X and Nostr around 20:00–22:00 Asia/Bangkok, then the follow-up 2–5 days later.

### How to validate

**Smoke**

```bash
python3 - <<'PY'
from pathlib import Path
p = Path("documentation/posts/assets/timewarp-mediator-14-card.png")
data = p.read_bytes()
w = int.from_bytes(data[16:20], "big")
h = int.from_bytes(data[20:24], "big")
print(f"{w}x{h}")
for f in sorted(Path("documentation/posts/blips").glob("*.md")):
    print(len(f.read_text().strip("\n")), f.name)
PY
rg -n "The Rewrite That Pays You Back|not a drop-in" \
  source/timewarp-mediator/readme.md \
  source/timewarp-mediator-contracts/readme.md \
  source/timewarp-mediator-generators/readme.md \
  documentation/release-notes/github-release-v14.0.0-draft.md \
  documentation/posts/blogs/2026-xx-xx-timewarp-mediator-14-release.md
```

**Expect**

- The PNG line is `1200x675`.
- Each blip prints a count of 280 or less: launch X 259, Nostr 265, follow-up 278.
- `rg` finds the headline in the release draft and the launch blog, and "not a drop-in" in the package readmes.
- The first heading of each `source/*/readme.md` is the package id. Links in those files are absolute. `./bin/dev pack` reports `Package layout verified` for all four nupkgs and `package-smoke: package-pong via TimeWarp.Mediator.Generated.Mediator`.

**Automated gate**

```bash
./bin/dev pack
ganda repo audit
```

Ran here on 2026-10-13: pack completed successfully (layout verified, package smoke printed `package-pong`). After `ganda repo audit --fix` set `core.hooksPath=.githooks`, audit reported Passed 30, Failed 0.

**Not in scope.** No public post, no GitHub Release, no version bump to `14.0.0`, no FreezeTeam stub. The card's numbers are the recorded baseline, not a new benchmark run.

