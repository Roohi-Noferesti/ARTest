using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.ARStarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

/// <summary>
/// Handles dismissing the object menu when clicking out the UI bounds, and showing the
/// menu again when the create menu button is clicked after dismissal. Manages object deletion in the AR demo scene,
/// and also handles the toggling between the object creation menu button and the delete button.
/// </summary>
public class ARMenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Button that deletes a selected object.")]
    Button m_DeleteButton;

    /// <summary>
    /// Button that deletes a selected object.
    /// </summary>
    public Button deleteButton
    {
        get => m_DeleteButton;
        set => m_DeleteButton = value;
    }

    [SerializeField]
    [Tooltip("The menu with all the creatable objects.")]
    GameObject m_ObjectMenu;

    /// <summary>
    /// The menu with all the creatable objects.
    /// </summary>
    public GameObject objectMenu
    {
        get => m_ObjectMenu;
        set => m_ObjectMenu = value;
    }

    [SerializeField]
    [Tooltip("The modal with debug options.")]
    GameObject m_ModalMenu;

    /// <summary>
    /// The modal with debug options.
    /// </summary>
    public GameObject modalMenu
    {
        get => m_ModalMenu;
        set => m_ModalMenu = value;
    }

    [SerializeField]
    [Tooltip("The CircularMenu for the object creation menu.")]
    CircularMenu m_ObjectMenuCircularMenu;

    /// <summary>
    /// The CircularMenu for the object creation menu.
    /// </summary>
    public CircularMenu objectMenuCircularMenu
    {
        get => m_ObjectMenuCircularMenu;
        set => m_ObjectMenuCircularMenu = value;
    }

    [SerializeField]
    [Tooltip("The object spawner component in charge of spawning new objects.")]
    ObjectSpawner m_ObjectSpawner;

    /// <summary>
    /// The object spawner component in charge of spawning new objects.
    /// </summary>
    public ObjectSpawner objectSpawner
    {
        get => m_ObjectSpawner;
        set => m_ObjectSpawner = value;
    }

    [SerializeField]
    [Tooltip("Button that closes the object creation menu.")]
    Button m_CancelButton;

    /// <summary>
    /// Button that closes the object creation menu.
    /// </summary>
    public Button cancelButton
    {
        get => m_CancelButton;
        set => m_CancelButton = value;
    }

    [SerializeField]
    [Tooltip("The interaction group for the AR demo scene.")]
    XRInteractionGroup m_InteractionGroup;

    /// <summary>
    /// The interaction group for the AR demo scene.
    /// </summary>
    public XRInteractionGroup interactionGroup
    {
        get => m_InteractionGroup;
        set => m_InteractionGroup = value;
    }

    [SerializeField]
    [Tooltip("The slider for activating plane debug visuals.")]
    DebugSlider m_DebugPlaneSlider;

    /// <summary>
    /// The slider for activating plane debug visuals.
    /// </summary>
    public DebugSlider debugPlaneSlider
    {
        get => m_DebugPlaneSlider;
        set => m_DebugPlaneSlider = value;
    }

    [SerializeField]
    [Tooltip("The plane prefab with shadows and debug visuals.")]
    GameObject m_DebugPlane;

    /// <summary>
    /// The plane prefab with shadows and debug visuals.
    /// </summary>
    public GameObject debugPlane
    {
        get => m_DebugPlane;
        set => m_DebugPlane = value;
    }

    [SerializeField]
    [Tooltip("The plane manager in the AR demo scene.")]
    ARPlaneManager m_PlaneManager;

    /// <summary>
    /// The plane manager in the AR demo scene.
    /// </summary>
    public ARPlaneManager planeManager
    {
        get => m_PlaneManager;
        set => m_PlaneManager = value;
    }

    [SerializeField]
    [Tooltip("The AR debug menu.")]
    ARDebugMenu m_DebugMenu;

    /// <summary>
    /// The AR debug menu.
    /// </summary>
    public ARDebugMenu debugMenu
    {
        get => m_DebugMenu;
        set => m_DebugMenu = value;
    }

    [SerializeField]
    [Tooltip("The slider for activating the debug menu.")]
    DebugSlider m_DebugMenuSlider;

    /// <summary>
    /// The slider for activating the debug menu.
    /// </summary>
    public DebugSlider debugMenuSlider
    {
        get => m_DebugMenuSlider;
        set => m_DebugMenuSlider = value;
    }

    [SerializeField]
    XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

    /// <summary>
    /// Input to use for the screen tap start position.
    /// </summary>
    /// <seealso cref="TouchscreenGestureInputController.tapStartPosition"/>
    public XRInputValueReader<Vector2> tapStartPositionInput
    {
        get => m_TapStartPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
    }

    [SerializeField]
    XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

    /// <summary>
    /// Input to use for the screen tap start position.
    /// </summary>
    /// <seealso cref="TouchscreenGestureInputController.dragCurrentPosition"/>
    public XRInputValueReader<Vector2> dragCurrentPositionInput
    {
        get => m_DragCurrentPositionInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
    }

    bool m_IsPointerOverUI;
    bool m_ShowObjectMenu;
    bool m_ShowOptionsModal;
    bool m_InitializingDebugMenu;
    Vector2 m_ObjectButtonOffset = Vector2.zero;
    Vector2 m_ObjectMenuOffset = Vector2.zero;
    readonly List<ARFeatheredPlaneMeshVisualizerCompanion> featheredPlaneMeshVisualizerCompanions = new List<ARFeatheredPlaneMeshVisualizerCompanion>();

    Vector3 m_PendingSpawnPoint = Vector3.zero;
    Vector3 m_PendingSpawnNormal = Vector3.up;
    bool m_HasPendingSpawn = false;
    GameObject m_PreviewInstance = null;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnEnable()
    {
        m_CancelButton.onClick.AddListener(HideMenu);
        m_DeleteButton.onClick.AddListener(DeleteFocusedObject);
        m_PlaneManager.trackablesChanged.AddListener(OnPlaneChanged);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void OnDisable()
    {
        m_ShowObjectMenu = false;
        m_CancelButton.onClick.RemoveListener(HideMenu);
        m_DeleteButton.onClick.RemoveListener(DeleteFocusedObject);
        m_PlaneManager.trackablesChanged.RemoveListener(OnPlaneChanged);
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void Start()
    {
        // Auto turn on/off debug menu. We want it initially active so it calls into 'Start', which will
        // allow us to move the menu properties later if the debug menu is turned on.
        m_DebugMenu.gameObject.SetActive(true);
        m_InitializingDebugMenu = true;

        m_PlaneManager.planePrefab = m_DebugPlane;
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>.
    /// </summary>
    void Update()
    {
        if (m_InitializingDebugMenu)
        {
            m_DebugMenu.gameObject.SetActive(false);
            m_InitializingDebugMenu = false;
        }

        if (m_ShowObjectMenu || m_ShowOptionsModal)
        {
            if (!m_IsPointerOverUI && (m_TapStartPositionInput.TryReadValue(out _) || m_DragCurrentPositionInput.TryReadValue(out _)))
            {
                if (m_ShowObjectMenu)
                    //HideMenu();

                if (m_ShowOptionsModal)
                    m_ModalMenu.SetActive(false);
            }

            if (m_ShowObjectMenu)
            {
                m_DeleteButton.gameObject.SetActive(false);
            }
            else
            {
                m_DeleteButton.gameObject.SetActive(m_InteractionGroup?.focusInteractable != null);
            }

            m_IsPointerOverUI = IsPointerOverUI();
        }
        else
        {
            m_IsPointerOverUI = false;
            m_DeleteButton.gameObject.SetActive(m_InteractionGroup?.focusInteractable != null);
        }

        if (!m_IsPointerOverUI && m_ShowOptionsModal)
        {
            m_IsPointerOverUI = IsPointerOverUI();
        }
    }

    /// <summary>
    /// Set the index of the object in the list on the ObjectSpawner to a specific value.
    /// This is effectively an override of the default behavior or randomly spawning an object.
    /// </summary>
    /// <param name="objectIndex">The index in the array of the object to spawn with the ObjectSpawner</param>
    public void SetObjectToSpawn(int objectIndex)
    {
        if (m_ObjectSpawner == null)
        {
            Debug.LogWarning("Object Spawner not configured correctly: no ObjectSpawner set.");
            return;
        }

        if (m_ObjectSpawner.objectPrefabs.Count <= objectIndex)
        {
            Debug.LogWarning("Object Spawner not configured correctly: object index larger than number of Object Prefabs.");
            return;
        }

        // set which prefab will be spawned
        m_ObjectSpawner.spawnOptionIndex = objectIndex;

        // if we have a stored spawn pose, attempt to spawn now
        if (m_HasPendingSpawn)
        {
            // spawn at stored pose
            bool spawned = m_ObjectSpawner.TrySpawnObject(m_PendingSpawnPoint, m_PendingSpawnNormal);

            // destroy preview if any
            if (m_PreviewInstance != null)
            {
                Destroy(m_PreviewInstance);
                m_PreviewInstance = null;
            }

            if (spawned)
            {
                m_HasPendingSpawn = false;
                HideMenu(); // close menu after successful spawn
            }
            else
            {
                // spawning may fail because the spawn point is out of camera view or other checks.
                Debug.LogWarning("Unable to spawn object at stored pose (out of view or invalid). Move the camera or retap the surface.");
                // keep the menu open so the user can try again (or retap)
            }
        }
        else
        {
            // No pending spawn — just hide menu (or you could keep open to choose a default behavior)
            HideMenu();
        }
    }

    public void OpenMenuAt(Vector3 spawnPoint, Vector3 spawnNormal)
    {
        // store pending spawn
        m_PendingSpawnPoint = spawnPoint;
        m_PendingSpawnNormal = spawnNormal;
        m_HasPendingSpawn = true;

        // show a preview visualization if available on the ObjectSpawner
        if (m_ObjectSpawner != null && m_ObjectSpawner.spawnVisualizationPrefab != null)
        {
            if (m_PreviewInstance != null)
                Destroy(m_PreviewInstance);

            m_PreviewInstance = Instantiate(m_ObjectSpawner.spawnVisualizationPrefab);
            m_PreviewInstance.transform.position = spawnPoint;
            // orient preview to face the camera similar to spawning
            var facePosition = (m_ObjectSpawner.cameraToFace != null) ? m_ObjectSpawner.cameraToFace.transform.position : Camera.main.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            m_PreviewInstance.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);
        }

        ShowMenu();
    }

    void ShowMenu()
    {
        m_ShowObjectMenu = true;
        m_ObjectMenu.SetActive(true);
        m_ObjectMenuCircularMenu.ToggleMenu();
        AdjustARDebugMenuPosition();
    }

    /// <summary>
    /// Shows or hides the menu modal when the options button is clicked.
    /// </summary>
    public void ShowHideModal()
    {
        if (m_ModalMenu.activeSelf)
        {
            m_ShowOptionsModal = false;
            m_ModalMenu.SetActive(false);
        }
        else
        {
            m_ShowOptionsModal = true;
            m_ModalMenu.SetActive(true);
        }
    }

    /// <summary>
    /// Shows or hides the plane debug visuals.
    /// </summary>
    public void ShowHideDebugPlane()
    {
        if (m_DebugPlaneSlider.value == 1)
        {
            m_DebugPlaneSlider.value = 0;
            ChangePlaneVisibility(false);
        }
        else
        {
            m_DebugPlaneSlider.value = 1;
            ChangePlaneVisibility(true);
        }
    }

    /// <summary>
    /// Shows or hides the AR debug menu.
    /// </summary>
    public void ShowHideDebugMenu()
    {
        if (m_DebugMenu.gameObject.activeSelf)
        {
            m_DebugMenuSlider.value = 0;
            m_DebugMenu.gameObject.SetActive(false);
        }
        else
        {
            m_DebugMenuSlider.value = 1;
            m_DebugMenu.gameObject.SetActive(true);
            AdjustARDebugMenuPosition();
        }
    }

    /// <summary>
    /// Clear all created objects in the scene.
    /// </summary>
    public void ClearAllObjects()
    {
        foreach (Transform child in m_ObjectSpawner.transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Triggers hide animation for menu.
    /// </summary>
    public void HideMenu()
    {
        m_ObjectMenuCircularMenu.ToggleMenu();
        m_ShowObjectMenu = false;

        m_HasPendingSpawn = false;
        if (m_PreviewInstance != null)
        {
            Destroy(m_PreviewInstance);
            m_PreviewInstance = null;
        }

        AdjustARDebugMenuPosition();
    }

    void ChangePlaneVisibility(bool setVisible)
    {
        var count = featheredPlaneMeshVisualizerCompanions.Count;
        for (int i = 0; i < count; ++i)
        {
            featheredPlaneMeshVisualizerCompanions[i].visualizeSurfaces = setVisible;
        }
    }

    void DeleteFocusedObject()
    {
        var currentFocusedObject = m_InteractionGroup.focusInteractable;
        if (currentFocusedObject != null)
        {
            Destroy(currentFocusedObject.transform.gameObject);
        }
    }

    void AdjustARDebugMenuPosition()
    {
        float screenWidthInInches = Screen.width / Screen.dpi;

        if (screenWidthInInches < 5)
        {
            Vector2 menuOffset = m_ShowObjectMenu ? m_ObjectMenuOffset : m_ObjectButtonOffset;

            if (m_DebugMenu.toolbar.TryGetComponent<RectTransform>(out var rect))
            {
                rect.anchorMin = new Vector2(0.5f, 0);
                rect.anchorMax = new Vector2(0.5f, 0);
                rect.eulerAngles = new Vector3(rect.eulerAngles.x, rect.eulerAngles.y, 90);
                rect.anchoredPosition = new Vector2(0, 20) + menuOffset;
            }

            if (m_DebugMenu.displayInfoMenuButton.TryGetComponent<RectTransform>(out var infoMenuButtonRect))
                infoMenuButtonRect.localEulerAngles = new Vector3(infoMenuButtonRect.localEulerAngles.x, infoMenuButtonRect.localEulerAngles.y, -90);

            if (m_DebugMenu.displayConfigurationsMenuButton.TryGetComponent<RectTransform>(out var configurationsMenuButtonRect))
                configurationsMenuButtonRect.localEulerAngles = new Vector3(configurationsMenuButtonRect.localEulerAngles.x, configurationsMenuButtonRect.localEulerAngles.y, -90);

            if (m_DebugMenu.displayCameraConfigurationsMenuButton.TryGetComponent<RectTransform>(out var cameraConfigurationsMenuButtonRect))
                cameraConfigurationsMenuButtonRect.localEulerAngles = new Vector3(cameraConfigurationsMenuButtonRect.localEulerAngles.x, cameraConfigurationsMenuButtonRect.localEulerAngles.y, -90);

            if (m_DebugMenu.displayDebugOptionsMenuButton.TryGetComponent<RectTransform>(out var debugOptionsMenuButtonRect))
                debugOptionsMenuButtonRect.localEulerAngles = new Vector3(debugOptionsMenuButtonRect.localEulerAngles.x, debugOptionsMenuButtonRect.localEulerAngles.y, -90);

            if (m_DebugMenu.infoMenu.TryGetComponent<RectTransform>(out var infoMenuRect))
            {
                infoMenuRect.anchorMin = new Vector2(0.5f, 0);
                infoMenuRect.anchorMax = new Vector2(0.5f, 0);
                infoMenuRect.pivot = new Vector2(0.5f, 0);
                infoMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
            }

            if (m_DebugMenu.configurationMenu.TryGetComponent<RectTransform>(out var configurationsMenuRect))
            {
                configurationsMenuRect.anchorMin = new Vector2(0.5f, 0);
                configurationsMenuRect.anchorMax = new Vector2(0.5f, 0);
                configurationsMenuRect.pivot = new Vector2(0.5f, 0);
                configurationsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
            }

            if (m_DebugMenu.cameraConfigurationMenu.TryGetComponent<RectTransform>(out var cameraConfigurationsMenuRect))
            {
                cameraConfigurationsMenuRect.anchorMin = new Vector2(0.5f, 0);
                cameraConfigurationsMenuRect.anchorMax = new Vector2(0.5f, 0);
                cameraConfigurationsMenuRect.pivot = new Vector2(0.5f, 0);
                cameraConfigurationsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
            }

            if (m_DebugMenu.debugOptionsMenu.TryGetComponent<RectTransform>(out var debugOptionsMenuRect))
            {
                debugOptionsMenuRect.anchorMin = new Vector2(0.5f, 0);
                debugOptionsMenuRect.anchorMax = new Vector2(0.5f, 0);
                debugOptionsMenuRect.pivot = new Vector2(0.5f, 0);
                debugOptionsMenuRect.anchoredPosition = new Vector2(0, 150) + menuOffset;
            }
        }
    }

    void OnPlaneChanged(ARTrackablesChangedEventArgs<ARPlane> eventArgs)
    {
        if (eventArgs.added.Count > 0)
        {
            foreach (var plane in eventArgs.added)
            {
                if (plane.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }

        if (eventArgs.removed.Count > 0)
        {
            foreach (var plane in eventArgs.removed)
            {
                if (plane.Value != null && plane.Value.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                    featheredPlaneMeshVisualizerCompanions.Remove(visualizer);
            }
        }

        // Fallback if the counts do not match after an update
        if (m_PlaneManager.trackables.count != featheredPlaneMeshVisualizerCompanions.Count)
        {
            featheredPlaneMeshVisualizerCompanions.Clear();
            foreach (var trackable in m_PlaneManager.trackables)
            {
                if (trackable.TryGetComponent<ARFeatheredPlaneMeshVisualizerCompanion>(out var visualizer))
                {
                    featheredPlaneMeshVisualizerCompanions.Add(visualizer);
                    visualizer.visualizeSurfaces = (m_DebugPlaneSlider.value != 0);
                }
            }
        }
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // If there are touches, check each touch's fingerId
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                var t = Input.touches[i];
                if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    return true;
            }
            return false;
        }

        // Fallback for mouse (editor/PC)
        return EventSystem.current.IsPointerOverGameObject();
    }
}
