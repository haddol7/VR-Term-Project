using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    public void InsideTrash(GameObject go)
    {
        go.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TriggerZone>().OnEnterEvent.AddListener(InsideTrash);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
