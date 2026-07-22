# CLAUDE.md — Mímir (`Norse.ReferenceData.Components` / `.Web.Server` / `.Worker`)

## 0. Wrong Root — Halt

Session root must be **Bifröst**, not this repo directly — org-wide settings (`superpowers`, permission rules) only apply from the actual root, and Claude Code never merges a submodule's own `.claude/settings.json` into a parent-launched session. If `claude` was run from inside **Mímir**, stop: don't read further, don't propose changes, don't run anything — tell the user to `cd ../Bifrost` and start there. (This repo's `.claude/settings.json` carries a `SessionStart` hook meant to block this before you ever see this file; if you're reading this anyway, the hook was bypassed, disabled, or failed — halt regardless.)

> **Do not commit, push, or rewrite git history** — stage (`git add`), show the diff, stop; the human reviews and commits. This applies even when a skill's flow includes a commit step. **US English spelling** everywhere — code, comments, docs, commits.

## 1. What This Repository Is

Mímir is **the serving layer** for the platform's reference data — `Norse.ReferenceData.Components` / `.Web.Server` / `.Worker`: Blazor components, the gRPC service host, and the background worker that keeps ISO/IANA data current. All three are pure NuGet classlibs, same as their sibling; nothing here is a standalone host either — everything is spun up from Yggdrasil's composition root. In the dependency chain it rides on Mímisbrunnr's entities and view models, and on Yggdrasil below that.

**Mímisbrunnr is the companion repository** — `Norse.ReferenceData.Data`: the entities, view models, TSV seeders, and migrations that own the schema and the canonical seed content (ISO country/currency codes, IANA time zones). The two share one namespace root (`Norse.ReferenceData.*`) deliberately — this is one bounded context split across two repositories for release-cadence reasons, not two distinct bounded contexts. Full rationale for the split lives in Mímisbrunnr's `CLAUDE.md` §1: current Ginnungagap release tooling only supports repo-scoped tags, and reference-data content churns far faster than service code, so independent cadence needed a repo boundary.

**Mímisbrunnr's entities and view models are the interop boundary this realm consumes.** A patch/minor bump on that side (new seed rows, non-breaking additions) needs no reaction here. A major bump (1.x → 2.x) means the entity or view model shape changed — that is the one event that forces this repo to update. Routine data-freshness work (pulling the next IANA tzdata release, a new ISO currency) is `Worker`'s job at runtime, against whatever schema Mímisbrunnr currently publishes — it is not a Mímisbrunnr release event, and it does not require a Mímir release either unless the refresh logic itself changes.

Named for the being renowned for wisdom, beheaded in the Æsir-Vanir war yet still carried and consulted by Odin for counsel wherever his head was taken. The story is the function: nobody needs the well itself (Mímisbrunnr) to get an answer, they need Mímir's head, wherever it's carried — exactly what `Components`, `Web.Server`, and `Worker` do against Mímisbrunnr's data. This pair is also the platform's reference-data template — see Mímisbrunnr's `CLAUDE.md` for the full framing.

This repository is currently a bare shell (LICENSE only) — no specs have converged here yet. Before writing any code: brainstorm → spec → plan, recorded in `../Glitnir/docs/Mimir/`, per the org's spec-first discipline. Do not scaffold a project structure ahead of a converged spec. When that plan is written, its REQUIRED SUB-SKILL line names `superpowers:subagent-driven-development` as the default (not a recommendation among equals — `executing-plans` is the narrow fallback for separate-session review checkpoints) paired with `superpowers:test-driven-development` — implementation here is subagent-orchestrated and test-driven, never one without the other (`../Glitnir/CLAUDE.md` §2.8).

See `../Bifrost/CLAUDE.md` (§2 The Naming Model) and `../Glitnir/CLAUDE.md` (§3 Bounded Context Map) for the full realm table and how Mímir fits the rest of the cosmos.
