using UnityEngine;

namespace JogoBebe
{
    public class CharSpec
    {
        public Color skin = new Color(.86f, .66f, .5f);
        public Color top = new Color(.5f, .5f, .5f);
        public Color bottom = new Color(.2f, .3f, .55f);
        public Color shoes = new Color(.1f, .1f, .1f);
        public Color hair = new Color(.3f, .2f, .1f);
        public int hairStyle = 1; // 0 careca, 1 curto, 2 rabo de cavalo
        public bool skirt, bikini, boots, beard;
    }

    /// <summary>
    /// Constroi personagens blocky de placeholder. Se existir um prefab em Resources/Models/&lt;chave&gt;
    /// (ex.: Protagonista, Rebeca, Pai), ele e usado no lugar. Pes na origem, frente = +Z, altura ~1,7 m.
    /// </summary>
    public static class CharacterFactory
    {
        public static CharSpec Protagonista()
        {
            return new CharSpec
            {
                skin = new Color(.9f, .68f, .46f), top = new Color(.5f, .32f, .16f),
                bottom = new Color(.22f, .36f, .62f), hair = new Color(.38f, .24f, .1f),
                hairStyle = 2, skirt = true, bikini = true, boots = true
            };
        }

        public static CharSpec Rebeca()
        {
            return new CharSpec
            {
                skin = new Color(.78f, .55f, .4f), top = new Color(.9f, .3f, .6f),
                bottom = new Color(.85f, .85f, .88f), shoes = new Color(.85f, .85f, .85f),
                hair = new Color(.07f, .05f, .05f), hairStyle = 1, skirt = true
            };
        }

        public static CharSpec Pai()
        {
            return new CharSpec
            {
                skin = new Color(.85f, .66f, .52f), top = new Color(.45f, .45f, .46f),
                bottom = new Color(.25f, .32f, .5f), shoes = new Color(.12f, .1f, .08f),
                hair = new Color(.35f, .24f, .14f), hairStyle = 0, beard = true
            };
        }

        public static CharSpec Motorista()
        {
            return new CharSpec
            {
                skin = Color.Lerp(new Color(.95f, .75f, .6f), new Color(.45f, .3f, .2f), Random.value),
                top = new Color(Random.value * .5f, Random.value * .5f, Random.value * .5f),
                bottom = new Color(.2f, .2f, .25f),
                hair = new Color(.15f, .1f, .08f), hairStyle = Random.value < .3f ? 0 : 1
            };
        }

        public static GameObject Build(string key, CharSpec s)
        {
            var prefab = LoadModel(key);
            if (prefab != null)
            {
                var prefabRoot = new GameObject(key);
                var inst = Object.Instantiate(prefab, prefabRoot.transform, false);
                inst.name = "ImportedModel";

                if (key == "WOMAN" || key == "REBECA") FitHumanHeight(inst.transform, 1.7f);

                return prefabRoot;
            }

            var root = new GameObject(key);
            var t = root.transform;
            var P = PrimitiveType.Cube;

            Color legCol = s.skirt ? Color.Lerp(s.skin, Color.black, .45f) : s.bottom;
            Gfx.Part(t, P, new Vector3(-.1f, .45f, 0), new Vector3(.15f, .8f, .17f), legCol);
            Gfx.Part(t, P, new Vector3(.1f, .45f, 0), new Vector3(.15f, .8f, .17f), legCol);

            if (s.boots)
            {
                foreach (float x in new[] { -.1f, .1f })
                {
                    Gfx.Part(t, P, new Vector3(x, .23f, .01f), new Vector3(.19f, .42f, .22f), s.shoes);
                    Gfx.Part(t, P, new Vector3(x, .05f, .03f), new Vector3(.21f, .1f, .28f), s.shoes);
                }
            }
            else
            {
                foreach (float x in new[] { -.1f, .1f })
                    Gfx.Part(t, P, new Vector3(x, .04f, .04f), new Vector3(.17f, .08f, .28f), s.shoes);
            }

            if (s.skirt) Gfx.Part(t, P, new Vector3(0, .88f, 0), new Vector3(.42f, .18f, .28f), s.bottom);

            Gfx.Part(t, P, new Vector3(0, 1.12f, 0), new Vector3(.44f, .55f, .24f), s.bikini ? s.skin : s.top);
            if (s.bikini) Gfx.Part(t, P, new Vector3(0, 1.28f, .01f), new Vector3(.44f, .22f, .26f), s.top);

            foreach (float x in new[] { -.28f, .28f })
            {
                Gfx.Part(t, P, new Vector3(x, 1.1f, 0), new Vector3(.1f, .55f, .11f), s.skin);
                if (!s.bikini) Gfx.Part(t, P, new Vector3(x, 1.3f, 0), new Vector3(.12f, .2f, .13f), s.top);
            }

            Gfx.Part(t, PrimitiveType.Sphere, new Vector3(0, 1.55f, 0), new Vector3(.24f, .28f, .25f), s.skin);
            Gfx.Part(t, P, new Vector3(-.05f, 1.57f, .12f), new Vector3(.03f, .03f, .02f), new Color(.05f, .05f, .08f));
            Gfx.Part(t, P, new Vector3(.05f, 1.57f, .12f), new Vector3(.03f, .03f, .02f), new Color(.05f, .05f, .08f));

            if (s.hairStyle >= 1)
                Gfx.Part(t, PrimitiveType.Sphere, new Vector3(0, 1.62f, -.015f), new Vector3(.27f, .2f, .28f), s.hair);
            if (s.hairStyle == 2)
            {
                var pony = Gfx.Part(t, P, new Vector3(0, 1.55f, -.17f), new Vector3(.08f, .3f, .08f), s.hair);
                pony.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
                Gfx.Part(t, P, new Vector3(0, 1.66f, -.14f), new Vector3(.1f, .05f, .1f), new Color(.9f, .2f, .6f));
            }
            if (s.beard) Gfx.Part(t, P, new Vector3(0, 1.47f, .1f), new Vector3(.18f, .1f, .1f), s.hair);

            return root;
        }

        /// <summary>Procura o modelo em varios lugares: prefab gerado pelo menu, prefab solto em Resources/Models
        /// e o proprio glTF importado dentro de Resources/Models/&lt;chave&gt;/.</summary>
        public static GameObject LoadModel(string key)
        {
            string[] paths =
            {
                "Models/Imported/" + key,
                "Models/" + key,
                "Models/" + key + "/scene",
                "Models/" + key + "/female_character/scene"
            };
            foreach (var p in paths)
            {
                var g = Resources.Load<GameObject>(p);
                if (g != null) return g;
            }
            return null;
        }

        static void FitHumanHeight(Transform model, float targetHeight)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            if (bounds.size.y < .001f) return;

            float scale = targetHeight / bounds.size.y;
            model.localScale = Vector3.one * scale;
            model.localPosition = Vector3.up * (-bounds.min.y * scale);
        }
    }
}
