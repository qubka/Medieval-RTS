using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class Town : MonoBehaviour
{
    [SerializeField] private Faction faction;
    [SerializeField] private Character owner;
    
    [Header("Children References")]
    [SerializeField] private GameObject townBar;
    private Text barText;
    private Rect barRect;
    [Space(10f)]
    [HideInInspector] public bool isMarker;
    [HideInInspector] public Vector3 initialPosition;
    [HideInInspector] public Transform barTransform;
    
    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);
    public Vector3 bannerPosition = new Vector3(0f, 2f, 0f);
    
    #region Local
    
#pragma warning disable 108,114
    private Camera camera;
#pragma warning restore 108,114
    private Transform camTransform;
    private RectTransform holderCanvas;

    #endregion
    
    private void Awake()
    {
        townBar = Instantiate(townBar);
        barTransform = townBar.transform;
        initialPosition = transform.position;
        initialPosition.y += canvasHeight;
    }

    private void Start()
    {
        // Get information from manager
        camera = Manager.mainCamera;
        camTransform = Manager.camTransform;
        holderCanvas = Manager.holderCanvas;
        
        //title.text = name;
        //shadow.color = faction.color;
        //TownTable.Instance.Add(gameObject, this);
        
        // Parent a bar to the screen
        barText = barTransform.GetComponentInChildren<Text>();
        //barText.color = owner.faction.color;
        barText.text = name; // TODO: Translation
        barRect = barTransform.GetComponent<Image>().GetPixelAdjustedRect();
        barTransform.SetParent(holderCanvas, false);
        barTransform.localScale = barScale;
    }
    
    public void Update()
    {
        //
        if (isMarker) {
            // Temporary variable to store the converted position from 3D world point to 2D screen point
            var pos = camera.WorldToScreenPointProjected(camTransform, initialPosition);
            
            // Giving limits to the icon so it sticks on the screen
            // Below calculations witht the assumption that the icon anchor point is in the middle
            // Minimum X position: half of the icon width
            var minX = barRect.width / 2f;
            // Maximum X position: screen width - half of the icon width
            var maxX = Screen.width - minX;

            // Minimum Y position: half of the height
            var minY = barRect.height / 2f;
            // Maximum Y position: screen height - half of the icon height
            var maxY = Screen.height - minY;

            // Check if the target is behind us, to only show the icon once the target is in front
            if (Vector.Dot((initialPosition - camTransform.position), camTransform.forward) < 0f) {
                // Check if the target is on the left side of the screen
                pos.x = pos.x < Screen.width / 2f ? maxX : minX;
            }

            // Limit the X and Y positions
            pos.x = math.clamp(pos.x, minX, maxX);
            pos.y = math.clamp(pos.y, minY, maxY);

            //
            RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, pos, null, out var canvasPos);
            barTransform.localPosition = canvasPos;
            townBar.SetActive(true);
        } else {
            // Calculate position for the ui bar
            var pos = camera.WorldToScreenPoint(initialPosition);
            
            // If the town is behind the camera, or too far away from the player, make sure to hide the bar completely
            if (pos.z < 0f) {
                townBar.SetActive(false);
            } else {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, pos, null, out var canvasPos);
                barTransform.localPosition = canvasPos;
                townBar.SetActive(true);
            }
        }
    }

#if UNITY_EDITOR    
    public void GenerateName(TownNames names)
    {
        var prefix = names.RandomPrefix;
        var anyfix = names.RandomAnyfix;
        while (string.Equals(prefix, anyfix, StringComparison.OrdinalIgnoreCase)){
            anyfix = names.RandomAnyfix;
        }
        
        name = prefix.FirstLetterCapital() + anyfix;
    }
#endif

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        
        UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, name, new GUIStyle("Button") {fontSize = 30});
    }
}
