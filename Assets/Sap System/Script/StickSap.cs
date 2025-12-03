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
            rb.isKinematic = true;

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
                //나중에 동작에 문제가 생겼을 때 코루틴으로 할 것 
                //StartCoroutine(ReregisterInteractable(rootInteract));
                other.transform.SetParent(rootObject, true);
                Debug.Log("Sap : 붙었음");
                childInteract.interactionManager.UnregisterInteractable(childInteract.GetComponent<IXRInteractable>());
                Destroy(childInteract);
                rootInteract.movementType = XRBaseInteractable.MovementType.Kinematic;
                rootInteract.interactionManager.UnregisterInteractable(rootInteract.GetComponent<IXRInteractable>());
                rootInteract.interactionManager.RegisterInteractable(rootInteract.GetComponent<IXRInteractable>());
            }
            else
            {
                other.transform.SetParent(null, true);
                Debug.Log("Sap : 수액으로 붙을 물체가 없음 말이 안됨");
            }

            // 수액 전체 제거
            Destroy(transform.parent.gameObject);
        }
    }

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
