using System.Collections.Generic;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>Carros e motos. Frente = +Z, origem no chao, centro do veiculo.
    /// Modelos importados: qualquer prefab/glb em Resources/Models/Cars (carros) e Resources/Models/Bikes (moto)
    /// entra sozinho no trafego e e ajustado em escala/posicao. Sem modelos, usa os placeholders de cubos.
    /// Prefabs em Resources/Models/&lt;modelKey&gt; (ex.: CarroPai) continuam substituindo o carro do pai.</summary>
    public static class Vehicles
    {
        /// <summary>Gire se algum pacote de carros vier com a frente virada (graus em Y).</summary>
        public static float ImportedCarYaw = 0f;
        public static float ImportedCarLength = 3.9f;
        public static float ImportedBikeLength = 2.0f;
        /// <summary>Chance de um carro do trafego usar um modelo importado (quando existir algum).</summary>
        public static float ImportedCarChance = .8f;

        static readonly Color[] Palette =
        {
            new Color(.6f, .1f, .1f), new Color(.1f, .2f, .5f), new Color(.7f, .7f, .7f),
            new Color(.15f, .15f, .15f), new Color(.8f, .75f, .5f), new Color(.2f, .4f, .25f)
        };

        static GameObject[] carPrefabs;
        static GameObject bikePrefab;
        static bool scanned;

        public static Color RandomColor() { return Palette[Random.Range(0, Palette.Length)]; }

        static GameObject[] Roots(GameObject[] all)
        {
            // so objetos-raiz com alguma malha (ignora sub-objetos e lixo)
            var list = new List<GameObject>();
            foreach (var g in all)
            {
                if (g == null || g.transform.parent != null) continue;
                if (g.GetComponentInChildren<Renderer>(true) == null) continue;
                list.Add(g);
            }
            return list.ToArray();
        }

        static void Scan()
        {
            if (scanned) return;
            scanned = true;
            carPrefabs = Roots(Resources.LoadAll<GameObject>("Models/Cars"));
            var bikes = Roots(Resources.LoadAll<GameObject>("Models/Bikes"));
            bikePrefab = bikes.Length > 0 ? bikes[0] : null;
        }

        static Bounds WorldBounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>(true);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        /// <summary>Instancia o prefab sob "parent" (que deve estar na origem, sem rotacao), vira o eixo
        /// comprido para Z, escala para o comprimento pedido, centraliza em XZ e apoia no chao.</summary>
        static GameObject InstantiateFitted(GameObject prefab, Transform parent, float length, float yaw)
        {
            var inst = Object.Instantiate(prefab, parent, false);
            inst.name = "Modelo";
            inst.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            if (inst.GetComponentInChildren<Renderer>(true) == null) return inst;

            var b = WorldBounds(inst.transform);
            if (b.size.x > b.size.z * 1.15f)
            {
                inst.transform.localRotation = Quaternion.Euler(0f, yaw + 90f, 0f);
                b = WorldBounds(inst.transform);
            }

            float s = length / Mathf.Max(b.size.z, .01f);
            inst.transform.localScale = Vector3.one * s;
            inst.transform.localPosition = new Vector3(-b.center.x * s, -b.min.y * s, -b.center.z * s);
            return inst;
        }

        public static bool HasImportedCars { get { Scan(); return carPrefabs.Length > 0; } }

        /// <summary>Carro do trafego: modelo importado (se houver) ou placeholder. O motorista so aparece no placeholder.</summary>
        public static GameObject TrafficCar(GameObject driver)
        {
            Scan();
            if (carPrefabs.Length > 0 && Random.value < ImportedCarChance)
            {
                if (driver != null) Object.Destroy(driver);
                return ImportedCar();
            }
            return Car(RandomColor(), driver, true);
        }

        static GameObject ImportedCar()
        {
            var root = new GameObject("Carro");
            var prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
            InstantiateFitted(prefab, root.transform, ImportedCarLength, ImportedCarYaw);
            Gfx.Spot(root.transform, new Vector3(0, .7f, ImportedCarLength * .5f), new Vector3(5f, 0, 0),
                new Color(1f, .93f, .75f), 14f, 2f, 55f);
            return root;
        }

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
            Scan();
            var root = new GameObject("Moto");
            var t = root.transform;
            var rider = CharacterFactory.Build("Motoqueiro", CharacterFactory.Motorista());

            if (bikePrefab != null)
            {
                InstantiateFitted(bikePrefab, t, ImportedBikeLength, ImportedCarYaw);
                Gfx.Spot(t, new Vector3(0, .8f, ImportedBikeLength * .5f), new Vector3(4f, 0, 0), new Color(1f, .93f, .75f), 11f, 1.5f, 45f);

                rider.transform.SetParent(t, false);
                rider.transform.localPosition = new Vector3(0, .3f, -.15f);
                rider.transform.localScale = Vector3.one * .72f;
                Gfx.Part(t, PrimitiveType.Sphere, new Vector3(0, 1.43f, -.12f), Vector3.one * .3f, new Color(.08f, .08f, .1f));
                return root;
            }

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

            rider.transform.SetParent(t, false);
            rider.transform.localPosition = new Vector3(0, .05f, -.15f);
            rider.transform.localScale = Vector3.one * .75f;
            Gfx.Part(t, PrimitiveType.Sphere, new Vector3(0, 1.22f, -.12f), Vector3.one * .3f, new Color(.08f, .08f, .1f));
            return root;
        }
    }
}
