using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGScript : MonoBehaviour
{
    [SerializeField] private GameObject BG1;
    [SerializeField] private GameObject BG2;

    [SerializeField] private float Speed = 1;

    private Vector3 sp;

    private void Start()
    {
        sp = BG2.transform.localPosition;

        BG1.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        BG1.GetComponent<SpriteRenderer>().sortingOrder = 0;
        BG2.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        BG2.GetComponent<SpriteRenderer>().sortingOrder = 0;
    }

    private void FixedUpdate()
    {
        BG1.transform.localPosition += Vector3.left * Speed;
        BG2.transform.localPosition += Vector3.left * Speed;

        if (BG1.transform.localPosition.x < -24f) BG1.transform.localPosition = sp;
        if (BG2.transform.localPosition.x < -24f) BG2.transform.localPosition = sp;

    }
}
