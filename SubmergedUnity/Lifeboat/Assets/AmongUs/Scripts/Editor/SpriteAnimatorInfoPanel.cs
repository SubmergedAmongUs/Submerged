//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2017 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PowerTools.Anim;

namespace PowerTools
{

public partial class SpriteAnimator
{
	#region Definitions


	#endregion
	#region Vars: Private

	ReorderableList m_framesReorderableList = null;
	Vector2 m_scrollPosition = Vector2.zero;
	bool m_settingsUnfolded = false;

	#endregion
	#region Funcs: Init

	void InitialiseFramesReorderableList()
	{
		m_framesReorderableList = new ReorderableList( m_frames, typeof(AnimFrame),true,true,true,true);
		m_framesReorderableList.drawHeaderCallback = (Rect rect) => 
		{ 
			EditorGUI.LabelField(rect,"Frames"); 
			EditorGUI.LabelField(new Rect(rect){x=rect.width-37,width=45},"Length"); 
		};
		m_framesReorderableList.drawElementCallback = LayoutFrameListFrame;
		m_framesReorderableList.onSelectCallback = (ReorderableList list) => 
		{
			SelectFrame(m_frames[m_framesReorderableList.index]);
		};
	}

	#endregion
	#region Funcs: Layout

	void LayoutInfoPanel( Rect rect )
	{

		GUILayout.BeginArea(rect, EditorStyles.inspectorFullWidthMargins);
		GUILayout.Space(20);

		// Animation length
		EditorGUILayout.LabelField( string.Format("Length: {0:0.00} sec  {1:D} samples", m_clip.length, Mathf.RoundToInt(m_clip.length/GetMinFrameTime())), new GUIStyle(EditorStyles.miniLabel){normal = { textColor = Color.gray }});


		// Speed/Framerate
        GUI.SetNextControlName("Framerate");
		float newFramerate = EditorGUILayout.DelayedFloatField( "Sample Rate", m_clip.frameRate );
		if ( Mathf.Approximately( newFramerate, m_clip.frameRate ) == false )
		{
			ChangeFrameRate(newFramerate, true);
		}
        GUI.SetNextControlName("Length");
		float oldLength = Utils.Snap( m_clip.length, 0.001f );
		float newLength = Utils.Snap( EditorGUILayout.FloatField( "Length (sec)", oldLength ), 0.001f );
		if ( Mathf.Approximately( newLength, oldLength ) == false && newLength > 0 )
		{
			newFramerate = Mathf.Max(Utils.Snap( (m_clip.frameRate * (m_clip.length/newLength)), 1 ), 1 );
			ChangeFrameRate(newFramerate, false);
		}

		// Looping tickbox
		bool looping = EditorGUILayout.Toggle( "Looping", m_clip.isLooping );
		if ( looping != m_clip.isLooping )
		{
			ChangeLooping(looping);
		}

		// UI Image option- Done as an enum to be clearer
		eAnimSpriteType animSpriteType = (eAnimSpriteType)EditorGUILayout.EnumPopup("Animated Sprite Type", m_uiImage ? eAnimSpriteType.UIImage : eAnimSpriteType.Sprite );
		SetIsUIImage( animSpriteType == eAnimSpriteType.UIImage );		

		// Path to sprite in game object
		if ( m_showAdvancedOptions )
			SetSpritePath( EditorGUILayout.DelayedTextField( "Sprite Path", m_spritePath ) );

		GUILayout.Space(10);

		// Frames list
		m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition,false,false);
		EditorGUI.BeginChangeCheck();
		m_framesReorderableList.DoLayoutList();
		if ( EditorGUI.EndChangeCheck() )
		{
			RecalcFrameTimes();
			Repaint();
			ApplyChanges();
		}

