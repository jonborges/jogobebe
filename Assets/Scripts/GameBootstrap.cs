using UnityEngine;

namespace JogoBebe
{
    /// <summary>
    /// Trecho reto escolhido para a abertura, medido na geometria do modelo MAP (Procedural City 5):
    /// rua norte-sul com pista de -7,5 a +7,5 em X, calcadas de 7,5 a 15,5 dos dois lados e paredes de predio
    /// em |X| = 15,5. O trecho livre fica entre as faixas de pedestres (Z = -52,5 e Z = -7,5);
    /// ao norte a rua termina numa praca (T), ao sul ela segue ate a borda do mapa.
    /// Unity: X para a direita, Z para frente. Mao direita: sentido +Z anda em X positivo.
    /// </summary>
    public static class StreetRoute
    {
        public const float RoadHalf = 7.5f;      // meio-fio
        public const float WallX = 15.4f;        // face das fachadas
        public const float SidewalkX = 11f;      // calcada da protagonista (lado +X)
        public const float RebecaX = 14.6f;      // Rebeca encostada na parede
        public const float PlayerZ = -50f;
        public const float RebecaZ = -11f;       // ~39 m a frente da protagonista
        public const float StartZ = -125f;       // onde o trafego nasce (longe, na neblina)
        public const float EndZ = 1.5f;          // onde o trafego some (cruzamento com a praca)
        public const float WalkMinZ = -54f;      // limites da area jogavel
        public const float WalkMaxZ = -5f;
        public const float NearLaneX = 2.0f;     // sentido +Z (lado da calcada da protagonista), centro da faixa livre
        public const float FarLaneX = -2.0f;     // sentido -Z
        public const float CurbStopX = 5.4f;     // onde o carro do pai encosta
        public const float SidewalkTop = .1f;    // altura da calcada em relacao a pista
    }

    /// <summary>
    /// Coloque este componente num GameObject vazio de uma cena vazia (ou use o menu Jogo Bebe > Criar cena Rua).
    /// Ele monta a rua noturna, a protagonista, a Rebeca, o trafego e o roteiro em tempo de execucao.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        static readonly Color NightColor = new Color(.03f, .04f, .09f);
        const int IgnoreRaycastLayer = 2;

        void Awake()
        {
            PlayerController.Frozen = false;
            TrafficSpawner.ReserveLane = false;
            GameUI.Objective = "";
            GameUI.TitleCard = "";

            QualitySettings.pixelLightCount = 6;
            QualitySettings.antiAliasing = 4;
            Application.targetFrameRate = 60;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.18f, .2f, .32f);
            // neblina ao estilo PS2: fecha rapido ao longe, esconde o fim da rua e o surgimento dos carros
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = .026f;
            RenderSettings.fogColor = NightColor;

            var moon = new GameObject("Lua").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(.45f, .55f, .9f);
            moon.intensity = .6f;
            moon.shadows = LightShadows.None;
            moon.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            BuildStreet();

            // camera
            var camGO = new GameObject("MainCamera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = NightColor;
            cam.fieldOfView = 62f;
            cam.nearClipPlane = .1f;
            cam.farClipPlane = 250f;
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<PS2Render>();

            // protagonista: comeca na calcada, olhando para a Rebeca (+Z)
            var playerGO = new GameObject("Protagonista");
            playerGO.transform.position = new Vector3(StreetRoute.SidewalkX, StreetRoute.SidewalkTop + .2f, StreetRoute.PlayerZ);
            var cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.7f; cc.radius = .3f; cc.center = new Vector3(0, .85f, 0);
            var model = CharacterFactory.Build("WOMAN", CharacterFactory.Protagonista());
            model.name = "Model";
            model.transform.SetParent(playerGO.transform, false);
            var pc = playerGO.AddComponent<PlayerController>();
            pc.model = model.transform;

            // Rebeca: encostada na parede, de frente para a rua, mexendo no celular. Fica parada.
            var reb = new GameObject("Rebeca");
            reb.transform.position = new Vector3(StreetRoute.RebecaX, StreetRoute.SidewalkTop, StreetRoute.RebecaZ);
            reb.transform.rotation = Quaternion.Euler(0, -90f, 0);   // de frente para -X (a pista)
            var rm = CharacterFactory.Build("REBECA", CharacterFactory.Rebeca());
            rm.transform.SetParent(reb.transform, false);
            rm.transform.localRotation = Quaternion.Euler(-8f, 0, 0); // inclinada para tras, apoiada na parede
            var phone = Gfx.Part(rm.transform, PrimitiveType.Cube, new Vector3(.1f, 1.15f, .22f),
                new Vector3(.08f, .14f, .015f), new Color(.6f, .85f, 1f), true, false, "Celular");
            phone.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            Gfx.Lamp(rm.transform, new Vector3(.1f, 1.25f, .35f), new Color(.5f, .8f, 1f), 2.5f, .8f);

            var talk = reb.AddComponent<TalkNPC>();
            talk.player = playerGO.transform;
            talk.label = "Falar com a Rebeca";
            talk.range = 3.5f;
            talk.facePlayer = false;   // ela nao fica virando para quem passa

            // sistemas
            var sys = new GameObject("Sistemas");
            sys.AddComponent<GameUI>();
            var traffic = sys.AddComponent<TrafficSpawner>();
            traffic.player = playerGO.transform;
            var story = sys.AddComponent<StoryDirector>();
            story.player = playerGO.transform;
            story.playerModel = playerGO;
            story.rebeca = talk;
        }

