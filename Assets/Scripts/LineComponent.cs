using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineComponent : MonoBehaviour
{
    public Vector3 offset;
    private LineRenderer line;
    private RectTransform tile;
    private RectTransform anchor;


    // Start is called before the first frame update
    private void Start()
    {
        line = GetComponent<LineRenderer>();
        anchor = transform.GetChild(0).GetComponent<RectTransform>();
        tile = transform.GetChild(1).GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void Update()
    {
        anchor.localPosition = line.GetPosition(0);
        tile.localPosition = line.GetPosition(1) + offset;
    }
}
