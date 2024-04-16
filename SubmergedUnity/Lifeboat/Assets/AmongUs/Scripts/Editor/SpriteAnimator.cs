//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2017 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor.Animations;
using PowerTools.Anim;

namespace PowerTools
{

public partial class SpriteAnimator : EditorWindow 
{
	#region Definitions

	// Class used internally to store info about a frame
	[System.Serializable]
	class AnimFrame
	{
		public float m_time = 0;
		public float m_length = 0;
		public Sprite m_sprite = null;
		public float EndTime {get{ return m_time+m_length; } }

		// Our custom data
		public List<Node> m_nodes = null;
	}

	// Class used internally to store info about an animation event
	[System.Serializable]
	class AnimEvent
	{
		// Event Data
		public float m_time = 0;
		public string m_functionName = string.Empty;
		public eAnimEventParameter m_paramType = eAnimEventParameter.None;
		public int m_paramInt = 0;
		public float m_paramFloat = 0;
		public string m_paramString = string.Empty;
		public Object m_paramObjectReference = null;
		public SendMessageOptions m_messageOptions = SendMessageOptions.DontRequireReceiver;

		// Our custom data
		public bool m_sendUpwards = true; // Whether events will be sent upwards (requires PowerAnimEventHandler or PowerSpriteAnim component)
		public bool m_usePrefix = true; // Whether "Anim" is prefixed to events

		public AnimFrame m_linkedFrame = null;
		public float m_linkedRatio = 0; // ratio through linked frame
	}

	// Class used for laying out events
	class AnimEventLayoutData 
	{
		public float start; 
		public float end; 
		public string text;
		public float textWidth;
		public int heightOffset;
		public bool selected;
	};

	enum eAnimEventParameter
	{
		None,
		Int,
		Float,
		String,
		Object
	}

	enum eAnimSpriteType
	{
		Sprite,
		UIImage,
	}

	// Static list of styles
    class Styles
    {

		public static readonly GUIStyle PREVIEW_BUTTON = new GUIStyle("preButton");		
		public static readonly GUIStyle PREVIEW_BUTTON_LOOP = new GUIStyle(Styles.PREVIEW_BUTTON) { padding = new RectOffset(0,0,2,0) };
        public static readonly GUIStyle PREVIEW_SLIDER = new GUIStyle("preSlider");
        public static readonly GUIStyle PREVIEW_SLIDER_THUMB = new GUIStyle("preSliderThumb");
		public static readonly GUIStyle PREVIEW_LABEL_BOLD = new GUIStyle("preLabel");
		public static readonly GUIStyle PREVIEW_LABEL_BOLD_SHADOW = new GUIStyle("preLabel") { normal = { textColor = Color.black }  };
		public static readonly GUIStyle PREVIEW_LABEL_SPEED = new GUIStyle("preLabel") { fontStyle = FontStyle.Normal, normal = { textColor = Color.gray }  };

		public static readonly GUIStyle TIMELINE_KEYFRAME_BG = new GUIStyle("AnimationKeyframeBackground");
		#if UNITY_5_3 || UNITY_5_4
			public static readonly GUIStyle TIMELINE_ANIM_BG = new GUIStyle("AnimationCurveEditorBackground");
		#else
			public static readonly GUIStyle TIMELINE_ANIM_BG = new GUIStyle("CurveEditorBackground");
		#endif
		public static readonly GUIStyle TIMELINE_BOTTOMBAR_BG = new GUIStyle("ProjectBrowserBottomBarBg");

		public static readonly GUIStyle TIMELINE_EVENT_TEXT = EditorStyles.miniLabel;
		public static readonly GUIStyle TIMELINE_EVENT_TICK = new GUIStyle();

		public static readonly GUIStyle TIMELINE_EVENT_TOGGLE = new GUIStyle(EditorStyles.toggle) { font = EditorStyles.miniLabel.font, fontSize = EditorStyles.miniLabel.fontSize, padding = new RectOffset(15,0,3,0) };

		public static readonly GUIStyle INFOPANEL_LABEL_RIGHTALIGN = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };

