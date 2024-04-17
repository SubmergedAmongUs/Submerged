//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2020 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PowerTools
{


[CustomEditor(typeof(SpriteAnim_POWERTOOLS))]
[CanEditMultipleObjects]
public class SpriteAnimEditor : Editor 
{	

	override public void OnInspectorGUI() 
	{	
		base.OnInspectorGUI();	
		if ( Application.isPlaying )
		{
			SpriteAnim_POWERTOOLS component = target as SpriteAnim_POWERTOOLS;
			
			string debugInfo;
			
			if ( component.Clip != null )
			{							
				if ( component.IsPlaying() == false )
					debugInfo = component.ClipName + "(Stopped)";				
				else 				
				{					
					debugInfo = string.Format("{0}: {1:0.00}",component.ClipName, component.Time);
					if ( component.Paused )
						debugInfo += " (Paused)";
				}
			}
			else debugInfo = "No animation playing";


			EditorGUILayout.HelpBox(debugInfo, MessageType.None);
			//GUILayout.Label( debugInfo, EditorStyles.whiteLabel );
				
		}
	}

	/*
	public void OnSceneGUI()
	{
		SpriteAnim component = target as SpriteAnim;
		if ( component == null )
			return;			
	}*/
}



} 