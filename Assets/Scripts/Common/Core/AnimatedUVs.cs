using UnityEngine;

public class AnimatedUVs : MonoBehaviour 
{
	public float scrollSpeed = 0.5F;
	private Material material;
	
	private void Awake()
	{
		material = GetComponent<Renderer>().material;
	}
	
	private void Update()
	{
		var offset = Time.time * scrollSpeed;
		material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
	}
}