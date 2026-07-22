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
                bool wasLastBullet = (currentAmmo == 1 && GameManager.Instance.maxAmmoReserve == 0);
                bool hitEnemy = Shoot();
                currentAmmo--;
                
                // Jika peluru terakhir dan tidak kena musuh, langsung cek gameover
                if (wasLastBullet && !hitEnemy)
                {
                    if (GetZombiesLeft() > 0)
                    {
                        GameManager.Instance.GameOver(false);
                    }
                }
                
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
        if (GameManager.Instance.maxAmmoReserve > 0)
        {
            AudioManager.Instance.PlayReloadSound();
            Debug.Log("Reloading...");
            yield return new WaitForSeconds(reloadDuration);
            GameManager.Instance.UseOneAmmo(); 
            currentAmmo = magazineSize;
        }
        else
        {
            AudioManager.Instance.PlayBulletEmpty();
            Debug.Log("No more ammo in reserve!");
            yield return new WaitForSeconds(reloadDuration);
            currentAmmo = 0;
        }

        isReloading = false;
        Debug.Log("Reloaded!");
    }

    bool Shoot()
    {
        Vector2 crosshairPos = crosshairController.GetCrosshairPosition();
        Ray ray = cam.ScreenPointToRay(crosshairPos);
        bool hitEnemy = false;

        // Gunakan RaycastAll untuk mendapatkan semua objek yang terkena raycast
        RaycastHit[] allHits = Physics.RaycastAll(ray, maxDistance);
        System.Array.Sort(allHits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

        RaycastHit primaryHit = new RaycastHit();
        bool hasHit = false;
        EnemyController targetEnemy = null;
        
        // Cari zombie/enemy terlebih dahulu dalam semua hits
        foreach (RaycastHit hit in allHits)
        {
            EnemyController controller = hit.collider.GetComponentInParent<EnemyController>();
            Debug.Log($"[Shoot] Checking hit object: {hit.collider.name} - EnemyController found: {controller != null}");
            if (controller != null)
            {
                targetEnemy = controller;
                primaryHit = hit;
                hasHit = true;
                Debug.Log("Tembakan kena zombie: " + hit.collider.name + " (Enemy: " + controller.name + ")");
                break;
            }
        }
        
        // Jika tidak ada zombie yang terkena, gunakan hit pertama (terdekat)
        if (!hasHit && allHits.Length > 0)
        {
            primaryHit = allHits[0];
            hasHit = true;
            Debug.Log("Tembakan kena objek: " + primaryHit.collider.name);
        }

        if (hasHit)
        {
            // Jika ada enemy yang terkena
            if (targetEnemy != null)
            {
                hitEnemy = true;
                targetEnemy.StopPatrol();
                Vector3 direction = primaryHit.point - bulletSpawnTransform.position;

                if (direction.magnitude >= minDistanceToPlayAnimation)
                {
                    targetEnemy.StopAnimation();
                    Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, bulletSpawnTransform.rotation);
                    bulletInstance.Launch(shootingForce, primaryHit.collider.transform, primaryHit.point);
                    bulletTimeController.StartSequence(bulletInstance, primaryHit.point);
                }
                else
                {
                    targetEnemy.OnEnemyShot(direction, primaryHit.collider.GetComponent<Rigidbody>());
                }
            }
            // Check for ragdoll if no enemy controller found
            else
            {
                RagdollController ragdoll = primaryHit.collider.GetComponentInParent<RagdollController>();
                if (ragdoll != null && !ragdoll.IsRagdollEnabled)
                {
                    ragdoll.EnableRagdoll();
                }

                // Spawn bullet with force for non-enemy hits
                Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.LookRotation(ray.direction));
                bulletInstance.Launch(shootingForce, null, primaryHit.point);
            }
        }
        else
        {
            Debug.Log("Tembakan meleset");
            // Handle missed shots
            Bullet bulletInstance = Instantiate(bulletPrefab, bulletSpawnTransform.position, Quaternion.LookRotation(ray.direction));
            bulletInstance.Launch(shootingForce, null, bulletSpawnTransform.position + ray.direction * maxDistance);
        }
        
        return hitEnemy;
    }

    private int GetZombiesLeft()
    {
        // Ambil value dari GameManager
        var field = typeof(GameManager)
            .GetField("zombiesLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int value = 0;
        if (field != null)
        {
            object val = field.GetValue(GameManager.Instance);
            if (val is int)
                value = (int)val;
        }
        Debug.Log($"[ShootingController] zombiesLeft: {value}");
        return value;
    }
}
