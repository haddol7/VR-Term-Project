using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RockSmasher : MonoBehaviour
{
    [Header("������ ����Ʈ")]
    public List<GameObject> brokenPieces;

    [Header("����")]
    public float breakForce = 15.0f;
    public float explosionForce = 100f;
    public float scatterRadius = 0.5f;

    void OnCollisionEnter(Collision collision)
    {
        //if (collision.gameObject.CompareTag("BreakingStone"))
        //{
        //    // 2. Ÿ�� �ӵ� ��� �� ���
        //    float hitSpeed = collision.relativeVelocity.magnitude;
        //    Debug.Log("���� Ÿ�� �ӵ�: " + hitSpeed + " (����: " + breakForce + ")");

        //    if (hitSpeed >= breakForce)
        //    {
        //        Debug.Log("�ı� ����!");
        //        StartCoroutine(SmashRoutine());
        //    }
        //    else
        //    {
        //        Debug.Log("�ʹ� ��� ������");
        //    }
        //}

            float hitSpeed = collision.relativeVelocity.magnitude;

            if (hitSpeed >= breakForce)
            {
                StartCoroutine(SmashRoutine());
            }
    }

    IEnumerator SmashRoutine()
    {
        Renderer myRenderer = GetComponentInChildren<Renderer>();
        if (myRenderer != null)
        {
            myRenderer.enabled = false; 
        }

        Collider myCollider = GetComponentInChildren<Collider>();
        if (myCollider != null)
        {
            myCollider.enabled = false; // �浹 ���
        }

        foreach (GameObject piece in brokenPieces)
        {
            if (piece != null)
            {
                Vector3 randomPos = transform.position + (Random.insideUnitSphere * scatterRadius);

                piece.transform.position = randomPos;
                piece.transform.rotation = Random.rotation; // ȸ���� �����ϰ�

                piece.SetActive(true);
                piece.transform.parent = null;

                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, 1f);
                }

                yield return new WaitForSeconds(0.01f);
            }
        }

        gameObject.SetActive(false);
    }
}