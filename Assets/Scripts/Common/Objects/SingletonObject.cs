using UnityEngine;

public abstract class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance {
		get {
			if (instance == null) {
				Debug.LogError(typeof(T).Name + " wasn't created at runtime!");
			}
			return instance;
		}
	}

	private static T instance;

	protected virtual void Awake()
	{
		if (instance != null) DestroyImmediate(gameObject);
		instance = GetComponent<T>();
	}
}