		m_settingsUnfolded = EditorGUILayout.Foldout(m_settingsUnfolded, "Editor Settings", new GUIStyle(EditorStyles.foldout){ normal = { textColor = Color.gray }});
		if ( m_settingsUnfolded )
		{
			string[] nodeNames = new string[m_defaultNodeNames.Length];
			for ( int i = 0; i < nodeNames.Length; ++i )
				nodeNames[i] = string.IsNullOrEmpty(m_defaultNodeNames[i]) ? ("Node "+i) : m_defaultNodeNames[i];
			m_visibleNodes = EditorGUILayout.MaskField("Visible Nodes", m_visibleNodes, nodeNames );
			GUI.SetNextControlName("AvdOpt");
			m_showAdvancedOptions = EditorGUILayout.Toggle( "Show Advanced Options", m_showAdvancedOptions );
			GUI.SetNextControlName("DefaultLen");
			m_defaultFrameLength = EditorGUILayout.DelayedFloatField( "Default Frame Length", m_defaultFrameLength );
			GUI.SetNextControlName("DefaultSamples");
			m_defaultFrameSamples = EditorGUILayout.DelayedIntField( "Default Frame Samples", m_defaultFrameSamples );
			GUI.SetNextControlName("IgnorePivot");
			m_ignorePivot = EditorGUILayout.Toggle( "Ignore Pivot", m_ignorePivot );

			GUI.SetNextControlName("InfoPanelWidth");
			m_infoPanelWidth = Mathf.Max(100, EditorGUILayout.FloatField( "Info Panel Width", m_infoPanelWidth ) );
			GUILayout.Space(20);
		}

		EditorGUILayout.EndScrollView();

		GUILayout.EndArea();

	}

	void LayoutFrameListFrame(Rect rect, int index, bool isActive, bool isFocused )
	{
		if ( m_frames == null || index < 0 || index >= m_frames.Count )
			return;
		AnimFrame frame = m_frames[index];

		EditorGUI.BeginChangeCheck();
		rect = new Rect(rect) { height = rect.height-4, y = rect.y+2 };


		// frame ID
		float xOffset = rect.x;
		float width = Styles.INFOPANEL_LABEL_RIGHTALIGN.CalcSize(new GUIContent(index.ToString())).x;
		EditorGUI.LabelField(new Rect(rect){x=xOffset,width=width},index.ToString(), Styles.INFOPANEL_LABEL_RIGHTALIGN );

		// Frame Sprite
		xOffset += width+5;
		width = (rect.xMax-5-28)-xOffset;

		// Sprite thingy
		Rect spriteFieldRect = new Rect(rect){x=xOffset,width=width,height=16};
		frame.m_sprite = EditorGUI.ObjectField(spriteFieldRect, frame.m_sprite, typeof(Sprite), false ) as Sprite;

		// Frame length (in samples)
		xOffset += width+5;
		width = 28;
		GUI.SetNextControlName("FrameLen");
		int frameLen = Mathf.RoundToInt( frame.m_length / GetMinFrameTime() );
		frameLen = EditorGUI.IntField( new Rect(rect){x=xOffset,width=width}, frameLen );
		SetFrameLength(frame, frameLen * GetMinFrameTime() );


		if ( EditorGUI.EndChangeCheck() )
		{
			// Apply events
			ApplyChanges();
		}
	}

	#endregion
	#region Funcs: Private

	void ChangeFrameRate(float newFramerate, bool preserveTiming )
	{
		Undo.RecordObject(m_clip, "Change Animation Framerate");

		// Scale each frame (if preserving timing) and clamp to closest sample time
		float minFrameTime = 1.0f/newFramerate;
		float scale = preserveTiming ? 1.0f : (m_clip.frameRate / newFramerate);
		foreach ( AnimFrame frame in m_frames )
		{
			frame.m_length = Mathf.Max( Utils.Snap( frame.m_length * scale, minFrameTime ), minFrameTime );
		}
		
		m_clip.frameRate = newFramerate;
		RecalcFrameTimes();
		ApplyChanges();
	}

	void ChangeLooping(bool looping)
	{
		Undo.RecordObject(m_clip, "Change Animation Looping");
		AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(m_clip);
		settings.loopTime = looping;
		AnimationUtility.SetAnimationClipSettings( m_clip, settings );

		m_previewloop = looping;

		// NB: When hitting play directly after this change, the looping state will be undone. So have to call ApplyChanges() afterwards even though frame data hasn't changed.
		ApplyChanges();
	}

	void SetIsUIImage( bool uiImage )
	{
		if ( m_uiImage != uiImage )
		{
			m_uiImage = uiImage;

			// Remove old curve binding
			RemoveExistingBinding();

			// Create new curve binding
			CreateCurveBinding();
			ApplyChanges();
		}
	}

	void SetSpritePath( string path )
	{
		if ( m_spritePath != path )
		{	
			// Remove old curve binding. 
			RemoveExistingBinding();

			m_spritePath = path;

			// Create new curve binding
			CreateCurveBinding();
			ApplyChanges();
		}
	}

	#endregion
}

}