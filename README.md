# SampSharp.OpenMp.ColAndreas

Managed C# bindings for [ColAndreas](https://github.com/Pottus/ColAndreas) on open.mp x64,
for gamemodes running on the SampSharp open.mp host. ColAndreas keeps a Bullet-physics copy
of the GTA: San Andreas static collision world server-side, so the server can answer
questions the game engine normally only answers on the client: what is under this point,
what does this ray hit, where is the ground here.

Unlike the sibling bridges in this family, this repository is **managed only**. There is no
native shim to build: the bindings P/Invoke the `CASharp_*` exports of `colandreas.dll`
directly.

## About this fork

This is a fork of [VS-RP/SampSharp.OpenMp.ColAndreas](https://github.com/VS-RP/SampSharp.OpenMp.ColAndreas). It adds:

| | VS-RP's repository | This fork |
|---|---|---|
| Install | Build from source, with SampSharp checked out next to it | The NuGet package `SampSharp.OpenMp.ColAndreas` |
| SampSharp | Referenced as projects from that checkout | The `SampSharp.OpenMp.Core` and `SampSharp.OpenMp.Entities` NuGet packages |
| Model id of a ray hit | Not returned (`RayCastLine` returns only `bool`) | `RayCastLineModel`, which returns the model id, 0 on a miss. For example, water is `ColAndreasConstants.WaterObject` (20000) |
| Pawn include helpers | None | `IsPlayerFacingWater` and `WaterObject`, ported from `colandreas.inc` |

Namespaces and method names are unchanged, so code written for VS-RP's version works as is.

## Install

```bash
dotnet add package SampSharp.OpenMp.ColAndreas --prerelease
```

The package depends on SampSharp 1.0 prereleases, so its versions are previews too.

## Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│  C# gamemode                                                         │
│     IColAndreasService                                               │
└──────────────────────────────────────────────────────────────────────┘
                               │   P/Invoke (CASharp_* exports)
                               ▼
┌──────────────────────────────────────────────────────────────────────┐
│  colandreas.dll  (open.mp component, Bullet physics + .cadb)         │
└──────────────────────────────────────────────────────────────────────┘
```

## Runtime dependencies

| Component            | Where from                             |
|----------------------|----------------------------------------|
| `colandreas.dll`/`.so` | ColAndreas open.mp build              |
| `SampSharp.dll`      | `SampSharp/src/sampsharp-component/`   |
| .NET 10 runtime      | System-wide                            |
| `ColAndreas.cadb`    | Collision database, ships with the plugin |

`IsDataLoaded` reports whether the `.cadb` was found and parsed; `IsInitialized` reports
whether `Init()` has built the dynamics world. Call `Init()` once at startup. Everything
else returns garbage until you do.

## Wiring

```csharp
services.AddColAndreas();
```

## Surface

- **Ray casts:** `RayCastLine` and variants returning the hit point, the hit model id
  (`RayCastLineModel`), the collision model id, the surface normal, the reflection vector,
  or the hit object's rotation
- **Water:** `ColAndreasConstants.WaterObject`, the model id a ray reports on water, and
  `colAndreas.IsPlayerFacingWater(player)`, a port of the include's `CA_IsPlayerFacingWater`
- **Ground height:** `FindZ_For2DCoord` / `FindZ_For3DCoord`, casting from `+700` down to
  `-1000` by default: high enough to clear the LV and SF airports, low enough to catch
  underwater terrain
- **Collision objects:** create, destroy, move, rotate and tag user objects in the
  physics world, independent of the visible objects players see
- **Building removal:** `RemoveBuilding` / `RestoreBuilding`, to keep the collision world
  in step with `RemoveBuildingForPlayer` on the client
- **Model geometry:** bounding sphere and bounding box per model id, plus
  `ContactTest` for "would this model at this position intersect anything"
- **Math helpers:** `EulerToQuat` / `QuatToEuler`

Method names deliberately follow the legacy x86 `SampSharp.ColAndreas` binding, including
the unidiomatic `FindZ_For2DCoord`, so gamemode code can move to open.mp x64 without a
rename pass.

## Building

```bash
dotnet build SampSharp.OpenMp.ColAndreas.csproj
```

SampSharp comes from NuGet, so no other repository needs to be checked out.

## Releases

`.github/workflows/release.yml` publishes the NuGet package `SampSharp.OpenMp.ColAndreas`
to nuget.org, with NuGet trusted publishing:

| When | Version |
|---|---|
| Every push to `main` | `1.0.0-preview.<year>.<week>.<n>` |
| Every tag | The tag's version |

`colandreas.dll` itself isn't part of this repository; it comes from the ColAndreas open.mp build.

## License

Apache-2.0, see [LICENSE](LICENSE). That covers the bindings in this
repository; ColAndreas itself is licensed separately by its authors.

---

Powered by [vs-rp.org](https://vs-rp.org)
