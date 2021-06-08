/*using Unity.Entities;
using UnityEngine;

public class WorldController : SingletonObject<WorldController> {
    
    [Header("Controllers")]
    public CameraController cameraController;
    public TimeController timeController;

    
    private void Start() {
        Enums.LoadDictionaries();

        BasicWorldSave save = SaveLoadMap.GetGameData();

        //load world if it exists
        if (save != null) {

            if (save is WorldProgressSave)
                LoadSavedGame((WorldProgressSave)save);
            else
                LoadScenario(save);
        }
        else
            CreateWorld();
        
        GenerateWorld();
    }

    public void LoadSavedGame(WorldProgressSave w) {
        
        timeController.Load(w.time);
        cameraController.Load(w.camera);

		//GO THROUGH LISTS OF OBJECTS AND ACTIVATE THEM USING THE LOADMAPOBJECT() FUNCTION
		//structures
		foreach (ObjSave save in w.structures)
            LoadMapObject(save).transform.parent = structures.transform;
		foreach (ObjSave save in w.jobcentres)
			LoadMapObject(save).transform.parent = structures.transform;
		foreach (ObjSave save in w.workplaces)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.storagebuildings)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.generators)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.stables)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.houses)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.wtps)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.canals)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.crops)
            LoadMapObject(save).transform.parent = structures.transform;
        foreach (ObjSave save in w.farmhouses)
            LoadMapObject(save).transform.parent = structures.transform;

        //walkers
        foreach (ObjSave save in w.animals)
            LoadMapObject(save).transform.parent = walkers.transform;
        foreach (ObjSave save in w.walkers)
            LoadMapObject(save).transform.parent = walkers.transform;

    }
    
    public GameObject LoadMapObject(ObjSave save) {

        //load object
        GameObject go = Instantiate(Resources.Load<GameObject>(save.resourcePath));

        //activate object
        Obj o = go.GetComponent<Obj>();
        o.world = this;
        o.Load(save);

        return go;
    }
    
    public void CreateWorld() {

        Node size = SaveLoadMap.newWorldSize;
        int szx = size.x;
        int szy = size.y;
        
        Map = new World(size);
        Map.terrain = mapGenerator.GetRandomTerrain(size);
        Map.elevation = mapGenerator.GetRandomElevation(size);
        //Map.elevation = new float[szx, szy];

        //until we've successfully placed the map entrance/exit, keep trying
        bool success = false;
        do { success = CreateMapEntrance(szx, szy); } while (!success);	

        money.Money = 10000;
        
        actionSelecter.FreshActions();

        if (notifications != null)
            notifications.FreshEvents();
        ProductivityController.CreateProductivities();
        ResourcesDatabase.CreateWhitelist();
        money.FreshStartingQuarter((int)timeController.CurrentSeason, timeController.CurrentYear);
        timeController.finances.LoadFinancialReports();

    }
}*/
