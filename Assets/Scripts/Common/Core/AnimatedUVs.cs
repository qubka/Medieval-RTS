using UnityEngine;

public class AnimatedUVs : MonoBehaviour 
{
	public float scrollSpeed = 0.5F;
	public Material material;
	
	private void Start()
	{
		material = GetComponent<Renderer>().material;
	}
	
	private void Update()
	{
		var offset = Time.time * scrollSpeed;
		material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
	}
}