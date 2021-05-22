using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class SquadLayout : Layout<Squad>
{
	private void Start()
 	{
 		manager = SquadManager.Instance;
 	}
}