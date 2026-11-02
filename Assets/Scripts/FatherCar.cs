using System;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>O carro do pai: chega por tras, desacelera e para ao lado da calcada onde esta a protagonista.</summary>
    public class FatherCar : MonoBehaviour
    {
        public Transform player;
        public Action onStopped;
        public float cruise = 13f, decel = 5f;

        bool locked, stopped;
        float stopZ;

        void Update()
        {
            if (stopped || player == null) return;

            float z = transform.position.z;
            float target = locked ? stopZ : player.position.z + 1.2f;
            float d = target - z;

            if (!locked && d < 20f) { locked = true; stopZ = target; }

            float vmax = Mathf.Sqrt(2f * decel * Mathf.Max(d, 0f));
            float v = Mathf.Min(cruise, vmax);

            if (locked && (d < .1f || v < .4f))
            {
                stopped = true;
                if (onStopped != null) onStopped();
                return;
            }
            transform.position += Vector3.forward * v * Time.deltaTime;
        }
    }
}
