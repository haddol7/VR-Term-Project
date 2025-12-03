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

    // 0번: 원래 과일
    // 1번: 한번 잘린 조각
    // => maxSliceCount = 2 이면 "한 번 더" 까지만 허용
    public int maxSliceCount = 2;

    void Update()
    {
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
            ~0,
            QueryTriggerInteraction.Ignore
        );

        Color debugColor = hasHit ? Color.green : Color.red;

        // 파라미터: (시작위치, 방향 * 길이, 색상, 지속시간)
        Debug.DrawRay(startSlicingPoint.position, slicingDirection * (distance + 10), debugColor, 2.0f);

        if (hasHit)
        {
            
            if (hit.transform.gameObject.layer == 9) // Bomb
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
            else
            {
                Slice(hit.transform.gameObject, hit.point, velocityEstimator.GetVelocityEstimate());
            }
            Debug.Log("Test for slice");
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


        Debug.Log("WE SLICE THE OBJECT");

        Vector3 slicingDirection = endSlicingPoint.position - startSlicingPoint.position;
        Vector3 planeNormal = Vector3.Cross(slicerVelocity, slicingDirection);

        SlicedHull hull = target.Slice(planePosition, planeNormal, slicedMaterial);

        if (hull != null)
        {
            DisplayScore.score++;

            GameObject upperHull = hull.CreateUpperHull(target, slicedMaterial);
            GameObject lowerHull = hull.CreateLowerHull(target, slicedMaterial);

            // 이번에 한 번 잘렸으니까 +1
            meta.sliceCount++;

            // 조각에도 정보/레이어/태그 복사
            CreateSlicedComponent(upperHull, target, meta.sliceCount);
            CreateSlicedComponent(lowerHull, target, meta.sliceCount);

            Destroy(target);
        }
    }

    void CreateSlicedComponent(GameObject slicedHull, GameObject original, int sliceCount)
    {
        // 레이어/태그는 원본 그대로 (다시 잘릴 수 있게)
        slicedHull.layer = original.layer;
        slicedHull.tag = original.tag;

        // 몇 번 잘렸는지 정보 복사
        SliceMeta meta = slicedHull.AddComponent<SliceMeta>();
        meta.sliceCount = sliceCount;

        // 물리 세팅
        Rigidbody rb = slicedHull.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        MeshCollider collider = slicedHull.AddComponent<MeshCollider>();
        collider.convex = true;          // 동적 MeshCollider 필수

        // 🔥 XR Grab Interactable 추가 (조각을 손으로 잡기 위함)
        XRGrabInteractable grab = slicedHull.AddComponent<XRGrabInteractable>();

        // XR Interaction Manager 직접 연결 (버전에 따라 자동 연결이 안 될 수 있음)
        XRInteractionManager manager = FindObjectOfType<XRInteractionManager>();
        if (manager != null)
        {
            grab.interactionManager = manager;
        }

        // 자연스럽게 손을 따라오게
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach = true;
        grab.trackPosition = true;
        grab.trackRotation = true;

        // (Interaction Layer는 일단 기본값 그대로 사용)

        // 약간 튕겨나가게 힘 주기
        rb.AddExplosionForce(
            slicedObjectInitialVelocity,
            slicedHull.transform.position,
            1f
        );

    }

}