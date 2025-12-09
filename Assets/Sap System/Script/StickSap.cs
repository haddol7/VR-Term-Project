using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StickSap : MonoBehaviour
{
    [Header("Settings")]
    public string targetTag = "Stickable";
    public float litetime = 10.0f;

    private float destroyTimer;

    private void Start()
    {
        destroyTimer = litetime;
    }

    public void Update()
    {
        if (destroyTimer > 0)
        {
            destroyTimer -= Time.deltaTime;

            if (destroyTimer <= 0)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            StickTarget(other);
        }
    }

    private void StickTarget(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true; // 붙는 자식(돌)은 물리 끄기 (잘 되어 있음)

            Transform rootObject = transform.parent.parent;
            if (rootObject && rootObject.parent != null) rootObject = rootObject.parent;

            if (rootObject != null)
            {
                if (rootObject == other.transform.parent)
                {
                    Debug.Log("Sap : 이미 붙어있음");
                    Destroy(transform.parent.gameObject);
                    return;
                }

                XRGrabInteractable childInteract = other.GetComponent<XRGrabInteractable>();
                if (childInteract == null) return;

                XRGrabInteractable rootInteract = rootObject.GetComponent<XRGrabInteractable>();
                if (rootInteract == null) return;

                if (childInteract.colliders != null)
                {
                    foreach (Collider col in childInteract.colliders)
                    {
                        rootInteract.colliders.Add(col);
                    }
                }
                childInteract.enabled = false;

                other.transform.SetParent(rootObject, true);
                Debug.Log("Sap : 붙었음");

                // 자식의 인터랙션 기능 제거
                childInteract.interactionManager.UnregisterInteractable(childInteract.GetComponent<IXRInteractable>());
                Destroy(childInteract);

                // ★★★ [수정된 부분] ★★★
                // 기존: rootInteract.movementType = XRBaseInteractable.MovementType.Kinematic; (이게 문제였음!)

                // 1. 무조건 'VelocityTracking'으로 설정해야 때릴 수 있음
                rootInteract.movementType = XRBaseInteractable.MovementType.VelocityTracking;

                // 2. 부모(무기)의 리지드바디가 혹시 꺼져있을까봐 강제로 켜줌
                Rigidbody rootRb = rootObject.GetComponent<Rigidbody>();
                if (rootRb != null)
                {
                    rootRb.isKinematic = false; // 물리 충돌 켜기
                    rootRb.useGravity = true;   // 중력 켜기 (떨어지게 하려면)
                }
                // ★★★★★★★★★★★★★★★★★

                // 변경된 설정(콜라이더 추가 등) 적용을 위해 새로고침
                rootInteract.interactionManager.UnregisterInteractable(rootInteract.GetComponent<IXRInteractable>());
                rootInteract.interactionManager.RegisterInteractable(rootInteract.GetComponent<IXRInteractable>());
            }
            else
            {
                other.transform.SetParent(null, true);
                Debug.Log("Sap : 수액으로 붙을 물체가 없음");
            }

            // 수액 전체 제거
            Destroy(transform.parent.gameObject);
        }
    }

    //private void StickTarget(Collider other)
    //{
    //    Rigidbody rb = other.GetComponent<Rigidbody>();

    //    if (rb != null)
    //    {
    //        rb.isKinematic = true;

    //        Transform rootObject = transform.parent.parent;
    //        if (rootObject && rootObject.parent != null) rootObject = rootObject.parent;

    //        if (rootObject != null)
    //        {
    //            if (rootObject == other.transform.parent)
    //            {
    //                Debug.Log("Sap : 이미 붙어있음");
    //                Destroy(transform.parent.gameObject);
    //                return;
    //            }

    //            XRGrabInteractable childInteract = other.GetComponent<XRGrabInteractable>();
    //            if (childInteract == null) return;

    //            XRGrabInteractable rootInteract = rootObject.GetComponent<XRGrabInteractable>();
    //            if (rootInteract == null) return; 

    //            if (childInteract.colliders != null)
    //            {
    //                foreach (Collider col in childInteract.colliders)
    //                {
    //                    rootInteract.colliders.Add(col);
    //                }
    //            }
    //            childInteract.enabled = false;
    //            //나중에 동작에 문제가 생겼을 때 코루틴으로 할 것 
    //            //StartCoroutine(ReregisterInteractable(rootInteract));
    //            other.transform.SetParent(rootObject, true);
    //            Debug.Log("Sap : 붙었음");
    //            childInteract.interactionManager.UnregisterInteractable(childInteract.GetComponent<IXRInteractable>());
    //            Destroy(childInteract);
    //            rootInteract.movementType = XRBaseInteractable.MovementType.Kinematic;
    //            rootInteract.interactionManager.UnregisterInteractable(rootInteract.GetComponent<IXRInteractable>());
    //            rootInteract.interactionManager.RegisterInteractable(rootInteract.GetComponent<IXRInteractable>());
    //        }
    //        else
    //        {
    //            other.transform.SetParent(null, true);
    //            Debug.Log("Sap : 수액으로 붙을 물체가 없음 말이 안됨");
    //        }

    //        // 수액 전체 제거
    //        Destroy(transform.parent.gameObject);
    //    }
    //}

    //나중에 동작에 문제가 생겼을 때 코루틴으로 할 것 
    private IEnumerator ReregisterInteractable(XRGrabInteractable rootInteract)
    {
        yield return new WaitForEndOfFrame();
        rootInteract.interactionManager.UnregisterInteractable(rootInteract as IXRInteractable);

        yield return new WaitForEndOfFrame();
        rootInteract.interactionManager.RegisterInteractable(rootInteract as IXRInteractable);

        yield return null;
    }

}
