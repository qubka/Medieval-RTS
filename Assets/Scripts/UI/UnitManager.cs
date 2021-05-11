﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.EventSystems;

public class UnitManager : SingletonObject<UnitManager>
{
	[Header("Main")]
	public GUIStyle rectangleStyle;
	public SquadDescription squadDesc;
	public SquadInfo squadInfo;
	
	[Header("Prefabs")]
	public GameObject movementLine;
	public GameObject arrowLine;
	public GameObject directionLine;
	public GameObject drawLine;
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
	public Texture2D drawCursor;
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
	public KeyCode shiftKey = KeyCode.LeftAlt;
	public KeyCode drawKey = KeyCode.LeftAlt;
	public KeyCode stopKey = KeyCode.Escape;

	//variables not visible in the inspector
	[HideInInspector] public Squad hover;
	[HideInInspector] public UnitLayout unitLayout;
	[HideInInspector] public List<Squad> selectedUnits = new List<Squad>();
	private Dictionary<Squad, IMovement> movementGroup = new Dictionary<Squad, IMovement>();
	private List<Formation> placedFormations = new List<Formation>();
	private List<Vector3> positions = new List<Vector3>();
	private Collider[] colliders = new Collider[1];
	private EventSystem eventSystem;
	private TerrainBorder border;
#pragma warning disable 108,114
	private AudioSource audio;
	private Camera camera;
#pragma warning restore 108,114	
	private CamController camController;
	private Line drawedLine;
	private Squad lastSelectSquad;
	private float lastSelectTime;
	private float lastClickTime;
	private Vector3 lastClickPos;
	private Interaction onDrag;
	private Interaction onDraw;
	private Interaction onSelect;
	private Interaction onPlace;
	private Interaction onShift;
	private int cursor;
	private float nextHoverTime;
	private Ray groundRay;
	private RaycastHit groundHit;
	private bool groundCast;
	private bool pointerUI;

	private bool InvalidHit => !groundCast || border.IsOutsideBorder(groundHit.point) || Input.GetKey(KeyCode.Escape);
	public int selectedCount => selectedUnits.Count;

	private void Start()
	{
		eventSystem = EventSystem.current;
		border = Manager.border;
		camera = Manager.mainCamera;
		camController = Manager.camController;
		audio = Manager.cameraSources[0];
		SetCursor(basicCursor);
	}

