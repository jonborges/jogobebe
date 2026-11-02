using System;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>O carro do pai: chega por tras, desacelera, encosta no meio-fio e para ao lado da protagonista.</summary>
    public class FatherCar : MonoBehaviour
    {
        public Transform player;
        public Action onStopped;
        public float cruise = 13f, decel = 5f;
        /// <summary>X onde o carro encosta (perto do meio-fio). Se NaN, nao muda de faixa.</summary>
        public float curbX = float.NaN;

        bool locked, stopped;
        float stopZ;

        void Update()
        {
            if (stopped || player == null) return;

            float z = transform.position.z;
            float target = locked ? stopZ : player.position.z + 1.2f;
            float d = target - z;

            if (!locked && d < 24f) { locked = true; stopZ = target; }

            float vmax = Mathf.Sqrt(2f * decel * Mathf.Max(d, 0f));
            float v = Mathf.Min(cruise, vmax);

            // encosta no meio-fio durante a frenagem
            float x = transform.position.x;
            float lateral = 0f;
            if (!float.IsNaN(curbX) && d < 38f)
            {
                float nx = Mathf.MoveTowards(x, curbX, 1.3f * Time.deltaTime);
                lateral = (nx - x) / Mathf.Max(Time.deltaTime, .0001f);
                x = nx;
            }

            if (locked && (d < .1f || v < .4f))
            {
                stopped = true;
                transform.rotation = Quaternion.identity;
                if (onStopped != null) onStopped();
                return;
            }

            transform.position = new Vector3(x, transform.position.y, z + v * Time.deltaTime);
            Vector3 mv = new Vector3(lateral, 0f, Mathf.Max(v, .3f));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mv), 6f * Time.deltaTime);
        }
    }
}
