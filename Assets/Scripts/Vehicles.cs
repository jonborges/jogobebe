using UnityEngine;

namespace JogoBebe
{
    /// <summary>Carros e motos placeholder. Frente = +Z, origem no chao, centro do veiculo.
    /// Prefabs em Resources/Models/&lt;modelKey&gt; (ex.: CarroPai) substituem o placeholder.</summary>
    public static class Vehicles
    {
        static readonly Color[] Palette =
        {
            new Color(.6f, .1f, .1f), new Color(.1f, .2f, .5f), new Color(.7f, .7f, .7f),
            new Color(.15f, .15f, .15f), new Color(.8f, .75f, .5f), new Color(.2f, .4f, .25f)
        };

        public static Color RandomColor() { return Palette[Random.Range(0, Palette.Length)]; }

        public static GameObject Car(Color body, GameObject driver, bool lights, string modelKey = null)
        {
            if (modelKey != null)
            {
                var prefab = Resources.Load<GameObject>("Models/" + modelKey);
                if (prefab != null)
                {
                    var inst = Object.Instantiate(prefab);
                    inst.name = modelKey;
                    if (driver != null) Object.Destroy(driver);
                    return inst;
                }
            }

            var root = new GameObject("Carro");
            var t = root.transform;
            var C = PrimitiveType.Cube;
            Color dark = new Color(.06f, .06f, .07f);

            Gfx.Part(t, C, new Vector3(0, .6f, 0), new Vector3(1.8f, .6f, 4.2f), body);
            Gfx.Part(t, C, new Vector3(0, .95f, 1.4f), new Vector3(1.7f, .15f, 1.2f), body);       // capo
            Gfx.Part(t, C, new Vector3(0, .45f, 2.1f), new Vector3(1.9f, .2f, .15f), dark);        // parachoque
            Gfx.Part(t, C, new Vector3(0, .45f, -2.1f), new Vector3(1.9f, .2f, .15f), dark);

            foreach (float x in new[] { -.95f, .95f })
                foreach (float z in new[] { -1.35f, 1.35f })
                {
                    var w = Gfx.Part(t, PrimitiveType.Cylinder, new Vector3(x, .35f, z), new Vector3(.7f, .1f, .7f), dark);
                    w.transform.localRotation = Quaternion.Euler(0, 0, 90f);
                }

            // cabine aberta (pilares + teto) para o motorista aparecer
            foreach (float x in new[] { -.82f, .82f })
                foreach (float z in new[] { .95f, -1.15f })
                    Gfx.Part(t, C, new Vector3(x, 1.4f, z), new Vector3(.08f, .9f, .08f), body);
            Gfx.Part(t, C, new Vector3(0, 1.87f, -.1f), new Vector3(1.7f, .08f, 2.2f), body);
            Gfx.Part(t, C, new Vector3(0, 1.4f, -1.15f), new Vector3(1.64f, .85f, .06f), body);

            Color head = new Color(1f, .95f, .7f);
            Gfx.Part(t, C, new Vector3(-.6f, .65f, 2.12f), new Vector3(.35f, .2f, .08f), head, true);
            Gfx.Part(t, C, new Vector3(.6f, .65f, 2.12f), new Vector3(.35f, .2f, .08f), head, true);
            Color tail = new Color(.9f, .05f, .05f);
            Gfx.Part(t, C, new Vector3(-.6f, .65f, -2.12f), new Vector3(.35f, .15f, .06f), tail, true);
            Gfx.Part(t, C, new Vector3(.6f, .65f, -2.12f), new Vector3(.35f, .15f, .06f), tail, true);

            if (lights) Gfx.Spot(t, new Vector3(0, .8f, 2.1f), new Vector3(5f, 0, 0), new Color(1f, .93f, .75f), 14f, 2f, 55f);

            if (driver != null)
            {
                driver.transform.SetParent(t, false);
                driver.transform.localPosition = new Vector3(-.45f, .2f, -.05f);
                driver.transform.localScale = Vector3.one * .85f;
            }
            return root;
        }

        public static GameObject Bike(Color body)
        {
            var root = new GameObject("Moto");
            var t = root.transform;
            var C = PrimitiveType.Cube;
            Color dark = new Color(.06f, .06f, .07f);

            Gfx.Part(t, C, new Vector3(0, .55f, 0), new Vector3(.25f, .35f, 1.5f), body);
            foreach (float z in new[] { -.65f, .65f })
            {
                var w = Gfx.Part(t, PrimitiveType.Cylinder, new Vector3(0, .32f, z), new Vector3(.64f, .06f, .64f), dark);
                w.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            }
            Gfx.Part(t, C, new Vector3(0, .8f, .55f), new Vector3(.6f, .06f, .06f), dark);                 // guidao
            Gfx.Part(t, C, new Vector3(0, .62f, .85f), new Vector3(.2f, .2f, .08f), new Color(1f, .95f, .7f), true);
            Gfx.Part(t, C, new Vector3(0, .62f, -.85f), new Vector3(.2f, .12f, .06f), new Color(.9f, .05f, .05f), true);
            Gfx.Spot(t, new Vector3(0, .7f, .9f), new Vector3(4f, 0, 0), new Color(1f, .93f, .75f), 11f, 1.5f, 45f);

            var rider = CharacterFactory.Build("Motoqueiro", CharacterFactory.Motorista());
            rider.transform.SetParent(t, false);
            rider.transform.localPosition = new Vector3(0, .05f, -.15f);
            rider.transform.localScale = Vector3.one * .75f;
            Gfx.Part(t, PrimitiveType.Sphere, new Vector3(0, 1.22f, -.12f), Vector3.one * .3f, new Color(.08f, .08f, .1f));
            return root;
        }
    }
}
