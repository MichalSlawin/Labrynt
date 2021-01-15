using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using TMPro;
using Random = UnityEngine.Random;

#pragma warning disable 618, 649
namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.
        [SerializeField] private AudioClip collectPointSound;
        [SerializeField] private AudioClip playerHitSound;
        [SerializeField] private AudioClip powerupSound;
        [SerializeField] private AudioClip respawnSound;

        private float originalWalkSpeed;
        private float originalRunSpeed;
        private float originalJumpSpeed;
        private float originalGravityMultiplier;

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        private Vector3 startingPosition;
        private GameController gameController;
        private SceneGenerator sceneGenerator;
        private System.Random random = new System.Random();
        private bool immortal = false;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.tag);
            if (other.transform.CompareTag("Death") && !immortal)
            {
                m_AudioSource.clip = playerHitSound;
                m_AudioSource.Play();

                TeleportPlayerTo(startingPosition);
                gameController.ChangePoints(-1);
            }

            if (other.transform.CompareTag("DisappearingFloor") && !immortal)
            {
                StartCoroutine(gameController.DisableObjectTemporarily(other.gameObject, GameController.FloorDissapearTime, GameController.FloorAppearTime));
            }

            if (other.transform.CompareTag("Respawn"))
            {
                if(Mathf.Abs(startingPosition.x - transform.position.x) > 4 || Mathf.Abs(startingPosition.z - transform.position.z) > 4)
                {
                    m_AudioSource.clip = respawnSound;
                    m_AudioSource.Play();

                    startingPosition = transform.position;
                }
            }

            if (other.transform.CompareTag("Finish"))
            {
                gameController.FinishGame();
            }

            if (other.transform.CompareTag("Point"))
            {
                m_AudioSource.clip = collectPointSound;
                m_AudioSource.Play();

                Destroy(other.gameObject);
                gameController.ChangePoints(1);
                sceneGenerator.IncreasePointsCollected();
                sceneGenerator.UpdatePointsCounter();
            }

            if(other.transform.CompareTag("Powerup"))
            {
                m_AudioSource.clip = powerupSound;
                m_AudioSource.Play();

                if (gameController.PowerupActive)
                {
                    gameController.RemovePowerup = false;
                }
                Destroy(other.gameObject);
                StartCoroutine(gameController.UseRandomPowerup(GameController.PowerupTime));
            }

            if(other.transform.CompareTag("Portal"))
            {
                TeleportPlayerToOtherPortal(other.gameObject);
            }
        }

        private void TeleportPlayerToOtherPortal(GameObject enterPortal)
        {
            GameObject exitPortal = null;
            GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");

            foreach(GameObject portal in portals)
            {
                if(!portal.Equals(enterPortal))
                {
                    exitPortal = portal;
                }
            }
            int offsetZ = 0;
            if (exitPortal.transform.position.z > 0) offsetZ = -2;
            if (exitPortal.transform.position.z < 0) offsetZ = 2;

            TeleportPlayerTo(new Vector3(exitPortal.transform.position.x, 3, exitPortal.transform.position.z + offsetZ));
        }

        private void TeleportPlayerTo(Vector3 position)
        {
            m_CharacterController.enabled = false; // when character controller is enabled it prevents changing transform.position
            transform.position = position;
            m_CharacterController.enabled = true;
        }

        // Use this for initialization
        private void Start()
        {
            RememberOriginalValues();

            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);

            startingPosition = transform.position;

            gameController = FindObjectOfType<GameController>();
            if (gameController == null) throw new Exception("Game controller not found!");
            sceneGenerator = FindObjectOfType<SceneGenerator>();
            if (sceneGenerator == null) throw new Exception("Scene generator not found!");
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }

        //---------------------------------------------------------------------------------------------

        private void RememberOriginalValues()
        {
            originalWalkSpeed = m_WalkSpeed;
            originalRunSpeed = m_RunSpeed;
            originalJumpSpeed = m_JumpSpeed;
            originalGravityMultiplier = m_GravityMultiplier;
        }

        public void RestoreOriginalValues()
        {
            m_WalkSpeed = originalWalkSpeed;
            m_RunSpeed = originalRunSpeed;
            m_JumpSpeed = originalJumpSpeed;
            m_GravityMultiplier = originalGravityMultiplier;
            immortal = false;
        }

        public void MultiplyWalkRunSpeeds(float multiplier, float duration)
        {
            m_WalkSpeed = originalWalkSpeed * multiplier;
            m_RunSpeed = originalRunSpeed * multiplier;
        }

        public void MultiplyJumpSpeed(float multiplier, float duration)
        {
            m_JumpSpeed = originalJumpSpeed * multiplier;
        }

        public void ChangeGravity(float value, float duration)
        {
            m_GravityMultiplier = value;
        }

        public void MakeImmortal(float duration)
        {
            immortal = true;
        }

        //---------------------------------------------------------------------------------------------

        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                //StopAllCoroutines();
                StopCoroutine(m_FovKick.FOVKickUp());
                StopCoroutine(m_FovKick.FOVKickDown());
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
