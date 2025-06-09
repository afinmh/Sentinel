using UnityEngine;
using TMPro;
using System.Collections;

public class ShootController : MonoBehaviour
{
    public Camera cam;
    public Crosshair crosshairController;

    [SerializeField] BulletTimeController bulletTimeController;
    [SerializeField] private float shootingForce = 50f;
    [SerializeField] Bullet bulletPrefab;  // Change to Bullet type instead of GameObject
    [SerializeField] Transform bulletSpawnTransform;
    [SerializeField] float maxDistance = 100f;
    [SerializeField] float minDistanceToPlayAnimation = 2f;
    [SerializeField] private float reloadDuration = 1.5f;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Transform ammoUIContainer;
    [SerializeField] private GameObject bulletIconPrefab;

    public SerialReaderThreaded serialReader;
    private bool prevShootInput = false;



    
    private bool isReloading = false;
    private const int magazineSize = 1;
    private int currentAmmo = magazineSize;

    void Update()
    {
        GetInput();
        UpdateAmmoUI();
    }

    private void GetInput()
    {
        bool shootInputRaw = false;
        bool reloadInputRaw = false;

        if (serialReader != null)
        {
            float p, r, y;
            bool shootBtn, reloadBtn, scopeBtn;
            serialReader.GetSensorData(out p, out r, out y, out shootBtn, out reloadBtn, out scopeBtn);

            shootInputRaw = shootBtn;
            reloadInputRaw = reloadBtn;
        }

        // fallback mouse & keyboard input jika serialReader tidak aktif atau tidak tekan tombol di serial
        if (!shootInputRaw)
        {
            shootInputRaw = Input.GetMouseButton(0);
        }
        if (!reloadInputRaw)
        {
            reloadInputRaw = Input.GetKeyDown(KeyCode.R);
        }

        // Debounce shoot: hanya ketika tombol baru ditekan (transisi false -> true)
        bool shootInput = shootInputRaw && !prevShootInput;
        prevShootInput = shootInputRaw;

        if (reloadInputRaw && !isReloading && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }

        if (shootInput && !isReloading)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                currentAmmo--;
                AudioManager.Instance.PlayShootingSound();
                AudioManager.Instance.PlayTrailSound();
            }
            else
            {
                AudioManager.Instance.PlayBulletEmpty();
            }
        }
    }


    private void UpdateAmmoUI()
    {
        ammoText.text = $" {currentAmmo}/{GameManager.Instance.maxAmmoReserve}";
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        AudioManager.Instance.PlayReloadSound();

        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadDuration);

        if (GameManager.Instance.maxAmmoReserve > 0)
        {
            GameManager.Instance.UseOneAmmo(); 
            currentAmmo = magazineSize;
        }
        else
        {
            Debug.Log("No more ammo in reserve!");
            currentAmmo = 0;
        }

        isReloading = false;
        Debug.Log("Reloaded!");
    }

    void Shoot()
    {
        Vector2 crosshairPos = crosshairController.GetCrosshairPosition();
        Ray ray = cam.ScreenPointToRay(crosshairPos);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Debug.Log("Tembakan kena: " + hit.collider.name);

            // Check for enemy controller first
            EnemyController controller = hit.collider.GetComponentInParent<EnemyController>();
            if (controller != null)
            {
                controller.StopPatrol();
                Vector3 direction = hit.point - bulletSpawnTransform.position;

                if (direction.magnitude >= minDistanceToPlayAnimation)
                {
                    controller.StopAnimation();
                    Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, bulletSpawnTransform.rotation);
                    bulletInstance.Launch(shootingForce, hit.collider.transform, hit.point);
                    bulletTimeController.StartSequence(bulletInstance, hit.point);
                }
                else
                {
                    controller.OnEnemyShot(direction, hit.collider.GetComponent<Rigidbody>());
                }
            }
            // Check for ragdoll if no enemy controller found
            else
            {
                RagdollController ragdoll = hit.collider.GetComponentInParent<RagdollController>();
                if (ragdoll != null && !ragdoll.IsRagdollEnabled)
                {
                    ragdoll.EnableRagdoll();
                }

                // Spawn bullet with force for non-enemy hits
                Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.LookRotation(ray.direction));
                bulletInstance.Launch(shootingForce, null, hit.point);
            }
        }
        else
        {
            Debug.Log("Tembakan meleset");
            // Handle missed shots
            Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.LookRotation(ray.direction));
            bulletInstance.Launch(shootingForce, null, bulletSpawnTransform.position + ray.direction * maxDistance);
        }
    }
}
