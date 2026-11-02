using UnityEngine;

namespace JogoBebe
{
    /// <summary>Trecho reto escolhido para a abertura. Centro da pista em X=0, sentido ao longo de Z.</summary>
    public static class StreetRoute
    {
        public const float SidewalkX = -10f;
        public const float PlayerZ = -48f;
        public const float RebecaZ = -8f;
        public const float StartZ = -52f;
        public const float EndZ = -6f;
        public const float NearLaneX = -5.5f;
        public const float FarLaneX = 5.5f;
    }

    /// <summary>
    /// Coloque este componente num GameObject vazio de uma cena vazia (ou use o menu Jogo Bebe > Criar cena Rua).
    /// Ele monta a rua noturna, a protagonista, a Rebeca, o trafego e o roteiro em tempo de execucao.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        static readonly Color NightColor = new Color(.03f, .04f, .09f);

        void Awake()
        {
            PlayerController.Frozen = false;
            TrafficSpawner.ReserveLane = false;
            GameUI.Objective = "";
            GameUI.TitleCard = "";

            QualitySettings.pixelLightCount = 4;
            QualitySettings.antiAliasing = 4;
            Application.targetFrameRate = 60;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.16f, .18f, .3f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .018f;
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

            // protagonista
            var playerGO = new GameObject("Protagonista");
            playerGO.transform.position = new Vector3(StreetRoute.SidewalkX, .2f, StreetRoute.PlayerZ);
            var cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.7f; cc.radius = .3f; cc.center = new Vector3(0, .85f, 0);
            var model = CharacterFactory.Build("WOMAN", CharacterFactory.Protagonista());
            model.name = "Model";
            model.transform.SetParent(playerGO.transform, false);
            var pc = playerGO.AddComponent<PlayerController>();
            pc.model = model.transform;

            // Rebeca, encostada na parede a 40 m, mexendo no celular
            var reb = new GameObject("Rebeca");
            reb.transform.position = new Vector3(StreetRoute.SidewalkX, .15f, StreetRoute.RebecaZ);
            reb.transform.rotation = Quaternion.Euler(0, 90f, 0);
            var rm = CharacterFactory.Build("REBECA", CharacterFactory.Rebeca());
            rm.transform.SetParent(reb.transform, false);
            rm.transform.localRotation = Quaternion.Euler(-8f, 0, 0); // inclinada, apoiada na parede
            var phone = Gfx.Part(rm.transform, PrimitiveType.Cube, new Vector3(.1f, 1.15f, .22f),
                new Vector3(.08f, .14f, .015f), new Color(.6f, .85f, 1f), true, false, "Celular");
            phone.transform.localRotation = Quaternion.Euler(-30f, 0, 0);
            Gfx.Lamp(rm.transform, new Vector3(.1f, 1.25f, .35f), new Color(.5f, .8f, 1f), 2.5f, .8f);

            var talk = reb.AddComponent<TalkNPC>();
            talk.player = playerGO.transform;
            talk.label = "Falar com a Rebeca";
            talk.range = 3.5f;

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

        void BuildStreet()
        {
            var root = new GameObject("Rua").transform;

            // O prefab e gerado pelo menu Jogo Bebe > Importar modelos glTF.
            // O mapa escolhido substitui os predios de cubo, preservando um fallback
            // caso o pacote ainda nao tenha sido importado no Editor.
            var importedMap = Resources.Load<GameObject>("Models/Imported/MAP");
            if (importedMap != null)
            {
                var map = Instantiate(importedMap, root);
                map.name = "MAP";
                // Corrige tambem prefabs antigos, antes mesmo de serem recriados pelo Editor.
                map.transform.localPosition = Vector3.zero;
                map.transform.localRotation = Quaternion.identity;
                foreach (var filter in map.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null || filter.GetComponent<Collider>() != null) continue;
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;

                    // O chão continua colidindo com a personagem, mas não intercepta
                    // o Linecast da câmera como se fosse uma parede.
                    for (var parent = filter.transform; parent != null && parent != map.transform; parent = parent.parent)
                    {
                        if (parent.name != "Plane.018_0") continue;
                        filter.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        break;
                    }
                }

                // Piso de seguranca no mesmo passeio reto, caso uma malha glTF nao tenha collider.
                var support = new GameObject("ApoioCalcada");
                support.transform.SetParent(root, false);
                var supportCollider = support.AddComponent<BoxCollider>();
                supportCollider.center = new Vector3(StreetRoute.SidewalkX, -.15f,
                    (StreetRoute.StartZ + StreetRoute.EndZ) * .5f);
                supportCollider.size = new Vector3(4f, .3f,
                    StreetRoute.EndZ - StreetRoute.StartZ + 8f);
                return;
            }

            var C = PrimitiveType.Cube;
            Color asphalt = new Color(.09f, .09f, .1f);
            Color walk = new Color(.35f, .35f, .37f);

            Gfx.Part(root, C, new Vector3(5f, 0f, 0f), new Vector3(10f, .1f, 420f), asphalt, false, true, "Asfalto");
            Gfx.Part(root, C, new Vector3(-2.5f, .075f, 0f), new Vector3(5f, .15f, 420f), walk, false, true, "Calcada");
            Gfx.Part(root, C, new Vector3(12.5f, .075f, 0f), new Vector3(5f, .15f, 420f), walk, false, true, "CalcadaOposta");

            for (float z = -200f; z <= 200f; z += 6f)
                Gfx.Part(root, C, new Vector3(5f, .06f, z), new Vector3(.15f, .02f, 2.5f), new Color(.8f, .7f, .2f), true);

            var rnd = new System.Random(13);
            for (float z = -196f; z <= 196f; z += 14f)
            {
                Building(root, -11f, z, -5f, 1f, rnd);   // lado da calcada da protagonista
                Building(root, 21f, z, 15f, -1f, rnd);   // lado oposto
            }

            for (float z = -190f; z <= 190f; z += 25f)
                LampPost(root, new Vector3(-.6f, .15f, z));

            // letreiro neon sobre a Rebeca (ponto focal a 40 m)
            Gfx.Part(root, C, new Vector3(-4.93f, 3.2f, 40f), new Vector3(.1f, .6f, 2.4f), new Color(1f, .2f, .6f), true, false, "Neon");
            Gfx.Lamp(root, new Vector3(-3.2f, 3.2f, 40f), new Color(1f, .3f, .65f), 10f, 2f);

            // paredes invisiveis: meio-fio (so o carro do pai e alcancavel pela janela) e pontas da rua
            var b = new GameObject("Barreiras");
            b.transform.SetParent(root, false);
            var curb = b.AddComponent<BoxCollider>();
            curb.center = new Vector3(.95f, 2f, 0f);
            curb.size = new Vector3(.2f, 4f, 420f);
            foreach (float z in new[] { -205f, 205f })
            {
                var e = new GameObject("PontaRua").AddComponent<BoxCollider>();
                e.transform.SetParent(root, false);
                e.center = new Vector3(5f, 2f, z);
                e.size = new Vector3(30f, 4f, 1f);
            }
        }

