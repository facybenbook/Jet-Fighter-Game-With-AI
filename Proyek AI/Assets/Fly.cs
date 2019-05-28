// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class Fly : MonoBehaviour
{
    public float maxSpeed = 50.0f;
    public string accelKey = "w";
    public string leftKey = "left";
    public string rightKey = "right";
    public string upKey = "up";
    public string downKey = "down";
    public float accel = 1.2f;
    public float decel = 0.8f;
    public float upDownSpeed = 20.0f;
    public float turnSpeed = 20.0f;
    public float speedToFly = 40.0f;
    public GameObject cam;
    public GameObject groundDetector;
    //public GameObject explosion;
    public bool lihat = true;

    public GameObject bullet;
    public Transform[] firePoints = new Transform[1];
    public float fireRate;
    private float nextFire;

    private float curSpeed = 0.0f;
    private float curFall = 0.0f;
    private bool isFalling = false;
    static bool isGrounded = true;
    public int health;
    void Start()
    {
        nextFire = 1 / fireRate;
    }

    void FixedUpdate()
    {
        bool fireButton = Input.GetButton("Fire1");
        Collider[] shipColliders = transform.GetComponentsInChildren<Collider>();
        if (fireButton)
        {
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
    }
    void Update()
    {
        if (isFalling == false)
        {
            if (Input.GetKeyDown(accelKey))
            {
                curSpeed++;
            }
            if (Input.GetKey(accelKey))
            {
                curSpeed += accel * Time.deltaTime;
            }
        }
        else
        {
            curSpeed--;
        }

        if (Input.GetKey(accelKey))
        {
        }
        else
        {
            if (isGrounded == false)
            {
                curSpeed -= decel * Time.deltaTime;
            }
            if (isGrounded == true)
            {
                curSpeed -= (decel * 2) * Time.deltaTime;
            }
        }
        if (curSpeed > maxSpeed)
        {
            curSpeed = maxSpeed;
        }
        if (curSpeed < 0)
        {
            curSpeed = 0;
        }
        if (isGrounded == true)
        {
            if (curSpeed < speedToFly)
            {
                //ransform.rotation.x = 0;
                //transform.rotation.z = 0;

            }
        }
        if (isGrounded == false)
        {
            if (curSpeed < speedToFly)
            {
                curFall += 9.8f * Time.deltaTime;
                transform.Translate(Vector3.down * curFall * Time.deltaTime);
                if (transform.position.y > 50)
                {
                    groundDetector.SetActive(false);
                    isFalling = true;
                }
            }
        }
        transform.Translate(Vector3.forward * curSpeed * Time.deltaTime);
        if (Input.GetKey(upKey))
        {
            if (curSpeed >= speedToFly)
            {
                transform.Rotate(Vector3.right * (-1 * upDownSpeed) * Time.deltaTime);
            }
        }
        if (Input.GetKey(leftKey))
        {
            transform.Rotate(Vector3.up * (-1 * turnSpeed) * Time.deltaTime);
        }
        if (Input.GetKey(rightKey))
        {
            transform.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
        }
        if (Input.GetKey(downKey))
        {
            transform.Rotate(Vector3.right * turnSpeed * Time.deltaTime);
        }
        print(curSpeed);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Terrain")
        {
            Debug.Log("Awas");
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Terrain")
        {
            GameObject cam1 = Instantiate(cam, cam.transform.position, cam.transform.rotation);
            cam1.SetActive(true);
            Destroy(this.gameObject);
            
        }
        if (col.gameObject.tag == "EnemyBullet")
        {
            health = health - 20;

            if (health <= 0)
            {
                GameObject cam1 = Instantiate(cam, cam.transform.position, cam.transform.rotation);
                cam1.SetActive(true);
                Destroy(this.gameObject);
            }
            
        }
    }
}