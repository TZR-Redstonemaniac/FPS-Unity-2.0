using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gun : MonoBehaviour
{
    ////////////////////////////////////////Variables/////////////////////////////////////////
    
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Player pm;

    [Header("Bullet")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootForwardForce, shootUpForce;
    
    [Header("Gun Stats")]
    [SerializeField] private bool allowButtonHold;
    [SerializeField] private float timeBetweenShots, timeBetweenBullets, spread, reloadTime;
    [SerializeField] private int magSize, bulletsPerTap;
    
    [Header("Recoil")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float recoilForce;
    
    [Header("Graphics")]
    [SerializeField] private GameObject muzzleFlash;
    
    [Header("Debugging")]
    [SerializeField] private bool allowInvoke;

    private int bulletsLeft, bulletsShot;

    private bool readyToShoot, shooting, reloading;
    private bool _ismuzzleFlashNotNull;

    ////////////////////////////////////////Code/////////////////////////////////////////

    private void Start()
    {
        _ismuzzleFlashNotNull = muzzleFlash != null;
    }

    private void Awake()
    {
        bulletsLeft = magSize;
        readyToShoot = true;
    }

    private void Update()
    {
        Input();
    }

    private void Input()
    {
        shooting = allowButtonHold ? pm.fireLeft : pm.fireDownLeft;

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = 0;

            Shoot();
        }
        
        if (pm.reload && bulletsLeft < magSize && !reloading) Reload();
        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();
    }

    private void Shoot()
    {
        readyToShoot = false;

        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        
        var targetPoint = Physics.Raycast(ray, out var hit) ? hit.point : ray.GetPoint(75);

        var directionWithoutSpread = targetPoint - attackPoint.position;

        var x = Random.Range(-spread, spread);
        var y = Random.Range(-spread, spread);
        var z = Random.Range(-spread, spread);
        
        var directionWithSpread = directionWithoutSpread + new Vector3(x, y, z);

        var currentBullet = Instantiate(bullet, attackPoint.position, quaternion.identity);
        currentBullet.transform.forward = directionWithSpread.normalized;
        
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * shootForwardForce, ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(cam.transform.up * shootUpForce, ForceMode.Impulse);
        
        if (_ismuzzleFlashNotNull) Instantiate(muzzleFlash, attackPoint.position, quaternion.identity);

        bulletsLeft--;
        bulletsShot++;

        if (allowInvoke)
        {
            allowInvoke = false;
            Invoke(nameof(ResetShot), timeBetweenShots);
            
            rb.AddForce(-directionWithSpread.normalized * recoilForce, ForceMode.Impulse);
        }

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0) Invoke(nameof(Shoot), timeBetweenBullets);
    }

    private void ResetShot()
    {
        readyToShoot = true;

        allowInvoke = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magSize;
        reloading = false;
    }
}
