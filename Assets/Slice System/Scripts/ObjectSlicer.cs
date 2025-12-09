using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

public class ObjectSlicer : MonoBehaviour
{
    public float slicedObjectInitialVelocity = 100;
    public Material slicedMaterial;
    public Transform startSlicingPoint;
    public Transform endSlicingPoint;
    public LayerMask sliceableLayer;          // 자를 레이어 (예: Sliceable)
    public VelocityEstimator velocityEstimator;

    // 0번: 원본, 1번: 한 번 잘린 조각 …
    public int maxSliceCount = 2;

    // 🔥 너무 잘 잘리는 거 막기용 (쿨타임)
    public float sliceCooldown = 0.15f;       // 한 번 자르고 나서 쉬는 시간(초)
    private bool canSlice = true;             // 지금 슬라이스 가능 여부

    void Update()
    {
        // 쿨타임 중이면 이번 프레임은 슬라이스 안 함
        if (!canSlice)
            return;

        // 시작~끝 사이 방향/거리 계산
        Vector3 slicingDirection = endSlicingPoint.position - startSlicingPoint.position;
        float distance = slicingDirection.magnitude;
        if (distance <= 0.001f)
            return;

        slicingDirection.Normalize();

        RaycastHit hit;
        bool hasHit = Physics.Raycast(
            startSlicingPoint.position,
            slicingDirection,
            out hit,
            distance,
            sliceableLayer,                    // ★ 이 레이어 마스크에 포함된 것만 맞음
            QueryTriggerInteraction.Ignore
        );

        // 디버그 레이 (Scene 뷰에서만 보임)
        Color debugColor = hasHit ? Color.green : Color.red;
        Debug.DrawRay(startSlicingPoint.position, slicingDirection * distance, debugColor, 0.1f);

        if (hasHit)
        {
            GameObject hitObj = hit.transform.gameObject;

            // 레이어 9 = Bomb 이면 바로 씬 리셋
            if (hitObj.layer == 9)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                return;
            }

            // 속도 추정 (VelocityEstimator null이어도 안전하게)
            Vector3 sliceVelocity;
            if (velocityEstimator != null)
                sliceVelocity = velocityEstimator.GetVelocityEstimate();
            else
                sliceVelocity = (endSlicingPoint.position - startSlicingPoint.position) /
                                Mathf.Max(Time.deltaTime, 0.0001f);

            Slice(hitObj, hit.point, sliceVelocity);
            StartCoroutine(SliceCooldown());          // 🔥 한 번 자르고 쿨타임 시작

            Debug.Log("Test for slice : " + hitObj.name);
        }
    }

    void Slice(GameObject target, Vector3 planePosition, Vector3 slicerVelocity)
    {
        // 잘린 횟수 체크
        SliceMeta meta = target.GetComponent<SliceMeta>();
        if (meta == null)
        {
            meta = target.AddComponent<SliceMeta>();
            meta.sliceCount = 0;
        }

        // 더 이상 자르고 싶으면 이 if 지우거나 maxSliceCount 크게 올리기
        if (maxSliceCount >= 0 && meta.sliceCount >= maxSliceCount)
        {
            Debug.Log("Max slice reached for " + target.name);
            return;
        }

        Debug.Log("WE SLICE THE OBJECT : " + target.name);

        Vector3 slicingDirection = endSlicingPoint.position - startSlicingPoint.position;
        Vector3 planeNormal = Vector3.Cross(slicerVelocity, slicingDirection);

        if (planeNormal == Vector3.zero)
        {
            planeNormal = Vector3.Cross(slicingDirection, Vector3.up);
        }

        SlicedHull hull = target.Slice(planePosition, planeNormal, slicedMaterial);

        if (hull != null)
        {
            DisplayScore.score++;

            GameObject upperHull = hull.CreateUpperHull(target, slicedMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, slicedMaterial);

            int newCount = meta.sliceCount + 1;

            CreateSlicedComponent(upperHull, original: target, sliceCount: newCount);
            CreateSlicedComponent(lowerHull, original: target, sliceCount: newCount);

            Destroy(target);
        }
        else
        {
            Debug.LogWarning("Hull is null, mesh might not be readable : " + target.name);
        }
    }

    void CreateSlicedComponent(GameObject slicedHull, GameObject original, int sliceCount)
    {
        // 위치/회전/스케일 복사
        slicedHull.transform.position = original.transform.position;
        slicedHull.transform.rotation = original.transform.rotation;
        slicedHull.transform.localScale = original.transform.localScale;

        // 레이어/태그 복사 (계속 Sliceable 유지)
        slicedHull.layer = original.layer;
        slicedHull.tag = original.tag;

        // 잘린 횟수 정보 붙이기
        SliceMeta meta = slicedHull.AddComponent<SliceMeta>();
        meta.sliceCount = sliceCount;

        // Rigidbody
        Rigidbody rb = slicedHull.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 콜라이더 – EzySlice용 MeshCollider
        MeshCollider collider = slicedHull.AddComponent<MeshCollider>();
        collider.convex = true;

        // XR로 잡을 수 있게
        XRGrabInteractable grab = slicedHull.AddComponent<XRGrabInteractable>();

        XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
        if (manager != null)
        {
            grab.interactionManager = manager;
        }

        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach = true;
        grab.trackPosition = true;
        grab.trackRotation = true;

        // 약간 튕겨 나가게
        rb.AddExplosionForce(
            slicedObjectInitialVelocity,
            slicedHull.transform.position,
            1f
        );
    }

    IEnumerator SliceCooldown()
    {
        canSlice = false;
        yield return new WaitForSeconds(sliceCooldown);
        canSlice = true;
    }
}
