using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Object = ObjectExtention;

[RequireComponent(typeof(Manager))]
public class UnitManager : MonoBehaviour 
{
	//variables visible in the inspector	
	public GUIStyle rectangleStyle;
	
	[Header("Prefabs")]
	public GameObject movementLine;
	public GameObject arrowLine;
	public GameObject directionLine;
	public GameObject moveParticle;
	public GameObject attackParticle;
	
	[Header("Sounds")]
	public AudioClip moveSound;
	public AudioClip attackSound;
	public AudioClip placeSound;
	public AudioClip selectSound;
	public AudioClip targetSound;
	
	[Header("Cursors")]
	public Texture2D basicCursor;
	public Texture2D moveCursor;
	public Texture2D invalidCursor;
	public Texture2D meleeCursor;
	public Texture2D rangeCursor;
	public Texture2D dragCursor;
	public Texture2D selectCursor;
	public Texture2D placeCursor;
	public Texture2D shiftCursor;
	public Texture2D addCursor;
	public Texture2D inclusiveCursor;
	public Texture2D lookCursor;
	
	//public Color allowColor;
	//public Color blockColor;
	[Header("Hot Keys")]
	public KeyCode addKey = KeyCode.LeftShift;
	public KeyCode inclusiveKey = KeyCode.LeftControl;
	public KeyCode dragKey = KeyCode.LeftAlt;
	public KeyCode stopKey = KeyCode.Escape;

	//variables not visible in the inspector
	//private EntityManager entityManager;
	//private GameObject unitList;
	[HideInInspector] public Squad target;
	[HideInInspector] public UnitLayout unitLayout;
	private EventSystem eventSystem;
	private TerrainBorder border;
	private Camera cam;
	private CamController controller;
	private UnitTable unitTable;
	//private Dictionary<Entity, GameObject> unitButtons;
	private Dictionary<Squad, Movement> movementGroup;
	private List<Formation> placedFormations;
	private List<Squad> selectedUnits;
	private List<Vector3> positions;
	private AudioSource clickAudio;
	private AudioSource selectAudio;
	private Collider[] colliders;
	private Squad lastSelectSquad;
	private float lastSelectTime;
	private float lastClickTime;
	private Vector3 lastClickPos;
	private Interaction onDrag;
	private Interaction onSelect;
	private Interaction onPlace;
	private Interaction onShift;
	private int cursor;
	private float maxDistance;
	private float nextHoverTime;
	private RaycastHit groundHit;
	private bool groundCast;
	private bool pointerUI;

	private bool InvalidHit => !groundCast || border.IsOutsideBorder(groundHit.point) || Input.GetKey(KeyCode.Escape);
	public int selectedCount => selectedUnits.Count;
	
	private void Awake()
	{
		colliders = new Collider[1];
		//unitButtons = new Dictionary<Entity, GameObject>();
		movementGroup = new Dictionary<Squad, Movement>();
		placedFormations = new List<Formation>();
		selectedUnits = new List<Squad>();
		positions = new List<Vector3>();
	}

	private void Start()
	{
		//entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		//unitList = GameObject.Find("Unit list");
		eventSystem = EventSystem.current;
		var size = Manager.terrain.terrainData.size;
		maxDistance = Mathf.Max(size.x, size.z) * 2f;
		unitTable = Manager.unitTable;
		border = Manager.border;
		cam = Manager.mainCamera;
		controller = Manager.camController;
		clickAudio = Manager.cameraSources[0];
		SetCursor(basicCursor);
	}

	private void LateUpdate()
	{
		var ray = cam.ScreenPointToRay(Input.mousePosition);
		groundCast = Physics.Raycast(ray, out groundHit, maxDistance, Manager.Ground);
		pointerUI = eventSystem.IsPointerOverGameObject();
		
		OnDragging();
		OnSelection();
		OnPlacement();
		OnShifting();
		OnMovement();
		OnHover();
		OnCursor();
	}

