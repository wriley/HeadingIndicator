﻿using UnityEngine;
using System.Collections;

public class rotateToMouse : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 mouse = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		Vector2 relmousepos = new Vector2(mouse.x - 0.5f, mouse.y - 0.5f);
		float angle = Vector2.Angle(Vector2.up, relmousepos);
		if(relmousepos.x > 0)
		{
			angle = 360-angle;
		}
		Quaternion quat = Quaternion.identity;
		quat.eulerAngles = new Vector3(0,0,angle);
		transform.rotation = quat;
	}
}
