using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class ExplosionEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private int segments = 50;
    private float currentRadius;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
    }

    public void Show(Vector3 center, float radius)
    {
        transform.position = center; 
        currentRadius = radius;

        DrawCircle(); 
        StartCoroutine(FadeOutRoutine()); 
    }

    void DrawCircle()
    {
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * currentRadius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * currentRadius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));

            angle += (360f / segments);
        }
    }

    IEnumerator FadeOutRoutine()
    {
        float duration = 0.3f; 
        float timer = 0f;

        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;
        startColor.a = 1f; endColor.a = 1f; 
        lineRenderer.startWidth = 0.15f;
        lineRenderer.endWidth = 0.15f;
        transform.localScale = Vector3.one;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float alpha = Mathf.Lerp(1f, 0f, t);
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);

            float scale = Mathf.Lerp(1f, 1.1f, t);
            transform.localScale = Vector3.one * scale;

            yield return null;
        }

        if (PoolManager.Instance != null) PoolManager.Instance.ReturnToPool(gameObject);
        else Destroy(gameObject);
    }
}