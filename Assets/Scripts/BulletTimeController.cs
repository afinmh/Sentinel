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

		CreateBulletPath(activeBullet.transform, selectedTrackingSetup.avaliableTrack);
		CreateDolly(selectedTrackingSetup);
		cameraBrain.gameObject.SetActive(true);
		shootController.gameObject.SetActive(false);
		canvas.gameObject.SetActive(false);
		float speed = CalculateDollySpeed();
		dollyInstance.InitDolly(trackInstance, activeBullet.transform, speed);
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
		Quaternion rotation = Quaternion.Euler(Vector3.up * bulletTransform.root.eulerAngles.y);
		trackInstance = Instantiate(selectedPath.path, enemytransform.position, rotation);
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

    DestroyCinemachineSetup();

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
            dollyInstance.InitDolly(trackInstance, hitTransform.transform);
            timeScaleController.SlowDownTime();
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
		Destroy(trackInstance.gameObject);
		Destroy(dollyInstance.gameObject);
	}

	private IEnumerator FinishSequence()
	{
		yield return new WaitForSecondsRealtime(finishingCameraDuration);

		cameraBrain.gameObject.SetActive(false);
		shootController.gameObject.SetActive(true);
		canvas.gameObject.SetActive(true);
		timeScaleController.SpeedUpTime();

		DestroyCinemachineSetup();

		// Cek apakah bullet masih ada sebelum menghancurkannya
		if (activeBullet != null)
		{
			Destroy(activeBullet.gameObject);
		}

		ResetVariables();
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