using UnityEngine;

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
	private Transform worldTrasnform;
	private Transform camTransform;
	private TerrainBorder border;
	private ObjectPool objectPool;
	
	#endregion

	#region Movement

	[Header("Movement")]
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
	
	[Header("Height")]
	public float maxHeight = 40f; //maximal height
	public float minHeight = 5f; //minimnal height
	public float keyboardZoomingSensitivity = 2f;
	public float scrollWheelZoomingSensitivity = 40f;
	public float zoomPos = 0.5f; //value in range (0, 1) used as t in Matf.Lerp
	public float smoothZoomTime = 0.1f; //Mathf.SmoothDamp interpolation
	private float currentZoomVelocity; // Mathf.SmoothDamp needs this for interpolation
	
	#endregion

	#region MapLimits

	[Header("Map Limits")]
	//public bool canRotate = true;
	public bool limitMap = true;
	//private float limitX; //x limit of map
	//private float limitZ; //z limit of map

	#endregion

	#region Targeting

	[Header("Targeting")]
	public Transform target; //target to follow
	public float distance = 10.0f;
	public float smoothRotationTime = 0.0001f;
	private float currentRotationVelocity;
	//private bool lookAtTarget;

	#endregion

	#region Shake

	[Header("Shake")]
	public float TraumaExponent = 1f;
	public Vector3 MaximumAngularShake = new Vector3(0.25f, 0.25f, 0.25f);
	public Vector3 MaximumTranslationShake = new Vector3(0.005f,  0.005f,  0.005f);
	private float trauma;
    private Vector3 lastPosition;
    private Vector3 lastRotation;

	#endregion

	#region Input

	[Header("Input")]
	public bool useScreenEdgeInput = true;
	public float screenEdgeBorder = 10f;
	[Space]
	public bool useKeyboardInput = true;
	public string horizontalAxis = "Horizontal";
	public string verticalAxis = "Vertical";
	[Space]
	public bool useKeyboardZooming = true;
	public KeyCode zoomInKey = KeyCode.Z;
	public KeyCode zoomOutKey = KeyCode.X;
	[Space]
	public bool useScrollwheelZooming = true;
	public string zoomingAxis = "Mouse ScrollWheel";
	[Space]
	public bool useKeyboardRotation = true;
	public KeyCode rotateRightKey = KeyCode.E;
	public KeyCode rotateLeftKey = KeyCode.Q;
	public KeyCode rotateUpKey = KeyCode.R;
	public KeyCode rotateDownKey = KeyCode.F;
	[Space]
	public bool useMousePanning = true;
	public KeyCode mousePanningKey = KeyCode.Mouse3;
	[Space]
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
		worldTrasnform = transform;
		camTransform = worldTrasnform.parent;
		var euler = camTransform.eulerAngles;
		rotationX = euler.x;
		rotationY = euler.y;
		border = Manager.border;
		objectPool = Manager.objectPool;
	}

	private void LateUpdate()
	{
		PcCamera();
	}
	
	#endregion

	#region PC_Methods

	/// <summary>
	/// update camera movement and rotation on pc
	/// </summary>
	private void PcCamera()
	{
		if (target) 
			FollowTarget();
		else
			Move();
		
		HeightCalculation();
		Rotation();
		LimitPosition();
		Shake();
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
			desiredMove = Quaternion.Euler(new Vector3(0f, camTransform.eulerAngles.y, 0f)) * desiredMove;
			camTransform.position += desiredMove;
		}

		if (useScreenEdgeInput) {
			var desiredMove = new Vector3();

			float height = Screen.height;
			float width = Screen.width;
			
			var leftRect = new Rect(0f, 0f, screenEdgeBorder, height);
			var rightRect = new Rect(Screen.width - screenEdgeBorder, 0f, screenEdgeBorder, height);
			var upRect = new Rect(0f, height - screenEdgeBorder, width, screenEdgeBorder);
			var downRect = new Rect(0f, 0f, width, screenEdgeBorder);

			desiredMove.x = leftRect.Contains(MouseInput) ? -1f : rightRect.Contains(MouseInput) ? 1f : 0f;
			desiredMove.z = upRect.Contains(MouseInput) ? 1f : downRect.Contains(MouseInput) ? -1f : 0f;

			desiredMove *= screenEdgeMovementSpeed;
			desiredMove *= Time.deltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, camTransform.eulerAngles.y, 0f)) * desiredMove;
			camTransform.position += desiredMove;
		}

		if (useMousePanning && Input.GetKey(mousePanningKey) && MouseAxis.SqMagnitude() > 0f) {
			var desiredMove = new Vector3(-MouseAxis.x, 0f, -MouseAxis.y);

			desiredMove *= panningSpeed;
			desiredMove *= Time.deltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, camTransform.eulerAngles.y, 0f)) * desiredMove;
			camTransform.position += desiredMove;
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
		
		var position = camTransform.position;
		var ray = new Ray(position, Vector3.down);
		if (Physics.Raycast(ray, out var hit, 1000f, Manager.Ground | Manager.Water)) {
			var desiredHeight = hit.point.y + Mathf.Lerp(minHeight, maxHeight, zoomPos);
			position.y = Mathf.SmoothDamp(position.y, desiredHeight, ref currentZoomVelocity, smoothZoomTime);
			camTransform.position = position;
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
			camTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}

		if (useMouseRotation && Input.GetKey(mouseRotationKey)) {
			var axis = MouseAxis * (Time.deltaTime * mouseRotationSpeed);
			rotationX -= axis.y;
			rotationY += axis.x;
			
			rotationX = Mathf.Clamp(rotationX, clampRotationAngle.x, clampRotationAngle.y);
			camTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}
	}

	/// <summary>
	/// limit camera position
	/// </summary>
	private void LimitPosition()
	{
		if (!limitMap)
			return;

		var position = camTransform.position;
		position = new Vector3(Mathf.Clamp(position.x, -border.limitX, border.limitX), position.y, Mathf.Clamp(position.z, -border.limitZ, border.limitZ));
		camTransform.position = position;
	}

	// TODO:
	private void Shake()
	{
		var shake = Mathf.Pow(trauma, TraumaExponent);
		
		/* Only apply this when there is active shake */
		if(shake > 0f) {
			var time = Time.time;
			var previousRotation = lastRotation;
			var previousPosition = lastPosition;
			
			/* In order to avoid affecting the transform current position and rotation each frame we substract the previous translation and rotation */
			lastPosition = new Vector3(
				MaximumTranslationShake.x * (Mathf.PerlinNoise(0f, time * 25f) * 2f - 1f),
				MaximumTranslationShake.y * (Mathf.PerlinNoise(1f, time * 25f) * 2f - 1f),
				MaximumTranslationShake.z * (Mathf.PerlinNoise(2f, time * 25f) * 2f - 1f)
			) * shake;

			lastRotation = new Vector3(
				MaximumAngularShake.x * (Mathf.PerlinNoise(3f, time * 25f) * 2f - 1f),
				MaximumAngularShake.y * (Mathf.PerlinNoise(4f, time * 25f) * 2f - 1f),
				MaximumAngularShake.z * (Mathf.PerlinNoise(5f, time * 25f) * 2f - 1f)
			) * shake;

			worldTrasnform.localPosition += lastPosition - previousPosition;
			worldTrasnform.localRotation = Quaternion.Euler(worldTrasnform.localRotation.eulerAngles + lastRotation - previousRotation);
			trauma = Mathf.Clamp01(trauma - Time.deltaTime);
		} else {
			if (lastPosition == Vector3.zero && lastRotation == Vector3.zero)
				return;
			
			/* Clear the transform of any left over translation and rotations */
			worldTrasnform.localPosition -= lastPosition;
			worldTrasnform.localRotation = Quaternion.Euler(worldTrasnform.localRotation.eulerAngles - lastRotation);
			lastPosition = Vector3.zero;
			lastRotation = Vector3.zero;
		}
	}
	
	/// <summary>
	///  Applies a shake value to the camera.
	/// </summary>
	/// <param name="shake">[0,1] Amount of shake to apply to the object</param>
	public void InduceShake(float shake)
	{
		trauma = Mathf.Clamp01(trauma + shake);
	}
	
	/// <summary>
	/// follow target if target != null
	/// </summary>
	private void FollowTarget()
	{
		// Calculate the current rotation angles
		var desiredRotationAngle = target.eulerAngles.y;
		var desiredPosition = target.position;
		var currentRotationAngle = camTransform.eulerAngles.y;
		var currentPosition = camTransform.position;

		// Damp the rotation around the y-axis
		currentRotationAngle = Mathf.SmoothDampAngle(currentRotationAngle, desiredRotationAngle, ref currentRotationVelocity, smoothRotationTime);

		// Convert the angle into a rotation
		var currentRotation = Quaternion.Euler(0f, currentRotationAngle, 0f);

		// Set the position of the camera on the x-z plane to:
		// distance meters behind the target
		desiredPosition -= currentRotation * Vector3.forward * distance;

		// Move to the desired position
		desiredPosition = new Vector3(desiredPosition.x, currentPosition.y, desiredPosition.z);
		currentPosition = Vector3.MoveTowards(currentPosition, desiredPosition,  followingSpeed * Time.deltaTime);
		camTransform.position = currentPosition;

		// Always look at the target
		/*if (lookAtTarget) {
			worldTrasnform.LookAt(target);
		}*/

		// Reset target if press any key
		if (KeyboardInput.SqMagnitude() > 0f) {
			ResetTarget();
		}
	}

	/// <summary>
	/// set the target
	/// </summary>
	/// <param name="trans">Any object transform to follow</param>
	/// <param name="lookAt">True to use look at feature on the target transform.</param>
	public void SetTarget(Transform trans, bool lookAt = false)
	{
		if (target && target.CompareTag("Way")) objectPool.ReturnToPool("Way", target.gameObject);
		//worldTrasnform.localRotation = Quaternion.identity;
		target = trans;
		//lookAtTarget = lookAt;
	}

	/// <summary>
	/// reset the target (target is set to null)
	/// </summary>
	public void ResetTarget()
	{
		if (target && target.CompareTag("Way")) objectPool.ReturnToPool("Way", target.gameObject);
		//worldTrasnform.localRotation = Quaternion.identity;
		target = null;
		//lookAtTarget = false;
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
		
		rotationY += mouseX * mouseSensitivity * deltaTime * 0.02f;
		rotationX += mouseY * mouseSensitivity * deltaTime * 0.02f;	
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

		var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).Magnitude();
		var touchDeltaMag = (touchZero.position - touchOne.position).Magnitude();

		var z = (prevTouchDeltaMag - touchDeltaMag) * 0.001f * scrollWheelZoomingSensitivity;

		worldTransform.Translate(new Vector3(0f, 0f, -z));
	}*/
	
	#endregion
}