	private void Update()
	{
		groundRay = camera.ScreenPointToRay(Input.mousePosition);
		groundCast = Physics.Raycast(groundRay, out groundHit, Manager.TerrainDistance, Manager.Ground);
		pointerUI = eventSystem.IsPointerOverGameObject();
		
		OnDragging();
		OnSelection();
		OnPlacement();
		OnShifting();
		OnDrawing();
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
		if (onDraw.enabled || onDrag.enabled || onPlace.enabled || onShift.enabled)
			return;

		// When left mouse button clicked (but not released)
		if (Input.GetMouseButtonDown(0)) {
			onSelect.OnDown(Input.mousePosition, pointerUI);
			
			if (InvalidHit || pointerUI)
				return;

			var currentTime = Time.unscaledTime;
			if ((currentTime - lastClickTime < 0.5f) && Vector.TruncDistance(lastClickPos, groundHit.point) <= 1f) {
				if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Squad) == 0) {
					camController.SetTarget(ObjectPool.Instance.SpawnFromPool(Manager.Way, groundHit.point).transform);
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
			if (Input.GetKey(stopKey) || Input.GetKey(shiftKey)) {
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
					var squad = UnitTable.Instance[colliders[0].gameObject].squad;
					if (Input.GetKey(inclusiveKey)) { //inclusive select
						AddSelected(squad, true);
					} else { //exclusive selected
						DeselectAllExcept(squad);
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
					var ray = camera.ScreenPointToRay(corner); //cast out to world space
					if (Physics.Raycast(ray, out var hit, Manager.TerrainDistance, Manager.Ground)) {
						vertices[i] = transform.InverseTransformPoint(hit.point);
						Debug.DrawLine(camera.ScreenToWorldPoint(corner), hit.point, Color.red, 5.0f);
					} else {
						return;
					}
				}
				
				audio.clip = selectSound;
				audio.Play();
				
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
		if (onDraw.enabled || onDrag.enabled || onSelect.enabled || onShift.enabled)
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
			if (Input.GetKey(stopKey)  || Input.GetKey(drawKey) || selectedUnits.Count == 0) {
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
					selectedUnits.Sort((a, b) => Vector.DistanceSq(onPlace.startPos, a.GetPosition()).CompareTo(Vector.DistanceSq(onPlace.startPos, b.GetPosition())));
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
					var length = direction.Magnitude(); // size of line
					var center = (end + start) / 2f; // center between end and pos
					var angle = -Vector.SignedAngle(direction.Normalized(), Vector3.right, Vector3.up);
					
					placedFormations[i].Expand(positions, start, end, center, angle, length);
				}
			}
		}

		// When right mouse button comes up
		else if (Input.GetMouseButtonUp(1)) {
			if (!onPlace.enabled) { //single place
				if (InvalidHit || selectedUnits.Count == 0 || Input.GetKey(drawKey))
					return;

				// If we hover on enemy units
				if (hover && hover.team == Team.Enemy) {
					var obj = hover.gameObject;
					foreach (var squad in selectedUnits) {
						AddToDynamicGroup(squad, obj);
					}
					
					audio.clip = attackSound;
					audio.Play();
					
					var pos = pointerUI ? hover.GetPosition() : groundHit.point;
					pos.y += 0.5f;
					Instantiate(attackParticle, pos, Quaternion.identity);
					
				} else if (!pointerUI) {

					var dest = groundHit.point;
					dest.y += 0.5f;
				
					var count = selectedUnits.Count;
					if (count == 1) {
						var squad = selectedUnits[0];
						AddToDynamicGroup(squad, ObjectPool.Instance.SpawnFromPool(Manager.Way, dest));
					} else {
						var position = Vector3.zero;
						foreach (var squad in selectedUnits) {
							position += squad.worldTransform.position;
						}
						position /= count;
						
						var pool = ObjectPool.Instance;
						
						var shift = dest - position;
						foreach (var squad in selectedUnits) {
							AddToDynamicGroup(squad, pool.SpawnFromPool(Manager.Way, squad.worldTransform.position + shift));
						}
					}
					
					audio.clip = moveSound;
					audio.Play();
					
					Instantiate(moveParticle, dest, Quaternion.identity);
				}
			} else {
				onPlace.enabled = false;
				
				var pool = ObjectPool.Instance;
				
				var playSound = false;
				foreach (var formation in placedFormations) {
					if (formation.active) {
						AddToDynamicGroup(formation.squad, pool.SpawnFromPool(Manager.Way, formation.centroid), formation.targetOrientation, formation.phalanxLength);
						playSound = true;
					}
				}
				
				if (playSound) {
					audio.clip = placeSound;
					audio.Play();
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
		if (onDraw.enabled || onDrag.enabled || onSelect.enabled || onPlace.enabled)
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
			if (Input.GetKey(stopKey) || !Input.GetKey(shiftKey) || selectedUnits.Count == 0) {
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

				var pool = ObjectPool.Instance;
				
				var playSound = false;
				foreach (var formation in placedFormations) {
					if (formation.active) {
						AddToDynamicGroup(formation.squad, pool.SpawnFromPool(Manager.Way, formation.centroid), formation.targetOrientation);
						playSound = true;
					}
				}
				if (playSound) {
					audio.clip = placeSound;
					audio.Play();
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

	private void OnDrawing()
	{
		if (onShift.enabled || onDrag.enabled || onSelect.enabled || onPlace.enabled)
			return;
		
		// When left mouse button clicked (but not released)
		if (Input.GetMouseButtonDown(1)) {
			onDraw.OnDown(groundHit.point, pointerUI);
		}
		// When left mouse button not locked
		else if (onDraw.locked) 
			return;
		
		// While left mouse button held
		if (Input.GetMouseButton(1)) {
			if (Input.GetKey(stopKey) || !Input.GetKey(drawKey) || selectedUnits.Count != 1) {
				drawedLine?.Destroy();
				onDraw.Lock();
			} else {
				if (InvalidHit ||  Vector.DistanceSq(groundHit.point, onDraw.lastPos) <= 1f)
					return;
				
				if (border.IsOutsideBorder(onDraw.startPos)) {
					onDraw.startPos = groundHit.point;
					return;
				}
				
				if (onDraw.OnHold(groundHit.point, pointerUI, 10f)) {
					drawedLine = new Line(drawLine);
				}
				
				if (onDraw.enabled) {
					var pos = onDraw.lastPos;
					pos.y += 0.5f;
					if (!drawedLine.Contains(pos)) {
						drawedLine.Add(pos);
						drawedLine.Render();
					}
				}
			}
		}
		// When right mouse button comes up
		else if (Input.GetMouseButtonUp(1)) {
			if (onDraw.enabled) {
				onDraw.enabled = false;

				var squad = selectedUnits[0];
				AddToStaticGroup(squad, drawedLine.Points);

				audio.clip = placeSound;
				audio.Play();
				
				drawedLine.Destroy();
			}
		}
		// Should be locked if happens
		else if (onDraw.enabled) {
			drawedLine?.Destroy();
			onDraw.Lock();
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
		var currentTime = Time.unscaledTime;
		if (currentTime > nextHoverTime) {
			if (!HoverOnTarget() && hover) {
				hover = null;
			}
			nextHoverTime = currentTime + 0.1f;
		}
	}

	private void OnCursor()
	{
		var hasUnits = selectedUnits.Count != 0;
		if (hover) {
			SetCursor(hover.team == Team.Enemy && hasUnits ? HasRange() ? rangeCursor : meleeCursor : lookCursor);
		} else if (InvalidHit) {
			SetCursor(invalidCursor);
		} else if (onDrag.enabled) {
			SetCursor(dragCursor);
		} else if (onDraw.enabled) {
			SetCursor(drawCursor);
		} else if (onSelect.enabled) {
			SetCursor(selectCursor);
		} else if (onPlace.enabled) {
			SetCursor(placeCursor);
		} else if (onShift.enabled || hasUnits && Input.GetKey(shiftKey)) {
			SetCursor(shiftCursor);
		} else if (Input.GetKey(inclusiveKey)) {
			SetCursor(inclusiveCursor);
		} else if (Input.GetKey(addKey)) {
			SetCursor(addCursor);
		} else if (hasUnits) {
			SetCursor(moveCursor);
		} else {
			SetCursor(basicCursor);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		AddSelected(UnitTable.Instance[collider.gameObject].squad);
	}

	private void OnGUI()
	{
		if (onSelect.enabled) {
			GUI.Box(Vector.GetScreenRect(onSelect.startPos, Input.mousePosition), "", rectangleStyle);
		}
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
				if (!squad.isEscape) {
					selectedUnits.Add(squad);
					squad.ChangeSelectState(true);
				}
			} else if (toggle) {
				selectedUnits.Remove(squad);
				squad.ChangeSelectState(false);
			}
			squadDesc.OnUpdate();
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

		var time = Time.unscaledTime;
		if ((time - lastSelectTime < 0.5f) && lastSelectSquad == filter) {
			var trans = filter.centerTransform;
			if (camController.target != trans) {
				camController.SetTarget(trans);
				audio.clip = targetSound;
				audio.Play();
			}
		}
		
		lastSelectTime = time;
		lastSelectSquad = filter;
	}

	private void AddToDynamicGroup(Squad squad, GameObject obj, float? orientation = null, float? length = null)
	{
		var target = new Target(obj, orientation, length);
		if (movementGroup.ContainsKey(squad)) {
			var movement = movementGroup[squad];
			if (movement is DynamicMovement) {
				if (Input.GetKey(addKey)) {
					movement.Append(target);
				} else {
					movement.Reset(target);
				}
			} else {
				movement.DestroyAll();
				movement = new DynamicMovement(squad, movementLine, arrowLine);
				movement.Reset(target);
				movementGroup[squad] = movement;
			}
		} else {
			var movement = new DynamicMovement(squad, movementLine, arrowLine);
			movement.Reset(target);
			movementGroup.Add(squad, movement);
		}
	}
	
	private void AddToStaticGroup(Squad squad, List<Vector3> points)
	{
		if (movementGroup.ContainsKey(squad)) {
			var movement = movementGroup[squad];
			movement.DestroyAll();
			movement = new StaticMovement(squad, movementLine, arrowLine, points);
			movementGroup[squad] = movement;
		} else {
			var movement = new StaticMovement(squad, movementLine, arrowLine, points);
			movementGroup.Add(squad, movement);
		}
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
		private ObjectPool pool;

		private const float DefaultAngle = 45f;

		public Formation(Squad squads, GameObject directionLine)
		{
			squad = squads;
			line = new Line(directionLine);
			
			border = Manager.border;
			pool = ObjectPool.Instance;
			
			selectors = new List<GameObject>(squad.unitCount);
			for (var i = 0; i < squad.unitCount; i++) {
				selectors.Add(pool.SpawnFromPool(Manager.Selector));
			}
			active = true;
		}

		public void Expand(List<Vector3> positions, Vector3 start, Vector3 end, Vector3 center, float angle, float length)
		{
			var totalLength = squad.unitCount * squad.unitSize.width;
			length = math.clamp(length, totalLength / 12f, totalLength);

			var shift = FormationUtils.GetFormation(positions, squad.formationShape, squad.unitSize, squad.unitCount, length, true);
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
					pool.ReturnToPool(Manager.Selector, selector);
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
			
			var terrain = Manager.terrain;
			
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
					pos.y = terrain.SampleHeight(pos) + 0.5f;
					selector.transform.SetPositionAndRotation(pos, rot);
				} else {
					pool.ReturnToPool(Manager.Selector, selector);
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
			var count = squad.unitCount;
			for (var i = selectors.Count - 1; i > -1; i--) {
				var selector = selectors[i];
				if (count > i) {
					selector.SetActive(state);
				} else {
					pool.ReturnToPool(Manager.Selector, selector);
					selectors.RemoveAt(i);
				}
			}
			
			active = state;
		}

		public void DestroyAll(bool effect)
		{
			if (effect && active) {
				squad.StartCoroutine(line.FadeLineRenderer(0.5f));

				var count = squad.unitCount;
				for (var i = 0; i < selectors.Count; i++) {
					var selector = selectors[i];
					if (count > i) {
						var trans = selector.transform;
						pool.SpawnFromPool(Manager.Pointer, trans.position, trans.rotation);
					}
					pool.ReturnToPool(Manager.Selector, selector);
				}
			} else {
				line.Destroy();
				
				foreach (var selector in selectors) {
					pool.ReturnToPool(Manager.Selector, selector);
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

	private class StaticMovement : IMovement 
	{
		private Line line;
		private Line head;
		
		private Squad squad;

		private static readonly Quaternion Left = Quaternion.Euler(0, -150, 0);
		private static readonly Quaternion Right = Quaternion.Euler(0, 150, 0);
		
		public StaticMovement(Squad squads, GameObject movementLine, GameObject arrowLine, List<Vector3> points)
		{
			squad = squads;
			line = new Line(movementLine);
			head = new Line(arrowLine);
			
			if (points.Count > 0) {
				line.AddLine(squad.GetPosition(), points[0]);
				for (var i = 1; i < points.Count; i++) {
					line.Add(points[i]);
				}
				
				var origin = line.Last;
				var direction = (origin - line.PreLast).Normalized() * 2.5f;
				var left = origin + Left * direction;
				var right = origin + Right * direction;

				head.Clear();
				head.AddPoint(left);
				head.AddLine(left, origin);
				head.AddLine(origin, right);
				head.Render();
			}
			//line.Smooth(0.15f);
			line.Render();

			var pool = ObjectPool.Instance;
			
			var output = new List<Vector3>();
			LineUtility.Simplify(points, 10f, output);
			if (output.Count > 0) {
				squad.SetDestination(false, new Target(pool.SpawnFromPool(Manager.Way, output[0])));
				for (var i = 1; i < output.Count; i++) {
					squad.SetDestination(true, new Target(pool.SpawnFromPool(Manager.Way, output[i])));
				}
			}
		}
		
		public void Append(Target target)
		{
		}

		public void Reset(Target target)
		{
		}

		public void SetActive(bool value)
		{
			line.SetActive(value);
			head.SetActive(value);
		}

		public bool Update()
		{
			if (line.IsActive && line.Count > 1) {
				var position = squad.worldTransform.position;
				if (Vector.DistanceSq(position, line.First) > Vector.DistanceSq(position, line.Second)) {
					line.RemoveAt(0);
					line.Render();
				}
			}
			
			return squad.isActive;
		}

		public void DestroyAll()
		{
			line.Destroy();
			head.Destroy();
		}
	}
	
	private class DynamicMovement : IMovement
	{
		private Line line;
		private Line head;
		private Squad squad;

		private List<GameObject> targets;
		private float nextUpdateTime;
		
		private readonly Dictionary<GameObject, Squad> cache = new Dictionary<GameObject, Squad>();

		private static readonly Quaternion Left = Quaternion.Euler(0, -150, 0);
		private static readonly Quaternion Right = Quaternion.Euler(0, 150, 0);

		public DynamicMovement(Squad squads, GameObject movementLine, GameObject arrowLine)
		{
			squad = squads;
			line = new Line(movementLine);
			head = new Line(arrowLine);
			targets = new List<GameObject>();
		}

		public void SetActive(bool value)
		{
			line.SetActive(value);
			head.SetActive(value);
		}
		
		public void Append(Target target)
		{
			if (targets.Count > 10)
				return;
			
			AddTarget(target.obj);
			squad.SetDestination(true, target);
		}
		
		public void Reset(Target target)
		{
			ClearAll();
			AddTarget(target.obj);
			squad.SetDestination(false, target);
		}
		
		private void AddTarget(GameObject target)
		{
			targets.Add(target);
			var enemy = SquadTable.Instance[target];
			if (enemy) {
				cache.Add(target, enemy);
			}
			nextUpdateTime = 0.1f;
		}

		public bool Update()
		{
			if (line.IsActive && targets.Count > 0) {
				var currentTime = Time.time;
				if (currentTime > nextUpdateTime) {
					line.Clear();
					
					GameObject toRemove = null;
					var toSmooth = true;
					
					if (squad.state == SquadFSM.Attack) {
						var start = squad.GetPosition();
						line.AddPoint(start);
						
						var target = targets[0];
						
						if (target) {
							var end = cache.ContainsKey(target) ? cache[target].GetPosition() : target.transform.position;
							if (squad.isRange) {
								line.AddCurve(start, end, 20f);
								toSmooth = false;
							} else {
								line.AddPoint(end);
							}
						} else {
							toRemove = target;
						}
					} else {
						var start = squad.worldTransform.position;
						line.AddPoint(start);
						
						foreach (var target in targets) {
							if (target && target.activeInHierarchy) {
								var end = cache.ContainsKey(target) ? cache[target].GetPosition() : target.transform.position;
								line.AddPoint(end);
							} else {
								toRemove = target;
							}
						}
					}
					
					if (toRemove) {
						targets.Remove(toRemove);
						cache.Remove(toRemove);
					}

					if (toSmooth) {
						line.Smooth(0.15f);
					}
					
					line.Render();
					
					DrawArrow();
					
					nextUpdateTime = currentTime + 1f;
				}
			}
			
			return squad.isActive;
		}

		private void DrawArrow()
		{
			int count = line.Count;
			if (count > 1) {
				var origin = line.Last;
				var direction = (origin - line.PreLast).Normalized() * 2.5f;
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
			line.Destroy();
			head.Destroy();
			ClearAll();
		}
		
		private void ClearAll()
		{
			var pool = ObjectPool.Instance;
			foreach (var target in targets) {
				if (target && target.CompareTag("Way")) pool.ReturnToPool(Manager.Way, target);
			}
			targets.Clear();
			cache.Clear();
		}
	}
	
	private interface IMovement
	{
		void Append(Target target);
		void Reset(Target target);
		void SetActive(bool value);
		bool Update();
		void DestroyAll();
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

            if (!pointer && !enabled && !locked && Vector.DistanceSq(startPos, lastPos) > distance * distance) {
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

	#region Utils
	
	private bool HasRange()
	{
		return selectedUnits.Any(squad => squad.isRange);
	}
		
	private bool HoverOnTarget()
	{
		//stop if some interaction is on
		if (onDrag.enabled || onDraw.enabled || onSelect.enabled || onPlace.enabled || onShift.enabled || InvalidHit || pointerUI)
			return false;

		if (Physics.OverlapSphereNonAlloc(groundHit.point, 2f, colliders, Manager.Unit) != 0) {
			hover = UnitTable.Instance[colliders[0].gameObject].squad;
			squadInfo.OnUpdate();
			return true;
		} 
		
		if (Physics.Raycast(groundRay, out var hit, Manager.TerrainDistance, Manager.Squad)) {
			hover = SquadTable.Instance[hit.transform.gameObject];
			squadInfo.OnUpdate();
			return true;
		}

		return false;
	}
	
	private void SetCursor(Texture2D texture)
	{
		var id = texture.GetInstanceID();
		if (cursor == id) 
			return;
		
		cursor = id;
		Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
	}
	
	#endregion
}