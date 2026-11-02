using UnityEngine;

namespace JogoBebe
{
    /// <summary>
    /// Terceira pessoa: WASD / analogico andam, Shift corre, mouse gira a camera, Esc solta o cursor
    /// (clique na tela para prender de novo). Movimento com aceleracao, corpo que vira suavemente,
    /// balanco procedural de passos (o modelo nao tem esqueleto) e camera com suavizacao e colisao.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static bool Frozen;

        public Transform model;
        public float walkSpeed = 3.2f, runSpeed = 5.6f, camDistance = 5.2f;
        public float acceleration = 20f, deceleration = 28f, turnSpeed = 13f;
        public float mouseSensitivity = 2.4f;

        CharacterController cc;
        Transform cam;
        Camera camComp;
        float baseFov = 62f;

        float yaw, pitch = 12f, vy;
        float lookYaw, lookPitch;       // versoes suavizadas
        Vector3 vel;                    // velocidade horizontal
        Vector3 focus;                  // ponto que a camera olha (suavizado)
        float camDist;
        Quaternion facing = Quaternion.identity;
        float stridePhase, bobAmt, leanAmt, rollAmt;

        void Start()
        {
            cc = GetComponent<CharacterController>();
            if (Camera.main != null)
            {
                cam = Camera.main.transform;
                camComp = Camera.main;
                baseFov = camComp.fieldOfView;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            lookYaw = yaw; lookPitch = pitch;
            focus = transform.position + Vector3.up * 1.5f;
            camDist = camDistance;
            if (model != null) facing = model.rotation;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            HandleCursor();

            bool free = !Frozen && !GameUI.Busy;
            bool locked = Cursor.lockState == CursorLockMode.Locked;

            if (free && locked)
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * mouseSensitivity * .85f, -8f, 55f);
            }

            // entrada -> direcao no mundo, relativa a camera
            Vector3 input = Vector3.zero;
            if (free) input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f) input.Normalize();
            Vector3 wish = Quaternion.Euler(0, yaw, 0) * input;

            bool running = free && Input.GetKey(KeyCode.LeftShift) && input.z > -.5f;
            float topSpeed = running ? runSpeed : walkSpeed;
            Vector3 desired = wish * topSpeed;

            float rate = desired.sqrMagnitude > .01f ? acceleration : deceleration;
            vel = Vector3.MoveTowards(vel, desired, rate * dt);

            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy -= 20f * dt;
            cc.Move((vel + Vector3.up * vy) * dt);

            AnimateModel(dt, running);
        }

        void HandleCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                bool locked = Cursor.lockState == CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = locked;
            }
            else if (Cursor.lockState != CursorLockMode.Locked && !GameUI.Busy && Input.GetMouseButtonDown(0)
                     && Time.frameCount > GameUI.ClosedFrame + 1)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        void AnimateModel(float dt, bool running)
        {
            if (model == null) return;

            float speed = vel.magnitude;
            float speed01 = Mathf.Clamp01(speed / runSpeed);
            bool moving = speed > .25f;

            // corpo vira para onde esta indo (so quando de fato anda)
            if (moving)
            {
                Quaternion want = Quaternion.LookRotation(vel.normalized);
                facing = Quaternion.Slerp(facing, want, 1f - Mathf.Exp(-turnSpeed * dt));
                stridePhase += speed * dt * (running ? 2.3f : 2.6f);
            }
            else
            {
                stridePhase = Mathf.Lerp(stridePhase, Mathf.Round(stridePhase / Mathf.PI) * Mathf.PI, 1f - Mathf.Exp(-8f * dt));
            }

            float k = 1f - Mathf.Exp(-10f * dt);
            bobAmt = Mathf.Lerp(bobAmt, moving ? 1f : 0f, k);
            leanAmt = Mathf.Lerp(leanAmt, speed01 * (running ? 9f : 4.5f), k);
            rollAmt = Mathf.Lerp(rollAmt, moving ? Mathf.Sin(stridePhase) * (running ? 3.5f : 2.2f) : 0f, 1f - Mathf.Exp(-16f * dt));

            float bob = Mathf.Abs(Mathf.Sin(stridePhase)) * (running ? .07f : .045f) * bobAmt;
            model.localPosition = new Vector3(0f, bob, 0f);
            model.rotation = facing * Quaternion.Euler(leanAmt, 0f, rollAmt);
        }

        void LateUpdate()
        {
            if (cam == null) return;
            float dt = Time.deltaTime;

            // leve suavizacao do olhar (curta o bastante para nao dar sensacao de atraso)
            float kl = 1f - Mathf.Exp(-35f * dt);
            lookYaw = Mathf.LerpAngle(lookYaw, yaw, kl);
            lookPitch = Mathf.Lerp(lookPitch, pitch, kl);

            // foco acompanha o corpo com um pequeno atraso, o que tira o "tranco" dos passos
            Vector3 goal = transform.position + Vector3.up * 1.5f;
            focus = Vector3.Lerp(focus, goal, 1f - Mathf.Exp(-20f * dt));

            Quaternion r = Quaternion.Euler(lookPitch, lookYaw, 0);
            Vector3 back = -(r * Vector3.forward);

            // distancia livre ate a parede mais proxima atras da protagonista
            float want = camDistance;
            RaycastHit hit;
            if (Physics.Linecast(focus, focus + back * (camDistance + .25f), out hit))
                want = Mathf.Max(.8f, hit.distance - .25f);

            // encolhe na hora (nao atravessa parede), volta devagar (nao "pula")
            camDist = want < camDist ? want : Mathf.MoveTowards(camDist, want, 7f * dt);

            Vector3 pos = focus + back * camDist;
            pos.y = Mathf.Max(pos.y, .4f);

            cam.position = pos;
            cam.LookAt(focus);

            if (camComp != null)
            {
                bool sprint = vel.magnitude > walkSpeed + .8f;
                camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, baseFov + (sprint ? 5f : 0f), 1f - Mathf.Exp(-6f * dt));
            }
        }
    }
}
