using System.Collections.Generic;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>Helpers para montar geometria simples (placeholder low-poly) e materiais com cache.</summary>
    public static class Gfx
    {
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();

        static Shader Find(params string[] names)
        {
            foreach (var n in names)
            {
                var s = Shader.Find(n);
                if (s != null) return s;
            }
            return null;
        }

        public static Material Mat(Color c, bool emissive = false)
        {
            string key = c.r.ToString("F2") + c.g.ToString("F2") + c.b.ToString("F2") + (emissive ? "e" : "l");
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;

            Shader sh = emissive
                ? Find("Universal Render Pipeline/Unlit", "Unlit/Color", "Sprites/Default")
                : Find("Universal Render Pipeline/Lit", "Standard", "Diffuse");
            m = new Material(sh);
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (!emissive)
            {
                if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.05f);
                if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.05f);
            }
            cache[key] = m;
            return m;
        }

        public static GameObject Part(Transform parent, PrimitiveType type, Vector3 localPos, Vector3 scale,
            Color color, bool emissive = false, bool collider = false, string name = null)
        {
            var g = GameObject.CreatePrimitive(type);
            if (name != null) g.name = name;
            if (!collider)
            {
                var col = g.GetComponent<Collider>();
                if (col != null) Object.Destroy(col);
            }
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(color, emissive);
            return g;
        }

        public static Light Lamp(Transform parent, Vector3 localPos, Color color, float range, float intensity)
        {
            var g = new GameObject("Luz");
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            var l = g.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            return l;
        }

        public static Light Spot(Transform parent, Vector3 localPos, Vector3 euler, Color color, float range, float intensity, float angle)
        {
            var g = new GameObject("Farol");
            g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localRotation = Quaternion.Euler(euler);
            var l = g.AddComponent<Light>();
            l.type = LightType.Spot;
            l.color = color;
            l.range = range;
            l.intensity = intensity;
            l.spotAngle = angle;
            l.shadows = LightShadows.None;
            return l;
        }
    }
}
