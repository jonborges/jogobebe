using System;
using System.Collections;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>HUD (objetivo, dica de interacao), caixa de dialogo com escolhas, fade e cartao de titulo. Tudo via OnGUI.</summary>
    public class GameUI : MonoBehaviour
    {
        public static GameUI I;
        public static string Objective = "";
        public static string TitleCard = "";
        public static int ClosedFrame = -10;
        public static bool Busy { get { return I != null && I.active; } }

        public struct Line
        {
            public string who, text;
            public Line(string who, string text) { this.who = who; this.text = text; }
        }

        static string prompt = "";
        static int promptFrame = -10;
        public static void SetPrompt(string s) { prompt = s; promptFrame = Time.frameCount; }

        public float fade;

        Line[] lines;
        string[] choices;
        Action done;
        Action<int> onChoice;
        int idx, startFrame;
        float chars;
        bool active, choosing;

        GUIStyle sName, sText, sHud, sBig, sHint;

        void Awake() { I = this; }

        public void Play(Line[] l, Action onDone = null, string[] ch = null, Action<int> onCh = null)
        {
            lines = l; idx = 0; chars = 0; done = onDone; choices = ch; onChoice = onCh;
            choosing = false; active = true; startFrame = Time.frameCount;
        }

        public IEnumerator FadeTo(float target, float dur)
        {
            float start = fade, t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                fade = Mathf.Lerp(start, target, t / dur);
                yield return null;
            }
            fade = target;
        }

        void Update()
        {
            if (!active || Time.frameCount <= startFrame) return;

            if (choosing)
            {
                for (int i = 0; i < choices.Length && i < 9; i++)
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { Pick(i); return; }
                return;
            }

            string full = lines[idx].text;
            chars += 40f * Time.deltaTime;
            bool adv = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)
                       || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0);
            if (adv)
            {
                if (chars < full.Length) chars = full.Length;
                else Next();
            }
        }

        void Next()
        {
            idx++; chars = 0;
            if (idx >= lines.Length)
            {
                if (choices != null && choices.Length > 0) { choosing = true; idx = lines.Length - 1; }
                else Close();
            }
        }

        void Close()
        {
            active = false; choosing = false; ClosedFrame = Time.frameCount;
            var cb = done; done = null;
            if (cb != null) cb();
        }

        void Pick(int i)
        {
            active = false; choosing = false; ClosedFrame = Time.frameCount;
            var oc = onChoice; onChoice = null; done = null;
            if (oc != null) oc(i);
        }

        void InitStyles()
        {
            sName = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            sText = new GUIStyle(GUI.skin.label) { fontSize = 24, wordWrap = true };
            sHud = new GUIStyle(GUI.skin.label) { fontSize = 20, wordWrap = true };
            sBig = new GUIStyle(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            sHint = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.LowerRight };
        }

        static void Shadow(Rect r, string txt, GUIStyle st, Color c)
        {
            var old = st.normal.textColor;
            st.normal.textColor = Color.black;
            GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), txt, st);
            st.normal.textColor = c;
            GUI.Label(r, txt, st);
            st.normal.textColor = old;
        }

        static void Box(Rect r, float alpha)
        {
            GUI.color = new Color(0, 0, 0, alpha);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static Color SpeakerColor(string who)
        {
            switch (who)
            {
                case "Rebeca": return new Color(.5f, .9f, 1f);
                case "Pai": return new Color(1f, .7f, .3f);
                case "Protagonista": return new Color(1f, .55f, .85f);
                default: return Color.white;
            }
        }

        void OnGUI()
        {
            if (sName == null) InitStyles();
            GUI.depth = 0;
            float s = Screen.height / 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(s, s, 1f));
            float vw = Screen.width / s;

            if (!string.IsNullOrEmpty(Objective))
                Shadow(new Rect(24, 18, 560, 90), "OBJETIVO: " + Objective, sHud, new Color(1f, .9f, .5f));

            if (!active && Time.frameCount - promptFrame <= 1 && prompt != "")
            {
                Box(new Rect(vw / 2 - 190, 600, 380, 44), .6f);
                Shadow(new Rect(vw / 2 - 190, 604, 380, 40), prompt, new GUIStyle(sHud) { alignment = TextAnchor.MiddleCenter }, Color.white);
            }

            if (active) DrawDialogue(vw);

            if (fade > .001f)
            {
                GUI.color = new Color(0, 0, 0, fade);
                GUI.DrawTexture(new Rect(0, 0, vw, 720), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            if (!string.IsNullOrEmpty(TitleCard))
                Shadow(new Rect(vw * .1f, 250, vw * .8f, 220), TitleCard, sBig, Color.white);
        }

        void DrawDialogue(float vw)
        {
            var panel = new Rect(vw * .1f, 520, vw * .8f, 180);
            Box(panel, .82f);

            if (!choosing)
            {
                var L = lines[idx];
                Shadow(new Rect(panel.x + 24, panel.y + 10, 400, 32), L.who, sName, SpeakerColor(L.who));
                int n = Mathf.Min(L.text.Length, (int)chars);
                GUI.Label(new Rect(panel.x + 24, panel.y + 48, panel.width - 48, 110), L.text.Substring(0, n), sText);
                GUI.Label(new Rect(panel.x, panel.y, panel.width - 14, panel.height - 8), "[E] continuar", sHint);
            }
            else
            {
                Shadow(new Rect(panel.x + 24, panel.y + 10, 400, 32), "Responder", sName, SpeakerColor("Protagonista"));
                for (int i = 0; i < choices.Length; i++)
                    GUI.Label(new Rect(panel.x + 24, panel.y + 52 + i * 40, panel.width - 48, 38), (i + 1) + ")  " + choices[i], sText);
            }
        }
    }
}
