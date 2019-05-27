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
    public GameObject explosion;

    public GameObject bullet;
    public Transform[] firePoints = new Transform[1];
    public float fireRate;
    private float nextFire;

    private float curSpeed = 0.0f;
    private float curFall = 0.0f;
    private bool isFalling = false;
    static bool isGrounded = true;

    void Start()
    {
        nextFire = 1 / fireRate;
    }

    void FixedUpdate()
    {
        bool fireButton = Input.GetButton("Fire1");
        if (fireButton)
        {
            nextFire -= Time.deltaTime;
            if (nextFire <= 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    Instantiate(bullet, firePoints[i].position, Quaternion.Euler(0, 0, 0));
                }
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
    void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Terrain")
        {
            GameObject cam1 = Instantiate(cam, cam.transform.position, cam.transform.rotation);
            cam1.SetActive(true);
            GameObject explo = Instantiate(explosion, transform.position, transform.rotation);
            Destroy(groundDetector);
            Destroy(gameObject);
        }
    }
}