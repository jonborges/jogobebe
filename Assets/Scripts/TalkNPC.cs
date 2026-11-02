using System;
using UnityEngine;

namespace JogoBebe
{
    /// <summary>Qualquer coisa com quem se fala apertando E quando o jogador chega perto.</summary>
    public class TalkNPC : MonoBehaviour
    {
        public Transform player;
        public string label = "Falar";
        public float range = 3f;
        public bool facePlayer = true;
        public Action onInteract;

        void Update()
        {
            if (player == null) return;
            Vector3 d = player.position - transform.position;
            d.y = 0f;
            float dist = d.magnitude;

            if (facePlayer && dist < 14f && d.sqrMagnitude > .01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 3f * Time.deltaTime);

            if (dist < range && !GameUI.Busy && !PlayerController.Frozen)
            {
                GameUI.SetPrompt("[E] " + label);
                if (Input.GetKeyDown(KeyCode.E) && Time.frameCount > GameUI.ClosedFrame + 1 && onInteract != null)
                    onInteract();
            }
        }
    }
}
