# Mímir

> Mímir — beheaded in the Æsir-Vanir war, yet still carried and consulted by Odin for counsel.

![Mímir — beheaded in the Æsir-Vanir war, yet still carried and consulted by Odin for counsel](https://github.com/user-attachments/assets/f47c4998-79d6-4df2-a615-159483164c9b "Mímir — the severed head Odin still consults for counsel")

*Image credit: [@norsemythologyclips](https://www.instagram.com/norsemythologyclips/) — go follow them.*

The serving layer for the Norse Architecture's reference data — **`Norse.Reference.Components`**, **`.Web.Server`**, and **`.Worker`**: Blazor components, the gRPC service host, and the background worker that keeps ISO/IANA data current. Nobody needs the well itself to get an answer — they need Mímir's head, wherever it's carried, which is exactly what this realm does against [Mímisbrunnr](https://github.com/NorseArchitecture/Mimisbrunnr)'s data. In the dependency chain it rides on Mímisbrunnr's entities and view models, and on Yggdrasil below that.

## Status

Mímir is the serving layer: **`Reference.Contracts`** (the wire records — `CountryRequest`/`CountryResponse`/`IReferenceService`) and **`Reference.Web.Server`** (the gRPC implementation, bound into Yggdrasil's hosting process). The generated reference surface — the `IsoCountryCode` enum, the `Iso3166` dataset, and `ReferenceNamespaces` — now generates in [Mímisbrunnr](https://github.com/NorseArchitecture/Mimisbrunnr) (`Reference.Data.Primitives`/`.Namespaces`) and arrives here by reference instead of by generation. `Reference.Seeds` is deleted; the canonical seed content lives in Mímisbrunnr.

## Why two repos

Mímisbrunnr and Mímir are one bounded context split across two repositories for a specific, verified reason: reference-data content (IANA reissuing time zone data, ISO adding or redenominating currencies) changes far more often than the service and component code that serves it, and the platform's release tooling only supports repo-scoped tags — packing and publishing happen for an entire repo at once, not per project. Splitting the repository is what lets `Data` cut a release without dragging `Components`/`Web.Server`/`Worker` along, and vice versa. This pair is a template for anyone whose own reference data has the same shape — not a pattern the platform applies by default.

## The cosmos

Mímir is one realm of the [Norse Architecture](https://github.com/NorseArchitecture). The whole platform composes at [Bifröst](https://github.com/NorseArchitecture/Bifrost) — clone once, cross the bridge, and every session starts there so decisions get brainstormed across the entire landscape, not in isolation. Every design is tried in [Glitnir](https://github.com/NorseArchitecture/Glitnir), the design court, before code is forged here; this realm's specs and plans will live in the court's [docs/Mímir/](https://github.com/NorseArchitecture/Glitnir/tree/master/docs/Mimir) once they converge.

## Soundtrack: War of the Gods
[![Soundtrack: War of the Gods](https://img.youtube.com/vi/FVAQQujgSxQ/maxresdefault.jpg)](https://www.youtube.com/watch?v=FVAQQujgSxQ)