        static GameObject LoadMapPrefab()
        {
            // prefab gerado pelo menu, ou o proprio glTF importado dentro de Resources
            string[] paths = { "Models/Imported/MAP", "Models/MAP/scene", "Models/MAP" };
            foreach (var p in paths)
            {
                var g = Resources.Load<GameObject>(p);
                if (g != null) return g;
            }
            return null;
        }

        void BuildStreet()
        {
            var root = new GameObject("Rua").transform;

            var importedMap = LoadMapPrefab();
            if (importedMap != null)
            {
                var map = Instantiate(importedMap, root);
                map.name = "MAP";
                map.transform.localPosition = Vector3.zero;
                map.transform.localRotation = Quaternion.identity;
                map.transform.localScale = Vector3.one;

                // Sem MeshCollider nos ~400 mil triangulos: a colisao e feita com caixas simples (BuildCorridor).
                // Isso carrega mais rapido e funciona em build mesmo com malhas nao legiveis.
                var rs = map.GetComponentsInChildren<Renderer>(true);
                if (rs.Length > 0)
                {
                    var b = rs[0].bounds;
                    for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
                    Debug.Log("[Rua] MAP carregado. Limites: centro " + b.center + ", tamanho " + b.size);
                    if (b.size.y > b.size.z)
                        Debug.LogWarning("[Rua] O mapa parece deitado (altura maior que a profundidade). " +
                                         "Confira a rotacao do prefab MAP; o esperado e cidade no plano XZ, ruas ao longo de Z.");
                }
            }
            else
            {
                Debug.LogWarning("[Rua] Modelo MAP nao encontrado em Resources/Models. Usando rua de cubos. " +
                                 "Rode o menu Jogo Bebe > Importar modelos glTF depois que o glTFast terminar de importar.");
                BuildFallbackCity(root);
            }

            BuildCorridor(root);
        }

        /// <summary>Colisao, limites, postes e neon do trecho jogavel. Vale para o mapa e para o fallback.</summary>
        void BuildCorridor(Transform root)
        {
            var c = new GameObject("Corredor").transform;
            c.SetParent(root, false);

            float zMin = StreetRoute.WalkMinZ - 6f, zMax = StreetRoute.WalkMaxZ + 2f;
            float zMid = (zMin + zMax) * .5f, len = zMax - zMin;
            float rh = StreetRoute.RoadHalf, wx = StreetRoute.WallX;
            float side = wx - rh;

            // chao: pista no nivel 0, calcadas 10 cm acima
            Box(c, "ChaoPista", new Vector3(0, -.5f, zMid), new Vector3(rh * 2f, 1f, len), 0);
            Box(c, "ChaoCalcadaD", new Vector3(rh + side * .5f, StreetRoute.SidewalkTop - .5f, zMid), new Vector3(side, 1f, len), 0);
            Box(c, "ChaoCalcadaE", new Vector3(-rh - side * .5f, StreetRoute.SidewalkTop - .5f, zMid), new Vector3(side, 1f, len), 0);

            // paredes dos predios (a camera colide com elas)
            float wz0 = StreetRoute.WalkMinZ, wz1 = StreetRoute.WalkMaxZ, wzMid = (wz0 + wz1) * .5f;
            Box(c, "ParedeD", new Vector3(wx + .5f, 4f, wzMid), new Vector3(1f, 8f, wz1 - wz0), 0);
            Box(c, "ParedeE", new Vector3(-wx - .5f, 4f, wzMid), new Vector3(1f, 8f, wz1 - wz0), 0);
            // limites das pontas (a camera ignora, senao ela encolheria no inicio)
            Box(c, "LimiteSul", new Vector3(0, 4f, wz0 - .5f), new Vector3(wx * 2f + 2f, 8f, 1f), IgnoreRaycastLayer);
            Box(c, "LimiteNorte", new Vector3(0, 4f, wz1 + .5f), new Vector3(wx * 2f + 2f, 8f, 1f), IgnoreRaycastLayer);

            // postes de luz na beira da calcada (braco aponta para a pista)
            foreach (float z in new[] { -46f, -29f, -12f })
                LampPost(c, new Vector3(rh + .7f, StreetRoute.SidewalkTop, z), 180f);
            foreach (float z in new[] { -38f, -21f })
                LampPost(c, new Vector3(-rh - .7f, StreetRoute.SidewalkTop, z), 0f);

            // letreiro neon sobre a Rebeca (ponto focal a ~40 m)
            float nz = StreetRoute.RebecaZ;
            Gfx.Part(c, PrimitiveType.Cube, new Vector3(wx - .12f, 3.2f, nz), new Vector3(.1f, .6f, 2.4f), new Color(1f, .2f, .6f), true, false, "Neon");
            Gfx.Lamp(c, new Vector3(wx - 2.2f, 3.2f, nz), new Color(1f, .3f, .65f), 10f, 2f);
        }

