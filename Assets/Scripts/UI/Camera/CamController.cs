using Metadesc.CameraShake;
using UnityEngine;
using Object = ObjectExtention; 

[RequireComponent(typeof(Camera))]
[AddComponentMenu("RTS Camera")]
public class CamController : MonoBehaviour 
{
	#region Foldouts

#if UNITY_EDITOR

	//public int lastTab = 0;
	/*
		public bool movementSettingsFoldout;
		public bool zoomingSettingsFoldout;
		public bool rotationSettingsFoldout;
		public bool heightSettingsFoldout;
		public bool mapLimitSettingsFoldout;
		public bool targetingSettingsFoldout;
		public bool inputSettingsFoldout;
	*/
#endif
	private Transform worldTransform;
	private ShakeManager shakeManager;
	private TerrainBorder border;
	
	#endregion

	#region Movement

	public float keyboardMovementSpeed = 80f; //speed with keyboard movement
	public float screenEdgeMovementSpeed = 40f; //spee with screen edge movement
	public float followingSpeed = 50f; //speed when following a target
	public float rotationSpeed = 100f;
	public float panningSpeed = 50f;
	public float mouseRotationSpeed = 80f;
	public Vector2 clampRotationAngle = new Vector2(10f, 45f);
	private float rotationX;
	private float rotationY;
	
	#endregion

	#region Height
	
	public float maxHeight = 40f; //maximal height
	public float minHeight = 5f; //minimnal height
	public float keyboardZoomingSensitivity = 2f;
	public float scrollWheelZoomingSensitivity = 40f;
	public float zoomPos = 0.5f; //value in range (0, 1) used as t in Matf.Lerp
	public float smoothTime = 0.1f; //Mathf.SmoothDamp interpolation
	private float currentVelocity; // Mathf.SmoothDamp needs this for interpolation
	
	#endregion

	#region MapLimits

	//public bool canRotate = true;
	public bool limitMap = true;
	//private float limitX; //x limit of map
	//private float limitZ; //z limit of map

	#endregion

	#region Targeting

	public GameObject target; //target to follow

	/// <summary>
	/// are we following target
	/// </summary>
	public bool FollowingTarget => target;

	#endregion

	#region Input

	public bool useScreenEdgeInput = true;
	public float screenEdgeBorder = 10f;

	public bool useKeyboardInput = true;
	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";

	public bool useKeyboardZooming = true;
	public KeyCode zoomInKey = KeyCode.Z;
	public KeyCode zoomOutKey = KeyCode.X;

	public bool useScrollwheelZooming = true;
	public string zoomingAxis = "Mouse ScrollWheel";

	public bool useKeyboardRotation = true;
	public KeyCode rotateRightKey = KeyCode.E;
	public KeyCode rotateLeftKey = KeyCode.Q;
	public KeyCode rotateUpKey = KeyCode.R;
	public KeyCode rotateDownKey = KeyCode.F;

	public bool useMousePanning = true;
	public KeyCode mousePanningKey = KeyCode.Mouse3;

	public bool useMouseRotation = true;
	public KeyCode mouseRotationKey = KeyCode.Mouse2;

	/*
		public bool useMouseMovement = true;
		public KeyCode mouseMovementKey = KeyCode.Mouse0;
	*/
	
