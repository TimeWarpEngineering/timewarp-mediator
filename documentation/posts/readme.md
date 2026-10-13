# Mediator 14.0 posts (unpublished)

Drafts for the stable release. The version is **14.0.0**, not 1.0. Nothing here has been
posted. Steven approves before the GitHub Release, the blog stub, X, or Nostr.

| File | When | Channel |
|---|---|---|
| `blogs/2026-xx-xx-timewarp-mediator-14-release.md` | Release day, after nuget.org has indexed 14.0.0 | Blog, the link target |
| `blips/2026-xx-xx-timewarp-mediator-14-release.md` | Same hour, 20:00–22:00 Asia/Bangkok | X. Attach the card. |
| `blips/2026-xx-xx-timewarp-mediator-14-release-nostr.md` | With the X post | Nostr. Attach the card. |
| `blogs/2026-xx-xx-timewarp-mediator-14-follow-up.md` | 2–5 days later | Blog |
| `blips/2026-xx-xx-timewarp-mediator-14-follow-up.md` | With that blog post | X and Nostr |
| `assets/timewarp-mediator-14-card.png` | With the launch posts | 1200×675 card |
| `video-script.md` | Only if a voice is available | Not recorded |
| `../release-notes/github-release-v14.0.0-draft.md` | Release day, pasted over `dev release` notes | GitHub Release |

`xx` in the blog URLs is the publish month and day. Blips must stay at or under 280 characters
so `ganda post` can take them. Do not run `ganda post` from this task.

The blog body lives in this repository, the same way the Nuru 3.0 draft does. The
TheFreezeTeamBlog stub (frontmatter `Image:`, Git shortcode, lowercase path) is a separate
repo and stays unopened until Steven wants it public. Nuru task 001 recorded the path-case
trap: the shortcode path must match `documentation/posts/blogs/` in this repo.

## Card

`assets/timewarp-mediator-14-card.html` is the source. Headless Chrome screenshots it at
1200×675 and device scale 1. The figures are the 2026-10-12 comparison baseline (ratio 0.43,
29.9 ns / 96 B against 69.0 ns / 224 B). Re-render after a new baseline. Do not typeset those
numbers in an image model.

```bash
google-chrome --headless=new --disable-gpu --hide-scrollbars --force-device-scale-factor=1 \
  --window-size=1200,675 \
  --screenshot=documentation/posts/assets/timewarp-mediator-14-card.png \
  documentation/posts/assets/timewarp-mediator-14-card.html
```

Run that from the repository root. Then check the PNG is 1200 by 675.
