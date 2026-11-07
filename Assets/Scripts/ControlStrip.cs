using System.Globalization;
using UnityEngine;

namespace DjVisuals
{
    /// <summary>
    /// Game-view output: the wide render cropped into one panel per screen, plus the control strip underneath
    /// (BPM, Sync, Beat, Accent, Drop, Build, Next/Back/Reset, and a row of numbered buttons that jump straight to a phase).
    /// Buttons and keys both call VisualActions. Uses IMGUI so it works with any input-handling setting.
    ///
    /// Keys: Space sync, B beat, A accent, D drop, ] next phase, [ previous phase, R reset, Up/Down build,
    ///       1-9 and 0 jump to phases 1-10; hold Shift for 11-20 and Alt for 21-30.
    /// </summary>
    public class ControlStrip : MonoBehaviour
    {
        public VisualActions Actions;
        public RenderTexture Wide;
        public int PanelCount = 3;

        string bpmText = "120";

        void OnGUI()
        {
            float s = Mathf.Max(1f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1));
            float W = Screen.width / s, H = Screen.height / s;

            HandleKeys();

            // --- panels: one window per screen ---
            const float margin = 8f;
            float panelW = (W - margin * (PanelCount + 1)) / PanelCount;
            float panelH = panelW * 9f / 16f;
            for (int i = 0; i < PanelCount; i++)
            {
                var r = new Rect(margin + i * (panelW + margin), margin, panelW, panelH);
                GUI.DrawTexture(new Rect(r.x - 2, r.y - 2, r.width + 4, r.height + 4), Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(0.15f, 0.15f, 0.17f), 0, 0);
                GUI.DrawTextureWithTexCoords(r, Wide, new Rect(i / (float)PanelCount, 0, 1f / PanelCount, 1));
            }

            // --- control strip ---
            float y = margin * 2 + panelH;
            GUILayout.BeginArea(new Rect(margin, y, W - margin * 2, H - y - margin));

            GUILayout.BeginHorizontal();
            GUILayout.Label("BPM", GUILayout.Width(32));
            GUI.SetNextControlName("bpm");
            bpmText = GUILayout.TextField(bpmText, 8, GUILayout.Width(64));
            if (GUI.GetNameOfFocusedControl() == "bpm" && Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                ApplyBpm();
                GUI.FocusControl(null);
                Event.current.Use();
            }
            if (GUILayout.Button("Set", GUILayout.Width(40))) { ApplyBpm(); GUI.FocusControl(null); }
            GUILayout.Space(10);
            if (GUILayout.Button("Sync (Space)", GUILayout.Width(100))) Actions.Sync();
            if (GUILayout.Button("Beat (B)", GUILayout.Width(80))) Actions.ManualBeat();
            if (GUILayout.Button("Accent (A)", GUILayout.Width(90))) Actions.FireAccent();
            if (GUILayout.Button("Drop (D)", GUILayout.Width(80))) Actions.FireDrop();
            GUILayout.Space(10);
            if (GUILayout.Button("< Back ([)", GUILayout.Width(90))) Actions.Back();
            if (GUILayout.Button("Next (])  >", GUILayout.Width(100))) Actions.Advance();
            if (GUILayout.Button("Reset (R)", GUILayout.Width(80))) Actions.DoReset();
            GUILayout.FlexibleSpace();
            DrawBeatIndicator();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Build (Up/Down)", GUILayout.Width(110));
            Actions.SetBuild(GUILayout.HorizontalSlider(Actions.Build, 0f, 1f, GUILayout.Width(300)));
            GUILayout.Label(Actions.Build.ToString("0.00"), GUILayout.Width(40));
            GUILayout.Space(20);
            GUILayout.Label(Actions.Clock.Running
                ? "Synced @ " + Actions.Clock.Bpm.ToString("0.##", CultureInfo.InvariantCulture) + " BPM"
                : "Not synced: press Sync on a downbeat");
            GUILayout.FlexibleSpace();
            if (!string.IsNullOrEmpty(Actions.Status)) GUILayout.Label(Actions.Status);
            GUILayout.EndHorizontal();

            // --- jump straight to a phase ---
            GUILayout.Space(6);
            GUILayout.Label("Jump to phase");
            int count = Mathf.Max(1, Actions.PhaseCount);
            var labels = new string[count];
            for (int i = 0; i < count; i++) labels[i] = (i + 1).ToString();
            int sel = GUILayout.SelectionGrid(Actions.Progression, labels, Mathf.Min(count, 12), GUILayout.Width(Mathf.Min(count, 12) * 46));
            if (sel != Actions.Progression) Actions.JumpTo(sel);

            GUILayout.EndArea();
        }

        void ApplyBpm()
        {
            if (double.TryParse(bpmText.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                Actions.SetBpm(v);
            bpmText = Actions.Clock.Bpm.ToString("0.##", CultureInfo.InvariantCulture);
        }

        void DrawBeatIndicator()
        {
            // Four cells, one per beat in the bar; downbeat is brighter. A thin bar shows phase within the beat.
            var clock = Actions.Clock;
            var area = GUILayoutUtility.GetRect(150, 24, GUILayout.Width(150));
            for (int i = 0; i < BeatClock.BeatsPerBar; i++)
            {
                bool on = clock.Running && clock.BeatInBar == i;
                var c = new Rect(area.x + i * 36, area.y, 32, 16);
                Color col = on ? (i == 0 ? new Color(0.4f, 0.8f, 1f) : new Color(0.2f, 0.5f, 1f)) : new Color(0.18f, 0.2f, 0.26f);
                GUI.DrawTexture(c, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, col, 0, 0);
            }
            GUI.DrawTexture(new Rect(area.x, area.y + 19, 144 * (clock.Running ? clock.BeatPhase : 0f), 3),
                Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(0.6f, 0.8f, 1f), 0, 0);
        }

        void HandleKeys()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            if (GUI.GetNameOfFocusedControl() == "bpm") return; // typing a tempo

            int digit = DigitIndex(e.keyCode);
            if (digit >= 0)
            {
                int offset = e.alt ? 20 : (e.shift ? 10 : 0);
                if (digit + offset < Actions.PhaseCount) Actions.JumpTo(digit + offset);
                e.Use();
                return;
            }
            switch (e.keyCode)
            {
                case KeyCode.Space: Actions.Sync(); break;
                case KeyCode.B: Actions.ManualBeat(); break;
                case KeyCode.A: Actions.FireAccent(); break;
                case KeyCode.D: Actions.FireDrop(); break;
                case KeyCode.RightBracket: Actions.Advance(); break;
                case KeyCode.LeftBracket: Actions.Back(); break;
                case KeyCode.P: Actions.Advance(); break;
                case KeyCode.R: Actions.DoReset(); break;
                case KeyCode.UpArrow: Actions.NudgeBuild(0.05f); break;
                case KeyCode.DownArrow: Actions.NudgeBuild(-0.05f); break;
                default: return;
            }
            e.Use();
        }

        /// <summary>1..9 map to 0..8 and 0 maps to 9.</summary>
        static int DigitIndex(KeyCode k)
        {
            if (k >= KeyCode.Alpha1 && k <= KeyCode.Alpha9) return k - KeyCode.Alpha1;
            if (k == KeyCode.Alpha0) return 9;
            return -1;
        }
    }
}