		public static readonly GUIStyle AUTOCOMPLETE_LABEL = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperLeft };



    }

    // Static list of content (built in unity icons and things)
    class Contents
    {
        public static readonly GUIContent PLAY = EditorGUIUtility.IconContent("PlayButton");
		public static readonly GUIContent PAUSE = EditorGUIUtility.IconContent("PauseButton");
        public static readonly GUIContent PREV = EditorGUIUtility.IconContent("Animation.PrevKey");
        public static readonly GUIContent NEXT = EditorGUIUtility.IconContent("Animation.NextKey");
		public static readonly GUIContent SPEEDSCALE = EditorGUIUtility.IconContent("SpeedScale");
		public static readonly GUIContent ZOOM = EditorGUIUtility.IconContent("ViewToolZoom");
		public static readonly GUIContent LOOP_OFF = EditorGUIUtility.IconContent("RotateTool");
		public static readonly GUIContent LOOP_ON = EditorGUIUtility.IconContent("RotateTool On");
		public static readonly GUIContent PLAY_HEAD = EditorGUIUtility.IconContent("me_playhead");
		public static readonly GUIContent EVENT_MARKER = EditorGUIUtility.IconContent("Animation.EventMarker");
		public static readonly GUIContent ANIM_MARKER = EditorGUIUtility.IconContent("blendKey");
	}

	// Stores cached data for rendereing sprites, for efficiency
	class SpriteRenderData
	{
		public Mesh m_previewMesh;
		public Material m_mat;
		public Sprite m_currSprite = null;
	}


	static readonly string PROPERTYNAME_SPRITE = "m_Sprite";

	// This prefix is added to animation events if the "Use Prefix" tickbox is ticked. It's easier to track bugs if all animation events start with the same prefix.
	static readonly string ANIM_EVENT_PREFIX = "Anim";

    static readonly float TIMELINE_HEIGHT = 200;

	static readonly float CHECKERBOARD_SCALE = 32.0f;

	#endregion
	#region Vars: Private

	static Texture2D s_textureCheckerboard;

	[SerializeField] AnimationClip m_clip = null;
	[SerializeField] EditorCurveBinding m_curveBinding = new EditorCurveBinding();
	[SerializeField] bool m_uiImage = false;
	[SerializeField] List<AnimFrame> m_frames = null;
	[SerializeField] List<AnimEvent> m_events = null;
	[SerializeField] string m_spritePath = string.Empty;
	[SerializeField] float m_infoPanelWidth = 260;

	[SerializeField] bool m_playing = false;
	float m_animTime = 0;
	double m_editorTimePrev = 0;

	float m_previewSpeedScale = 1.0f;
	float m_previewScale = 1.0f;
	Vector2 m_previewOffset = Vector2.zero;
	[SerializeField] bool m_previewloop = false;
	bool m_previewResetScale = false; // When true, the preview will scale to best fit next update

	// When creating a new event, these options are used
	[SerializeField] SendMessageOptions m_defaultEventMessageOptions = SendMessageOptions.DontRequireReceiver;
	[SerializeField] bool m_defaultEventSendUpwards = false;
	[SerializeField] bool m_defaultEventUsePrefix = false;

	[SerializeField] string[] m_defaultNodeNames = new string[SpriteAnimNodes.NUM_NODES];
	[SerializeField] Sprite[] m_defaultNodeSprites = new Sprite[SpriteAnimNodes.NUM_NODES];

	// When true, the anim plays automatically when animation is selected. Set to false when users manually stops an animation
	[SerializeField] bool m_autoPlay = true;

	// Default frame length + num samples, etc
	[SerializeField] float m_defaultFrameLength = 0.1f;
	[SerializeField] int m_defaultFrameSamples = 6;
	[SerializeField] bool m_ignorePivot = false;
	[SerializeField] bool m_showAdvancedOptions = false;
	[SerializeField] int m_visibleNodes = ~0;

	// Timeline view's offset from left (in pixels)
	float m_timelineOffset = -TIMELINE_OFFSET_MIN;
	// Unit per second on timeline
	float m_timelineScale = 1000;
	float m_timelineAnimWidth = 1;

	// Used to repaint while drag and dropping into editor to show position indicator
	bool m_dragDropHovering = true; 

	// When new event is created it's focused so you can immediately type the name
	AnimEvent m_focusEvent = null;

	eDragState m_dragState = eDragState.None;

	// List of selected frames or events
	List<AnimFrame> m_selectedFrames = new List<AnimFrame>();
	List<AnimEvent> m_selectedEvents = new List<AnimEvent>();

	// List of copied frames or events
	[SerializeField] List<AnimFrame> m_copiedFrames = null;
	[SerializeField] List<AnimEvent> m_copiedEvents = null;

	// Used to clear selection when hit play (avoids selection references being broken)
	bool m_wasPlaying = false;

	// Lazy instantiated so don't have to search for default sprite shader for every frame
	Shader m_defaultSpriteShader = null;

	// Stores cached data for rendereing sprites, for efficiency
	PreviewRenderUtility m_prevRender = null;
	Dictionary< Sprite, SpriteRenderData > m_spriteRenderData = new Dictionary<Sprite, SpriteRenderData>();

	#endregion
	#region Funcs: Init

	[MenuItem("Window/PowerSprite Animator")]
	static void ShowWindow() 
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow(typeof(SpriteAnimator), false, "Power Anim");
	}

	public SpriteAnimator()
	{
		EditorApplication.update += Update;
		Undo.undoRedoPerformed += OnUndoRedo;
	}

	void OnDestroy() 
	{
		EditorApplication.update -= Update;
	}

	#endregion
	#region Funcs: Layout

	void OnEnable()
	{
		m_editorTimePrev = EditorApplication.timeSinceStartup;

		// Load editor preferences
		m_defaultEventMessageOptions = (SendMessageOptions)EditorPrefs.GetInt("PSAMsgOpt", (int)m_defaultEventMessageOptions);
		m_defaultEventUsePrefix = EditorPrefs.GetBool("PSAPrefix", m_defaultEventUsePrefix);
		m_defaultEventSendUpwards = EditorPrefs.GetBool("PSASendUp", m_defaultEventSendUpwards);

		for ( int i = 0; i < SpriteAnimNodes.NUM_NODES; ++i )
		{
			m_defaultNodeNames[i] = EditorPrefs.GetString("PSANN"+i,string.Empty);
		}

		m_defaultFrameLength = EditorPrefs.GetFloat( "PSDefFrLen", m_defaultFrameLength );
		m_defaultFrameSamples = EditorPrefs.GetInt( "PSDefFrSmpl", m_defaultFrameSamples );
		m_ignorePivot = EditorPrefs.GetBool( "PSIgnPiv", m_ignorePivot );
		m_showAdvancedOptions = EditorPrefs.GetBool("PSAO", m_showAdvancedOptions);

		InitialiseFramesReorderableList();

		OnSelectionChange();
	}

	void OnDisable()
	{
		// Save editor preferences
		EditorPrefs.SetInt("PSAMsgOpt", (int)m_defaultEventMessageOptions);
		EditorPrefs.SetBool("PSAPrefix", m_defaultEventUsePrefix);
		EditorPrefs.SetBool("PSASendUp", m_defaultEventSendUpwards);

		for ( int i = 0; i < SpriteAnimNodes.NUM_NODES; ++i )
		{
			EditorPrefs.SetString("PSANN"+i, m_defaultNodeNames[i]);
		}

		EditorPrefs.SetFloat( "PSDefFrLen", m_defaultFrameLength );
		EditorPrefs.SetInt( "PSDefFrSmpl", m_defaultFrameSamples );
		EditorPrefs.SetBool( "PSIgnPiv", m_ignorePivot );
		EditorPrefs.SetBool( "PSAO", m_showAdvancedOptions );

		if ( m_prevRender != null )
			m_prevRender.Cleanup();

	}

	void OnFocus()
	{
		OnSelectionChange();
	}

	void OnGUI() 		
	{	
		GUI.SetNextControlName("none");
		// If no sprite selected, show editor	
		if ( m_clip == null || m_frames == null )
		{
			GUILayout.Space(10);
			GUILayout.Label("No animation selected", EditorStyles.centeredGreyMiniLabel);
			return;
		}

		//
		// Toolbar
		// 

		#if UNITY_2019_3_OR_NEWER
		GUILayout.Space(2);
		GUILayout.BeginHorizontal( EditorStyles.toolbar );
		#else
		GUILayout.BeginHorizontal( Styles.PREVIEW_BUTTON );
		#endif
		{
			LayoutToolbarPlay();
			GUI.SetNextControlName("Toolbar");
			LayoutToolbarPrevFrame();
			LayoutToolbarNextFrame();
			LayoutToolbarLoop();
			LayoutToolbarSpeedSlider();
			LayoutToolbarScaleSlider();
			LayoutToolbarAnimName();
		}
		GUILayout.EndHorizontal();

		//
		// Preview
		//

		Rect lastRect = GUILayoutUtility.GetLastRect();

		Rect previewRect = new Rect(lastRect.xMin, lastRect.yMax, position.width-m_infoPanelWidth, position.height-lastRect.yMax-TIMELINE_HEIGHT);
		if ( m_previewResetScale )
		{
			ResetPreviewScale(previewRect);
			m_previewResetScale = false;

			// Also reset timeline length
			m_timelineScale = position.width / (Mathf.Max(0.5f, m_clip.length) * 1.25f);
		}
		LayoutPreview( previewRect );
		LayoutAnimationNodesPanel( previewRect );

		//
		// Info Panel
		//
		Rect infoPanelRect = new Rect(lastRect.xMin+position.width-m_infoPanelWidth, lastRect.yMax, m_infoPanelWidth, position.height-lastRect.yMax-TIMELINE_HEIGHT);
		LayoutInfoPanel(infoPanelRect);

		//
		// Timeline
		//
		Rect timelineRect = new Rect(0, previewRect.yMax, position.width, TIMELINE_HEIGHT );
		LayoutTimeline(timelineRect);

		//
		// Handle keypress events that are also used in text fields, this requires check that a text box doesn't have focus
		//
		Event e = Event.current;
		if ( focusedWindow == this )
		{
			bool allowKeypress = string.IsNullOrEmpty( GUI.GetNameOfFocusedControl() ) || GUI.GetNameOfFocusedControl() == "none";
			if ( allowKeypress && e.type == EventType.KeyDown )
			{
				switch ( e.keyCode )
				{
					case KeyCode.Space:
					{
						TogglePlayback();
						e.Use();
					} break;
					case KeyCode.LeftArrow:
					case KeyCode.RightArrow:
					{
						int index = 0;
						// Change selected frame (if only 1 frame is selected)
						if ( m_selectedFrames.Count > 0)
						{
							// Find index of frame before selected frames (if left arrow) or after selected frames (if right arrow)
							if ( e.keyCode == KeyCode.LeftArrow )
								index = m_frames.FindIndex(frame=>frame == m_selectedFrames[0]) - 1;
							else
								index = m_frames.FindLastIndex(frame=>frame == m_selectedFrames[m_selectedFrames.Count-1]) + 1;							
						}
						else 
						{
							index = GetCurrentFrameId() + ( e.keyCode == KeyCode.LeftArrow ? -1 : 1);
						}
						index = Mathf.Clamp(index,0,m_frames.Count-1);
						if ( m_selectedNode >= 0 )
						{
							// When a node's selected scrub through without affecting selection
							if ( m_playing == false )
								m_animTime = m_frames[index].m_time;	
						}
						else 
						{
							SelectFrame( m_frames[index] );							
						}
						e.Use();
						Repaint();
							
					} break;
				}
			}
		}

		//
		// Handle event commands- Delete, selectall, duplicate, copy, paste...
		//
		if ( e.type == EventType.ValidateCommand  )
		{			
			switch( e.commandName )
			{
				case "Delete":
				case "SoftDelete":
				case "SelectAll":
				case "Duplicate":
				case "Copy":
				case "Paste":
				{
					e.Use();
				} break;
			}
		}
		if ( e.type == EventType.ExecuteCommand  )
		{
			switch( e.commandName )
			{
				case "Delete":
				case "SoftDelete":
				{
					DeleteSelected();
					e.Use();
				} break;

				case "SelectAll":
				{
					if ( m_selectedEvents.Count > 0 && m_events.Count > 0 )
					{
						m_selectedEvents.Clear();
						m_selectedEvents.AddRange( m_events );
					}
					else if ( m_frames.Count > 0 )
					{
						m_selectedFrames.Clear();
						m_selectedFrames.AddRange( m_frames );
					}
					m_selectedNode = -1;
				
					e.Use();
				} break;

				case "Duplicate":
				{
					DuplicateSelected();
					e.Use();
				} break;

				case "Copy":
				{
					m_copiedEvents = null;
					m_copiedFrames = null;
					CopySelected();
					e.Use();
				} break;

				case "Paste":
				{
					Paste();
					e.Use();
				} break;
			}	
		}
	}

	void LayoutToolbarPlay()
	{
      	EditorGUI.BeginChangeCheck();
		GUILayout.Toggle( m_playing, m_playing ? Contents.PAUSE : Contents.PLAY, Styles.PREVIEW_BUTTON, GUILayout.Width(40) );
        if (EditorGUI.EndChangeCheck())
		{
			TogglePlayback();
        }
	}
	void TogglePlayback()
	{
		m_playing = !m_playing;

		// Set the auto play variable. Anims will auto play when selected unless user has manually stopped an anim.
		m_autoPlay = m_playing;

    	if ( m_playing )
    	{
    		// Clicked play

    		// If anim is at end, restart
    		if ( m_animTime >= GetAnimLength() )
    		{
    			m_animTime = 0;
    		}
    	}
	}

	void LayoutToolbarNextFrame()
	{
		if ( GUILayout.Button( Contents.NEXT, Styles.PREVIEW_BUTTON, GUILayout.Width(25) ) )
		{		
			if ( m_frames.Count <= 1 )
				return;

			m_playing = false;
			int frame = Mathf.Clamp(GetCurrentFrameId()+1, 0,m_frames.Count-1);
			m_animTime = m_frames[frame].m_time;
		}
	}
	void LayoutToolbarPrevFrame()
	{
		if ( GUILayout.Button( Contents.PREV, Styles.PREVIEW_BUTTON, GUILayout.Width(25) ) )
		{
			if ( m_frames.Count <= 1 )
				return;

			m_playing = false;
			int frame = Mathf.Clamp(GetCurrentFrameId()-1, 0,m_frames.Count-1);
			m_animTime = m_frames[frame].m_time;
		}
	}

	void LayoutToolbarLoop()
	{
		m_previewloop = GUILayout.Toggle( m_previewloop, m_previewloop ? Contents.LOOP_ON : Contents.LOOP_OFF, Styles.PREVIEW_BUTTON_LOOP, GUILayout.Width(25) );
	}

	void LayoutToolbarSpeedSlider()
	{
		if (GUILayout.Button(Contents.SPEEDSCALE, Styles.PREVIEW_LABEL_BOLD, GUILayout.Width(30)) )
			m_previewSpeedScale = 1;
		m_previewSpeedScale = GUILayout.HorizontalSlider(m_previewSpeedScale, 0, 4, Styles.PREVIEW_SLIDER, Styles.PREVIEW_SLIDER_THUMB, GUILayout.Width(50));
		GUILayout.Label(m_previewSpeedScale.ToString("0.00"), Styles.PREVIEW_LABEL_SPEED, GUILayout.Width(40));
	}

	void LayoutToolbarScaleSlider()
	{
		// When press the zoom button, if scale isn't 1, set it to 1, otherwise, scale to it (so it toggles baseically)
		if (GUILayout.Button(Contents.ZOOM, Styles.PREVIEW_LABEL_BOLD, GUILayout.Width(30)) )
		{
			if ( m_previewScale == 1 )
				m_previewResetScale = true;
			else 
				m_previewScale = 1;
		}
		m_previewScale = GUILayout.HorizontalSlider(m_previewScale, 0.1f, 5, Styles.PREVIEW_SLIDER, Styles.PREVIEW_SLIDER_THUMB, GUILayout.Width(50));
		GUILayout.Label(m_previewScale.ToString("0.0"), Styles.PREVIEW_LABEL_SPEED, GUILayout.Width(40));
	}

	void LayoutToolbarAnimName()
	{
		GUILayout.Space(10);
		if ( GUILayout.Button(m_clip.name, new GUIStyle(Styles.PREVIEW_BUTTON) { stretchWidth = true, alignment = TextAnchor.MiddleLeft } ) )
		{
			Selection.activeObject = m_clip;
			EditorGUIUtility.PingObject(m_clip);
		}
	}


	void LayoutPreview( Rect rect )
	{
		

		//
		// Draw checkerboard
		//
		Rect checkboardCoords = new Rect( Vector2.zero, rect.size / (CHECKERBOARD_SCALE * m_previewScale) );
		checkboardCoords.center = new Vector2(-m_previewOffset.x,m_previewOffset.y) / (CHECKERBOARD_SCALE * m_previewScale);
		GUI.DrawTextureWithTexCoords(rect, GetCheckerboardTexture(), checkboardCoords, false);

		//
		// Draw sprite
		//
		Sprite sprite = null;
		if ( m_frames.Count > 0 )
		{
			sprite = GetSpriteAtTime(m_animTime);
		}

		if ( sprite != null )
		{
			#if (UNITY_2017_1_OR_NEWER && !UNITY_2017_4_OR_NEWER) || (UNITY_2019_1_OR_NEWER && !UNITY_2019_3_OR_NEWER)
			// In 2017.1 and 2019.1 Can't display packed sprites while game is running, so don't bother trying
			if ( Application.isPlaying && (UnityEditor.EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOn || UnityEditor.EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas) && sprite.packed && sprite.packingMode != SpritePackingMode.Rectangle )
			{
				EditorGUI.LabelField(rect,"Disabled in Play Mode for Packed Sprites", new GUIStyle( EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } } );
				return;
			}
            #endif

			LayoutFrameSprite(rect, sprite, m_previewScale, m_previewOffset, false, true);
		}

		//
		// Handle layout events
		//
		Event e = Event.current;
		if ( rect.Contains( e.mousePosition ) )
		{
			if ( e.type == EventType.ScrollWheel )
			{
				float scale = 1000.0f;
				while ( (m_previewScale/scale) < 1.0f || (m_previewScale/scale) > 10.0f ) {
                    scale /= 10.0f;
                }
				m_previewScale -= e.delta.y * scale * 0.05f;
				m_previewScale = Mathf.Clamp(m_previewScale,0.1f,100.0f);
				Repaint();
				e.Use();
			}
			else if ( e.type == EventType.MouseDrag ) 
			{
				if ( e.button == 1 || e.button == 2 )
				{
					if ( sprite != null )
					{
						m_previewOffset += e.delta;
						Repaint();
						e.Use();
					}
				}
			}
		}
	}

	void LayoutFrameSprite(Rect rect, Sprite sprite, float scale, Vector2 offset, bool useTextureRect, bool clipToRect, float angle = 0 )
	{
		if ( rect.width <= 10 || rect.height <= 10 )
			return;

		#if (UNITY_2017_1_OR_NEWER && !UNITY_2017_4_OR_NEWER) || (UNITY_2019_1_OR_NEWER && !UNITY_2019_3_OR_NEWER)
			LayoutFrameSpriteTexture(rect,sprite,scale,offset,useTextureRect,clipToRect, angle);
		#else
			LayoutFrameSpriteRendered(rect,sprite,scale,offset,useTextureRect,clipToRect, angle);
		#endif
	}

		// This layout just draws the sprite using gui tools - PreviewRenderUtility  is broken in Unity 2017 so this is necessary
	void LayoutFrameSpriteTexture(Rect rect, Sprite sprite, float scale, Vector2 offset, bool useTextureRect, bool clipToRect, float angle = 0 )
	{
		// Calculate the pivot offset
		Vector2 pivotOffset = Vector2.zero;
		#if  (UNITY_2017_1_OR_NEWER && !UNITY_2017_4_OR_NEWER) || (UNITY_2019_1_OR_NEWER && !UNITY_2019_3_OR_NEWER)
		if ( Application.isPlaying && (UnityEditor.EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOn || UnityEditor.EditorSettings.spritePackerMode == SpritePackerMode.AlwaysOnAtlas) && sprite.packed && sprite.packingMode != SpritePackingMode.Rectangle )
			return; //	useTextureRect = false; // When playing, the sprite shows a meaningless section of the atlas, so just return immediately
		#endif
		if ( useTextureRect == false && m_ignorePivot == false )
		{
			pivotOffset = ((sprite.rect.size*0.5f) - sprite.pivot)*scale;
			pivotOffset.y = -pivotOffset.y;
		}

		Rect spriteRectOriginal = (useTextureRect ? sprite.textureRect : sprite.rect);
		Rect texCoords = new Rect( spriteRectOriginal.x/sprite.texture.width, spriteRectOriginal.y/sprite.texture.height, spriteRectOriginal.width/ sprite.texture.width, spriteRectOriginal.height/sprite.texture.height );

		Rect spriteRect = new Rect(Vector2.zero, spriteRectOriginal.size * scale);
		spriteRect.center = rect.center + offset + pivotOffset;

		if ( clipToRect )
		{
			// If the sprite doesn't fit in the rect, it needs to be cropped, and have it's uv's scaled to compensate (This is way more complicated than it should be!)
			Vector2 croppedRectOffset = new Vector2(Mathf.Max(spriteRect.xMin,rect.xMin), Mathf.Max(spriteRect.yMin,rect.yMin));
			Vector2 croppedRectSize = new Vector2(Mathf.Min(spriteRect.xMax, rect.xMax), Mathf.Min(spriteRect.yMax, rect.yMax)) - croppedRectOffset;
			Rect croppedRect = new Rect( croppedRectOffset, croppedRectSize );
			texCoords.x += ((croppedRect.xMin-spriteRect.xMin)/spriteRect.width)*texCoords.width;
			texCoords.y += ((spriteRect.yMax-croppedRect.yMax)/spriteRect.height)*texCoords.height;
			texCoords.width *= (1.0f-(spriteRect.width-croppedRect.width)/spriteRect.width);
			texCoords.height *= (1.0f-(spriteRect.height-croppedRect.height)/spriteRect.height);

			// Draw the texture
			GUI.DrawTextureWithTexCoords(croppedRect, sprite.texture, texCoords, true);
		}
		else 
		{
			// Draw the texture
			GUI.DrawTextureWithTexCoords(spriteRect, sprite.texture, texCoords, true);
		}
	}

	
	// This layout renders the sprite polygon with a camera and junk. More complicated/expensive but works with atlases/polygon sprites
	public void LayoutFrameSpriteRendered(Rect rect, Sprite sprite, float scale, Vector2 offset, bool useTextureRect, bool clipToRect, float angle = 0 )
	{
		Camera previewCamera = null;
		SpriteRenderData data = null;

		if ( m_prevRender == null )
		{
			m_prevRender = new PreviewRenderUtility();
			#if UNITY_2017_4_OR_NEWER
				previewCamera = m_prevRender.camera;
			#else 
				previewCamera = m_prevRender.m_Camera;
			#endif

			previewCamera.orthographic = true;
			previewCamera.transform.rotation = Quaternion.identity;
			previewCamera.nearClipPlane = 1;
			previewCamera.farClipPlane = 30;
			previewCamera.backgroundColor = Color.white.WithAlpha(0);
		}

		if ( m_spriteRenderData.TryGetValue( sprite, out data ) == false )
		{
			// First time this sprite has been encountered, so lazy instantiate the render data for it and cache it, since this is expensive
			data = new SpriteRenderData();
			data.m_currSprite = sprite;

			if ( m_defaultSpriteShader == null )
				m_defaultSpriteShader = Shader.Find("Sprites/Default");

			data.m_mat =  new Material(m_defaultSpriteShader);
			data.m_previewMesh = new Mesh();

			Vector3[] newMesh = new Vector3[sprite.vertices.Length];
			for ( int i = 0; i < newMesh.Length; ++i )
				newMesh[i] = sprite.vertices[i];
			int[] newTris = new int[sprite.triangles.Length];
			for ( int i = 0; i < newTris.Length; ++i )
				newTris[i] = sprite.triangles[i];

			data.m_mat.mainTexture = sprite.texture;
			data.m_previewMesh.vertices = newMesh;
			data.m_previewMesh.uv = sprite.uv;
			data.m_previewMesh.triangles = newTris;
			data.m_previewMesh.RecalculateBounds();
			data.m_previewMesh.RecalculateNormals();

			m_spriteRenderData.Add(sprite, data);
		}

		if ( data.m_mat == null || data.m_previewMesh == null )
		{
			m_spriteRenderData.Clear();
			LayoutFrameSpriteRendered(rect, sprite, scale, offset, useTextureRect, clipToRect);
			return;
		}

		// Setup preview camera size/pos
		float finalScaleInv = 1.0f / (scale * sprite.pixelsPerUnit);

		#if UNITY_2017_4_OR_NEWER
			previewCamera = m_prevRender.camera;			
		#else
			previewCamera = m_prevRender.m_Camera;
		#endif
		previewCamera.orthographicSize = 0.5f * rect.height * finalScaleInv;
		previewCamera.transform.position = new Vector3(-offset.x * finalScaleInv, offset.y * finalScaleInv,-10f);

		// begin preview
		m_prevRender.BeginPreview(rect, GUIStyle.none);

		// Offset from pivot so that sprite is centered correctly)
		Vector2 pivotOffset = Vector2.zero;

		// If using the texture rect (eg. in timeline), or ignoring pivot- remove pivot offset
		if ( useTextureRect || m_ignorePivot )
			pivotOffset = -( (sprite.rect.size*0.5f) - sprite.pivot);

		// If using the texture rect (eg. in timeline)- Remove difference between centerpoint of sprite rect and texture rect (can't do it if playing with tight packing though)
		if ( useTextureRect && (sprite.packed == false || Application.isPlaying == false) )
			pivotOffset += sprite.rect.center - sprite.textureRect.center;

		// Draw the mesh
		m_prevRender.DrawMesh(data.m_previewMesh, pivotOffset / sprite.pixelsPerUnit, Quaternion.Euler(0,0,angle), data.m_mat, 0);

		// Render preview to texture
		previewCamera.Render();
		Texture texture = m_prevRender.EndPreview();
		texture.filterMode = FilterMode.Point;

		// Draw on the gui
        GUI.DrawTexture(rect, texture);
    }

	#endregion
	#region Funcs: Update

	void Update()
	{
		if ( m_clip != null && m_playing && m_dragState != eDragState.Scrub )
		{
			// Update anim time if playing (and not scrubbing)
			float delta = (float)(EditorApplication.timeSinceStartup - m_editorTimePrev);

			m_animTime += delta * m_previewSpeedScale;

			if ( m_animTime >= GetAnimLength() )
			{
				if ( m_previewloop )
				{
					m_animTime -= GetAnimLength();
				}
				else 
				{
 					m_playing = false;
					m_animTime = 0;
				}
			}

			Repaint();
		}
		else if ( m_dragDropHovering || m_dragState != eDragState.None )
		{
			Repaint();
		}

		// When going to Play, we need to clear the selection since references get broken.
		if ( m_wasPlaying != EditorApplication.isPlayingOrWillChangePlaymode )
		{
			m_wasPlaying = EditorApplication.isPlayingOrWillChangePlaymode;
			if ( m_wasPlaying )
				ClearSelection();
		}

		m_editorTimePrev = EditorApplication.timeSinceStartup;
	}

	#endregion
	#region Funcs: Changes to Anim


	void OnClipChange( bool resetPreview = true )
	{
		if ( m_clip == null )
			return;

		m_frames = null;

		// Find curve binding for the sprite. This property works for both UI anims and sprite anims :D
		m_curveBinding = System.Array.Find( AnimationUtility.GetObjectReferenceCurveBindings(m_clip), item=>item.propertyName == PROPERTYNAME_SPRITE ); 
		if ( m_curveBinding.isPPtrCurve )
		{
			// Convert frames from ObjectReferenceKeyframe (struct with time & sprite) to our easier to use list of AnimFrame
			ObjectReferenceKeyframe[] objRefKeyframes = AnimationUtility.GetObjectReferenceCurve(m_clip, m_curveBinding );
			m_frames = new List<AnimFrame>(	
				System.Array.ConvertAll<ObjectReferenceKeyframe, AnimFrame>( objRefKeyframes, keyframe => 
				{ 
					return new AnimFrame() { m_time = keyframe.time, m_sprite = keyframe.value as Sprite }; 
				} ));			
		}
		if ( m_curveBinding.isPPtrCurve )
		{
			m_uiImage = m_curveBinding.type == typeof(UnityEngine.UI.Image);
			m_spritePath = m_curveBinding.path;
		}

		if ( m_frames == null )
		{
			// Don't have any frames
			m_frames = new List<AnimFrame>();
		}

		RecalcFrameLengths();

		//
		// Hack/Unhack the final frame. To get around unity limitation of final frame always being 1 sample long a dummy duplicate frame is added to the end
		//
		int numFrames = m_frames.Count;
		if ( numFrames >= 2 )
		{
			AnimFrame lastFrame =  m_frames[numFrames-1];
			AnimFrame secondlastFrame = m_frames[numFrames-2];
			if ( lastFrame.m_sprite == secondlastFrame.m_sprite )
			{		
				// Yep, last frame was a duplicate, so just increase the length of the second last frame and remove the dummy one
				secondlastFrame.m_length += GetMinFrameTime();
				m_frames.RemoveAt(numFrames-1);
			}
			else 
			{
				lastFrame.m_length = GetMinFrameTime();
			}
		}


		//
		// Read nodes
		//
		ReadNodesFromClip();

		//
		// Copy animation events into our own event class which has a bunch of extra stuff in it
		// 
		m_events = new List<AnimEvent>();
		AnimationEvent[] events = AnimationUtility.GetAnimationEvents(m_clip);
		for ( int i = 0; i < events.Length; ++i )
		{
			AnimationEvent animEvent = events[i];

			AnimEvent newEvent = new AnimEvent()
			{
				m_time = animEvent.time,
				m_functionName = animEvent.functionName,
				m_paramInt = animEvent.intParameter,
				m_paramFloat = animEvent.floatParameter,
				m_paramString = animEvent.stringParameter,
				m_paramObjectReference = animEvent.objectReferenceParameter,
				m_messageOptions = animEvent.messageOptions
			};
			m_events.Add(newEvent);

			// check if it's one of our own custom functions
			newEvent.m_sendUpwards = newEvent.m_functionName.Length > 0 && newEvent.m_functionName[0] == SpriteAnimEventHandler.EventParser.MESSAGE_NOPARAM[0];

			if ( newEvent.m_sendUpwards )
			{
				// It's a custom event, so convert the params/function name

				// Use function name to detect which data type to use
				if ( newEvent.m_functionName.Length <= SpriteAnimEventHandler.EventParser.MESSAGE_NOPARAM.Length )
				{
					newEvent.m_paramType = eAnimEventParameter.None;
				}
				else 
				{
					switch( newEvent.m_functionName[SpriteAnimEventHandler.EventParser.MESSAGE_NOPARAM.Length] )
					{
					case 'I': newEvent.m_paramType = eAnimEventParameter.Int;				break; // 'I'nt
					case 'F': newEvent.m_paramType = eAnimEventParameter.Float;				break; // 'F'loat
					case 'S': newEvent.m_paramType = eAnimEventParameter.String;			break; // 'S'tring
					case 'O': newEvent.m_paramType = eAnimEventParameter.Object;			break; // 'O'bject
					default: break;
					}
				}

				switch (newEvent.m_paramType)
				{
					default:
					case eAnimEventParameter.None:
					{
						newEvent.m_functionName = newEvent.m_paramString;
						newEvent.m_paramString = null;
					} break;
					case eAnimEventParameter.Int:
					{
						newEvent.m_functionName = newEvent.m_paramString;
						newEvent.m_paramInt = SpriteAnimEventHandler.EventParser.ParseInt(ref newEvent.m_functionName);
						newEvent.m_paramString = null;
					} break;
					case eAnimEventParameter.Float:
					{
						newEvent.m_functionName = newEvent.m_paramString;
						newEvent.m_paramFloat = SpriteAnimEventHandler.EventParser.ParseFloat(ref newEvent.m_functionName);
						newEvent.m_paramString = null;
					} break;
					case eAnimEventParameter.String:
					{
						newEvent.m_functionName = newEvent.m_paramString;
						newEvent.m_paramString = SpriteAnimEventHandler.EventParser.ParseString(ref newEvent.m_functionName);
					} break;
					case eAnimEventParameter.Object:
					{
						// Object events are split into too. Hopefully they come through consecutively, will probably need more checks

						// First event (either string or object ref param)
						if ( string.IsNullOrEmpty( animEvent.stringParameter ) == false )
							newEvent.m_functionName = animEvent.stringParameter;
						if ( animEvent.objectReferenceParameter != null )
							newEvent.m_paramObjectReference = animEvent.objectReferenceParameter;

						// Second event (either string or object ref param)
						i++;
						if ( i < events.Length )
						{
							animEvent = events[i];
							if ( string.IsNullOrEmpty( animEvent.stringParameter ) == false )
								newEvent.m_functionName = animEvent.stringParameter;
							if ( animEvent.objectReferenceParameter != null )
								newEvent.m_paramObjectReference = animEvent.objectReferenceParameter;
						}

						if ( string.IsNullOrEmpty( newEvent.m_functionName ) ||  newEvent.m_paramObjectReference == null )
						{
							Debug.LogError("Failed to read animation event with object reference parameter");
						}

					} break;
				}
			}
			else 
			{
				// It's not a custom event, so set the type by checking if any params aren't default
				newEvent.m_paramType = eAnimEventParameter.None;
				if ( newEvent.m_paramInt != default(int) )
					newEvent.m_paramType = eAnimEventParameter.Int;
				else if ( newEvent.m_paramFloat != default(float) )
					newEvent.m_paramType = eAnimEventParameter.Float;
				else if ( string.IsNullOrEmpty( newEvent.m_paramString ) == false )
					newEvent.m_paramType = eAnimEventParameter.String;
				else if ( newEvent.m_paramObjectReference != null )
					newEvent.m_paramType = eAnimEventParameter.Object;
			}

			// Strip prefix if it's there
			newEvent.m_usePrefix = false;
			if ( newEvent.m_functionName.StartsWith(ANIM_EVENT_PREFIX) )
			{
				newEvent.m_usePrefix = true;
				newEvent.m_functionName = newEvent.m_functionName.Substring(ANIM_EVENT_PREFIX.Length);
			}
		
		}

		RecalcEventFrameLinks();	

		//
		// Update other interal data.
		//
		m_framesReorderableList.list = m_frames;
		if ( resetPreview )
		{
			m_previewResetScale = true;
			m_previewloop = m_clip.isLooping;
			m_animTime = 0;
			m_playing = m_autoPlay;
			m_scrollPosition = Vector2.zero;
			ClearSelection();
			m_previewOffset = Vector2.zero;
			m_timelineOffset = -TIMELINE_OFFSET_MIN;

			m_spriteRenderData.Clear();
		}
		Repaint();


		// If num frames is 1, and that frame is empty, delete it, it's a new animnation
		if ( numFrames == 1 )
		{
			if ( m_frames[0].m_sprite == null )
			{
				m_frames.RemoveAt(0);
				ClearSelection();
				Repaint();
		    	ApplyChanges();
			}
		}
	}

	/// Saves changes in the internal m_frames to the actual animation clip
	void ApplyChanges()
	{
		ObjectReferenceKeyframe[] keyframes = m_frames.ConvertAll<ObjectReferenceKeyframe>( item=>{return new ObjectReferenceKeyframe{time = item.m_time, value = item.m_sprite}; } ).ToArray();

		bool hasFrames = keyframes.Length > 0;
		bool hadFrames = m_curveBinding.isPPtrCurve == true;


		// If final keyframe is > sample rate, there eneds to be a duplicate keyframe added to the end
		if ( hasFrames )
		{
			// Keyframes are stored as array of structs, with time the frame starts, and the sprite.
			// This means the last element will always be 1 sample long which SUCKS
			// So if final frame is larger than 1 sample long, then we add a duplicate

			AnimFrame lastFrame = m_frames[m_frames.Count-1];
			if ( lastFrame.m_length > GetMinFrameTime()+0.0001f )
			{
				// Add another frame
				System.Array.Resize( ref keyframes, keyframes.Length+1 );
				keyframes[keyframes.Length-1] = new ObjectReferenceKeyframe(){ value = lastFrame.m_sprite, time = lastFrame.EndTime-GetMinFrameTime() };
			}
		}

		Undo.RecordObject(m_clip, "Animation Change");

		if ( hasFrames )
		{
			if ( hadFrames == false )
			{			
				// Adding first frames, so need to create curve binding
				CreateCurveBinding();
			}

			// Apply the changes
			AnimationUtility.SetObjectReferenceCurve(m_clip, m_curveBinding, keyframes);
		}
		else if ( hadFrames )
		{
			// Had frames, but they've all been removed, so remove the curve binding
			AnimationUtility.SetObjectReferenceCurve(m_clip, m_curveBinding, null);
		}
		else 
		{
			// Didn't have frames, and still doesn't. Do nothing
		}

		//
		//  Apply node changes
		//
		ApplyNodeChanges();

		//
		// Apply animation events
		//

		// Sort anim events by time
		m_events.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );

		// Clamp values and snap to frame positions.
		foreach ( AnimEvent animEvent in m_events )
		{

			animEvent.m_time = Mathf.Max(0, SnapTimeToFrameRate( animEvent.m_time ) );
		}

		// Convert animation events. 
		List<AnimationEvent> events = new List<AnimationEvent>(m_events.Count);

		for ( int i = 0; i < m_events.Count; ++i )
		{
			AnimEvent from = m_events[i];
			AnimationEvent to = new AnimationEvent();
			events.Add(to);

			// Events on time zero, or end frame get hit twice, so moved to slightly after/before. NB: Hackily avoiding floating point issues here, by decrementing by scaled value
			to.time = Mathf.Clamp(from.m_time, Mathf.Epsilon, m_clip.length-(m_clip.length/100000.0f) ); 

			to.messageOptions = from.m_messageOptions;

			// Function name has "Anim" stripped if we're using the automatic prefix
			string functionName = from.m_usePrefix ? ANIM_EVENT_PREFIX + from.m_functionName : from.m_functionName;

			if ( from.m_sendUpwards )
			{
				// Encode message and parameter into delimited string parameter, or into two messages if need to send an object
				switch( from.m_paramType )
				{
					default: 
					case eAnimEventParameter.None:
					{
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_NOPARAM;
						to.stringParameter = functionName;
					} break;
					case eAnimEventParameter.Int:
					{
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_INT;
						to.stringParameter = functionName + SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER + from.m_paramInt.ToString();
					} break;
					case eAnimEventParameter.Float:
					{
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_FLOAT;
						to.stringParameter = functionName + SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER + from.m_paramFloat.ToString();
					} break;
					case eAnimEventParameter.String:
					{
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_STRING;
						to.stringParameter = functionName + SpriteAnimEventHandler.EventParser.MESSAGE_DELIMITER + from.m_paramString;
					} break;
					case eAnimEventParameter.Object:
					{	
						// Object events are split into two. The first is the string function name, second is object name					
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_OBJECT_FUNCNAME;
						to.stringParameter = functionName;
						to.objectReferenceParameter = null;

						to = new AnimationEvent();
						events.Add(to);
						to.time = from.m_time;
						to.messageOptions = from.m_messageOptions;
						to.functionName = SpriteAnimEventHandler.EventParser.MESSAGE_OBJECT_DATA;
						to.objectReferenceParameter = from.m_paramObjectReference;
					} break;
				}
			}
			else 
			{
				to.functionName = functionName;
				to.intParameter = from.m_paramInt;
				to.floatParameter = from.m_paramFloat;
				to.stringParameter = from.m_paramString;
				to.objectReferenceParameter = from.m_paramObjectReference;

			}

		}

		AnimationUtility.SetAnimationEvents(m_clip, events.ToArray());

		// Add events to autocomplete, excluding ones currently being edited.
		foreach ( AnimEvent animEvent in m_events )
		{
			if ( animEvent.m_functionName.Length > 2
				&& m_selectedEvents.Contains(animEvent) == false 
				&& m_acListAll.Exists(item=>item.m_functionName==animEvent.m_functionName) == false )
			{
				m_acListAll.Add(animEvent);
			}
		}
		m_acListAll.Sort((a,b)=>a.m_functionName.CompareTo(b.m_functionName));
		if ( m_acListAll.Count > 50 )
			m_acListAll.RemoveAt(0); // Don't let ac list get too big

	}

	/// Sets the length of a particular frame, updates other frame times
    void SetFrameLength(int frameId, float length)
    {
		if (  Mathf.Approximately( length, m_frames[frameId].m_length ) == false )
		{
			m_frames[frameId].m_length = Mathf.Max(GetMinFrameTime(), SnapTimeToFrameRate(length));
	    	RecalcFrameTimes();
	    }
	}
    void SetFrameLength(AnimFrame frame, float length)
    {
		if ( Mathf.Approximately( length, frame.m_length ) == false )
		{
			frame.m_length = Mathf.Max(GetMinFrameTime(), SnapTimeToFrameRate(length));
	    	RecalcFrameTimes();
	    }
    }

    /// Update the times of all frames based on the lengths
    void RecalcFrameTimes()
    {    	
    	float time = 0;
    	foreach ( AnimFrame frame in m_frames )
    	{
    		frame.m_time = time;
    		time += frame.m_length;
    	}

		RepositionLinkedEvents();
    }


    /// Update the lengths of each frame based on the times
	void RecalcFrameLengths()
	{
		for ( int i = 0; i < m_frames.Count-1; ++i )
		{
			m_frames[i].m_length = m_frames[i+1].m_time - m_frames[i].m_time;
    	}
    	// If last frame has invalid length, set it to minimum length
		if ( m_frames.Count > 0 && m_frames[m_frames.Count-1].m_length < GetMinFrameTime() )
			m_frames[m_frames.Count-1].m_length = GetMinFrameTime();

		RepositionLinkedEvents();
	}

	/// Moves any currently selected frames to after the specified index
	void MoveSelectedFrames( int toIndex )
	{
		// Sort selected items by time so they can be moved in correct order
		m_selectedFrames.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );

		bool insertAtEnd = toIndex >= m_frames.Count;

		// Insert all items (remove from list, and re-add in correct position.
		foreach( AnimFrame frame in m_selectedFrames )
		{
			if ( insertAtEnd )
			{
				if ( m_frames[m_frames.Count-1] != frame )
				{
					m_frames.Remove(frame);
					m_frames.Add(frame);			
				}
			}
			else 
			{			
				AnimFrame insertBeforeFrame = m_frames[toIndex];
				if ( insertBeforeFrame != frame )
				{
					m_frames.Remove(frame);
					toIndex = m_frames.FindIndex(item=>item==insertBeforeFrame);
					m_frames.Insert(toIndex,frame);			
				}
				toIndex++;
			}
		}
		RecalcFrameTimes();
		Repaint();
    	ApplyChanges();
	}

	/// Add frames at a specific position
	void InsertFrames(Sprite[] sprites, int atPos)
	{
		float frameLength = GetMinFrameTime();

		if ( m_frames.Count > 0 )
		{
			// Find previous frame's length to use for inserted frames
			frameLength = m_frames[(atPos == 0 || atPos >= m_frames.Count) ? 0 : atPos-1].m_length;
		}
		else 
		{
			// First frame, use default FPS
			if ( m_defaultFrameLength > 0 && m_defaultFrameSamples > 0 )
			{
				m_clip.frameRate = (float)m_defaultFrameSamples/m_defaultFrameLength;
				frameLength = m_defaultFrameLength;		
			}
		}

		AnimFrame[] newFrames = System.Array.ConvertAll<Sprite, AnimFrame>( sprites, sprite => 
			{ 
				return new AnimFrame() { m_sprite = sprite, m_length = frameLength }; 
			} );

		atPos = Mathf.Clamp(atPos,0,m_frames.Count);
		m_frames.InsertRange(atPos, newFrames);		
			
		RecalcFrameTimes();
		Repaint();
    	ApplyChanges();
	}

	/// Replaces sprites starting at a specific frame position. If there are more sprites than existing frames, more are created
	void ReplaceFrames(Sprite[] sprites, int atPos)
	{
		// Thanks to Fausto Cheder for help with this feature!

		// If there's no frames, or replacing after last frame- just do a normal insert.
		if ( m_frames == null || m_frames.Count <= atPos )
		{
			InsertFrames(sprites, atPos);
			return;
		}

	    List<Sprite> extraSpritesToInsert = null;
		for (int i = 0; i < sprites.Length; i++) 
	    {
			if ( i < m_frames.Count-atPos ) 
			{
	       		// if the amount of dragged sprites fit the current list
				m_frames[i+atPos].m_sprite = sprites[i]; 
	        } 
	        else 
	        {
	            if (extraSpritesToInsert == null) 
					extraSpritesToInsert = new List<Sprite>(sprites.Length - (m_frames.Count - atPos));
	            extraSpritesToInsert.Add(sprites[i]);
	        }
	    }

	    if (extraSpritesToInsert != null) 
	    {
	        // if we have too many sprites to fit the current list, insert the extra ones at the end
	        InsertFrames(extraSpritesToInsert.ToArray(), m_frames.Count);
	    } 
	    else 
	    {
	        RecalcFrameTimes();
	        Repaint();
	        ApplyChanges();
	    }
	}

	/// Creates a new event on the timeline at a specific time.
	void InsertEvent(float eventTime, bool shouldSelect )
	{
		AnimEvent newEvent = new AnimEvent(){ m_time = eventTime, m_functionName = "", m_messageOptions = m_defaultEventMessageOptions, m_usePrefix = m_defaultEventUsePrefix, m_sendUpwards = m_defaultEventSendUpwards };

		m_events.Add(newEvent);
		if ( shouldSelect )
		{
			ClearSelection();
			m_selectedEvents.Add(newEvent);
			EditorGUI.FocusTextInControl("");
			m_focusEvent = newEvent;
		}
		RecalcEventFrameLinks();
		ApplyChanges();
		Repaint();
	}

	/// Moves all selected events by the specified amount (gui space) Does not apply the change)
	void MoveSelectedEvents( float mouseDelta )
	{
		// Translate dist into time
		float timeDiff = mouseDelta / m_timelineScale;

		// Find min so can't move events < 0
		float earliestTime = float.MaxValue;
		foreach( AnimEvent animEvent in m_selectedEvents )
		{
			if ( animEvent.m_time < earliestTime )
				earliestTime = animEvent.m_time;
		}

		// Don't allow events to be moved < 0
		if (  earliestTime + timeDiff < 0 )
			timeDiff -= earliestTime + timeDiff;

		foreach( AnimEvent animEvent in m_selectedEvents )
		{
			animEvent.m_time = animEvent.m_time + timeDiff;
		}	
		RecalcEventFrameLinks();	
	}

	/// Delete all selected frames or events
	void DeleteSelected()
	{
		if ( m_selectedFrames.Count > 0 )
		{
			m_frames.RemoveAll( item => m_selectedFrames.Contains(item) );
			RecalcFrameTimes();
		}
		else if ( m_selectedEvents.Count > 0 )
		{
			m_events.RemoveAll( item => m_selectedEvents.Contains(item) );
			RecalcEventFrameLinks();
		}
		else if ( m_selectedNode >= 0 )
		{
			DeleteNode(m_selectedNode);
		}
		ClearSelection();
		Repaint();
    	ApplyChanges();
	}

	/// Duplicates the selected frames or events, selecting new items
	bool DuplicateSelected()
	{
		if ( DuplicateSelected(m_events, ref m_selectedEvents) )
		{
			RecalcEventFrameLinks();
			Repaint();
	    	ApplyChanges();
			return true;
		}
		else if ( DuplicateSelected(m_frames, ref m_selectedFrames) )
		{
			RecalcFrameTimes();
			Repaint();
	    	ApplyChanges();
			return true;
		}
		return false;
	}

	/// Templated function that duplicates the selected frames or events, selecting new items
	bool DuplicateSelected<T>( List<T> list, ref List<T> selectedList ) where T : class, new()
	{
		if ( list.Count == 0 || selectedList.Count == 0 ) 					
			return false;	

		T lastSelected = selectedList[selectedList.Count-1];
		int index = list.FindLastIndex( item=>item == lastSelected )+1;

		// Clone all items
		List<T> duplicatedItems = selectedList.ConvertAll<T>( item => Utils.Clone(item) );

		// Add the duplicated frames
		list.InsertRange( index, duplicatedItems );

		// Select the newly created frames
		ClearSelection();
		selectedList = duplicatedItems;

		return true;
	}

	/// Duplicates the selected frames or events, selecting new items
	bool CopySelected()
	{
		if ( CopySelected(ref m_copiedEvents, m_selectedEvents) == false )
		{
			return CopySelected(ref m_copiedFrames, m_selectedFrames);
		}
		return false;
	}
	bool CopySelected<T>( ref List<T> list, List<T> selectedList ) where T : class, new()
	{
		if ( selectedList.Count == 0 ) 					
			return false;

		list = selectedList.ConvertAll<T>( item => Utils.Clone(item) );
		return true;
	}

	/// Duplicates the selected frames or events, selecting new items
	bool Paste()
	{		
		// Point to insert is either after selected frame, or at selected
		if ( m_copiedEvents != null && m_copiedEvents.Count > 0 )
		{
			List<AnimEvent> pastedItems = m_copiedEvents.ConvertAll<AnimEvent>( item => Utils.Clone(item) );

			if ( m_playing == false )
			{
				// Insert frames at playhead by offseting their times from the first event's time.
				float timeOffset = SnapTimeToFrameRate( m_animTime ) - pastedItems[0].m_time;
				if ( m_animTime >= 0 && m_animTime <= m_clip.length )
				{
					foreach( AnimEvent item in pastedItems )
					{
						item.m_time += timeOffset;
					}
				}
			}
			m_events.AddRange(pastedItems);
			ClearSelection();
			m_selectedEvents = pastedItems;
			RecalcEventFrameLinks();
		}
		else if ( m_copiedFrames != null && m_copiedFrames.Count > 0 )
		{
			// Find place to insert, either after selected frame, at caret, or at end of anim

			if ( m_frames == null )
				m_frames = new List<AnimFrame>();
			int index = m_frames.Count;
			if ( m_selectedFrames.Count > 0 )
			{
				// If there's a selected item, then insert after it
				AnimFrame lastSelected = m_selectedFrames[m_selectedFrames.Count-1];
				index = m_frames.FindLastIndex( item=>item == lastSelected )+1;
			}
			else if ( m_playing == false )
			{
				index = GetCurrentFrameId();
			}

			List<AnimFrame> pastedItems = m_copiedFrames.ConvertAll<AnimFrame>( item => Utils.Clone(item) );
			index = Mathf.Clamp(index,0,m_frames.Count);
			m_frames.InsertRange(index, pastedItems);
			ClearSelection();
			m_selectedFrames = pastedItems;

			RecalcFrameTimes();
		}
		Repaint();
    	ApplyChanges();

		return true;
	}


	#endregion
	#region Funcs: Private

	/// Unity event called when the selectd object changes
	void OnSelectionChange()
	{
		Object obj = Selection.activeObject;
		if ( obj != m_clip && obj is AnimationClip )
		{
			m_clip = Selection.activeObject as AnimationClip;
			OnClipChange();
		}
	}

	/// Handles selection a single frame on timeline and list, and puts playhead at start
	void SelectFrame( AnimFrame selectedFrame )
	{		
		bool ctrlClick = Event.current.control;
		bool shiftClick = Event.current.shift && m_selectedFrames.Count == 1; // Can only shift click if 1 is selected already

		// Clear existing events unless ctrl is clicked, or we're select dragging
		if ( ctrlClick == false && shiftClick == false )
		{			
			m_selectedFrames.Clear();
		}
		m_selectedEvents.Clear();
		m_selectedNode = -1;

		// Don't add if already in selection list, and if holding ctrl remove it from the list
		if ( m_selectedFrames.Contains(selectedFrame) == false )
		{

			if ( shiftClick )
			{
				// Add frames between selectd and clicked.
				int indexFrom = m_frames.FindIndex(item=>item == m_selectedFrames[0]);
				int indexTo = m_frames.FindIndex(item=>item == selectedFrame);
				if ( indexFrom > indexTo ) Utils.Swap( ref indexFrom, ref indexTo );
				for ( int i = indexFrom+1; i < indexTo; ++i )
				{
					m_selectedFrames.Add(m_frames[i]);
				}
			}
			m_selectedFrames.Add(selectedFrame);


			if ( ctrlClick == false )
			{
				m_framesReorderableList.index = m_frames.FindIndex(item=>item == selectedFrame);

				// Put playhead at beginning of selected frame (if not playing)
				if ( m_playing == false )
					m_animTime = selectedFrame.m_time;			
			}
		}
		else if ( ctrlClick )
		{
			m_selectedFrames.Remove(selectedFrame);
		}

		// Sort selection
		m_selectedFrames.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );
	}


	/// Handles selection a single event on timeline
	void SelectEvent( AnimEvent selectedEvent )
	{		
		bool ctrlClick = Event.current.control;
		// Clear existing events unless ctrl is clicked, or we're select dragging
		if ( ctrlClick == false )
		{			
			m_selectedEvents.Clear();
		}
		m_selectedFrames.Clear();
		m_selectedNode = -1;

		// Don't add if already in selection list, and if holding ctrl remove it from the list
		if ( m_selectedEvents.Contains(selectedEvent) == false )
		{
			m_selectedEvents.Add(selectedEvent);

			// Put playhead at beginning of selected frame (if not playing)
			if ( m_playing == false && ctrlClick == false )
				m_animTime = selectedEvent.m_time;	
		}
		else if ( ctrlClick )
		{
			m_selectedEvents.Remove(selectedEvent);
		}

		// Sort selection
		m_selectedEvents.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );
	}

	void ResetPreviewScale( Rect rect )
	{
		Sprite sprite = null;
		if ( m_frames.Count > 0 )
			sprite = m_frames[0].m_sprite as Sprite;

		m_previewScale = 1;
		if ( sprite != null && rect.width > 0 && rect.height > 0 && sprite.rect.width > 0 && sprite.rect.height > 0 )
		{
			float widthScaled = rect.width / sprite.rect.width;
			float heightScaled = rect.height / sprite.rect.height;

			// Finds best fit for preview window based on sprite size
			if ( widthScaled < heightScaled)
			{
				m_previewScale = rect.width / sprite.rect.width;
			}
			else 
			{
				m_previewScale = rect.height / sprite.rect.height;
			}

			m_previewScale = Mathf.Clamp(m_previewScale,0.1f,100.0f) * 0.95f;

			// Set the preview offset to center the sprite
			if ( m_ignorePivot )
			{
				m_previewOffset = Vector2.zero;
			}
			else 
			{
				m_previewOffset = -( (sprite.rect.size*0.5f) - sprite.pivot) * m_previewScale;
				m_previewOffset.y = -m_previewOffset.y;
			}
		}
	}

	// Returns a usable texture that looks like a high-contrast checker board.
    static Texture2D GetCheckerboardTexture()
    {
        if (s_textureCheckerboard == null)
        {
			s_textureCheckerboard = new Texture2D(2, 2);
			s_textureCheckerboard.name = "[Generated] Checkerboard Texture";
			s_textureCheckerboard.hideFlags = HideFlags.DontSave;
			s_textureCheckerboard.filterMode = FilterMode.Point;
			s_textureCheckerboard.wrapMode = TextureWrapMode.Repeat;

			Color c0 = new Color(0.4f,0.4f,0.4f,1.0f);
			Color c1 = new Color(0.278f,0.278f, 0.278f, 1.0f);
			s_textureCheckerboard.SetPixel(0,0,c0);
			s_textureCheckerboard.SetPixel(1,1,c0);
			s_textureCheckerboard.SetPixel(0,1,c1);
			s_textureCheckerboard.SetPixel(1,0,c1);
			s_textureCheckerboard.Apply();
		}			
        return s_textureCheckerboard;
        
    }

    float GetAnimLength()
    {
		if (m_frames != null && m_frames.Count > 0) 
		{
			AnimFrame lastFrame = m_frames[m_frames.Count-1];
			return lastFrame.m_time+lastFrame.m_length;
		}
		return 0;
	}

    AnimFrame GetCurrentFrame()
	{
		int id = GetCurrentFrameId();
		return ( id < 0 ) ? null : m_frames[id];
    }

    int GetCurrentFrameId()
	{
    	if ( m_frames == null || m_frames.Count == 0)
    		return -1;
		int frame = m_frames.FindIndex( item => item.m_time > m_animTime );
		if ( frame < 0 ) 
			frame = m_frames.Count;
		frame--;
    	return frame;
    }

    AnimFrame GetFrameAtTime(float time)
    {
    	if ( m_frames == null || m_frames.Count == 0 )
    		return null;
		int frame = m_frames.FindIndex( item => item.m_time > time );
		if ( frame <= 0 || frame > m_frames.Count  ) 
			frame = m_frames.Count;
		frame--;
    	return m_frames[frame];
	}


    Sprite GetSpriteAtTime(float time)
    {   
    	AnimFrame frame = GetFrameAtTime(time);
    	return ( frame != null ) ? frame.m_sprite as Sprite : null;
    }

    static void DrawLine( Vector2 from, Vector2 to, Color color, float width = 0, bool snap = true ) 
    {
        if ( (to - from).sqrMagnitude <= float.Epsilon )
            return;

		if ( snap )
		{
	        from.x = Mathf.FloorToInt(from.x); 
	        from.y = Mathf.FloorToInt(from.y);
	        to.x = Mathf.FloorToInt(to.x); 
	        to.y = Mathf.FloorToInt(to.y);
	    }

        Color savedColor = Handles.color;
        Handles.color = color;

        if ( width > 1.0f ) 
            Handles.DrawAAPolyLine(width, new Vector3[] { from, to } );
        else 
            Handles.DrawLine( from, to );

        

        Handles.color = savedColor;
    }


    static void DrawRect( Rect rect, Color backgroundColor ) 
    {
        EditorGUI.DrawRect(rect, backgroundColor);
    }

    static void DrawRect( Rect rect, Color backgroundColor, Color borderColor, float borderWidth = 1 ) 
    {
        // draw background
        EditorGUI.DrawRect(rect, backgroundColor);

		// Draw border
        rect.width = rect.width - borderWidth;
		rect.height = rect.height - borderWidth;
		DrawLine(new Vector2(rect.xMin,rect.yMin), new Vector2(rect.xMin,rect.yMax), borderColor, borderWidth);
		DrawLine(new Vector2(rect.xMin,rect.yMax), new Vector2(rect.xMax,rect.yMax), borderColor, borderWidth);
		DrawLine(new Vector2(rect.xMax,rect.yMax), new Vector2(rect.xMax,rect.yMin), borderColor, borderWidth);
		DrawLine(new Vector2(rect.xMax,rect.yMin), new Vector2(rect.xMin,rect.yMin), borderColor, borderWidth);
    }

	float GetMinFrameTime()
	{
		return 1.0f/m_clip.frameRate;
	}

	void OnUndoRedo()
	{
		OnClipChange(false);
	}

	/// Snaps a time to the closest sample time on the timeline
	float SnapTimeToFrameRate(float value)
	{
		return Mathf.Round(value * m_clip.frameRate) / m_clip.frameRate;
	}

	/// Update the frame and ratio that each event is linked to. Call when events have changed
	void RecalcEventFrameLinks()
	{
		// Events store data so they can be moved when animation frames are scaled, etc.
		foreach( AnimEvent animEvent in m_events )
		{
			AnimFrame frame = GetFrameAtTime( animEvent.m_time );
			animEvent.m_linkedFrame = frame;
			if ( frame != null && frame.m_length > 0)
				animEvent.m_linkedRatio = (animEvent.m_time-frame.m_time) / frame.m_length;

			// Don't link frames that are past end
			if ( animEvent.m_linkedRatio < 0 || animEvent.m_linkedRatio > 1 )
				animEvent.m_linkedFrame = null; 
		}
	}

	/// Change time of events that are linked to a frame if that frame has changed. Call when animation timing has changed
	void RepositionLinkedEvents()
	{
		// Events store data so they can be moved when animation frames are scaled, etc.
		if ( m_events == null )
			return;
		foreach( AnimEvent animEvent in m_events )
		{
			if ( animEvent.m_linkedFrame != null && m_frames.Contains( animEvent.m_linkedFrame ) )
			{
				animEvent.m_time = SnapTimeToFrameRate( animEvent.m_linkedFrame.m_time + (animEvent.m_linkedRatio * animEvent.m_linkedFrame.m_length) );
			}
			else 
			{
				animEvent.m_linkedFrame = null;
			}
		}
	}


	void RemoveExistingBinding()
	{
		if ( m_curveBinding.propertyName == PROPERTYNAME_SPRITE )
		{
			AnimationUtility.SetObjectReferenceCurve(m_clip, m_curveBinding, null);	
		}
		else 
		{
			// Old binding may have been unloaded due to playing/stopping the game, so if the binding wasn't found, try finding it again to avoid creating a second binding.
			m_curveBinding = System.Array.Find( AnimationUtility.GetObjectReferenceCurveBindings(m_clip), item=>item.propertyName == PROPERTYNAME_SPRITE ); 
			AnimationUtility.SetObjectReferenceCurve(m_clip, m_curveBinding, null);	
		}
	}

	void CreateCurveBinding()
	{
		m_curveBinding = new EditorCurveBinding();
		// I want to change the sprites of the sprite renderer, so I put the typeof(SpriteRenderer) as the binding type.
		m_curveBinding.type = m_uiImage ? typeof(UnityEngine.UI.Image) : typeof(SpriteRenderer);
		// Regular path to the gameobject that will be changed (empty string means root)
		m_curveBinding.path = m_spritePath;
		// This is the property name to change the sprite of a sprite renderer
		m_curveBinding.propertyName = PROPERTYNAME_SPRITE;
	}

	#endregion

}

}

