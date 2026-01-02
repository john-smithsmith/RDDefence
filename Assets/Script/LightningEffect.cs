using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    public void Show(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        float duration = 0.2f; 
        float timer = 0f;
        float startWidth = 0.15f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float currentWidth = Mathf.Lerp(startWidth, 0f, t);
            lineRenderer.startWidth = currentWidth;
            lineRenderer.endWidth = currentWidth;

            yield return null;
        }

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}