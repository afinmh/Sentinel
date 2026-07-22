using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TimeScaleController))]
public class BulletTimeController : MonoBehaviour
{
	[Serializable]
	public class TargetTrackingSetup
	{
		public CinemachinePathController avaliableTrack;
		public CameraCartController avaliableDolly;
	}

	[Serializable]
	public class BulletTrackingSetup : TargetTrackingSetup
	{
		public float minDistance;
		public float maxDistance;
	}

	[SerializeField] private GameObject canvas;
	[SerializeField] private CinemachineBrain cameraBrain;
	[SerializeField] private BulletTrackingSetup[] bulletTackingSetup;
	[SerializeField] private TargetTrackingSetup[] enemyTrackingSetup;
	[SerializeField] private ShootController shootController;
	[SerializeField] private float distanceToChangeCamera;
	[SerializeField] private float finishingCameraDuration;

	private TimeScaleController timeScaleController;
	private CinemachineSmoothPath trackInstance;
	private CameraCartController dollyInstance;
	private Bullet activeBullet;
	private Vector3 targetPosition;
	private List<TargetTrackingSetup> clearTracks = new List<TargetTrackingSetup>();
	private bool isLastCameraActive = false;

	private void Awake()
	{
		timeScaleController = GetComponent<TimeScaleController>();
	}

internal void StartSequence(Bullet activeBullet, Vector3 targetPosition)
{
	ResetVariables();
	float distanceToTarget = Vector3.Distance(activeBullet.transform.position, targetPosition);
	Debug.Log($"[BulletTime] Distance to target: {distanceToTarget}");

	var setupsInRange = bulletTackingSetup.Where(s => 
		distanceToTarget > s.minDistance && 
		distanceToTarget < s.maxDistance).ToArray();
	Debug.Log($"[BulletTime] Setups in range: {setupsInRange.Length}");

	var selectedTrackingSetup = SelectTrackingSetup(activeBullet.transform, setupsInRange, activeBullet.transform.rotation);
	if (selectedTrackingSetup == null && enemyTrackingSetup.Length > 0)
	{
		Debug.LogWarning("[BulletTime] No clear track found, using first available enemy tracking setup as fallback.");
		selectedTrackingSetup = enemyTrackingSetup[0];
	}
	this.activeBullet = activeBullet;
	this.targetPosition = targetPosition;

	// Subscribe event agar BulletTimeController yang handle bullet hit
	activeBullet.OnBulletHit += OnBulletHitHandler;

	CreateBulletPath(activeBullet.transform, selectedTrackingSetup.avaliableTrack);
	CreateDolly(selectedTrackingSetup);
	cameraBrain.gameObject.SetActive(true);
	shootController.gameObject.SetActive(false);
	canvas.gameObject.SetActive(false);
	float speed = CalculateDollySpeed();
	dollyInstance.InitDolly(trackInstance, activeBullet.transform, speed);
}
// Handler event saat bullet kena sesuatu
private void OnBulletHitHandler(Bullet bullet, RaycastHit hit)
{
	Debug.Log("[BulletTime] Bullet hit something: " + hit.transform.name);
	// Lanjutkan sequence seperti biasa (ChangeCamera)
	// Unsubscribe agar tidak double
	bullet.OnBulletHit -= OnBulletHitHandler;
	// Pastikan bullet tidak langsung di-destroy, tunggu sequence selesai
	ChangeCamera();
}

	private void CreateDolly(TargetTrackingSetup setup)
	{
		var selectedDolly = setup.avaliableDolly;
		dollyInstance = Instantiate(selectedDolly);
	}

	private void CreateBulletPath(Transform bulletTransform, CinemachinePathController selectedPath)
	{
		trackInstance = Instantiate(selectedPath.path, bulletTransform);
		trackInstance.transform.localPosition = selectedPath.transform.position;
		trackInstance.transform.localRotation = selectedPath.transform.rotation;
	}