namespace PowerTools.Anim
{

/// Handy extention methods
public static class ExtentionMethods
{
	public static Color WithAlpha(this Color col, float alpha )
	{
		return new Color(col.r,col.g,col.b,alpha);
	}
}

/// Handy utils
public static class Utils
{
	/// Returns float value snapped to closest point
	public static float Snap(float value, float snapTo)
	{
		if ( snapTo <= 0 ) return value;
		return Mathf.Round(value / snapTo) * snapTo;
	}

	/// Swaps two objects
	public static void Swap<T>(ref T lhs, ref T rhs)
	{
	    T temp;
	    temp = lhs;
	    lhs = rhs;
	    rhs = temp;
	}

	// Creates new instance of passed object and copies variables
	public static T Clone<T>(T from) where T : new()
	{
		T result = new T();

		FieldInfo[] finfos = from.GetType().GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
		for ( int i = 0; i < finfos.Length; ++i )
		{			
			finfos[i].SetValue(result, finfos[i].GetValue(from));
		}
		return result;
	}

	public static class BitMask
	{			
		// And some static functions if you don't wanna construt the bitmask  and just wanna pass in/out an int
		public static int SetAt(int mask, int index) { return mask | 1 << index; }
		public static int UnsetAt(int mask, int index)  { return mask & ~(1 << index); }
		public static bool IsSet(int mask, int index) { return (mask & 1 << index) != 0; }

