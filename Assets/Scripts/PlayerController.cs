using UnityEngine;

namespace JogoBebe
{
    /// <summary>Terceira pessoa: WASD, Shift corre, mouse gira a camera, Esc solta o cursor.</summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static bool Frozen;

        public Transform model;
        public float walkSpeed = 3.2f, runSpeed = 5.6f, camDistance = 6f;

        CharacterController cc;
        Transform cam;
        float yaw, pitch = 14f, vy;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            if (Camera.main != null) cam = Camera.main.transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }

            bool free = !Frozen && !GameUI.Busy;
            if (free && Cursor.lockState == CursorLockMode.Locked)
            {
                yaw += Input.GetAxis("Mouse X") * 3f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2.5f, -5f, 55f);
            }

            Vector3 input = Vector3.zero;
            if (free) input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 move = Quaternion.Euler(0, yaw, 0) * input;
            float sp = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy -= 20f * Time.deltaTime;
            cc.Move((move * sp + Vector3.up * vy) * Time.deltaTime);

            bool moving = move.sqrMagnitude > .01f;
            if (model != null)
            {
                if (moving) model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(move), 12f * Time.deltaTime);
                float bob = moving ? Mathf.Abs(Mathf.Sin(Time.time * (sp > walkSpeed ? 13f : 9f))) * .05f : 0f;
                model.localPosition = new Vector3(0, bob, 0);
            }
        }

        void LateUpdate()
        {
            if (cam == null) return;
            Vector3 target = transform.position + Vector3.up * 1.5f;
            Quaternion r = Quaternion.Euler(pitch, yaw, 0);
            Vector3 pos = target - r * Vector3.forward * camDistance;
            pos.y = Mathf.Max(pos.y, .4f);

            RaycastHit hit;
            if (Physics.Linecast(target, pos, out hit))
                pos = hit.point + hit.normal * .2f;

            cam.position = pos;
            cam.LookAt(target);
        }
    }
}
