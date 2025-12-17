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
    public LayerMask sliceableLayer;
    public VelocityEstimator velocityEstimator;

    // [추가 1] 최소 이 속도 이상으로 휘둘러야 잘림 (너무 느리면 안 잘림)
    public float minSliceVelocity = 2.0f;

    // [추가 2] 한 번 자르고 나면 0.5초 동안은 같은 물체 안 자름 (다단히트 방지)
    public float sliceCooldown = 0.5f;

    void Update()
    {
        Vector3 slicingDirection = endSlicingPoint.position - startSlicingPoint.position;
        float distance = slicingDirection.magnitude;
        if (distance <= 0.001f) return;

        slicingDirection.Normalize();

        RaycastHit hit;

        // SphereCast로 두께감 있게 감지
        bool hasHit = Physics.SphereCast(
            startSlicingPoint.position,
            0.05f,
            slicingDirection,
            out hit,
            distance,
            sliceableLayer,
            QueryTriggerInteraction.Ignore
        );

        if (hasHit)
        {
            // [핵심 수정] 속도 체크!
            // 칼의 속도가 설정한 값(2.0)보다 느리면 "그냥 닿은 것"으로 치고 무시함
            Vector3 velocity = velocityEstimator.GetVelocityEstimate();
            if (velocity.magnitude < minSliceVelocity)
            {
                return; // 살살 닿으면 안 자름
            }

            GameObject target = hit.transform.gameObject;

            // [핵심 수정] 쿨타임 체크!
            // 방금 자른 놈이면 또 자르지 않음
            SliceMeta meta = target.GetComponent<SliceMeta>();
            if (meta != null && Time.time - meta.lastSliceTime < sliceCooldown)
            {
                return;
            }

            Slice(target, hit.point, velocity);
        }
    }

    void Slice(GameObject target, Vector3 planePosition, Vector3 slicerVelocity)
    {
        // 너무 작은 조각 자르기 방지 (아까 추가한 것 유지)
        MeshRenderer renderer = target.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.bounds.size.magnitude < 0.1f)
        {
            return;
        }

        // --- 손에 쥐기 로직 유지 ---
        XRGrabInteractable currentGrab = target.GetComponent<XRGrabInteractable>();
        IXRSelectInteractor holdingInteractor = null;
        if (currentGrab != null && currentGrab.isSelected)
        {
            holdingInteractor = currentGrab.interactorsSelecting[0];
        }
        // -----------------------

        // 메타 데이터 및 쿨타임 갱신
        SliceMeta meta = target.GetComponent<SliceMeta>();
        if (meta == null)
        {
            meta = target.AddComponent<SliceMeta>();
            meta.sliceCount = 0;
        }
        meta.lastSliceTime = Time.time; // ★ 자른 시간 기록 (쿨타임 시작)

        Vector3 slicingDirection = endSlicingPoint.position - startSlicingPoint.position;
        Vector3 planeNormal = Vector3.Cross(slicerVelocity, slicingDirection);

        SlicedHull hull = target.Slice(planePosition, planeNormal, slicedMaterial);

        if (hull != null)
        {
            // DisplayScore.score++; // 필요하면 주석 해제

            GameObject upperHull = hull.CreateUpperHull(target, slicedMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, slicedMaterial);

            meta.sliceCount++;

            CreateSlicedComponent(upperHull, target, meta.sliceCount);
            CreateSlicedComponent(lowerHull, target, meta.sliceCount);

            if (holdingInteractor != null)
            {
                float distUpper = Vector3.Distance(holdingInteractor.transform.position, upperHull.GetComponent<Collider>().bounds.center);
                float distLower = Vector3.Distance(holdingInteractor.transform.position, lowerHull.GetComponent<Collider>().bounds.center);
                GameObject objectToGrab = (distUpper < distLower) ? upperHull : lowerHull;

                XRGrabInteractable newGrab = objectToGrab.GetComponent<XRGrabInteractable>();
                XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
                if (manager != null && newGrab != null)
                {
                    manager.SelectEnter(holdingInteractor, newGrab);
                }
            }

            Destroy(target);
        }
    }

    void CreateSlicedComponent(GameObject slicedHull, GameObject original, int sliceCount)
    {
        // 1. 레이어와 태그를 원본과 똑같이 (또 자를 수 있게)
        slicedHull.layer = original.layer;
        slicedHull.tag = original.tag;

        // 2. 메타 데이터(횟수, 시간) 기록
        SliceMeta meta = slicedHull.AddComponent<SliceMeta>();
        meta.sliceCount = sliceCount;
        meta.lastSliceTime = Time.time; // 쿨타임 초기화

        // 3. 물리(Rigidbody) 설정
        Rigidbody rb = slicedHull.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 4. 콜라이더 설정
        MeshCollider collider = slicedHull.AddComponent<MeshCollider>();
        collider.convex = true;

        // 5. 잡기(XR Grab) 설정
        XRGrabInteractable grab = slicedHull.AddComponent<XRGrabInteractable>();
        XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
        if (manager != null) grab.interactionManager = manager;
     
        // 에러 안 나게 리스트에 '추가'하는 방식으로 변경
        grab.colliders.Add(collider);
        

        // 6. 인터랙션 레이어 복사 (왼손/오른손 설정 유지)
        XRGrabInteractable originalGrab = original.GetComponent<XRGrabInteractable>();
        if (originalGrab != null)
        {
            grab.interactionLayers = originalGrab.interactionLayers;
        }

        // 7. 물리 움직임 설정 (Velocity Tracking)
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach = true;
        grab.trackPosition = true;
        grab.trackRotation = true;

        // 8. 튕겨나가는 힘 주기
        rb.AddExplosionForce(slicedObjectInitialVelocity, slicedHull.transform.position, 1f);
    }
}