		public static uint GetNumberOfSetBits(uint i)
		{
			// From http://stackoverflow.com/questions/109023/how-to-count-the-number-of-set-bits-in-a-32-bit-integer
			i = i - ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}
	}

	// Returns angle normalized 2d direction vector in degrees between 0 and 360
	public static float GetDirectionAngle( this Vector2 directionNormalised )
	{		
		if ( Mathf.Approximately( directionNormalised.y, 0) )
		{
			if ( directionNormalised.x < 0 )
				return 180.0f;
			else 
				return 0;
		}
		else if ( Mathf.Approximately( directionNormalised.x, 0 ) )
		{
			if ( directionNormalised.y < 0 )
				return 270.0f;
			else 
				return 90.0f;
		}
		
		return Mathf.Repeat( Mathf.Rad2Deg * Mathf.Atan2(directionNormalised.y, directionNormalised.x), 360 );
	}
}

/// For sorting strings by natural order (so, for example walk_9.png is sorted before walk_10.png)
public class NaturalComparer: Comparer<string>, System.IDisposable 
{
	// NaturalComparer function courtesy of Justin Jones http://www.codeproject.com/Articles/22517/Natural-Sort-Comparer

	Dictionary<string, string[]> m_table = null;

	public NaturalComparer() 
	{
		m_table = new Dictionary<string, string[]>();
	}

	public void Dispose() 
	{
		m_table.Clear();
		m_table = null;
	}

	public override int Compare(string x, string y) 
	{
		if (x == y) 
			return 0;

		string[] x1, y1;
		if (!m_table.TryGetValue(x, out x1)) 
		{
			x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
			m_table.Add(x, x1);
		}
		if (!m_table.TryGetValue(y, out y1)) 
		{
			y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
			m_table.Add(y, y1);
		}

		for (int i = 0; i < x1.Length && i < y1.Length; i++) 
		{
			if (x1[i] != y1[i]) 
			{
				return PartCompare(x1[i], y1[i]);
			}
		}

		if (y1.Length > x1.Length) 
		{
			return 1;
		} 
		else if (x1.Length > y1.Length) 
		{
			return -1;
		} 

		return 0;		
	}


	static int PartCompare(string left, string right) 
	{
		int x, y;
		if (!int.TryParse(left, out x)) 
		{
			return left.CompareTo(right);
		}

		if (!int.TryParse(right, out y)) 
		{
			return left.CompareTo(right);
		}

		return x.CompareTo(y);
	}

}

}