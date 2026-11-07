using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DjVisuals
{
    /// <summary>
    /// Test harness (only active when launched with -djcapture &lt;dir&gt;): runs a scripted timeline
    /// (sync, jump through phases) and saves PNGs of the wide render, then quits the editor.
    ///   Unity.exe -batchmode -projectPath . -executeMethod DjVisuals.EditorTools.CaptureTool.Run -djcapture &lt;dir&gt;
    /// Optional: -djphases "1,5,9" shoots just those phases (1-based); default is every phase.
    /// </summary>
    public class AutoCapture : MonoBehaviour
    {
        public VisualActions Actions;
        public RenderTexture Wide;
        public string OutDir;

        struct Step { public float t; public string kind; public string arg; }
        Step[] steps;
        int next;
        float t0 = -1f;

        public static string ArgDir()
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == "-djcapture") return a[i + 1];
            return null;
        }

        static string Arg(string name)
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
            return null;
        }

        void Start()
        {
            Directory.CreateDirectory(OutDir);
            // How many phases to walk through, and which. Seconds per phase: 7 (14 beats at 120 BPM).
            var list = new List<Step>();
            list.Add(new Step { t = 0.3f, kind = "sync" });
            var wanted = new List<int>();
            var spec = Arg("-djphases");
            if (spec != null) foreach (var p in spec.Split(',')) { if (int.TryParse(p, out int v)) wanted.Add(v - 1); }
            else for (int p = 0; p < Actions.PhaseCount; p++) wanted.Add(p);
            float t = 0.5f;
            foreach (var p in wanted)
            {
                list.Add(new Step { t = t, kind = "jump", arg = p.ToString() });
                list.Add(new Step { t = t + 2.0f, kind = "shot", arg = "p" + (p + 1).ToString("00") + "_a" });
                list.Add(new Step { t = t + 5.5f, kind = "shot", arg = "p" + (p + 1).ToString("00") + "_b" });
                if (Arg("-djlong") != null)   // keep going across more than one loop (2 s = 4 beats at 120 BPM)
                {
                    list.Add(new Step { t = t + 9.5f, kind = "shot", arg = "p" + (p + 1).ToString("00") + "_c" });
                    list.Add(new Step { t = t + 13.5f, kind = "shot", arg = "p" + (p + 1).ToString("00") + "_d" });
                    list.Add(new Step { t = t + 17.5f, kind = "shot", arg = "p" + (p + 1).ToString("00") + "_e" });
                    t += 18.5f;
                }
                else t += 7f;
            }
            list.Add(new Step { t = t + 0.5f, kind = "quit" });
            steps = list.ToArray();
        }

        void Update()
        {
            // Shader compilation stalls the first frames in batch mode; start the clock once frames flow.
            if (t0 < 0f)
            {
                if (Time.frameCount < 90 || Time.deltaTime > 0.1f) return;
                t0 = Time.realtimeSinceStartup;
            }
            float t = Time.realtimeSinceStartup - t0;
            while (next < steps.Length && t >= steps[next].t)
            {
                var s = steps[next++];
                switch (s.kind)
                {
                    case "sync": Actions.SetBpm(120); Actions.Sync(); break;
                    case "jump": Actions.JumpTo(int.Parse(s.arg)); break;
                    case "accent": Actions.FireAccent(); break;
                    case "drop": Actions.FireDrop(); break;
                    case "build": Actions.SetBuild(0.8f); break;
                    case "shot": Shot(s.arg); break;
                    case "quit":
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.Exit(0);
#else
                        Application.Quit();
#endif
                        break;
                }
            }
        }

        void Shot(string name)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = Wide;
            var tex = new Texture2D(Wide.width, Wide.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, Wide.width, Wide.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(OutDir, name + ".png"), tex.EncodeToPNG());
            Destroy(tex);
            Debug.Log("[DjVisuals] captured " + name);
        }
    }
}
