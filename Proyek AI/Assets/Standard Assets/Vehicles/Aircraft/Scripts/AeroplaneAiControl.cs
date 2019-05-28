using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof(AeroplaneController))]
    public class AeroplaneAiControl : MonoBehaviour
    {
        // This script represents an AI 'pilot' capable of flying the plane towards a designated target.
        // It sends the equivalent of the inputs that a user would send to the Aeroplane controller.
        [SerializeField]
        private float m_RollSensitivity = .2f;         // How sensitively the AI applies the roll controls
        [SerializeField]
        private float m_PitchSensitivity = .5f;        // How sensitively the AI applies the pitch controls
        [SerializeField]
        private float m_LateralWanderDistance = 5;     // The amount that the plane can wander by when heading for a target
        [SerializeField]
        private float m_LateralWanderSpeed = 0.11f;    // The speed at which the plane will wander laterally
        [SerializeField]
        private float m_MaxClimbAngle = 45;            // The maximum angle that the AI will attempt to make plane can climb at
        [SerializeField]
        private float m_MaxRollAngle = 45;             // The maximum angle that the AI will attempt to u
        [SerializeField]
        private float m_SpeedEffect = 0.01f;           // This increases the effect of the controls based on the plane's speed.
        [SerializeField]
        private float m_TakeoffHeight = 20;            // the AI will fly straight and only pitch upwards until reaching this height
        [SerializeField]
        private Transform m_Target;                    // the target to fly towards
        [SerializeField]
        private Transform m_Friend;    

        private AeroplaneController m_AeroplaneController;  // The aeroplane controller that is used to move the plane
        private float m_RandomPerlin;                       // Used for generating random point on perlin noise so that the plane will wander off path slightly
        private bool m_TakenOff;                            // Has the plane taken off yet

        public GameObject bullet;
        public Transform[] firePoints = new Transform[1];
        private float fireRate = 5;
        private float nextFire;
        Boolean stateFollow = false;
        Boolean stateAttack = false;
        Boolean stateProtect = false;
        Boolean stateFlee = false;
        public int health;
        Vector3 friendPos;
        // setup script properties
        private void Awake()
        {
            // get the reference to the aeroplane controller, so we can send move input to it and read its current state.
            m_AeroplaneController = GetComponent<AeroplaneController>();

            // pick a random perlin starting point for lateral wandering
            m_RandomPerlin = Random.Range(0f, 100f);
        }


        // reset the object to sensible values
        public void Reset()
        {
            m_TakenOff = false;
        }


        // fixed update is called in time with the physics system update
        private void FixedUpdate()
        {
            Vector3 targetPos = m_Target.position +
                                    transform.right *
                                    (Mathf.PerlinNoise(Time.time * m_LateralWanderSpeed, m_RandomPerlin) * 2 - 1) *
                                    m_LateralWanderDistance;

            if (m_Friend != null)
            {
                friendPos = m_Friend.position +
                                       transform.right *
                                       (Mathf.PerlinNoise(Time.time * m_LateralWanderSpeed, m_RandomPerlin) * 2 - 1) *
                                       m_LateralWanderDistance;
            }
            if (m_Target != null && (transform.position - targetPos).magnitude < 1000)
            {
                // make the plane wander from the path, useful for making the AI seem more human, less robotic.
                stateAttack = false;
                stateFlee = false;
                stateFollow = true;
                stateProtect = false;

                if ((transform.position - targetPos).magnitude < 350)
                {
                    stateAttack = true;
                    stateFlee = false;
                    stateFollow = false;
                    stateProtect = false;

                    //AI BISA NEMBAK
                    Collider[] shipColliders = transform.GetComponentsInChildren<Collider>();
                    nextFire -= Time.deltaTime;
                    if (nextFire <= 0)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            GameObject bulletClone = Instantiate(bullet, firePoints[i].position, transform.rotation);
                            for (int j = 0; j < shipColliders.Length; j++)
                            {
                                Physics.IgnoreCollision(bulletClone.transform.GetComponent<Collider>(), shipColliders[j]);
                            }
                        }
                        nextFire = 1 / fireRate;
                    }
                }
                
                
                // adjust the yaw and pitch towards the target
                Vector3 localTarget = transform.InverseTransformPoint(targetPos);
                float targetAngleYaw = Mathf.Atan2(localTarget.x, localTarget.z);
                float targetAnglePitch = -Mathf.Atan2(localTarget.y, localTarget.z);


                // Set the target for the planes pitch, we check later that this has not passed the maximum threshold
                targetAnglePitch = Mathf.Clamp(targetAnglePitch, -m_MaxClimbAngle * Mathf.Deg2Rad,
                                               m_MaxClimbAngle * Mathf.Deg2Rad);

                // calculate the difference between current pitch and desired pitch
                float changePitch = targetAnglePitch - m_AeroplaneController.PitchAngle;

                // AI always applies gentle forward throttle
                const float throttleInput = 0.5f;

                // AI applies elevator control (pitch, rotation around x) to reach the target angle
                float pitchInput = changePitch * m_PitchSensitivity;

                // clamp the planes roll
                float desiredRoll = Mathf.Clamp(targetAngleYaw, -m_MaxRollAngle * Mathf.Deg2Rad, m_MaxRollAngle * Mathf.Deg2Rad);
                float yawInput = 0;
                float rollInput = 0;

                if (!m_TakenOff)
                {
                    // If the planes altitude is above m_TakeoffHeight we class this as taken off
                    if (m_AeroplaneController.Altitude > m_TakeoffHeight)
                    {
                        m_TakenOff = true;
                    }
                }
                else
                {
                    // now we have taken off to a safe height, we can use the rudder and ailerons to yaw and roll
                    yawInput = targetAngleYaw;
                    rollInput = -(m_AeroplaneController.RollAngle - desiredRoll) * m_RollSensitivity;
                }

                // adjust how fast the AI is changing the controls based on the speed. Faster speed = faster on the controls.
                float currentSpeedEffect = 1 + (m_AeroplaneController.ForwardSpeed * m_SpeedEffect);
                rollInput *= currentSpeedEffect;
                pitchInput *= currentSpeedEffect;
                yawInput *= currentSpeedEffect;

                // pass the current input to the plane (false = because AI never uses air brakes!)
                m_AeroplaneController.Move(rollInput, pitchInput, yawInput, throttleInput, false);
            }
            else if (m_Friend != null)
            {
                if((transform.position - friendPos).magnitude < 500)
                {
                    stateAttack = false;
                stateFlee = false;
                stateFollow = false;
                stateProtect = true;
                // make the plane wander from the path, useful for making the AI seem more human, less robotic.

                // adjust the yaw and pitch towards the friend
                Vector3 localFriend = transform.InverseTransformPoint(friendPos);
                float friendAngleYaw = Mathf.Atan2(localFriend.x, localFriend.z);
                float friendAnglePitch = -Mathf.Atan2(localFriend.y, localFriend.z);


                // Set the friend for the planes pitch, we check later that this has not passed the maximum threshold
                friendAnglePitch = Mathf.Clamp(friendAnglePitch, -m_MaxClimbAngle * Mathf.Deg2Rad,
                                               m_MaxClimbAngle * Mathf.Deg2Rad);

                // calculate the difference between current pitch and desired pitch
                float changePitch = friendAnglePitch - m_AeroplaneController.PitchAngle;

                // AI always applies gentle forward throttle
                const float throttleInput = 0.5f;

                // AI applies elevator control (pitch, rotation around x) to reach the friend angle
                float pitchInput = changePitch * m_PitchSensitivity;

                // clamp the planes roll
                float desiredRoll = Mathf.Clamp(friendAngleYaw, -m_MaxRollAngle * Mathf.Deg2Rad, m_MaxRollAngle * Mathf.Deg2Rad);
                float yawInput = 0;
                float rollInput = 0;

                if (!m_TakenOff)
                {
                    // If the planes altitude is above m_TakeoffHeight we class this as taken off
                    if (m_AeroplaneController.Altitude > m_TakeoffHeight)
                    {
                        m_TakenOff = true;
                    }
                }
                else
                {
                    // now we have taken off to a safe height, we can use the rudder and ailerons to yaw and roll
                    yawInput = friendAngleYaw;
                    rollInput = -(m_AeroplaneController.RollAngle - desiredRoll) * m_RollSensitivity;
                }

                // adjust how fast the AI is changing the controls based on the speed. Faster speed = faster on the controls.
                float currentSpeedEffect = 1 + (m_AeroplaneController.ForwardSpeed * m_SpeedEffect);
                rollInput *= currentSpeedEffect;
                pitchInput *= currentSpeedEffect;
                yawInput *= currentSpeedEffect;

                // pass the current input to the plane (false = because AI never uses air brakes!)
                m_AeroplaneController.Move(rollInput, pitchInput, yawInput, throttleInput, false);
                }
            }
            else
            {
                // no friend set, send zeroed input to the planeW
                m_AeroplaneController.Move(0, 0, 0, 0, false);
            }
        }


        // allows other scripts to set the plane's target
        public void SetTarget(Transform target)
        {
            m_Target = target;
        }

        public void SetFriend(Transform friend)
        {
            m_Friend = friend;
        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.tag == "Terrain")
            {
                Destroy(this.gameObject);

            }
            if (col.gameObject.tag == "Bullet")
            {
                health = health - 20;

                if (health <= 0)
                {
                    Destroy(this.gameObject);
                }
                
            }
        }
    }
}
