using System;
using UnityEngine;

namespace DjVisuals
{
    /// <summary>
    /// The single set of actions. Keyboard, on-screen buttons, and later MIDI all call these;
    /// the cube world subscribes to the events and never cares where an action came from.
    /// </summary>
    public class VisualActions : MonoBehaviour
    {
        public BeatClock Clock;

        public float Build { get; private set; }
        public int Progression { get; private set; }   // the current phase, 0-based
        public int PhaseCount = 1;                      // set by the world

        /// <summary>One line the world shows in the control strip (the current phase name).</summary>
        public string Status = "";

        public event Action<int> Beat;          // beat index (clock) or -1 (manual pulse)
        public event Action Accent;
        public event Action Drop;
        public event Action<int> Progressed;    // the phase to show (Next, Back and Jump all raise this)
        public event Action ResetRequested;

        // Not Awake: Bootstrap assigns Clock after AddComponent, so Awake would see null.
        void Start() { Clock.OnBeat += i => Beat?.Invoke(i); }

        public void SetBpm(double bpm) => Clock.SetBpm(bpm);
        public void Sync() => Clock.Sync();
        public void ManualBeat() => Beat?.Invoke(-1);
        public void FireAccent() => Accent?.Invoke();
        public void SetBuild(float v) => Build = Mathf.Clamp01(v);
        public void NudgeBuild(float d) => SetBuild(Build + d);
        public void FireDrop() => Drop?.Invoke();

        public void Advance() => JumpTo(Progression + 1);
        public void Back() => JumpTo(Progression - 1);
        public void JumpTo(int phase)
        {
            int n = Mathf.Max(1, PhaseCount);
            Progression = ((phase % n) + n) % n;
            Progressed?.Invoke(Progression);
        }

        public void DoReset()
        {
            Build = 0;
            Progression = 0;
            ResetRequested?.Invoke();
        }
    }
}