	private Vector2 KeyboardInput => useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)) : Vector2.zero;
	private Vector2 MouseInput => Input.mousePosition;
	private float ScrollWheel => -Input.GetAxis(zoomingAxis);
	private Vector2 MouseAxis => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
	private int TouchCount => Input.touchCount;

	private int ZoomDirection {
		get {
			var zoomIn = Input.GetKey(zoomInKey);
			var zoomOut = Input.GetKey(zoomOutKey);
			if (zoomIn && zoomOut) {
				return 0;
			} else if (zoomIn) {
				return -1;
			} else if (zoomOut) {
				return 1;
			} else {
				return 0;
			}
		}
	}

	private int HorizontalRotation {
		get {
			var rotateRight = Input.GetKey(rotateRightKey);
			var rotateLeft = Input.GetKey(rotateLeftKey);
			if (rotateLeft && rotateRight) {
				return 0;
			} else if (rotateLeft) {
				return -1;
			} else if (rotateRight) {
				return 1;
			} else {
				return 0;
			}
		}
	}

	private int VerticalRotation {
		get {
			var rotateUp = Input.GetKey(rotateUpKey);
			var rotateDown = Input.GetKey(rotateDownKey);
			if (rotateUp && rotateDown) {
				return 0;
			} else if (rotateUp) {
				return 1;
			} else if (rotateDown) {
				return -1;
			} else {
				return 0;
			}
		}
	}

	#endregion

	#region Unity_Methods

	private void Start()
	{
		worldTransform = transform;
		var euler = worldTransform.eulerAngles;
		rotationX = euler.x;
		rotationY = euler.y;
		border = Manager.border;
		shakeManager = ShakeManager.I;
	}

	private void LateUpdate()
	{
		PcCamera();
		
		// After this just set the shake position and rotation changes.
		// If your camera script modifies the rotation and position in LateUpdate, 
		// then move this to LateUpdate after changing these attributes.
		// This call add a shake offset to the camera position and rotation, that the reason why this works.
		var shakeResult = shakeManager.UpdateAndGetShakeResult();
		if (shakeResult.DoProcessShake) {
			worldTransform.localPosition += shakeResult.ShakeLocalPos;
			worldTransform.localRotation *= shakeResult.ShakeLocalRot;
		}
	}
	
	#endregion

	#region PC_Methods

	/// <summary>
	/// update camera movement and rotation on pc
	/// </summary>
	private void PcCamera()
	{
		if (FollowingTarget) 
			FollowTarget();
		else
			Move();
		
		HeightCalculation();
		Rotation();
		LimitPosition();
	}

	/// <summary>
	/// move camera with keyboard or with screen edge
	/// </summary>
	private void Move()
	{
		if (useKeyboardInput) {
			var desiredMove = new Vector3(KeyboardInput.x, 0f, KeyboardInput.y);

			desiredMove *= keyboardMovementSpeed;
			desiredMove *= Time.deltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, worldTransform.eulerAngles.y, 0f)) * desiredMove;
			worldTransform.position += desiredMove;
		}

		if (useScreenEdgeInput) {
			var desiredMove = new Vector3();

			var leftRect = new Rect(0f, 0f, screenEdgeBorder, Screen.height);
			var rightRect = new Rect(Screen.width - screenEdgeBorder, 0f, screenEdgeBorder, Screen.height);
			var upRect = new Rect(0f, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
			var downRect = new Rect(0f, 0f, Screen.width, screenEdgeBorder);

			desiredMove.x = leftRect.Contains(MouseInput) ? -1f : rightRect.Contains(MouseInput) ? 1f : 0f;
			desiredMove.z = upRect.Contains(MouseInput) ? 1f : downRect.Contains(MouseInput) ? -1f : 0f;

			desiredMove *= screenEdgeMovementSpeed;
			desiredMove *= Time.deltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, worldTransform.eulerAngles.y, 0f)) * desiredMove;
			worldTransform.position += desiredMove;
		}

		if (useMousePanning && Input.GetKey(mousePanningKey) && MouseAxis.sqrMagnitude > 0f) {
			var desiredMove = new Vector3(-MouseAxis.x, 0f, -MouseAxis.y);

			desiredMove *= panningSpeed;
			desiredMove *= Time.deltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, worldTransform.eulerAngles.y, 0f)) * desiredMove;
			worldTransform.position += desiredMove;
		}
	}

	/// <summary>
	/// calcualte height
	/// </summary>
	private void HeightCalculation()
	{
		if (useScrollwheelZooming)
			zoomPos += ScrollWheel * Time.deltaTime * scrollWheelZoomingSensitivity;
		if (useKeyboardZooming)
			zoomPos += ZoomDirection * Time.deltaTime * keyboardZoomingSensitivity;

		zoomPos = Mathf.Clamp01(zoomPos);
		
		var position = worldTransform.position;
		var ray = new Ray(position, Vector3.down);
		if (Physics.Raycast(ray, out var hit, 1000f, Manager.Ground | Manager.Water)) {
			var targetHeight = hit.point.y + Mathf.Lerp(minHeight, maxHeight, zoomPos);
			position.y = Mathf.SmoothDamp(position.y, targetHeight, ref currentVelocity, smoothTime);
			worldTransform.position = position;
		}
	}

	/// <summary>
	/// rotate camera
	/// </summary>
	private void Rotation()
	{
		if (useKeyboardRotation) {
			var speed = Time.deltaTime * rotationSpeed;
			rotationX -= VerticalRotation * speed;
			rotationY += HorizontalRotation * speed;
			
			rotationX = Mathf.Clamp(rotationX, clampRotationAngle.x, clampRotationAngle.y);
			worldTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}

		if (useMouseRotation && Input.GetKey(mouseRotationKey)) {
			var axis = MouseAxis * (Time.deltaTime * mouseRotationSpeed);
			rotationX -= axis.y;
			rotationY += axis.x;
			
			rotationX = Mathf.Clamp(rotationX, clampRotationAngle.x, clampRotationAngle.y);
			worldTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}
	}

	/// <summary>
	/// limit camera position
	/// </summary>
	private void LimitPosition()
	{
		if (!limitMap)
			return;

		var position = worldTransform.position;
		position = new Vector3(Mathf.Clamp(position.x, -border.limitX, border.limitX), position.y, Mathf.Clamp(position.z, -border.limitZ, border.limitZ));
		worldTransform.position = position;
	}
	
	/// <summary>
	/// follow target if target != null
	/// </summary>
	private void FollowTarget()
	{
		var position = worldTransform.position;
		var followPos = target.transform.position;
		var targetPos = new Vector3(followPos.x, position.y, followPos.z);
		position = Vector3.MoveTowards(position, targetPos, Time.deltaTime * followingSpeed);
		worldTransform.position = position;

		if (KeyboardInput.sqrMagnitude > 0f) {
			ResetTarget();
		}
	}

	/// <summary>
	/// set the target
	/// </summary>
	/// <param name="gameObject"></param>
	public void SetTarget(GameObject gameObject)
	{
		Object.DestroyIfNamed(target, "wayPointer");
		target = gameObject;
	}

	/// <summary>
	/// reset the target (target is set to null)
	/// </summary>
	public void ResetTarget()
	{
		Object.DestroyIfNamed(target, "wayPointer");
		target = null;
	}

	#endregion

	#region Mobile_Methods

	/*/// <summary>
	/// update camera movement and rotation on mobile
	/// </summary>
	private void MobileCamera()
	{
		switch (TouchCount) {
			case 1:
				SingleTouch();
				break;
			case 2:
				DoubleTouch();
				break;
		}
	}

	/// <summary>
	/// single touch
	/// </summary>
	private void SingleTouch()
	{
		var touch = Input.GetTouch(0);

		if (touch.phase == TouchPhase.Began) {
			if (EventSystem.current.IsPointerOverGameObject() ||
			    EventSystem.current.IsPointerOverGameObject(touch.fingerId)) {
				canRotate = false;
			} else {
				canRotate = true;
			}
		}

		if (!canRotate)
			return;

		var mouseX = touch.deltaPosition.x;
		var mouseY = -touch.deltaPosition.y;
		
		rotationY += mouseX * mouseSensitivity * Time.deltaTime * 0.02f;
		rotationX += mouseY * mouseSensitivity * Time.deltaTime * 0.02f;	
		rotationX = Mathf.Clamp(rotationX, -clampAngle, clampAngle);
		worldTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
	}

	/// <summary>
	/// double touch
	/// </summary>
	private void DoubleTouch()
	{
		var touchZero = Input.GetTouch(0);
		var touchOne = Input.GetTouch(1);

		var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
		var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

		var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
		var touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

		var z = (prevTouchDeltaMag - touchDeltaMag) * 0.001f * scrollWheelZoomingSensitivity;

		worldTransform.Translate(new Vector3(0f, 0f, -z));
	}*/
	
	#endregion
}