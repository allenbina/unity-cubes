using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DjVisuals
{
    /// <summary>
    /// Builds the whole experiment at Play time, so any empty scene works:
    /// beat clock -> actions -> wide render target -> cameras + post-processing -> the cube world -> panels and controls.
    /// The cube world adds one camera per screen itself; this is the shared plumbing.
    /// </summary>
    public static class Bootstrap
    {
        const int PanelCount = 3;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Start()
        {
            // Silence whatever the default scene ships with.
            foreach (var c in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) c.enabled = false;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) l.enabled = false;

            var root = new GameObject("DjCubes");

            var clock = root.AddComponent<BeatClock>();
            var actions = root.AddComponent<VisualActions>();
            actions.Clock = clock;

            // One wide render, cropped into one window per screen by the control strip.
            var rt = new RenderTexture(1280 * PanelCount, 720, 24, RenderTextureFormat.ARGB32) { name = "WideView" };
            rt.Create();

            var camGo = new GameObject("MainCamera");
            camGo.transform.SetParent(root.transform);
            var cam = camGo.AddComponent<Camera>();
            cam.targetTexture = rt;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 600f;
            cam.allowHDR = true;
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;

            var fx = PostFx.Create(root.transform);

            var world = new GameObject("CubeWorld").AddComponent<CubeWorld>();
            world.transform.SetParent(root.transform);
            world.Init(actions, cam, fx);

            // Game view needs a camera that renders to screen or it shows a warning; this one just clears to black.
            var screenCam = new GameObject("ScreenClearCamera").AddComponent<Camera>();
            screenCam.transform.SetParent(root.transform);
            screenCam.clearFlags = CameraClearFlags.SolidColor;
            screenCam.backgroundColor = new Color(0.03f, 0.03f, 0.04f);
            screenCam.cullingMask = 0;
            screenCam.depth = -100;

            var strip = root.AddComponent<ControlStrip>();
            strip.Actions = actions;
            strip.Wide = rt;
            strip.PanelCount = PanelCount;

            var capDir = AutoCapture.ArgDir();
            if (capDir != null)
            {
                var cap = root.AddComponent<AutoCapture>();
                cap.Actions = actions;
                cap.Wide = rt;
                cap.OutDir = capDir;
            }
        }
    }
}
