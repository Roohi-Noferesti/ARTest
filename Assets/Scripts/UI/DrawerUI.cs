using System.Collections;
using UnityEngine;

public class DrawerUI : MonoBehaviour
{
    [SerializeField] RectTransform drawerPanel;
    [SerializeField] float slideDuration = 0.3f;

    bool isOpen = false;

    public void ToggleDrawer()
    {
        StopAllCoroutines();
        StartCoroutine(SlideDrawer(isOpen ? Vector2.down : Vector2.up));
        isOpen = !isOpen;
    }

    private IEnumerator SlideDrawer(Vector2 direction)
    {
        Vector2 startPos = drawerPanel.anchoredPosition;
        Vector2 endPos = direction == Vector2.up ? new Vector2(0, 0) : new Vector2(0, -drawerPanel.rect.height);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / slideDuration;
            drawerPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
    }
}
