using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] float fireRate = 0.5f;
    float nextFire;
    [SerializeField] float range = 700;
    [SerializeField] float hitForce = 10;
    [SerializeField] int gunDamage = 1;

    [Header("Weapon Shooting Visuals/Audio")]
    [SerializeField] float shotDuration = 0.1f;

    AudioSource _as;
    LineRenderer _lr;

    [Header("Camera and Positioning")]
    [SerializeField] Transform gunEnd;
    Camera playerCam;

    private Vector3 lazerPoint = Vector3.zero;
    private float lazerXAngle = 0;
    private float lazerYAngle = 0;
    private float lazerLength = 0;

    private int ammo = 16;
    private int maxAmmo = 16;

    [SerializeField] private TMP_Text ammoText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lr = GetComponent<LineRenderer>();
        _as = GetComponent<AudioSource>();
        playerCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ammo = maxAmmo;
            ammoText.text = "Ammo: " + ammo + "/" + maxAmmo;
        }

        if (Input.GetButton("Fire1") && Time.time > nextFire && ammo > 0)
        {
            nextFire = Time.time + fireRate;

            ammo--;
            ammoText.text = "Ammo: " + ammo + "/" + maxAmmo;
            Vector3 rayOrigin = playerCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            //_lr.SetPosition(0, gunEnd.transform.position);

            if(Physics.Raycast(rayOrigin, playerCam.transform.forward, out hit, range))
            {
                //_lr.SetPosition(1, hit.point);
                //lazerPoint = hit.point - playerCam.transform.forward * 10000;
                lazerXAngle = Vector3.SignedAngle(hit.point - gunEnd.transform.position, gunEnd.transform.forward, gunEnd.transform.up);
                lazerYAngle = Vector3.SignedAngle(hit.point - gunEnd.transform.position, gunEnd.transform.forward, gunEnd.transform.right);
                lazerLength = (hit.point - gunEnd.transform.position).magnitude;

                ShootableBox targetBox = hit.transform.GetComponent<ShootableBox>();
                BadGuy badGuy = hit.transform.root.GetComponent<BadGuy>();

                if (targetBox != null)
                {
                    targetBox.Damage(gunDamage);
                }

                if (badGuy != null)
                {
                    badGuy.Damage(gunDamage);
                }

                if (hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce(-hit.normal * hitForce, ForceMode.Impulse);
                }
            }
            else
            {
                //_lr.SetPosition(1, playerCam.transform.forward * 10000);
                //lazerPoint = Vector3.zero;
                lazerXAngle = Vector3.SignedAngle(playerCam.transform.forward * 10000 - gunEnd.transform.position, gunEnd.transform.forward, gunEnd.transform.up);
                lazerYAngle = Vector3.SignedAngle(playerCam.transform.forward * 10000 - gunEnd.transform.position, gunEnd.transform.forward, gunEnd.transform.right);
                lazerLength = (playerCam.transform.forward * 10000 - gunEnd.transform.position).magnitude;
            }

            StartCoroutine(ShootingEffect());
        }
        Vector3 thevector = Quaternion.AngleAxis(-lazerXAngle, gunEnd.transform.up) * gunEnd.transform.forward;
        thevector = Quaternion.AngleAxis(-lazerYAngle * 0.5f, gunEnd.transform.right) * thevector;
        _lr.SetPosition(1, gunEnd.transform.position + thevector * lazerLength);
        _lr.SetPosition(0, gunEnd.transform.position);
    }

    IEnumerator ShootingEffect()
    {
        _as.Play();
        _lr.enabled = true;

        yield return new WaitForSeconds(shotDuration);

        _lr.enabled = false;
    }
}