	private void OnDragging()
	{
		if (onSelect.enabled || onPlace.enabled || onShift.enabled)
			return;

		onDrag.enabled = unitLayout;
	}
	
	private void OnSelection()
	{
		if (onDrag.enabled || onPlace.enabled || onShift.enabled)
			return;

		// When left mouse button clicked (but not released)
		if (Input.GetMouseButtonDown(0)) {
			onSelect.OnDown(Input.mousePosition, pointerUI);
			
			if (InvalidHit || pointerUI)
				return;

			var currentTime = Time.time;
			if ((currentTime - lastClickTime < 0.5f) && Vector.TruncDistance(lastClickPos, groundHit.point) <= 1f) {
				if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Squad) == 0) {
					controller.SetTarget(CreateTarget(groundHit.point));
					//clickAudio.clip = targetSound;
					//clickAudio.Play();
					onSelect.locked = true;
				}
			}

			lastClickTime = currentTime;
			lastClickPos = groundHit.point;
		}
		// When left mouse button not locked
		else if (onSelect.locked) 
			return;

		// While left mouse button held
		if (Input.GetMouseButton(0)) {
			if (Input.GetKey(stopKey) || Input.GetKey(dragKey)) {
				onSelect.Lock();
			} else {
				onSelect.OnHold(Input.mousePosition, pointerUI, 40f);
			}
		}
		// When left mouse button comes up
		else if (Input.GetMouseButtonUp(0)) {
			if (!onSelect.enabled) { //single select 
				if (InvalidHit || pointerUI)
					return;
				
				// Try to use sphere to find nearby units
				if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Unit) != 0) { //if we found units in radius, choose the first one
					var unit = unitTable.GetUnit(colliders[0].gameObject);
					if (unit) {
						if (Input.GetKey(inclusiveKey)) { //inclusive select
							AddSelected(unit.squad, true);
						} else { //exclusive selected
							DeselectAllExcept(unit.squad);
						}
					}
				} else { //if we didnt hit something
					if (Input.GetKey(inclusiveKey)) {
						//do nothing
					} else {
						DeselectAll();
					}
				}
			} else { //marquee select
				onSelect.enabled = false;
				
				if (!Input.GetKey(inclusiveKey)) {
					DeselectAll();
				}

				var vertices = new Vector3[4];
				var corners = Vector.GetBoundingBox(onSelect.startPos, onSelect.lastPos); //get 4 corners of our box
				
				for (var i = 0; i < corners.Length; i++) {
					var corner = corners[i];
					var ray = cam.ScreenPointToRay(corner); //cast out to world space
					if (Physics.Raycast(ray, out var hit, maxDistance, Manager.Ground)) {
						vertices[i] = transform.InverseTransformPoint(hit.point);
						Debug.DrawLine(cam.ScreenToWorldPoint(corner), hit.point, Color.red, 5.0f);
					} else {
						return;
					}
				}
				
				clickAudio.clip = selectSound;
				clickAudio.Play();
				
				// Generate box from 4 vertices
				var selectionMesh = Vector.GenerateSelectionMesh(vertices);
				var selectionBox = gameObject.AddComponent<MeshCollider>();
				selectionBox.sharedMesh = selectionMesh;
				selectionBox.convex = true;
				selectionBox.isTrigger = true;

				// Destroy our selected box after 1/50th of a second
				Destroy(selectionBox, 0.02f);
			}
		}
		// Should be locked if happens
		else if (onSelect.enabled) {
			onSelect.Lock();
		}
	}

	private void OnPlacement()
	{
		if (onDrag.enabled || onSelect.enabled || onShift.enabled)
			return;
		
		// When right mouse button clicked (but not released)
		if (Input.GetMouseButtonDown(1)) {
			onPlace.OnDown(groundHit.point, pointerUI);
		}
		// When right mouse button not locked
		else if (onPlace.locked) 
			return;
			
		// While right mouse button held
		if (Input.GetMouseButton(1)) {
			if (Input.GetKey(stopKey) || selectedUnits.Count == 0) {
				RemoveFormations(false);
				onPlace.Lock();
			} else {
				if (InvalidHit || Vector.DistanceSq(groundHit.point, onPlace.lastPos) <= 0.25f)
					return;

				if (border.IsOutsideBorder(onPlace.startPos)) {
					onPlace.startPos = groundHit.point;
					return;
				}

				if (onPlace.OnHold(groundHit.point, pointerUI, 20f * selectedUnits.Count)) {
					selectedUnits.Sort((a, b) => Vector.DistanceSq(onPlace.startPos, a.centroid).CompareTo(Vector.DistanceSq(onPlace.startPos, b.centroid)));
					foreach (var squad in selectedUnits) {
						placedFormations.Add(new Formation(squad, directionLine));
					}
				}
			}

			if (onPlace.enabled) {
				var segments = Vector.SplitLineToSegments(onPlace.startPos, onPlace.lastPos, selectedUnits.Count, 2f);
				for (var i = 0; i < segments.Count; i++) {
					var (start, end) = segments[i];
					
					var direction = end - start; // of line from left to right
					var length = direction.magnitude; // size of line
					var center = (end + start) / 2f; // center between end and pos
					var angle = -Vector.SignedAngle(direction.normalized, Vector3.right, Vector3.up);
					
					placedFormations[i].Expand(positions, start, end, center, angle, length);
				}
			}
		}

		// When right mouse button comes up
		else if (Input.GetMouseButtonUp(1)) {
			if (!onPlace.enabled) { //single place
				if (InvalidHit || selectedUnits.Count == 0)
					return;

				// If we hover on enemy units
				if (target) {
					var obj = target.gameObject;
					foreach (var squad in selectedUnits) {
						AddToMovementGroup(squad, obj);
					}
					
					clickAudio.clip = attackSound;
					clickAudio.Play();
					
					var pos = pointerUI ? target.centroid : groundHit.point;
					pos.y += 0.5f;
					Instantiate(attackParticle, pos, Quaternion.identity);
					
				} else if (!pointerUI) {

					var dest = groundHit.point;
					dest.y += 0.5f;
				
					var count = selectedUnits.Count;
					if (count == 1) {
						AddToMovementGroup(selectedUnits[0], CreateTarget(dest));
					} else {
						var position = Vector3.zero;
						foreach (var squad in selectedUnits) {
							position += squad.worldTransform.position;
						}
						position /= count;
						
						var shift = dest - position;
						foreach (var squad in selectedUnits) {
							AddToMovementGroup(squad, CreateTarget(squad.worldTransform.position + shift));
						}
					}
					
					clickAudio.clip = moveSound;
					clickAudio.Play();
					
					Instantiate(moveParticle, dest, Quaternion.identity);
				}
			} else {
				onPlace.enabled = false;
				
				var playSound = false;
				foreach (var formation in placedFormations) {
					if (formation.active) {
						AddToMovementGroup(formation.squad, CreateTarget(formation.centroid), formation.targetOrientation, formation.phalanxLength);
						playSound = true;
					}
				}
				
				if (playSound) {
					clickAudio.clip = placeSound;
					clickAudio.Play();
				}

				RemoveFormations(true);
			}
		}
		// Should be locked if happens
		else if (onPlace.enabled) {
			RemoveFormations(false);
			onPlace.Lock();
		}
	}

	private void OnShifting()
	{
		if (onDrag.enabled || onSelect.enabled || onPlace.enabled)
			return;
		
		// When left mouse button clicked (but not released)
		if (Input.GetMouseButtonDown(0)) {
			onShift.OnDown(groundHit.point, pointerUI);
		}
		// When left mouse button not locked
		else if (onShift.locked) 
			return;
		
		// While left mouse button held
		if (Input.GetMouseButton(0)) {
			if (Input.GetKey(stopKey) || !Input.GetKey(dragKey) || selectedUnits.Count == 0) {
				RemoveFormations(false);
				onShift.Lock();
			} else {
				if (InvalidHit || Vector.DistanceSq(groundHit.point, onPlace.lastPos) <= 0.25f)
					return;

				if (border.IsOutsideBorder(onPlace.startPos)) {
					onPlace.startPos = groundHit.point;
					return;
				}
				
				if (onShift.OnHold(groundHit.point, pointerUI, 5f)) {
					positions.Clear();
					foreach (var squad in selectedUnits) {
						positions.Add(squad.worldTransform.position); // store initial positions
						placedFormations.Add(new Formation(squad, directionLine));
					}
				}
				
				if (onShift.enabled) {
					for (var i = 0; i < selectedUnits.Count; i++) {
						placedFormations[i].Shift(positions[i], onShift.lastPos - onShift.startPos);
					}
				}
			}
		}
		// When right mouse button comes up
		else if (Input.GetMouseButtonUp(0)) {
			if (onShift.enabled) {
				onShift.enabled = false;

				var playSound = false;
				foreach (var formation in placedFormations) {
					if (formation.active) {
						AddToMovementGroup(formation.squad, CreateTarget(formation.centroid), formation.targetOrientation);
						playSound = true;
					}
				}
				if (playSound) {
					clickAudio.clip = placeSound;
					clickAudio.Play();
				}

				RemoveFormations(true);
			}
		}
		// Should be locked if happens
		else if (onShift.enabled) {
			RemoveFormations(false);
			onShift.Lock();
		}
	}

	private void OnMovement()
	{
		List<Squad> toRemove = null;
		
		foreach (var pair in movementGroup) {
			var squad = pair.Key;
			var movement = pair.Value;
			if (movement.Update()) {
				movement.SetActive(selectedUnits.Contains(squad));
			} else {
				//keep a list of the keys to remove afterwards
				if (toRemove == null) {
					toRemove = new List<Squad>(movementGroup.Count);
				}
				toRemove.Add(squad);
				movement.DestroyAll();
			}
		}

		if (toRemove != null) {
			foreach (var squad in toRemove) {
				movementGroup.Remove(squad);
			}
		}
	}
	
	private void OnHover()
	{
		var currentTime = Time.time;
		if (currentTime > nextHoverTime) {
			if (!HoverOnTarget() && target) {
				target.ChangeSelectState(false);
				target = null;
			}
			nextHoverTime = currentTime + 0.1f;
		}
	}

	private void OnCursor()
	{
		if (target) {
			SetCursor(target.team == Team.Allied || selectedUnits.Count != 0 ? meleeCursor : lookCursor); // How use the range cursor ?
		} else if (InvalidHit) {
			SetCursor(invalidCursor);
		} else if (onDrag.enabled) {
			SetCursor(dragCursor);
		} else if (onSelect.enabled) {
			SetCursor(selectCursor);
		} else if (onPlace.enabled) {
			SetCursor(placeCursor);
		} else if (onShift.enabled || selectedUnits.Count != 0 && Input.GetKey(dragKey)) {
			SetCursor(shiftCursor);
		} else if (Input.GetKey(inclusiveKey)) {
			SetCursor(inclusiveCursor);
		} else if (Input.GetKey(addKey)) {
			SetCursor(addCursor);
		} else if (selectedUnits.Count != 0) {
			SetCursor(moveCursor);
		} else {
			SetCursor(basicCursor);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		var unit = unitTable.GetUnit(collider.gameObject);
		if (unit) {
			AddSelected(unit.squad);
		}
	}

	private void OnGUI()
	{
		if (onSelect.enabled) {
			GUI.Box(Vector.GetScreenRect(onSelect.startPos, Input.mousePosition), "", rectangleStyle);
		}
	}
	
	private bool HoverOnTarget()
	{
		//stop if some interaction is on
		if (onDrag.enabled || onSelect.enabled || onPlace.enabled || onShift.enabled || InvalidHit)
			return false;

		if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Unit) != 0) { //if we found units in radius, choose the first one
			var unit = unitTable.GetUnit(colliders[0].gameObject);
			if (unit) {
				var squad = unit.squad;
				if (squad.team != Team.Self) {
					target = squad;
					target.ChangeSelectState(true);
					return true;
				}
			}
		}

		return false;
	}

	public void RemoveSquad(Squad squad)
	{
		var index = selectedUnits.IndexOf(squad);
		if (index != -1) {
			selectedUnits.RemoveAt(index);
			if (index < placedFormations.Count) {
				placedFormations[index].DestroyAll(false);
				placedFormations.RemoveAt(index);
			}
			if (movementGroup.ContainsKey(squad)) {
				movementGroup[squad].DestroyAll();
			}
		}
	}

	public void AddSelected(Squad squad, bool toggle = false)
	{
		if (squad.team == Team.Self) {
			if (!selectedUnits.Contains(squad)) {
				selectedUnits.Add(squad);
				squad.ChangeSelectState(true);
			} else if (toggle) {
				selectedUnits.Remove(squad);
				squad.ChangeSelectState(false);
			}
		}
	}

	public void DeselectAll()
	{
		foreach (var squad in selectedUnits) {
			if (squad) {
				squad.ChangeSelectState(false);
			}
		}
		selectedUnits.Clear();
	}

	public void DeselectAllExcept(Squad filter)
	{
		foreach (var squad in selectedUnits) {
			if (squad != filter) {
				squad.ChangeSelectState(false);
			}
		}

		// if exist, remove all except that, otherwise clear and add as new instance
		if (selectedUnits.Contains(filter)) {
			selectedUnits.RemoveAll(s => s != filter);
		} else {
			selectedUnits.Clear();
			AddSelected(filter);
		}

		var time = Time.time;
		if ((time - lastSelectTime < 0.5f) && lastSelectSquad == filter) {
			var obj = filter.gameObject;
			if (controller.target != obj) {
				controller.SetTarget(obj);
				clickAudio.clip = targetSound;
				clickAudio.Play();
			}
		}
		
		lastSelectTime = time;
		lastSelectSquad = filter;
	}

	private void AddToMovementGroup(Squad squad, GameObject target, float? orientation = null, float? length = null)
	{
		if (movementGroup.ContainsKey(squad)) {
			if (Input.GetKey(addKey)) {
				movementGroup[squad].Append(target, orientation, length);
			} else {
				movementGroup[squad].Reset(target, orientation, length);
			}
		} else {
			var movement = new Movement(squad, movementLine, arrowLine);
			movement.Reset(target, orientation, length);
			movementGroup.Add(squad, movement);
		}
	}
	
	private void SetCursor(Texture2D texture)
	{
		var id = texture.GetInstanceID();
		if (cursor == id) 
			return;
		
		cursor = id;
		Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
	}


	#region Formation
	
	private class Formation
	{
		public Squad squad;
		public List<GameObject> selectors;
		public float phalanxLength;
		public float targetOrientation;
		public Vector3 centroid;
		public bool active;
		
		private Line line;
		private TerrainBorder border;
		private ObjectPooler objectPool;
		
		private const float DefaultAngle = 45f;
		
		public Formation(Squad squads, GameObject directionLine)
		{
			squad = squads;
			line = new Line(directionLine);
			
			border = Manager.border;
			objectPool = Manager.objectPooler;

			selectors = new List<GameObject>(squad.UnitCount);
			for (var i = 0; i < squad.UnitCount; i++) {
				selectors.Add(objectPool.SpawnFromPool("selector", Vector3.zero, Quaternion.identity));
			}

			active = true;
		}

		public void Expand(List<Vector3> positions, Vector3 start, Vector3 end, Vector3 center, float angle, float length)
		{
			var totalLength = squad.UnitCount * squad.unitSize.width;
			length = Mathf.Clamp(length, totalLength / 12f, totalLength);

			var shift = FormationUtils.GetFormation(positions, squad.formationShape, squad.unitSize, squad.UnitCount, length, true);
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (shift != 0f) {
				FormationUtils.GetPositions(positions, center, angle);
				for (var j = positions.Count - 1; j > -1; j--) {
					if (border.IsOutsideBorder(positions[j])) {
						return;
					}
				}
			} else {
				SetActive(false);
				return;
			}

			var rot = Quaternion.Euler(0f, DefaultAngle + angle, 0f);
			
			// Remove selectors if no more units available
			var count = positions.Count;
			for (var i = selectors.Count - 1; i > -1; i--) {
				var selector = selectors[i];
				if (count > i) {
					selector.transform.SetPositionAndRotation(positions[i], rot);
				} else {
					objectPool.ReturnToPool("selector", selector);
					selectors.RemoveAt(i);
				}
			}

			// Find the center between start and end position
			rot = Quaternion.Euler(0f, angle, 0f);
			
			// Clear position holder
			line.Clear();
			
			// Draw full mode
			if (length > 6f) {
				var p1 = start + rot * new Vector3(0f, 0f, 1f);
				var p2 = center + rot * new Vector3(-3f, 0f, 1f);
				var p3 = center + rot * new Vector3(0f, 0f, 3f);
				var p4 = center + rot * new Vector3(3f, 0f, 1f);
				var p5 = end + rot * new Vector3(0f, 0f, 1f);
				
				line.AddPoint(p1);
				line.AddLine(p1, p2);
				line.AddLine(p2, p3);
				line.AddLine(p3, p4);
				line.AddLine(p4, p5);
			} else {
				var p1 = start + rot * new Vector3(0f, 0f, 1f);
				var p2 = center + rot * new Vector3(0f, 0f, 3f);
				var p3 = end + rot * new Vector3(0f, 0f, 1f);
				
				line.AddPoint(p1);
				line.AddLine(p1, p2);
				line.AddLine(p2, p3);
			}
			
			line.Render();
			
			phalanxLength = length;
			targetOrientation = angle;
			centroid = center + rot * new Vector3(0f, 0f, -shift);

			SetActive(true);
		}

		public void Shift(Vector3 position, Vector3 shift)
		{
			var angle = squad.worldTransform.eulerAngles.y;
			var rot = Quaternion.Euler(0f, DefaultAngle + angle, 0f);
			
			// Remove selectors if no more units available
			var count = squad.positions.Count;
			for (var i = selectors.Count - 1; i > -1; i--) {
				var selector = selectors[i];
				if (count > i) {
					var pos = squad.worldTransform.TransformPoint(squad.positions[i]) + shift;
					if (border.IsOutsideBorder(pos)) {
						SetActive(false);
						return;
					}
					pos.y = Manager.terrain.SampleHeight(pos) + 0.5f;
					selector.transform.SetPositionAndRotation(pos, rot);
				} else {
					objectPool.ReturnToPool("selector", selector);
					selectors.RemoveAt(i);
				}
			}
			
			centroid = position + shift;
			targetOrientation = angle;
			
			SetActive(true);
		}

		public void SetActive(bool state)
		{
			if (active == state)
				return;
			
			line.SetActive(state);
			
			// Remove selectors if no more units available
			var count = squad.UnitCount;
			for (var i = selectors.Count - 1; i > -1; i--) {
				var selector = selectors[i];
				if (count > i) {
					selector.SetActive(state);
				} else {
					objectPool.ReturnToPool("selector", selector);
					selectors.RemoveAt(i);
				}
			}
			
			active = state;
		}

		public void DestroyAll(bool effect)
		{
			if (effect && active) {
				squad.StartCoroutine(line.FadeLineRenderer(0.5f));

				var count = squad.UnitCount;
				for (var i = 0; i < selectors.Count; i++) {
					var selector = selectors[i];
					if (count > i) {
						var trans = selector.transform;
						objectPool.SpawnFromPool("pointer", trans.position, trans.rotation);
					}
					objectPool.ReturnToPool("selector", selector);
				}
			} else {
				line.Destroy();
				
				foreach (var selector in selectors) {
					objectPool.ReturnToPool("selector", selector);
				}
			}
		}
	}

	private void RemoveFormations(bool effect)
	{
		if (placedFormations.Count > 0) {
			foreach (var formation in placedFormations) {
				formation.DestroyAll(effect);
			}
			placedFormations.Clear();
		}
	}
	
	#endregion

	#region Movement

	private class Movement
	{
		private Line line;
		private Line head;

		private Squad squad;

		private List<GameObject> targets;
		private Dictionary<GameObject, Squad> cache;
		private float nextUpdateTime;

		private static readonly Quaternion Left = Quaternion.Euler(0, -150, 0);
		private static readonly Quaternion Right = Quaternion.Euler(0, 150, 0);

		public Movement(Squad squads, GameObject movementLine, GameObject arrowLine)
		{
			squad = squads;
			line = new Line(movementLine);
			head = new Line(arrowLine);
			targets = new List<GameObject>();
			cache = new Dictionary<GameObject, Squad>();
		}

		public void SetActive(bool value)
		{
			line.SetActive(value);
			head.SetActive(value);
		}
		
		public void Append(GameObject target, float? orientation, float? length)
		{
			AddTarget(target);
			squad.SetDestination(true, target, orientation, length);
		}
		
		public void Reset(GameObject target, float? orientation, float? length)
		{
			ClearAll();
			AddTarget(target);
			squad.SetDestination(false, target, orientation, length);
		}
		
		private void AddTarget(GameObject target)
		{
			targets.Add(target);
			var enemy = target.GetComponent<Squad>();
			if (enemy) {
				cache.Add(target, enemy);
			}
		}

		public bool Update()
		{
			if (line.IsActive) {
				var currentTime = Time.time;
				if (currentTime > nextUpdateTime) {
					var start = squad.state == SquadFSM.Attack ? squad.centroid : squad.worldTransform.position;
				
					line.Clear();
					line.AddPoint(start);
				
					GameObject toRemove = null;
					foreach (var target in targets) {
						if (target) {
							var end = cache.ContainsKey(target) ? cache[target].centroid : target.transform.position;
							line.AddLine(start, end);
							start = end;
						} else {
							toRemove = target;
						}
					}
					if (toRemove) {
						targets.Remove(toRemove);
						cache.Remove(toRemove);
					}
				
					line.Render();
					DrawArrow();

					nextUpdateTime = currentTime + 0.1f;
				}
			}
			
			return squad.state != SquadFSM.Idle;
		}

		private void DrawArrow()
		{
			int count = line.Count;
			if (count > 2) {
				var origin = line.Last;
				var direction = (origin - line.PreLast).normalized * 2.5f;
				var left = origin + Left * direction;
				var right = origin + Right * direction;

				head.Clear();
				head.AddPoint(left);
				head.AddLine(left, origin);
				head.AddLine(origin, right);
				head.Render();
			}
		}

		public void DestroyAll()
		{
			squad.StartCoroutine(head.FadeLineRenderer(0.5f));
			line.Destroy();
			ClearAll();
		}
		
		private void ClearAll()
		{
			foreach (var target in targets) {
				Object.DestroyIfNamed(target, "wayPointer");
			}
			targets.Clear();
			cache.Clear();
		}
	}
	
	#endregion

	#region Interaction

	private struct Interaction
	{
		public Vector3 startPos;
		public Vector3 lastPos;
		public bool locked;
		public bool enabled;

		public bool OnHold(Vector3 pos, bool pointer, float distance)
		{
			lastPos = pos;

            if (!enabled && !locked && Vector.DistanceSq(startPos, lastPos) > distance * distance && !pointer) {
	            enabled = true;
	            return true;
            }

            return false;
		}
		
		public void OnDown(Vector3 pos, bool pointer)
		{
			startPos = pos;
			lastPos = pos;
			locked = pointer;
		}
		
		public void Lock()
		{
			locked = true;
			enabled = false;
		}
	}
	
	#endregion

	public static GameObject CreateTarget(Vector3 dest)
	{
		var gameObject = new GameObject("wayPointer");
		gameObject.transform.position = dest;
		return gameObject;
	}
}