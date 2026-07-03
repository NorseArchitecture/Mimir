# Mímir

> Mímir — beheaded in the Æsir-Vanir war, yet still carried and consulted by Odin for counsel.

The serving layer for the Norse Architecture's reference data — **`Norse.ReferenceData.Components`**, **`.Web.Server`**, and **`.Worker`**: Blazor components, the gRPC service host, and the background worker that keeps ISO/IANA data current. Nobody needs the well itself to get an answer — they need Mímir's head, wherever it's carried, which is exactly what this realm does against [Mímisbrunnr](https://github.com/NorseArchitecture/Mimisbrunnr)'s data. In the dependency chain it rides on Mímisbrunnr's entities and view models, and on Yggdrasil below that.

## Status

This realm is currently a bare shell — no code, no specs converged yet. Design happens first: brainstorm → spec → plan, recorded in Glitnir's `docs/Mimir/`, before any project is scaffolded here.

## Why two repos

Mímisbrunnr and Mímir are one bounded context split across two repositories for a specific, verified reason: reference-data content (IANA reissuing time zone data, ISO adding or redenominating currencies) changes far more often than the service and component code that serves it, and the platform's release tooling only supports repo-scoped tags — packing and publishing happen for an entire repo at once, not per project. Splitting the repository is what lets `Data` cut a release without dragging `Components`/`Web.Server`/`Worker` along, and vice versa. This pair is a template for anyone whose own reference data has the same shape — not a pattern the platform applies by default.

## The cosmos

Mímir is one realm of the [Norse Architecture](https://github.com/NorseArchitecture). The whole platform composes at [Bifröst](https://github.com/NorseArchitecture/Bifrost) — clone once, cross the bridge, and every session starts there so decisions get brainstormed across the entire landscape, not in isolation. Every design is tried in [Glitnir](https://github.com/NorseArchitecture/Glitnir), the design court, before code is forged here; this realm's specs and plans will live in the court's [docs/Mimir/](https://github.com/NorseArchitecture/Glitnir/tree/master/docs/Mimir) once they converge.
