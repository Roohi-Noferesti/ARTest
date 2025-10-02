using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CircularButton : MonoBehaviour
{
    public Vector2 TargetPos { get; private set; }

    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;

    public void Setup(CircularMenu.MenuOption option, Vector2 targetPos, ARMenuManager menuManager, CircularMenu circularMenu, int menuObjectIndex)
    {
        TargetPos = targetPos;

        if (button == null) button = GetComponent<Button>();
        if (iconImage == null) iconImage = GetComponent<Image>();

        iconImage.sprite = option.icon;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(() =>
        {
            menuManager.SetObjectToSpawn(menuObjectIndex);
            circularMenu.CloseMenu();
        });
    }
}