        void Building(Transform root, float cx, float z, float facadeX, float sign, System.Random rnd)
        {
            float h = 8f + (float)rnd.NextDouble() * 10f;
            float g = .12f + (float)rnd.NextDouble() * .1f;
            Gfx.Part(root, PrimitiveType.Cube, new Vector3(cx, h / 2f, z), new Vector3(12f, h, 14f),
                new Color(g, g * .95f, g * 1.1f), false, true, "Predio");

            for (float y = 3f; y < h - 1f; y += 3f)
                for (int k = 0; k < 4; k++)
                    if (rnd.NextDouble() < .4)
                        Gfx.Part(root, PrimitiveType.Cube, new Vector3(facadeX + sign * .03f, y, z - 5f + k * 3.3f),
                            new Vector3(.06f, 1.2f, 1.6f), new Color(1f, .8f, .45f), true);
        }

        void LampPost(Transform root, Vector3 pos)
        {
            var g = new GameObject("Poste");
            g.transform.SetParent(root, false);
            g.transform.position = pos;
            Color dark = new Color(.08f, .08f, .09f);
            Gfx.Part(g.transform, PrimitiveType.Cylinder, new Vector3(0, 2.5f, 0), new Vector3(.12f, 2.5f, .12f), dark);
            Gfx.Part(g.transform, PrimitiveType.Cube, new Vector3(.4f, 5f, 0), new Vector3(1f, .1f, .15f), dark);
            Gfx.Part(g.transform, PrimitiveType.Sphere, new Vector3(.8f, 4.9f, 0), Vector3.one * .4f, new Color(1f, .85f, .55f), true);
            Gfx.Lamp(g.transform, new Vector3(.8f, 4.7f, 0), new Color(1f, .8f, .5f), 16f, 1.6f);
        }
    }
}
