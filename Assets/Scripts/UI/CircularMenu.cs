using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CircularMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    public float radius = 150f;
    public float animationDuration = 0.3f;
    public Ease animationEase = Ease.OutBack;
    public GameObject buttonPrefab;

    [Header("Fade Settings")]
    public float fadeDuration = 0.25f;

    [System.Serializable]
    public class MenuOption
    {
        public Sprite icon;
        public string name;
    }

    [Header("Options")]
    public List<MenuOption> options;

    private List<CircularButton> spawnedButtons = new List<CircularButton>();
    private bool isOpen = false;

    [SerializeField] ARMenuManager menuManager;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ToggleMenu()
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (isOpen) return;
        isOpen = true;
        gameObject.SetActive(true);

        // Fade in panel
        canvasGroup.DOFade(1f, fadeDuration).From(0f);

        SpawnButtons();

        foreach (var btn in spawnedButtons)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();

            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.zero;

            rt.DOAnchorPos(btn.TargetPos, animationDuration).SetEase(animationEase);
            rt.DOScale(Vector3.one, animationDuration).SetEase(animationEase);
        }
    }

    public void CloseMenu()
    {
        if (!isOpen) return;
        isOpen = false;

        foreach (var btn in spawnedButtons)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();

            rt.DOAnchorPos(Vector2.zero, animationDuration).SetEase(Ease.InBack);
            rt.DOScale(Vector3.zero, animationDuration).SetEase(Ease.InBack)
              .OnComplete(() => Destroy(btn.gameObject));
        }

        spawnedButtons.Clear();

        canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
        {
            if (!isOpen) gameObject.SetActive(false);
        });
    }

    void SpawnButtons()
    {
        foreach (var b in spawnedButtons)
            Destroy(b.gameObject);
        spawnedButtons.Clear();

        int count = options.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, transform);
            CircularButton cb = btnObj.GetComponent<CircularButton>();

            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 targetPos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            cb.Setup(options[i], targetPos, menuManager, this, i);

            spawnedButtons.Add(cb);
        }
    }
}
