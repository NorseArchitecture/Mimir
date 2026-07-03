# CLAUDE.md — Mimir (`Norse.ReferenceData.Components` / `.Web.Server` / `.Worker`)

## 0. Wrong Root — Halt

If you are reading this because **Mimir itself is the Claude Code session root** — someone ran `claude` from inside this directory instead of `../Bifrost` — stop here. Do not read further, do not propose changes, do not run anything.

Tell the user: every Norse Architecture session starts from **Bifrost**. Org-wide settings (the `superpowers` plugin, permission rules) only apply when Bifrost is the actual session root — Claude Code never merges a submodule's own `.claude/settings.json` into a parent-launched session. Exit, `cd ../Bifrost`, and run `claude` there instead.

This repo's own `.claude/settings.json` carries a `SessionStart` hook that should already have blocked this session before this file was ever read. If you're reading this anyway, hooks were bypassed, disabled, or failed — halt regardless; this rule does not depend on the hook to hold.

---

> **Do not commit, push, or rewrite git history.** Stage edits (`git add`), show the diff, and stop — the human reviews and commits.

> **Use US English spelling** in code, identifiers, comments, docs, and commit/PR copy.

## 1. What This Repository Is

Mimir is **the serving layer** for the platform's reference data — `Norse.ReferenceData.Components` / `.Web.Server` / `.Worker`: Blazor components, the gRPC service host, and the background worker that keeps ISO/IANA data current. All three are pure NuGet classlibs, same as their sibling; nothing here is a standalone host either — everything is spun up from Yggdrasil's composition root. In the dependency chain it rides on Mimisbrunnr's entities and view models, and on Yggdrasil below that.

**Mimisbrunnr is the companion repository** — `Norse.ReferenceData.Data`: the entities, view models, TSV seeders, and migrations that own the schema and the canonical seed content (ISO country/currency codes, IANA time zones). The two share one namespace root (`Norse.ReferenceData.*`) deliberately — this is one bounded context split across two repositories for release-cadence reasons, not two distinct bounded contexts. Full rationale for the split lives in Mimisbrunnr's `CLAUDE.md` §1: current Ginnungagap release tooling only supports repo-scoped tags, and reference-data content churns far faster than service code, so independent cadence needed a repo boundary.

**Mimisbrunnr's entities and view models are the interop boundary this realm consumes.** A patch/minor bump on that side (new seed rows, non-breaking additions) needs no reaction here. A major bump (1.x → 2.x) means the entity or view model shape changed — that is the one event that forces this repo to update. Routine data-freshness work (pulling the next IANA tzdata release, a new ISO currency) is `Worker`'s job at runtime, against whatever schema Mimisbrunnr currently publishes — it is not a Mimisbrunnr release event, and it does not require a Mimir release either unless the refresh logic itself changes.

Named for the being renowned for wisdom, beheaded in the Æsir-Vanir war yet still carried and consulted by Odin for counsel wherever his head was taken. The story is the function: nobody needs the well itself (Mimisbrunnr) to get an answer, they need Mímir's head, wherever it's carried — exactly what `Components`, `Web.Server`, and `Worker` do against Mimisbrunnr's data. This pair is also the platform's reference-data template — see Mimisbrunnr's `CLAUDE.md` for the full framing.

This repository is currently a bare shell (LICENSE only) — no specs have converged here yet. Before writing any code: brainstorm → spec → plan, recorded in `../Glitnir/docs/Mimir/`, per the org's spec-first discipline. Do not scaffold a project structure ahead of a converged spec. When that plan is written, its REQUIRED SUB-SKILL line names `superpowers:subagent-driven-development` as the default (not a recommendation among equals — `executing-plans` is the narrow fallback for separate-session review checkpoints) paired with `superpowers:test-driven-development` — implementation here is subagent-orchestrated and test-driven, never one without the other (`../Glitnir/CLAUDE.md` §2.8).

See `../Bifrost/CLAUDE.md` (§2 The Naming Model) and `../Glitnir/CLAUDE.md` (§3 Bounded Context Map) for the full realm table and how Mimir fits the rest of the cosmos.