        static void Box(Transform parent, string name, Vector3 center, Vector3 size, int layer)
        {
            var g = new GameObject(name);
            g.layer = layer;
            g.transform.SetParent(parent, false);
            g.transform.localPosition = center;
            var col = g.AddComponent<BoxCollider>();
            col.size = size;
        }

        void BuildFallbackCity(Transform root)
        {
            var C = PrimitiveType.Cube;
            Color asphalt = new Color(.09f, .09f, .1f);
            Color walk = new Color(.35f, .35f, .37f);
            float rh = StreetRoute.RoadHalf, wx = StreetRoute.WallX;
            float z0 = -130f, z1 = 60f, zMid = (z0 + z1) * .5f, len = z1 - z0;

            Gfx.Part(root, C, new Vector3(0, -.05f, zMid), new Vector3(rh * 2f, .1f, len), asphalt, false, false, "Asfalto");
            Gfx.Part(root, C, new Vector3(rh + (wx - rh) * .5f, StreetRoute.SidewalkTop - .05f, zMid), new Vector3(wx - rh, .1f, len), walk, false, false, "Calcada");
            Gfx.Part(root, C, new Vector3(-rh - (wx - rh) * .5f, StreetRoute.SidewalkTop - .05f, zMid), new Vector3(wx - rh, .1f, len), walk, false, false, "CalcadaOposta");

            for (float z = z0; z <= z1; z += 6f)
                Gfx.Part(root, C, new Vector3(0, .01f, z), new Vector3(.15f, .02f, 2.5f), new Color(.8f, .7f, .2f), true);

            var rnd = new System.Random(13);
            for (float z = z0 + 6f; z <= z1; z += 14f)
            {
                Building(root, -(wx + 6.1f), z, -wx, 1f, rnd);   // lado da protagonista e oposto
                Building(root, wx + 6.1f, z, wx, -1f, rnd);
            }
        }

        void Building(Transform root, float cx, float z, float facadeX, float sign, System.Random rnd)
        {
            float h = 8f + (float)rnd.NextDouble() * 10f;
            float g = .12f + (float)rnd.NextDouble() * .1f;
            Gfx.Part(root, PrimitiveType.Cube, new Vector3(cx, h / 2f, z), new Vector3(12f, h, 14f),
                new Color(g, g * .95f, g * 1.1f), false, false, "Predio");

            for (float y = 3f; y < h - 1f; y += 3f)
                for (int k = 0; k < 4; k++)
                    if (rnd.NextDouble() < .4)
                        Gfx.Part(root, PrimitiveType.Cube, new Vector3(facadeX + sign * .03f, y, z - 5f + k * 3.3f),
                            new Vector3(.06f, 1.2f, 1.6f), new Color(1f, .8f, .45f), true);
        }

        void LampPost(Transform root, Vector3 pos, float yaw)
        {
            var g = new GameObject("Poste");
            g.transform.SetParent(root, false);
            g.transform.position = pos;
            g.transform.rotation = Quaternion.Euler(0, yaw, 0);
            Color dark = new Color(.08f, .08f, .09f);
            Gfx.Part(g.transform, PrimitiveType.Cylinder, new Vector3(0, 2.5f, 0), new Vector3(.12f, 2.5f, .12f), dark);
            Gfx.Part(g.transform, PrimitiveType.Cube, new Vector3(.4f, 5f, 0), new Vector3(1f, .1f, .15f), dark);
            Gfx.Part(g.transform, PrimitiveType.Sphere, new Vector3(.8f, 4.9f, 0), Vector3.one * .4f, new Color(1f, .85f, .55f), true);
            Gfx.Lamp(g.transform, new Vector3(.8f, 4.7f, 0), new Color(1f, .8f, .5f), 16f, 1.6f);

            var col = g.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1.5f, 0);
            col.size = new Vector3(.35f, 3f, .35f);
        }
    }
}
