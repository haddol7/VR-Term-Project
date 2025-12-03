using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pen : MonoBehaviour
{
    [Header("Pen Properties")]
    public Transform tip;
    public Material tipMaterial;
    public GameObject sapViewer;

    [Header("Sap Generation Settings")]
    public GameObject sapPrefab;
    public float sapSpawnRate = 1.0f;
    public float sapAmount = 1.0f;

    private Vector3 lastTipPosition;
    private float lastSpawnTime;
    private int currentColorIndex;
    private bool drawActive = false;

    private void Start()
    {
        lastTipPosition = tip.position;
        lastSpawnTime = 0f;

        XRGrabInteractable grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(x => ActiveDraw());
            grabInteractable.deactivated.AddListener(x => StopDraw());
        }
    }

    private void Update()
    {
        if (drawActive) Draw();
    }

    private void ActiveDraw()
    {
        drawActive = true;
        lastTipPosition = tip.position;
    }

    private void StopDraw()
    {
        drawActive = false;
    }

    private void Draw()
    {
        if (Time.time - lastSpawnTime < sapSpawnRate)
        {
            lastTipPosition = tip.position;
            lastSpawnTime = Time.time;
            return;
        }

        Vector3 currentPosition = tip.position;
        Vector3 direction = currentPosition - lastTipPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(currentPosition, tip.forward, out RaycastHit hit, 0.1f, ~0, QueryTriggerInteraction.Ignore))
        {
            SpawnSap(hit);
            sapAmount -= 0.5f;
            if (sapAmount < 0.0f)
            {
                sapAmount = 0.0f;
                sapViewer.SetActive(false);
            }
            else
            {
                Vector3 tempScale = sapViewer.transform.localScale;
                tempScale.z -= 0.1f;
                sapViewer.transform.localScale = tempScale;
            }
        }

        lastTipPosition = currentPosition;
        lastSpawnTime = Time.time;
    }

    private void SpawnSap(RaycastHit hit)
    {
        Vector3 spawnPosition = hit.point + (hit.normal * 0.005f);

        Quaternion baseRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

        float randomRollAngle = UnityEngine.Random.Range(0f, 360f);
        Quaternion randomRoll = Quaternion.AngleAxis(randomRollAngle, hit.normal);

        Quaternion finalRotation = randomRoll * baseRotation;

        GameObject newSap = Instantiate(sapPrefab, spawnPosition, finalRotation);

        newSap.transform.SetParent(hit.transform, true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Paint"))
        {

            if (sapAmount == 0.0f)
            {
                sapViewer.SetActive(true);
            }
            if (sapAmount < 3.0f)
            {
                sapAmount += 0.01f;
                Vector3 tempScale = sapViewer.transform.localScale;
                tempScale.z += 0.03f;
                sapViewer.transform.localScale = tempScale;
            }
        }
    }
}
