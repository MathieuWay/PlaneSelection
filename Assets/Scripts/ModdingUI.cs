using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum Method{
    AngleFromCenter,
    ClosestFromEllipse
}
public class ModdingUI : MonoBehaviour
{
    public bool drawDebugEllipse;
    public Method method;
    [Space(10)]
    [Header("Reference Camera / Target")]
    public Camera camera;
    public Transform target;
    [Space(10)]
    [Header("Ellipse Size")]
    [Range(100, 900)]
    public float horizontalRadius;
    [Range(100, 500)]
    public float VerticalRadius;
    [Range(4, 250)]
    public int verticesCount;
    [Space(10)]
    [Header("Ellipse Geometry Quality")]
    [Range(8, 32)]
    public int samplePointsCount;
    [Range(0, 6)]
    public int precision;
    [Space(10)]
    [Header("UI")]
    public GameObject linePrefab;
    [Range(5, 90)]
    public float uiSize;
    private LineRenderer ellipse;
    private List<Transform> transformArray;
    private LineRenderer[] lineArray;
    private List<float> anglesSelected;

    private void OnValidate()
    {
        //SetLine(Vector3.zero, angle);
    }

    private void Start()
    {
        ellipse = GetComponent<LineRenderer>();
        if(target)
            LoadTarget();
        DrawEllipse();
    }

    private void Update()
    {
        if (drawDebugEllipse && !ellipse.enabled)
            ellipse.enabled = true;
        else if(!drawDebugEllipse && ellipse.enabled)
            ellipse.enabled = false;
        anglesSelected.Clear();
        for (int i = 0; i < transformArray.Count; i++)
        {
            SetLineFromTransform(i, transformArray[i]);
        }
    }

    private void SetLineFromTransform(int index, Transform transform)
    {
        Vector3 screenPos = camera.WorldToScreenPoint(transform.position);
        screenPos.x -= Screen.width / 2;
        screenPos.y -= Screen.height / 2;
        screenPos.z = 0;    
        float angleFromCenter = Mathf.Atan2(screenPos.normalized.y, screenPos.normalized.x) * Mathf.Rad2Deg;

        lineArray[index].SetPosition(0, screenPos);
        Vector3 endPoint = Vector3.zero;
        switch (method)
        {
            case Method.AngleFromCenter:
                //Debug.Log("AngleFromCenter angle:"+ angleFromCenter);
                endPoint = GetPointPositionFromAngleToEllipse(angleFromCenter);
                break;
            case Method.ClosestFromEllipse:
                endPoint = GetPointPositionFromAngleToEllipse(GetPointPositionFromPointToEllipse(screenPos));
                break;
        }
        lineArray[index].SetPosition(1, endPoint);
    }

    private Vector3 GetPointPositionFromAngleToEllipse(float angle)
    {
        float t = 0;
        if (90 < angle && angle <= 180)
        {
            t = Mathf.Atan(horizontalRadius * Mathf.Tan(angle * Mathf.PI / 180) / VerticalRadius) + Mathf.PI;
        }
        else if (-90 <= angle && angle <= 90)
        {
            t = Mathf.Atan(horizontalRadius * Mathf.Tan(angle * Mathf.PI / 180) / VerticalRadius);
        }
        else if (-180 <= angle && angle < -90)
        {
            t = Mathf.Atan(horizontalRadius * Mathf.Tan(angle * Mathf.PI / 180) / VerticalRadius) - Mathf.PI;
        }
        return new Vector3(horizontalRadius * Mathf.Cos(t), VerticalRadius * Mathf.Sin(t), 0);
    }

    private float GetPointPositionFromPointToEllipse(Vector2 point)
    {
        //Debug.Log(point.ToString());
        float angle = 0;
        if(point.x > 0 && point.y > 0)
            angle = ArcPrecision(point, 0, 90, samplePointsCount, precision);
        else if (point.x > 0 && point.y < 0)
            angle = ArcPrecision(point, -90, 0, samplePointsCount, precision);
        else if (point.x < 0 && point.y < 0)
            angle = ArcPrecision(point, -180, -90, samplePointsCount, precision);
        else
            angle = ArcPrecision(point, 90, 180, samplePointsCount, precision);
        anglesSelected.Add(angle);
        return angle;
    }

    private float ArcPrecision(Vector2 point, float startAngle, float endAngle, int SamplePointCount, int currentPrecision)
    {
        int i = 0;
        float shortest = Mathf.Infinity;
        float shortestIndex = -1;
        float stepAngle = (endAngle - startAngle) / SamplePointCount;
        for (float currentAngle = startAngle; currentAngle <= endAngle; currentAngle += stepAngle)
        {
            //currentAngle += (endAngle - startAngle) / SamplePointCount * i;
            Vector3 samplePoint = GetPointPositionFromAngleToEllipse(currentAngle);
            //Debug.Log(currentAngle.ToString() + "pos:" + samplePoint.ToString() +"/distance("+ Vector2.Distance(point, samplePoint)+")");
            if (Vector2.Distance(point, samplePoint) < shortest && !isAngleOverlapp(currentAngle))
            {
                shortest = Vector2.Distance(point, samplePoint);
                shortestIndex = i;
            }
            i++;
        }
        //Debug.Log("start:" + startAngle + "     /    end:" + endAngle + "   /   step:" + stepAngle + "  /   shortest:" + (startAngle + (stepAngle * shortestIndex)));
        if (currentPrecision > 0)
        {
            return ArcPrecision(point,  startAngle + (stepAngle * (shortestIndex-1)), startAngle + (stepAngle * (shortestIndex + 1)), SamplePointCount, currentPrecision - 1);
        }
        else
        {
            //return new Vector3(horizontalRadius * Mathf.Cos(startAngle + (stepAngle * shortestIndex)), VerticalRadius * Mathf.Sin(startAngle + (stepAngle * shortestIndex)), 0);
            //Debug.Log("Closest Point angle:"+ (startAngle + (stepAngle * shortestIndex)));
            return startAngle + (stepAngle * shortestIndex);
        }
    }

    private bool isAngleOverlapp(float angle)
    {
        int i = 0;
        while (i < anglesSelected.Count)
        {
            //Debug.Log((anglesSelected[i] - uiSize / 2).ToString() + " <=" + angle.ToString() + "&&" + angle.ToString() + "<=" + (anglesSelected[i] + uiSize / 2).ToString());
            
            if (anglesSelected[i] - uiSize / 2 <= angle && angle <= anglesSelected[i] + uiSize / 2)
                return true;
            i++;
        }
        return false;
    }

    public void LoadTarget(bool isReload = true)
    {
        if (isReload)
        {
            foreach (Transform transform in transform)
                Destroy(transform.gameObject);
        }

        anglesSelected = new List<float>();
        transformArray = new List<Transform>();
        foreach (Transform tr in target)
        {
            if (tr.tag == "Component")
            {
                transformArray.Add(tr);
            }
        }
        lineArray = new LineRenderer[transformArray.Count];
        for (int i = 0; i < transformArray.Count; i++)
        {
            lineArray[i] = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
        }
    }

    private void DrawEllipse()
    {
        Vector3 position = Vector3.zero;
        ellipse.positionCount = verticesCount;
        for (int i = 0; i < verticesCount; i++)
        {
            float angle = Mathf.PI * 2 / verticesCount * i;
            position = Vector3.zero;
            position = new Vector3(horizontalRadius * Mathf.Cos(angle), VerticalRadius * Mathf.Sin(angle));
            ellipse.SetPosition(i, position);
        }
    }
}
