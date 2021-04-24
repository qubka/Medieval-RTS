using System.Collections.Generic;
using GPUInstancer.CrowdAnimations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Army : MonoBehaviour, ISortable
{
    public Team team;
    public List<Squadron> troops;

    [Header("Children References")]
    public GameObject banner;
    [Space(5f)]
    public GameObject armyBar;
    private Text barText;
    [Space(10f)]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform worldTransform;
    [HideInInspector] public Transform barTransform;

    [Header("Misc")]
    public float canvasHeight;
    public Vector3 barScale = new Vector3(1f, 1f, 1f);

    private Camera cam;
    //private CamController camController;
    private SortList sortList;
    private ArmyTable armyTable;
    private RectTransform holderCanvas;
    
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        armyBar = Instantiate(armyBar);
        barTransform = armyBar.transform;
        worldTransform = transform;
        banner = Instantiate(banner);
        banner.GetComponent<Attachment>().parentTransform = worldTransform;
    }

    private void Start()
    {
        // Get information from manager
        cam = Manager.mainCamera;
        holderCanvas = Manager.holderCanvas;
        sortList = Manager.sortList;
        armyTable = Manager.armyTable;
        
        // Add a army to the tables
        armyTable.Add(gameObject, this);
        sortList.Add(this);

        // Parent a bar to the screen
        barText = barTransform.GetComponentInChildren<Text>();
        barTransform.SetParent(holderCanvas, false);
        barTransform.localScale = barScale;
    }

    public void Update()
    {
        // Calculate position for the ui bar
        var center = worldTransform.position;
        center.y += canvasHeight;
        center = cam.WorldToScreenPoint(center);
        
        // If the army is behind the camera, or too far away from the player, make sure to hide the health bar completely
        if (center.z < 0f) {
            armyBar.SetActive(false);
        } else {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(holderCanvas, center, null, out var canvasPos);
            barTransform.localPosition = canvasPos;
            armyBar.SetActive(true);
        }
    }
    
    #region Sorting

    public Vector3 GetPosition()
    {
        return worldTransform.position;
    }

    public Transform GetTransform()
    {
        return barTransform;
    }

    #endregion
}