	private float CalculateDollySpeed()
	{
		if (trackInstance == null || activeBullet == null)
			return 0f;

		float distanceToTarget = Vector3.Distance(activeBullet.transform.position, targetPosition);
		float speed = activeBullet.GetBulletSpeed();
		float pathDistance = trackInstance.PathLength;
		return pathDistance * speed / distanceToTarget;
	}


   private void CreateEnemyPath(Transform enemytransform, Transform bulletTransform, CinemachinePathController selectedPath)
   {
	   // Cinematic: fokus ke musuh yang kena tembak
	   // Path start dari belakang bullet ke depan musuh, look-at ke musuh
	   Vector3 enemyPos = enemytransform.position;
	   Vector3 bulletPos = bulletTransform.position;
	   Vector3 dir = (enemyPos - bulletPos).normalized;
	   float offsetBehind = 2.5f; // jarak start path di belakang musuh
	   float offsetAbove = 1.2f; // sedikit di atas musuh
	   Vector3 pathStart = enemyPos - dir * offsetBehind + Vector3.up * offsetAbove;
	   Quaternion rotation = Quaternion.LookRotation(dir, Vector3.up);
	   trackInstance = Instantiate(selectedPath.path, pathStart, rotation);

	   // Jika path support look-at, set target ke musuh
	   var vcam = trackInstance.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
	   if (vcam != null)
	   {
		   vcam.LookAt = enemytransform;
		   Debug.Log("[BulletTime] Enemy dolly camera LookAt set to: " + enemytransform.name);
	   }
   }

	private TargetTrackingSetup SelectTrackingSetup(Transform trans, TargetTrackingSetup[] setups, Quaternion orientation)
	{
		clearTracks.Clear();
		for (int i = 0; i < setups.Length; i++)
		{
			if (CheckIfPathIsClear(setups[i].avaliableTrack, trans, orientation))
				clearTracks.Add(setups[i]);
		}
		if (clearTracks.Count == 0)
			return null;
		return clearTracks[UnityEngine.Random.Range(0, clearTracks.Count)];
	}

	private bool CheckIfPathIsClear(CinemachinePathController path, Transform trans, Quaternion orientation)
	{
		float dist = Vector3.Distance(trans.position, targetPosition);
		bool clear = path.CheckIfPathISClear(trans, dist, orientation);
		Debug.Log($"Check path clear? {clear} | Distance: {dist} | Path: {path.name}");
		return clear;
	}


	private void Update()
	{
		if (activeBullet == null)
			return;

		if (CheckIfBulletIsNearTarget())
			ChangeCamera();
	}


	private bool CheckIfBulletIsNearTarget()
	{
		return Vector3.Distance(activeBullet.transform.position, targetPosition) < distanceToChangeCamera;
	}

private void ChangeCamera()
{
	if (isLastCameraActive) return;
	isLastCameraActive = true;

	// Destroy Dolly Bullet Cart (dollyInstance) dan trackInstance sebelum membuat dolly enemy
	if (dollyInstance != null)
	{
		Debug.Log("[BulletTime] Destroying Dolly Bullet Cart: " + dollyInstance.name);
		Destroy(dollyInstance.gameObject);
		dollyInstance = null;
	}
	if (trackInstance != null)
	{
		Debug.Log("[BulletTime] Destroying Bullet Track: " + trackInstance.name);
		Destroy(trackInstance.gameObject);
		trackInstance = null;
	}

	Transform hitTransform = activeBullet.GetHitEnemyTransform();
	if (hitTransform == null)
	{
		Debug.LogWarning("[BulletTime] No enemy hit. Skipping enemy dolly camera.");
	}
	else
	{
		Debug.Log("[BulletTime] Enemy hit: " + hitTransform.name);
		Quaternion rotation = Quaternion.Euler(Vector3.up * activeBullet.transform.rotation.eulerAngles.y);
		var selectedTrackingSetup = SelectTrackingSetup(hitTransform, enemyTrackingSetup, rotation);

		if (selectedTrackingSetup != null)
		{
			Debug.Log("[BulletTime] Creating enemy path and dolly for enemy: " + hitTransform.name);
			CreateEnemyPath(hitTransform, activeBullet.transform, selectedTrackingSetup.avaliableTrack);
			CreateDolly(selectedTrackingSetup);
			if (dollyInstance != null && trackInstance != null)
			{
				dollyInstance.InitDolly(trackInstance, hitTransform.transform);
				timeScaleController.SlowDownTime();
			}
			else
			{
				Debug.LogError("[BulletTime] Dolly Enemy atau Track Enemy gagal dibuat!");
			}
		}
		else
		{
			Debug.LogWarning("[BulletTime] No enemy tracking setup found for enemy hit.");
		}
	}

	StartCoroutine(FinishSequence());
}


