using System.Collections.Generic;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>Carros comuns e algumas motos que passam e NUNCA param pra protagonista (o carro do pai fica no meio-fio, longe da faixa deles).
    /// Mao direita: sentido +Z na faixa do lado da calcada dela (+X), sentido -Z na faixa oposta.</summary>
    public class TrafficSpawner : MonoBehaviour
    {
        public Transform player;
        /// <summary>Quando true, para de gerar carros na faixa do carro do pai (sentido +Z).</summary>
        public static bool ReserveLane;

        float timer = 3f;

        void Start()
        {
            // trafego ja rolando quando a cena abre
            Spawn(1, StreetRoute.StartZ + 30f);
            Spawn(-1, StreetRoute.PlayerZ + 28f);
            Spawn(1, StreetRoute.StartZ + 62f);
            Spawn(-1, StreetRoute.EndZ - 2f);
        }

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = Random.Range(2.5f, 6f);
                Spawn(Random.value < .5f ? 1 : -1);
            }
        }

        void Spawn(int dir, float? zOverride = null)
        {
            bool bike = Random.value < 0.22f;
            if (dir == 1 && ReserveLane) dir = -1;

            float z = zOverride.HasValue
                ? zOverride.Value
                : (dir == 1 ? StreetRoute.StartZ : StreetRoute.EndZ);

            // nao nasce em cima de outro veiculo da mesma faixa
            if (!TrafficVehicle.LaneClear(dir, z, 20f)) return;

            float lane = dir == 1 ? StreetRoute.NearLaneX : StreetRoute.FarLaneX;
            if (bike) lane += Random.Range(-.7f, .7f);

            GameObject go = bike
                ? Vehicles.Bike(Vehicles.RandomColor())
                : Vehicles.TrafficCar(CharacterFactory.Build("Motorista", CharacterFactory.Motorista()));

            go.transform.position = new Vector3(lane, .05f, z);
            go.transform.rotation = dir == 1 ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
            var tv = go.AddComponent<TrafficVehicle>();
            tv.player = player;
            tv.direction = dir;
            tv.speed = bike ? Random.Range(15f, 20f) : Random.Range(9f, 14f);
        }
    }

    public class TrafficVehicle : MonoBehaviour
    {
        public static readonly List<TrafficVehicle> All = new List<TrafficVehicle>();
        public Transform player;
        public float speed = 12f;
        public int direction = 1;

        float cruise, current;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }

        void Start()
        {
            cruise = speed;
            current = speed;
        }

        public static bool LaneClear(int dir, float z, float gap)
        {
            foreach (var v in All)
                if (v != null && v.direction == dir && Mathf.Abs(v.transform.position.z - z) < gap) return false;
            return true;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Vector3 p = transform.position;

            // segue o veiculo da frente na mesma faixa (sem parar de vez: so desacelera)
            float target = cruise;
            foreach (var v in All)
            {
                if (v == this || v == null || v.direction != direction) continue;
                Vector3 q = v.transform.position;
                float ahead = (q.z - p.z) * direction;
                if (ahead > 0f && ahead < 12f && Mathf.Abs(q.x - p.x) < 1.6f)
                    target = Mathf.Min(target, v.current * (ahead < 6.5f ? .6f : .95f));
            }
            current = Mathf.MoveTowards(current, target, 8f * dt);

            transform.position = p + new Vector3(0f, 0f, direction * current * dt);

            float z = transform.position.z;
            if ((direction > 0 && z > StreetRoute.EndZ) || (direction < 0 && z < StreetRoute.StartZ))
                Destroy(gameObject);
        }
    }
}
