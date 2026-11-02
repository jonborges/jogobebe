using UnityEngine;

namespace JogoBebe
{
    /// <summary>Renderiza em Full HD e suaviza a imagem para telas de alta resolucao.</summary>
    [RequireComponent(typeof(Camera))]
    public class PS2Render : MonoBehaviour
    {
        public int height = 1080;

        Camera cam;
        RenderTexture rt;
        int lastW, lastH;
        GameObject displayCam;

        void OnEnable()
        {
            cam = GetComponent<Camera>();

            // camera "vazia" so para a tela nao ficar sem camera renderizando
            displayCam = new GameObject("DisplayCam");
            var d = displayCam.AddComponent<Camera>();
            d.clearFlags = CameraClearFlags.SolidColor;
            d.backgroundColor = Color.black;
            d.cullingMask = 0;
            d.depth = -100;

            Rebuild();
        }

        void Rebuild()
        {
            if (rt != null) { cam.targetTexture = null; rt.Release(); Destroy(rt); }
            lastW = Screen.width; lastH = Screen.height;
            int w = Mathf.Max(64, Mathf.RoundToInt(height * (float)lastW / Mathf.Max(1, lastH)));
            rt = new RenderTexture(w, height, 24) { filterMode = FilterMode.Bilinear, antiAliasing = 4 };
            rt.Create();
            cam.targetTexture = rt;
        }

        void Update()
        {
            if (Screen.width != lastW || Screen.height != lastH) Rebuild();
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || rt == null) return;
            GUI.depth = 10;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), rt, ScaleMode.StretchToFill, false);
        }

        void OnDisable()
        {
            if (cam != null) cam.targetTexture = null;
            if (rt != null) { rt.Release(); Destroy(rt); }
            if (displayCam != null) Destroy(displayCam);
        }
    }
}