   private void DestroyCinemachineSetup()
   {
	   if (trackInstance != null)
	   {
		   Debug.Log("[BulletTime] Destroying trackInstance: " + trackInstance.name);
		   Destroy(trackInstance.gameObject);
		   trackInstance = null;
	   }
	   else
	   {
		   Debug.LogWarning("[BulletTime] trackInstance already null or destroyed.");
	   }
	   if (dollyInstance != null)
	   {
		   Debug.Log("[BulletTime] Destroying dollyInstance: " + dollyInstance.name);
		   Destroy(dollyInstance.gameObject);
		   dollyInstance = null;
	   }
	   else
	   {
		   Debug.LogWarning("[BulletTime] dollyInstance already null or destroyed.");
	   }
   }

private IEnumerator FinishSequence()
{
   yield return new WaitForSecondsRealtime(finishingCameraDuration);

   cameraBrain.gameObject.SetActive(false);
   shootController.gameObject.SetActive(true);
   canvas.gameObject.SetActive(true);
   timeScaleController.SpeedUpTime();

   // Destroy dolly & track sebelum bullet di-destroy
   DestroyCinemachineSetup();

   // Cek apakah bullet masih ada sebelum menghancurkannya
   if (activeBullet != null)
   {
	   Debug.Log("[BulletTime] Destroying activeBullet: " + activeBullet.name);
	   activeBullet.ForceDestroy();
	   activeBullet = null;
   }
   else
   {
	   Debug.LogWarning("[BulletTime] activeBullet already null or destroyed.");
   }

   // Debugging: pastikan dollyInstance dan trackInstance sudah null
   if (dollyInstance != null)
	   Debug.LogError("[BulletTime] dollyInstance masih ada setelah bullet dihancurkan!");
   if (trackInstance != null)
	   Debug.LogError("[BulletTime] trackInstance masih ada setelah bullet dihancurkan!");

   // Cek gameover setelah animasi selesai (untuk peluru terakhir yang kena musuh)
   CheckGameOverAfterAnimation();

   ResetVariables();
}

// Method untuk cek gameover setelah animasi bullet time selesai
private void CheckGameOverAfterAnimation()
{
    var shootController = FindObjectOfType<ShootController>();
    if (shootController != null)
    {
        var currentAmmoField = shootController.GetType().GetField("currentAmmo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        int currentAmmo = 0;
        if (currentAmmoField != null)
        {
            currentAmmo = (int)currentAmmoField.GetValue(shootController);
        }
        
        var gameManager = GameManager.Instance;
        if (currentAmmo == 0 && gameManager.maxAmmoReserve == 0)
        {
            var zombiesLeftField = gameManager.GetType().GetField("zombiesLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int zombiesLeft = 0;
            if (zombiesLeftField != null)
            {
                zombiesLeft = (int)zombiesLeftField.GetValue(gameManager);
            }
            
            if (zombiesLeft > 0)
            {
                gameManager.GameOver(false); // Kalah
            }
            // Jika zombiesLeft == 0, menang sudah dicek di OnZombieKilled
        }
    }
}

	private void ResetVariables()
	{
		isLastCameraActive = false;
		trackInstance = null;
		dollyInstance = null;
		activeBullet = null;
		clearTracks.Clear();
		targetPosition = Vector3.zero;
	}
}