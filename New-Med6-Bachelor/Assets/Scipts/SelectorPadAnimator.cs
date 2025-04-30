using UnityEngine;

public class SelectorPadAnimator : MonoBehaviour
{
    public float floatDistance = 0.2f;
    public float floatDuration = 0.5f;
    public float fadeDuration = 0.5f;

    private Vector3 startPos;
    private Vector3 endPos;
    private CanvasGroup canvasGroup;
    private Renderer padRenderer;

    private void Awake()
    {
        startPos = transform.position - new Vector3(0, floatDistance, 0);
        endPos = transform.position;

        transform.position = startPos;

        padRenderer = GetComponentInChildren<Renderer>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        StartCoroutine(FloatUpAndFadeIn());
    }

    private System.Collections.IEnumerator FloatUpAndFadeIn()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / floatDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        transform.position = endPos;
        canvasGroup.alpha = 1f;
    }
}
