using GPUInstancer.CrowdAnimations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Army : MonoBehaviour
{
    public Team team;
    [SerializeField] private Text amount;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform worldTransform;
    
    private Camera cam;
    //private CamController camController;
    private ArmyTable armyTable;
    private GPUICrowdManager modelManager;
    private Ray groundRay;
    private RaycastHit groundHit;
    
    private bool paused;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        worldTransform = transform;
    }

    private void Start()
    {
        // Get information from manager
        modelManager = Manager.modelManager;
        cam = Manager.mainCamera;
        armyTable = Manager.armyTable;
        armyTable.Add(gameObject, this);
        //camController = Manager.camController;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(1)) {
            groundRay = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground)) {
                agent.SetDestination(groundHit.point);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;
        }
    }
}
