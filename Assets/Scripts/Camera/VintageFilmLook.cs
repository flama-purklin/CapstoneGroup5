using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class VintageFilmLook : MonoBehaviour
{
    [Header("Volume Settings")]
    public bool isGlobal = true;
    [Range(0, 100)]
    public int priority = 10;

    [Header("Color Grading")]
    [Range(0f, 30f)]
    public float temperature = 15f;
    [Range(-20f, 20f)]
    public float tint = 5f;
    [Range(0.5f, 1.5f)]
    public float contrast = 1.1f;
    [Range(0.5f, 1.5f)]
    public float saturation = 1.2f;
    public Color colorFilter = new Color(1.0f, 0.97f, 0.9f, 1f); // Warm wash
    
    [Header("Split Toning")]
    public Color shadowsTint = new Color(0.16f, 0.27f, 0.35f, 1f); // Slight teal in shadows
    [Range(0f, 1f)]
    public float shadowsBalance = 0.8f;
    public Color highlightsTint = new Color(1.0f, 0.84f, 0.67f, 1f); // Orange-yellow highlights
    [Range(0f, 1f)]
    public float highlightsBalance = 0.2f;
    [Range(-100f, 100f)]
    public float balance = -10f; // Favor shadows slightly

    [Header("Bloom Settings")]
    [Range(0f, 1f)]
    public float bloomIntensity = 0.3f;
    [Range(0f, 1f)]
    public float bloomThreshold = 0.9f;
    [Range(0f, 1f)]
    public float bloomScatter = 0.7f;
    public Color bloomTint = new Color(1.0f, 0.88f, 0.75f, 1f); // Warm orange tint
    public Texture2D dirtTexture;
    [Range(0f, 1f)]
    public float dirtIntensity = 0.5f;

    [Header("Halation Effect")]
    [Range(0.0f, 1.0f)]
    public float halationIntensity = 0.3f;
    [Range(0.5f, 3.0f)]
    public float halationRadius = 2.0f;
    public Color halationColor = new Color(1.0f, 0.5f, 0.3f, 1.0f); // Red-orange color
    [Range(0.5f, 2.0f)]
    public float halationThreshold = 1.2f;
    public bool useHalationEffect = true;

    [Header("Film Grain")]
    [Range(0f, 1f)]
    public float grainIntensity = 0.4f;
    [Range(0f, 1f)]
    public float grainResponse = 0.8f; // Higher values mean less grain in bright areas

    [Header("Lens Effects")]
    [Range(-0.5f, 0.5f)]
    public float lensDistortion = -0.1f; // Slight barrel distortion
    [Range(0f, 1f)]
    public float chromaticAberration = 0.1f;
    [Range(0f, 1f)]
    public float vignetteIntensity = 0.25f;
    [Range(0f, 1f)]
    public float vignetteSmoothness = 0.4f;

    [Header("Gate Weave")]
    public bool useGateWeave = true;
    [Range(0f, 0.1f)]
    public float weaveAmount = 0.03f;
    [Range(0f, 1f)]
    public float weaveSpeed = 0.2f;
    [Range(0f, 0.2f)]
    public float rotationAmount = 0.05f;
    [Range(0f, 1f)]
    public float rotationSpeed = 0.1f;

    [Header("Required Assets")]
    public VolumeProfile mainProfile; // Assign in Inspector
    public VolumeProfile halationProfile; // Assign in Inspector (if useHalationEffect is true)

    // References to volume components
    private Volume mainVolume;
    private Volume halationVolume;
    private ColorAdjustments colorAdjustments;
    private WhiteBalance whiteBalance;
    private SplitToning splitToning;
    private Bloom mainBloom;
    private Bloom halationBloom;
    private FilmGrain filmGrain;
    private LensDistortion lensDistortionComponent;
    private ChromaticAberration chromaticAberrationComponent;
    private Vignette vignetteComponent;
    private Tonemapping tonemapping;

    // For gate weave effect - Calculation only, no direct manipulation
    private float positionSeed;
    private float rotationSeed;
    // Removed: mainCamera, originalCameraPosition, originalCameraRotation

    private void OnEnable()
    {
        // Get or create the main volume component
        mainVolume = GetComponent<Volume>();
        if (mainVolume == null)
        {
            mainVolume = gameObject.AddComponent<Volume>();
        }
        
        // Update volume settings
        mainVolume.isGlobal = isGlobal;
        mainVolume.priority = priority;

        // Assign the main profile
        if (mainProfile != null)
        {
             mainVolume.profile = mainProfile;
        }
        else
        {
            Debug.LogError("VintageFilmLook: Main Volume Profile is not assigned in the inspector!", this);
            // Optionally disable the component or handle the error appropriately
            // this.enabled = false; 
            // return;
        }

        // Create a separate volume for halation if needed
        if (useHalationEffect && halationVolume == null)
        {
            // Check if a child GameObject already exists for halation
            Transform halationTransform = transform.Find("HalationVolume");
            GameObject halationObject;
            
            if (halationTransform != null)
            {
                halationObject = halationTransform.gameObject;
            }
            else
            {
                // Create a new child GameObject for halation
                halationObject = new GameObject("HalationVolume");
                halationObject.transform.SetParent(transform, false);
            }
            
            // Add or get the Volume component
            halationVolume = halationObject.GetComponent<Volume>();
            if (halationVolume == null)
            {
                halationVolume = halationObject.AddComponent<Volume>();
            }
            
            halationVolume.isGlobal = isGlobal;
            halationVolume.priority = priority + 1; // Higher priority than main volume
            
            // Assign the halation profile
            if (halationProfile != null)
            {
                halationVolume.profile = halationProfile;
            }
            else
            {
                 Debug.LogError("VintageFilmLook: Halation Volume Profile is not assigned in the inspector!", this);
                 // Optionally disable halation or the component
                 // useHalationEffect = false; 
            }
        }

        // Initialize seeds for gate weave effect (if used)
        if (useGateWeave)
        {
             positionSeed = Random.Range(0f, 10000f);
             rotationSeed = Random.Range(0f, 10000f);
        }
        // Removed camera finding/storing logic

        // Setup all volume components
        SetupVolumeComponents();
    }

private void SetupVolumeComponents()
    {
        // Just get references to existing components, do NOT add new ones
        // The volume already has a properly configured profile as seen in the screenshots
        mainVolume.profile.TryGet(out colorAdjustments);
        mainVolume.profile.TryGet(out whiteBalance);
        mainVolume.profile.TryGet(out splitToning);
        mainVolume.profile.TryGet(out mainBloom);
        mainVolume.profile.TryGet(out filmGrain);
        mainVolume.profile.TryGet(out lensDistortionComponent);
        mainVolume.profile.TryGet(out chromaticAberrationComponent);
        mainVolume.profile.TryGet(out vignetteComponent);
        mainVolume.profile.TryGet(out tonemapping);
        
        // Setup halation bloom if needed
        if (useHalationEffect && halationVolume != null)
        {
            halationVolume.profile.TryGet(out halationBloom);
        }
    }

    private void OnDisable()
    {
        // Removed camera reset logic
        
        // Clean up halation volume if it exists and the GameObject hasn't already been destroyed
        if (halationVolume != null && halationVolume.gameObject != null)
        {
            if (Application.isPlaying)
            {
                Destroy(halationVolume.gameObject);
            }
            else
            {
                DestroyImmediate(halationVolume.gameObject);
            }
            halationVolume = null;
        }
    }

    private void Update()
    {
        // Update volume settings
        if (mainVolume != null)
        {
            mainVolume.isGlobal = isGlobal;
            mainVolume.priority = priority;
        }
        
        if (halationVolume != null)
        {
            halationVolume.isGlobal = isGlobal;
            halationVolume.priority = priority + 1;
        }
        
        // We no longer need to update component parameters every frame
        // The Volume profile already has all the settings configured
        // This prevents duplication of effects
        
        // Only calculate gate weave when needed via GetGateWeaveOffset
    }

    // All Update*() methods removed - these were causing the duplication of effects
    // The custom volume profile already has all these settings configured

    // New method for CameraControl to call
    public (Vector3 positionOffset, Quaternion rotationOffset) GetGateWeaveOffset()
    {
        if (!useGateWeave)
        {
            return (Vector3.zero, Quaternion.identity);
        }

        // Position weave (subtle random movement)
        float xOffset = Mathf.PerlinNoise(Time.time * weaveSpeed, positionSeed) * 2 - 1;
        float yOffset = Mathf.PerlinNoise(positionSeed, Time.time * weaveSpeed) * 2 - 1;
        
        Vector3 positionOffset = new Vector3(
            xOffset * weaveAmount, 
            yOffset * weaveAmount, 
            0
        );
        
        // Rotation weave (even more subtle rotation)
        float rotZ = Mathf.PerlinNoise(rotationSeed, Time.time * rotationSpeed) * 2 - 1;
        
        Quaternion rotationOffset = Quaternion.Euler(0, 0, rotZ * rotationAmount);

        return (positionOffset, rotationOffset);
    }

    // Removed UpdateGateWeave method
}
