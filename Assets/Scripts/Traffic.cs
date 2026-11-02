using UnityEngine;

namespace JogoBebe
{
    /// <summary>Carros comuns e algumas motos que passam e NUNCA param.</summary>
    public class TrafficSpawner : MonoBehaviour
    {
        public Transform player;
        /// <summary>Quando true, para de gerar carros na faixa do carro do pai (sentido +Z).</summary>
        public static bool ReserveLane;

        float timer = 2f;

        void Start()
        {
            Spawn(1, StreetRoute.StartZ);
            Spawn(-1, StreetRoute.EndZ);
            Spawn(1, StreetRoute.StartZ + 8f);
            Spawn(-1, StreetRoute.EndZ - 8f);
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

            float lane = dir == 1 ? StreetRoute.NearLaneX : StreetRoute.FarLaneX;
            if (bike) lane += Random.Range(-.6f, .6f);
            float z = zOverride.HasValue
                ? zOverride.Value
                : (dir == 1 ? StreetRoute.StartZ : StreetRoute.EndZ);

            GameObject go = bike
                ? Vehicles.Bike(Vehicles.RandomColor())
                : Vehicles.Car(Vehicles.RandomColor(), CharacterFactory.Build("Motorista", CharacterFactory.Motorista()), true);

            go.transform.position = new Vector3(lane, .05f, z);
            go.transform.rotation = dir == 1 ? Quaternion.identity : Quaternion.Euler(0, 180f, 0);
            var tv = go.AddComponent<TrafficVehicle>();
            tv.player = player;
            tv.direction = dir;
            tv.speed = bike ? Random.Range(16f, 22f) : Random.Range(9f, 15f);
        }
    }

    public class TrafficVehicle : MonoBehaviour
    {
        public Transform player;
        public float speed = 12f;
        public int direction = 1;

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            if ((direction > 0 && transform.position.z > StreetRoute.EndZ + 12f) ||
                (direction < 0 && transform.position.z < StreetRoute.StartZ - 12f))
                Destroy(gameObject);
        }
    }
}
