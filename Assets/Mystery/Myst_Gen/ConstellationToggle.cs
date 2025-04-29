using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class ConstellationToggle : MonoBehaviour
{
    private Canvas canvas;

    [Header("Object Refs")]
    [SerializeField] RectTransform mysteryHolder;
    [SerializeField] Camera mysteryCam;
    [SerializeField] GameObject mysteryHUD;
    [SerializeField] GameObject mainHUD;
    [SerializeField] AudioControl audioControl;
    
    [Header("UI Elements")]
    [SerializeField] GameObject controlsTooltip; // Tooltip showing keyboard/mouse controls
    [SerializeField] TMP_Text tooltipText; // Text element to show dynamic tips
    
    // Transition settings
    [Header("Transition Settings")]
    [SerializeField] float transitionSpeed = 5f; // Speed of transitions
    [SerializeField] float tooltipDuration = 3f; // How long tooltips are shown
    
    // Private fields
    private Camera mainCam;
    private CameraClearFlags originalClearFlags;
    private int originalCullingMask;
    private Coroutine tooltipCoroutine;
    private Coroutine transitionCoroutine;
    
    // Animation curves for smooth transitions
    [SerializeField] AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Start()
    {
        // Try to get the Canvas component on this GameObject
        canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            Debug.LogWarning("No Canvas component found for Constellation.");
        }

        // Set up controls tooltip if it exists
        if (controlsTooltip != null)
        {
            controlsTooltip.SetActive(false);
        }

        // Set the mystery stuff for main gameplay
        canvas.enabled = false;
        mysteryHolder.gameObject.SetActive(false);
        mysteryCam.gameObject.SetActive(false);
        mysteryHUD.SetActive(false);
        mainHUD.SetActive(true);

        mainCam = Camera.main;
    }

    void Update()
    {
        // Check for M key (open/close) OR Escape key (close only)
        bool mKeyPressed = Input.GetKeyDown(KeyCode.M);
        bool escKeyPressed = Input.GetKeyDown(KeyCode.Escape);
        
        // Check for Home key (center view)
        bool homeKeyPressed = Input.GetKeyDown(KeyCode.Home);
        
        if (mKeyPressed || (escKeyPressed && GameControl.GameController.currentState == GameState.MYSTERY))
        {
            if (canvas != null)
            {
                if (GameControl.GameController.currentState == GameState.DEFAULT)
                {
                    OpenConstellationView();
                }
                else if (GameControl.GameController.currentState == GameState.MYSTERY)
                {
                    CloseConstellationView();
                }    
            }
            else
            {
                // Toggle the entire GameObject if no canvas is found
                gameObject.SetActive(!gameObject.activeSelf);
            }
        }
        
        // Center the view when Home key is pressed while in Mystery state
        if (homeKeyPressed && GameControl.GameController.currentState == GameState.MYSTERY)
        {
            CenterView();
        }
        
        // Show controls tooltip when F1 is pressed
        if (Input.GetKeyDown(KeyCode.F1) && GameControl.GameController.currentState == GameState.MYSTERY)
        {
            ShowControlsTooltip();
        }
    }
    
    // Open the constellation view with smooth transitions
    void OpenConstellationView()
    {
        // TIMING IMPROVEMENT: Hide main HUD immediately to ensure the mystery map icon
        // disappears before any animations start
        mainHUD.SetActive(false);
        
        // Toggle just the canvas visibility
        canvas.enabled = true;
        mysteryHolder.gameObject.SetActive(true);

        // Store original camera settings
        originalClearFlags = mainCam.clearFlags;
        originalCullingMask = mainCam.cullingMask;
        
        // Keep main camera active but set it to not render anything (for post-processing)
        mainCam.clearFlags = CameraClearFlags.Nothing;
        mainCam.cullingMask = 0; // Don't render any layers
        mainCam.gameObject.SetActive(true);
        
        // Activate mystery camera 
        mysteryCam.gameObject.SetActive(true);

        // Smooth camera transition
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateCameraTransition(true));
        
        // Turn on the mystery HUD with a slight delay for smoother visual transition
        // Note: We're now only activating mysteryHUD here since mainHUD is already deactivated
        StartCoroutine(DelayedMysteryHUDActivation(0.2f));

        GameControl.GameController.currentState = GameState.MYSTERY;

        audioControl.PlaySFX_Enter();

        // Update simulation count on the nodeControl
        GetComponent<NodeControl>().MapEnabled();
        
        // Show brief controls tooltip
        ShowTip("Left Mouse - Click and Drag Nodes | Right Mouse - Inspect Node | Middle Mouse - Pan | Scroll to Zoom | 'M' to Exit");
    }
    
    // Close the constellation view with smooth transitions
    void CloseConstellationView()
    {
        // Begin camera transition before hiding UI
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(AnimateCameraTransition(false));
        
        // Delay UI deactivation for smoother transition
        StartCoroutine(DelayedUIDeactivation(0.3f));
        
        // Immediate state change
        GameControl.GameController.currentState = GameState.DEFAULT;
        
        audioControl.PlaySFX_Exit();
    }
    
    // Animate camera transition
    IEnumerator AnimateCameraTransition(bool opening)
    {
        float duration = 0.5f; // Transition duration in seconds
        float elapsed = 0;
        
        // Initial scale for mystery holder if opening
        if (opening)
        {
            mysteryHolder.localScale = Vector3.one * 0.9f; // Start slightly smaller
        }
        
        while (elapsed < duration)
        {
            float t = transitionCurve.Evaluate(elapsed / duration);
            
            if (opening)
            {
                // Grow from 90% to 100% scale
                mysteryHolder.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
            }
            else
            {
                // Shrink from 100% to 90% scale
                mysteryHolder.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.9f, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final state
        mysteryHolder.localScale = opening ? Vector3.one : Vector3.one * 0.9f;
    }
    
    // Delayed activation of mystery HUD for smoother transitions
    IEnumerator DelayedMysteryHUDActivation(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only turn on mystery HUD after delay (mainHUD is already off)
        mysteryHUD.SetActive(true);
    }
    
    // Delayed UI deactivation when closing
    IEnumerator DelayedUIDeactivation(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Turn off the mystery HUD
        mysteryHUD.SetActive(false);
        mainHUD.SetActive(true);
        
        // Restore original camera settings
        mainCam.clearFlags = originalClearFlags;
        mainCam.cullingMask = originalCullingMask;
        mainCam.gameObject.SetActive(true);
        
        // Deactivate mystery camera
        mysteryCam.gameObject.SetActive(false);
        
        // Deactivate mystery holder and canvas
        canvas.enabled = false;
        mysteryHolder.gameObject.SetActive(false);
    }
    
    // Center the view on the nodes
    void CenterView()
    {
        // Find NodeControl to access nodes
        NodeControl nodeControl = GetComponent<NodeControl>();
        if (nodeControl == null || nodeControl.visualNodes == null || nodeControl.visualNodes.Count == 0)
            return;
            
        // Calculate the average position of all visible nodes
        Vector2 centerPos = Vector2.zero;
        int visibleNodes = 0;
        
        foreach (var nodePair in nodeControl.visualNodes)
        {
            if (nodePair.Value.activeInHierarchy)
            {
                centerPos += (Vector2)nodePair.Value.transform.localPosition;
                visibleNodes++;
            }
        }
        
        if (visibleNodes > 0)
        {
            centerPos /= visibleNodes;
            
            // Animate the centering
            StartCoroutine(AnimatePanToPosition(centerPos));
            
            // Visual feedback
            ShowTip("View centered");
        }
    }
    
    // Animate panning to a position
    IEnumerator AnimatePanToPosition(Vector2 targetPos)
    {
        float duration = 0.5f;
        float elapsed = 0;
        
        Vector2 startPos = mysteryHolder.anchoredPosition;
        
        while (elapsed < duration)
        {
            float t = transitionCurve.Evaluate(elapsed / duration);
            mysteryHolder.anchoredPosition = Vector2.Lerp(startPos, -targetPos, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final position
        mysteryHolder.anchoredPosition = -targetPos;
    }
    
    // Show controls tooltip
    void ShowControlsTooltip()
    {
        if (controlsTooltip != null)
        {
            controlsTooltip.SetActive(true);
            
            if (tooltipCoroutine != null)
                StopCoroutine(tooltipCoroutine);
                
            tooltipCoroutine = StartCoroutine(HideTooltipAfterDelay(8f)); // Longer duration for controls
        }
    }
    
    // Show a temporary tip message
    public void ShowTip(string message)
    {
        if (tooltipText != null)
        {
            tooltipText.text = message;
            tooltipText.gameObject.SetActive(true);
            
            if (tooltipCoroutine != null)
                StopCoroutine(tooltipCoroutine);
                
            tooltipCoroutine = StartCoroutine(HideTooltipAfterDelay(tooltipDuration));
        }
    }
    
    // Hide tooltip after delay
    IEnumerator HideTooltipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (controlsTooltip != null)
            controlsTooltip.SetActive(false);
            
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }
}
