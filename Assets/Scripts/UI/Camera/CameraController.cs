using System;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("RTS Camera")]
public class CameraController : MonoBehaviour 
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
	private Transform cameraTransform;
	private TerrainBorder border;
	
	#endregion

	#region Movement

	[Header("Movement")]
	public float keyboardMovementSpeed = 80f; //speed with keyboard movement
	public float screenEdgeMovementSpeed = 40f; //speed with screen edge movement
	public float followingSpeed = 50f; //speed when following a target
	//public float lookingSpeed = 10f; // speed when looking at target
	public float rotationSpeed = 100f;
	public float panningSpeed = 50f;
	public float mouseRotationSpeed = 80f;
	public Vector2 clampRotationAngle = new Vector2(10f, 45f);
	[ReadOnly] public float rotationX;
	[ReadOnly] public float rotationY;
	
	#endregion

	#region Height
	
	[Header("Height")]
	public float maxHeight = 40f; //maximal height
	public float minHeight = 5f; //minimnal height
	public float keyboardZoomingSensitivity = 2f;
	public float scrollWheelZoomingSensitivity = 40f;
	public float zoomPos = 0.5f; //value in range (0, 1) used as t in Matf.Lerp
	public float smoothZoomTime = 0.1f; //
	public bool useZoomRotation;
	public AnimationCurve rotationCurve;
	private float currentHeight;
	private float currentZoomVelocity;
	private RaycastHit groundHit;

	public float DistToGround => currentHeight / maxHeight;
	
	#endregion

	#region MapLimits

	[Header("Map Limits")]
	//public bool canRotate = true;
	public bool limitMap = true;
	//private float limitX; //x limit of map
	//private float limitZ; //z limit of map

	public Transform Transform => cameraTransform;
	
	#endregion

	#region Targeting

	[Header("Targeting")]
	public Transform target; //target to follow
	public float distance = 10.0f;

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
	public string mouseHorizontalAxis = "Mouse X";
	public bool useHorizontalRotation = true;
	public KeyCode rotateUpKey = KeyCode.R;
	public KeyCode rotateDownKey = KeyCode.F;
	public string mouseVerticalAxis = "Mouse Y";
	public bool useVerticalRotation = true;
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
	
	//TODO: Replace by Raw with smoothing
	private Vector2 KeyboardInput => new Vector2(Input.GetAxisRaw(horizontalAxis), Input.GetAxisRaw(verticalAxis));
	private Vector2 MouseInput => Input.mousePosition;
	private float ScrollWheel => -Input.GetAxis(zoomingAxis);
	private Vector2 MouseAxis => new Vector2(Input.GetAxis(mouseHorizontalAxis), Input.GetAxis(mouseVerticalAxis));
	private int TouchCount => Input.touchCount;
	private static float DeltaTime => Time.unscaledDeltaTime;

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

	private void Awake()
	{
		worldTrasnform = transform;
		cameraTransform = worldTrasnform.parent;
		var euler = cameraTransform.eulerAngles;
		rotationX = euler.x;
		rotationY = euler.y;
	}

	private void Start()
	{
		border = Manager.border;
	}

	private void LateUpdate()
	{
		PcCamera();
	}
	
	public void Load(CameraSave save)
	{
		cameraTransform.position = save.position;
		rotationX = save.rotationX;
		rotationY = save.rotationY;
		zoomPos = save.zoomPos;
	}
	
	#endregion

	#region PC_Methods

	/// <summary>
	/// update camera movement and rotation on pc
	/// </summary>
	private void PcCamera()
	{
		HeightCalculation();
		LimitPosition();
		ShakeEffect();
		Rotation();
		
		if (target) {
			FollowTarget();
		} else {
			Move();
		}
	}

	/// <summary>
	/// move camera with keyboard or with screen edge
	/// </summary>
	private void Move()
	{
		if (useKeyboardInput) {
			var input = KeyboardInput;
			if (input.SqMagnitude() > 0f) {
				var desiredMove = input.Project();

				desiredMove *= keyboardMovementSpeed;
				desiredMove *= DeltaTime;
				desiredMove = Quaternion.Euler(new Vector3(0f, cameraTransform.eulerAngles.y, 0f)) * desiredMove;
				cameraTransform.position += desiredMove;
			}
		}

		if (useScreenEdgeInput) {
			var desiredMove = new Vector3();

			float height = Screen.height;
			float width = Screen.width;
			
			var leftRect = new Rect(0f, 0f, screenEdgeBorder, height);
			var rightRect = new Rect(Screen.width - screenEdgeBorder, 0f, screenEdgeBorder, height);
			var upRect = new Rect(0f, height - screenEdgeBorder, width, screenEdgeBorder);
			var downRect = new Rect(0f, 0f, width, screenEdgeBorder);

			var input = MouseInput;
			desiredMove.x = leftRect.Contains(input) ? -1f : rightRect.Contains(input) ? 1f : 0f;
			desiredMove.z = upRect.Contains(input) ? 1f : downRect.Contains(input) ? -1f : 0f;

			desiredMove *= screenEdgeMovementSpeed;
			desiredMove *= DeltaTime;
			desiredMove = Quaternion.Euler(new Vector3(0f, cameraTransform.eulerAngles.y, 0f)) * desiredMove;
			cameraTransform.position += desiredMove;
		}
		
		if (useMousePanning && Input.GetKey(mousePanningKey)) {
			var axis = MouseAxis;
			if (axis.SqMagnitude() > 0f) {
				var desiredMove = -axis.Project();

				desiredMove *= panningSpeed;
				desiredMove *= DeltaTime;
				desiredMove = Quaternion.Euler(new Vector3(0f, cameraTransform.eulerAngles.y, 0f)) * desiredMove;
				cameraTransform.position += desiredMove;
			}
		}
	}

	/// <summary>
	/// calcualte height
	/// </summary>
	private void HeightCalculation()
	{
		if (useScrollwheelZooming)
			zoomPos += ScrollWheel * DeltaTime * scrollWheelZoomingSensitivity;
		if (useKeyboardZooming)
			zoomPos += ZoomDirection * DeltaTime * keyboardZoomingSensitivity;

		zoomPos = MathExtention.Clamp01(zoomPos);
		
		var position = cameraTransform.position;
		var ray = new Ray(position, Vector3.down);
		if (Physics.Raycast(ray, out groundHit, 1000f, Manager.Ground | Manager.Water)) {
			var desiredHeight = groundHit.point.y + math.lerp(minHeight, maxHeight, zoomPos);
			position.y = Mathf.SmoothDamp(position.y, desiredHeight, ref currentZoomVelocity, smoothZoomTime, float.PositiveInfinity, DeltaTime);
			cameraTransform.position = position;
			currentHeight = groundHit.distance;
		}
	}

	/// <summary>
	/// rotate camera
	/// </summary>
	private void Rotation()
	{
		if (useKeyboardRotation) {
			var speed = DeltaTime * rotationSpeed;
			if (useVerticalRotation)
				rotationX -= VerticalRotation * speed;
			if (useHorizontalRotation)
				rotationY += HorizontalRotation * speed;

			rotationX = useZoomRotation ? rotationCurve.Evaluate(DistToGround) : math.clamp(rotationX, clampRotationAngle.x, clampRotationAngle.y);
			cameraTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}

		if (useMouseRotation && Input.GetKey(mouseRotationKey)) {
			var axis = MouseAxis * (DeltaTime * mouseRotationSpeed);
			rotationX -= axis.y;
			rotationY += axis.x;
			
			rotationX = math.clamp(rotationX, clampRotationAngle.x, clampRotationAngle.y);
			cameraTransform.eulerAngles = new Vector3(rotationX, rotationY, 0f);
		}
	}

	/// <summary>
	/// limit camera position
	/// </summary>
	private void LimitPosition()
	{
		if (!limitMap)
			return;

		var position = cameraTransform.position;
		position = new Vector3(math.clamp(position.x, -border.limitX, border.limitX), position.y, math.clamp(position.z, -border.limitZ, border.limitZ));
		cameraTransform.position = position;
	}

	/// <summary>
	/// shake camera
	/// </summary>
	private void ShakeEffect()
	{
		var shake = math.pow(trauma, TraumaExponent);
		
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
			trauma = MathExtention.Clamp01(trauma - DeltaTime);
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
		trauma = MathExtention.Clamp01(trauma + shake);
	}
	
	/// <summary>
	/// follow target if target != null
	/// </summary>
	private void FollowTarget()
	{
		var currentPosition = cameraTransform.position;
		//var currentRotation = cameraTransform.rotation;
		var targetPosition = target.position;
		//var targetRotation = Quaternion.LookRotation(targetPosition - currentPosition);
		
		targetPosition -= Quaternion.Euler(0f, rotationY, 0f) * Vector3.forward * distance;
		targetPosition = new Vector3(targetPosition.x, currentPosition.y, targetPosition.z);
		//currentRotation = Quaternion.Slerp(currentRotation, targetRotation, lookingSpeed * DeltaTime);
		currentPosition = Vector.MoveTowards(currentPosition, targetPosition,  followingSpeed * DeltaTime);
		cameraTransform.position = currentPosition;

		if (useKeyboardInput && KeyboardInput.SqMagnitude() > 0f) {
			ResetTarget();
		}
	}

	/// <summary>
	/// set the target
	/// </summary>
	/// <param name="trans">Any object transform to follow</param>
	public void SetTarget(Transform trans)
	{
		if (target && target.CompareTag("Way")) ObjectPool.Instance.ReturnToPool(Manager.Way, target.gameObject);
		target = trans;
	}

	/// <summary>
	/// reset the target (target is set to null)
	/// </summary>
	public void ResetTarget()
	{
		if (target && target.CompareTag("Way")) ObjectPool.Instance.ReturnToPool(Manager.Way, target.gameObject);
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
		
		rotationY += mouseX * mouseSensitivity * DeltaTime * 0.02f;
		rotationX += mouseY * mouseSensitivity * DeltaTime * 0.02f;	
		rotationX = math.clamp(rotationX, -clampAngle, clampAngle);
		cameraTransform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
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

[Serializable]
public class CameraSave {

	public Vector3 position;
	public float rotationX;
	public float rotationY;
	public float zoomPos;
	
	public CameraSave(CameraController cc)
	{
		position = cc.Transform.position;
		rotationX = cc.rotationX;
		rotationY = cc.rotationY;
		zoomPos = cc.zoomPos;
	}
}
