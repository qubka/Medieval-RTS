using System;
using UnityEngine;

public abstract class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance {
		get {
			/*if (instance == null) {
				var obj = new GameObject(typeof(T).Name);
				instance = obj.AddComponent<T>();
			}*/
			return instance;
		}
	}

	private static T instance;

	protected virtual void Awake()
	{
		if (instance == null) {
			instance = GetComponent<T>();
		} else {
			DestroyImmediate(this);
		}
	}
}