# unity-cubes

A six-box take on Etienne de Crecy's **Beats 'n' Cubes** live show (Transmusicales, Rennes, 2007), built in Unity 6 and played against a separate track. Split out of [unity-visuals](https://github.com/allenbina/unity-visuals), where it began as the Cube world; here it is its own project.

- **One box per screen.** Phases that fit one box (dim red, flood and line, trace, reveal, look down, tumble, dissolve, build, breathe) show exactly one plain box per screen with nothing inside it; bars of light and a rising fill carry the behavior. Four phases (cells play, slide, extrude, subdivide) need the inner grid and still draw it; they would want more virtual space, e.g. continuing off the left and right.
- **One box per panel, always.** Six panels in a 3x2 grid (boxes 1 2 3 on top, 4 5 6 below; the DJ stands in front of 5, nothing special is done for that). Each panel has its own camera looking straight at its own box, so all six are seen identically. Each cube is a bold outer frame divided into a 3x3x3 grid of thinner glowing boxes, with bright dots at the joints and an open center column where the DJ stood, in a black void. The cubes turn, tilt, tumble and zoom in place. All six boxes move as a single virtual object, in lockstep (same orientation, tumble, zoom and brightness). **Edge to edge by default:** every box is stretched so its front edges are exactly the edges of its screen. Effects deliberately bring the edges inside (tilt wobbles, the tumble, pull-backs) and every loop returns to edge-to-edge.
- **Phases from the reference.** Each phase follows a scene from the show. The reference notes are in [`docs/beats-n-cubes-scenes.md`](docs/beats-n-cubes-scenes.md); background research is in [`docs/beats-n-cubes-research.md`](docs/beats-n-cubes-research.md).
- **Loopable, never dark.** Each phase is a cycle, not a timeline: it repeats every 16 beats (24 for the subdivision phase) and never ends or fades to black. Phases that used to fade out now breathe between full and sparse with the frames always visible; the reveal flash returns every loop; the tumble turns exactly once per loop.
- **Beat-driven.** Everything is a function of one beat clock. Type a BPM, press Sync on a downbeat of the track you are playing, and it stays in time.

Tested on Unity **6000.0.85f2** (URP 17.0.4, HDR, bloom). No third-party assets.

## Running it

1. Install Unity Hub and Unity 6000.0.85f2 (Unity Personal is fine).
2. Hub > Add > select this folder. The first open creates the URP assets for you.
3. Press Play in any empty scene (the project builds itself at Play time). Maximize the Game view on the second monitor.

## Controls

Buttons and keys call the same methods in `VisualActions`, so MIDI or other input can drive them later.

| Key | Control | What it does |
|---|---|---|
| (field) | BPM | Type a tempo, Enter. |
| Space | Sync | Press on the track's downbeat. |
| B | Beat | Manual pulse. |
| A | Accent | A bar of light sweeps across the six panels. |
| D | Drop | Floods the cubes white. |
| Up / Down | Build | Brighter lines and more camera drift (also a slider). |
| ] and [ | Next / Back | Step through the phases. |
| 1-9, 0 | Jump | Phases 1-10. Hold Shift for 11-20, Alt for 21-30. Also a row of numbered buttons in the strip. |
| R | Reset | Back to phase 1. |

## Layout

- `Assets/Scripts/CubeWorld.cs` the world: cubes, cameras, phases.
- `Assets/Shaders/DjBoxEdge.shader` the glowing-edge box (edges, joints, optional fill, widths in world units).
- `Assets/Scripts/BeatClock.cs`, `VisualActions.cs`, `ControlStrip.cs`, `PostFx.cs`, `Bootstrap.cs` the shared plumbing.
- `Assets/Editor/` URP auto-setup and the headless capture tool.

## Headless screenshot test

Renders a few frames of every phase without anyone watching (the editor must be closed):

```
Unity.exe -batchmode -projectPath . -executeMethod DjVisuals.EditorTools.CaptureTool.Run -djcapture <outDir> [-djphases "1,5,13"]
```

## Reference footage

Local reference videos live in `reference/`, which is git-ignored. Nothing from them is committed.
