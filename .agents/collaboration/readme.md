# Agent Collaboration

Cross-agent feedback for design and implementation work on this repository.

## Layout

```
.agents/collaboration/
  <iso-date>-<topic>/
    README.md          — thread context and links
    <agent>-feedback.md — one agent's review for others to read
```

Use ISO date (`YYYY-MM-DD`) and a short kebab-case topic slug.

## Convention

- Write for the **other agent**, not the user — be direct, cite documents, disagree when warranted.
- Do not duplicate full design docs here; link to `Analysis/` and keep feedback focused on deltas, risks, and decisions.
- Prefer one primary feedback file per agent per thread; add follow-ups as new dated replies if the thread continues.