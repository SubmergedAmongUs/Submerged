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
using System.Reflection;
using PowerTools.Anim;

namespace PowerTools
{

public partial class SpriteAnimator
{
	#region Definitions

	[System.Serializable]
	class Node
	{
		public int m_nodeId;
		public Vector2 m_offset;
		public float m_angle;
	}

	static readonly string PROPERTY_NODE_X = "m_node{0}.x";
	static readonly string PROPERTY_NODE_Y = "m_node{0}.y";
	static readonly string PROPERTY_NODE_ANGLE = "m_ang{0}";

	static readonly System.Type TYPE_SPRITEANIMNODES = typeof(SpriteAnimNodes);

	static EditorCurveBinding[] NODE_BINDINGS_X = null;
	static EditorCurveBinding[] NODE_BINDINGS_Y = null;
	static EditorCurveBinding[] NODE_BINDINGS_ANGLE = null;

	static readonly float ROTATE_ARROW_LENGTH = 35;

	static Color[] NODE_COLORS = 
	{ 
		new Color(1.000f, 0.620f, 0.247f),
		new Color(0.329f, 0.843f, 0.933f),
		new Color(0.584f, 0.898f, 0.427f),
		new Color(0.925f, 0.302f, 0.235f),
		new Color(0.447f, 0.310f, 0.898f),
		new Color(1.000f, 0.831f, 0.310f),
		new Color(0.051f, 0.702f, 0.388f),
		new Color(0.992f, 0.498f, 0.812f),
		new Color(0.216f, 0.306f, 0.843f),
		new Color(0.835f, 0.294f, 0.953f),
		new Color(0.890f, 0.114f, 0.353f)
	};

	// This class is used to store keyframe data temporarily after reading from anim, before storing in AnimFrames
	class KeyframeReadInData
	{
		public KeyframeReadInData( System.Action<AnimFrame, int, float> functionSetNodeAtFrame, Keyframe keyframe )
		{
			m_functionSetNodeAtFrame = functionSetNodeAtFrame;
			m_time = keyframe.time;
			m_value = keyframe.value;
		}

		public System.Action<AnimFrame, int, float> m_functionSetNodeAtFrame = null;
		public float m_time = 0;
		public float m_value = 0;
	}

	#if UNITY_5_3

	// Necessary hacks to support unity 5.3: Taken from http://answers.unity3d.com/questions/313276/undocumented-property-keyframetangentmode.html
	static readonly int KEY_MODE_CONSTANT = 4; // Based on secret hidden unity enum
	static readonly int KEY_MODE_CONSTANT_RIGHT_LEFT = ((KEY_MODE_CONSTANT << 1) & -25) | (KEY_MODE_CONSTANT << 3 );

	#endif

	#endregion
	#region Vars: Private

	// Cached node values for current frame
	[SerializeField] int m_nodeMask = 0; // Which nodes are overridden
	[SerializeField] Vector2[] m_nodePositions = new Vector2[SpriteAnimNodes.NUM_NODES]; // positions of the nodes
	[SerializeField] float[] m_nodeAngles = new float[SpriteAnimNodes.NUM_NODES]; // angles of the nodes

	int m_selectedNode = -1;

	#endregion
	#region Funcs: Init



	#endregion
	#region Funcs: Layout

	void LayoutAnimationNodesPanel( Rect rect )
	{
		UpdateCurrentFrameNodePositions();

		Event e = Event.current;

		AnimFrame currentFrame = GetCurrentFrame();

		//
		// Draw Nodes - back to front (selected at front)
		//
		for ( int i = 0; i < SpriteAnimNodes.NUM_NODES; ++i ) 
		{
			if ( i != m_selectedNode && Utils.BitMask.IsSet(m_visibleNodes,i) )
				LayoutNode(rect, i, currentFrame);
		}
		if ( m_selectedNode >= 0 )
		{
			LayoutNode(rect, m_selectedNode, currentFrame);		
		}

		//
		// Handle node events - front to back (selected last)
		//
		if ( m_selectedNode >= 0 )
		{
			HandleNodeEvents(rect, m_selectedNode, e);		
		}
		for ( int i = SpriteAnimNodes.NUM_NODES-1; i >= 0; --i ) 
		{
			if ( i != m_selectedNode  && Utils.BitMask.IsSet(m_visibleNodes,i) )
				HandleNodeEvents(rect, i, e);
		}

		//
		// Check for double click to add node
		//

		if ( m_playing == false && e.button == 0 && e.type == EventType.MouseDown && rect.Contains(e.mousePosition) && m_dragState == eDragState.None )
		{
			if ( m_selectedNode >= 0 )
			{
				ClearSelection();
				GUI.FocusControl("none");
				e.Use();
			}
			if ( e.clickCount == 2 &&  GetUnusedNodeId() >= 0  )
			{
				// Double clicked, add new node at position, with the cached angle from the last set frame
				AddNode((e.mousePosition - rect.center - m_previewOffset) / m_previewScale );
				GUI.FocusControl("none");
				e.Use();
			}
		}
	}

	void LayoutNode( Rect rect, int nodeId, AnimFrame currentFrame )
	{
		if ( Utils.BitMask.IsSet(m_nodeMask, nodeId) == false )
			return;

		//
		// Translate node position to preview window
		//
		Vector2 nodePos = m_nodePositions[nodeId];
		nodePos = nodePos * m_previewScale;
		nodePos = nodePos + m_previewOffset;
		Rect nodeRect = new Rect( rect.center.x + nodePos.x-8, rect.center.y +  nodePos.y-8, 16,16);

		// Check rect is visible
		if ( rect.Contains(nodeRect.center) == false )
			return;

		//
		// Draw
		//
        Color savedColor = Handles.color;
		Handles.color = Color.white;// NODE_COLORS[i%NODE_COLORS.Length];

		bool hasSprite = m_defaultNodeSprites[nodeId] != null;
		if ( hasSprite )
		{
			// Render node sprite if it has one
			LayoutFrameSprite(new Rect(rect){center = nodeRect.center}, m_defaultNodeSprites[nodeId], m_previewScale, Vector2.zero /*m_previewOffset*/, false, true, m_nodeAngles[nodeId]);
		}
		string nodeText = nodeId.ToString();
		if ( string.IsNullOrEmpty(m_defaultNodeNames[nodeId]) == false && hasSprite == false )
			nodeText = string.Concat(nodeText, ':', m_defaultNodeNames[nodeId]);
		float textWidth = Styles.PREVIEW_LABEL_BOLD.CalcSize(new GUIContent(nodeText)).x + 2;
		Rect labelRectName = new Rect(nodeRect) { x = nodeRect.x + 12f, y = nodeRect.y-1, width = textWidth };
		// Offset while dragging or if angle arrow would be overlapping text
		if ( m_selectedNode == nodeId && (m_dragState == eDragState.MoveNode || (m_nodeAngles[nodeId] < 10 || m_nodeAngles[nodeId] > 340 )) )
			labelRectName.y = labelRectName.y - 10f;
		GUI.Label(new Rect(labelRectName) { x = labelRectName.x + 1f, y = labelRectName.y + 1f }, nodeText, new GUIStyle( Styles.PREVIEW_LABEL_BOLD_SHADOW ) { fontSize = 10 } );
		GUI.Label(new Rect(labelRectName) { x = labelRectName.x + 1f, y = labelRectName.y   }, nodeText, new GUIStyle( Styles.PREVIEW_LABEL_BOLD_SHADOW ) { fontSize = 10 } );
		GUI.Label(new Rect(labelRectName) { x = labelRectName.x - 1f, y = labelRectName.y - 1f  }, nodeText, new GUIStyle( Styles.PREVIEW_LABEL_BOLD_SHADOW ) { fontSize = 10 } );
		GUI.Label( labelRectName, nodeText, new GUIStyle( Styles.PREVIEW_LABEL_BOLD ) { fontSize = 10 } );

		// Alpha lower when the frame doesn't have the node overriden
		bool frameHasNode = ( currentFrame != null && currentFrame.m_nodes != null && currentFrame.m_nodes.Exists(item=>item.m_nodeId == nodeId) );
		float alpha = frameHasNode ? 1.0f : 0.25f;
		float sizeInner = frameHasNode ? 4 : 3;

		Color colorSelectedOutline = Color.white.WithAlpha(alpha);
		Color colorUnselectedOutline = Color.black.WithAlpha(0.5f * alpha);

		Quaternion nodeRotation = Quaternion.Euler(0,0, -m_nodeAngles[nodeId]);

		if ( m_selectedNode == nodeId )
		{

			if ( m_dragState == eDragState.MoveNode )
			{	
				// Draw crosshair
				float crosshairLength = 256;

				Handles.color = Color.white.WithAlpha(0.4f);
				DrawLine( new Vector2( nodeRect.center.x+1-crosshairLength, nodeRect.center.y+1 ), new Vector2( nodeRect.center.x+1+crosshairLength, nodeRect.center.y+1 ), Handles.color, 0);
				DrawLine( new Vector2( nodeRect.center.x+1, nodeRect.center.y+1-crosshairLength ), new Vector2( nodeRect.center.x+1, nodeRect.center.y+1+crosshairLength ), Handles.color, 0);				
				Handles.DrawWireDisc( nodeRect.center,Vector3.back,128 );
				Handles.DrawWireDisc( nodeRect.center,Vector3.back,96 );
				Handles.DrawWireDisc( nodeRect.center,Vector3.back,64 );
				Handles.DrawWireDisc( nodeRect.center,Vector3.back,32 );

				crosshairLength = 20;
				DrawLine( new Vector2( nodeRect.center.x+1-crosshairLength, nodeRect.center.y+1 ), new Vector2( nodeRect.center.x+1+crosshairLength, nodeRect.center.y+1 ), Color.black, 0);
				DrawLine( new Vector2( nodeRect.center.x+1, nodeRect.center.y+1-crosshairLength ), new Vector2( nodeRect.center.x+1, nodeRect.center.y+1+crosshairLength ), Color.black, 0);					
				DrawLine( new Vector2( nodeRect.center.x-crosshairLength, nodeRect.center.y ), new Vector2( nodeRect.center.x+crosshairLength, nodeRect.center.y ), NODE_COLORS[nodeId%NODE_COLORS.Length],0 );
				DrawLine( new Vector2( nodeRect.center.x, nodeRect.center.y-crosshairLength ), new Vector2( nodeRect.center.x, nodeRect.center.y+crosshairLength ), NODE_COLORS[nodeId%NODE_COLORS.Length],0 );
				// Draw position string
				string positionString = string.Format("{0:0.0}, {1:0.0}",m_nodePositions[nodeId].x,m_nodePositions[nodeId].y);
				Vector2 labelSize = Styles.PREVIEW_LABEL_BOLD.CalcSize( new GUIContent(positionString) );
				Rect labelRect = new Rect( nodeRect.x+30, nodeRect.y+5, labelSize.x,labelSize.y );
				GUI.Label( new Rect(labelRect) { x = labelRect.x+1f, y = labelRect.y+1f }, positionString, Styles.PREVIEW_LABEL_BOLD_SHADOW );
				GUI.Label( new Rect(labelRect) { x = labelRect.x-1f, y = labelRect.y-1f }, positionString, Styles.PREVIEW_LABEL_BOLD_SHADOW );
				GUI.Label( labelRect, positionString, Styles.PREVIEW_LABEL_BOLD );
			}
			else 
			{
				Handles.color = NODE_COLORS[nodeId%NODE_COLORS.Length].WithAlpha(alpha);

				Vector2 angleOffset = nodeRotation * Vector2.right * ROTATE_ARROW_LENGTH;

				Vector2 lineOffset = nodeRotation*Vector2.up;
				DrawLine( nodeRect.center + lineOffset, nodeRect.center + angleOffset + lineOffset,Handles.color);
				lineOffset = nodeRotation*Vector2.down;
				DrawLine( nodeRect.center+lineOffset, nodeRect.center + angleOffset + lineOffset,Handles.color);
				Handles.DrawSolidArc( (Vector3)nodeRect.center + (nodeRotation * Vector2.right * (ROTATE_ARROW_LENGTH+3)),Vector3.back, Quaternion.Euler(0,0,-m_nodeAngles[nodeId]+20f) * Vector2.left, 40, 10); 
				DrawLine( nodeRect.center, nodeRect.center + angleOffset,Color.white);

				Handles.DrawSolidDisc( nodeRect.center, Vector3.back, sizeInner+3);

				Handles.color = colorSelectedOutline;
				Handles.DrawSolidDisc( nodeRect.center, Vector3.back, sizeInner+2);

				Handles.color = NODE_COLORS[nodeId%NODE_COLORS.Length].WithAlpha(alpha);
				Handles.DrawSolidDisc( nodeRect.center, Vector3.back, sizeInner);

				if ( m_dragState == eDragState.RotateNode )
				{
					// Draw angle string
					string angleString = string.Format("{0:0}\u00B0", m_nodeAngles[nodeId]);
					Vector2 labelSize = Styles.PREVIEW_LABEL_BOLD.CalcSize( new GUIContent(angleString) );
					Rect labelRect = new Rect( nodeRect.x + 15, nodeRect.y+15, labelSize.x,labelSize.y );
					GUI.Label( new Rect(labelRect) { x = labelRect.x+1f, y = labelRect.y+1f }, angleString, Styles.PREVIEW_LABEL_BOLD_SHADOW );
					GUI.Label( new Rect(labelRect) { x = labelRect.x-1f, y = labelRect.y-1f }, angleString, Styles.PREVIEW_LABEL_BOLD_SHADOW );
					GUI.Label( labelRect, angleString, Styles.PREVIEW_LABEL_BOLD );
				}

			}

		}
		else 
		{		

			Handles.color = colorUnselectedOutline;
			Handles.DrawSolidDisc( new Vector2( nodeRect.center.x, nodeRect.center.y ), Vector3.back, sizeInner+1);
			// Draw angle arrow
			float arrowSize = sizeInner * 3.0f;
			const float arrowAngle = 40;
			if ( m_nodeAngles[nodeId] != 0 )
				Handles.DrawSolidArc( (Vector3)nodeRect.center + (nodeRotation * Vector2.right * (arrowSize+3)),Vector3.back, Quaternion.Euler(0,0,-m_nodeAngles[nodeId]+(arrowAngle*0.5f)) * Vector2.left, arrowAngle, arrowSize); 
			Handles.color = NODE_COLORS[nodeId%NODE_COLORS.Length].WithAlpha(alpha);
			if ( m_nodeAngles[nodeId] != 0 )
				Handles.DrawSolidArc( (Vector3)nodeRect.center + (nodeRotation * Vector2.right * arrowSize),Vector3.back, Quaternion.Euler(0,0,-m_nodeAngles[nodeId]+(arrowAngle*0.5f)) * Vector2.left, arrowAngle, arrowSize); 

			// Draw foreground dot
			Handles.DrawSolidDisc( new Vector2( nodeRect.center.x, nodeRect.center.y ), Vector3.back, sizeInner);
		}
		Handles.color = savedColor;
	}



	void HandleNodeEvents( Rect rect, int nodeId, Event e )
	{
		if ( Utils.BitMask.IsSet(m_nodeMask, nodeId) == false )
			return;

		//
		// Translate node position to preview window
		//
		Vector2 nodePos = m_nodePositions[nodeId];
		nodePos = nodePos * m_previewScale;
		nodePos = nodePos + m_previewOffset;
		Rect nodeRect = new Rect( rect.center.x + nodePos.x-8, rect.center.y +  nodePos.y-8, 16,16);

		Vector2 rotateHandlePos = nodeRect.center + (Vector2)(Quaternion.Euler(0,0, -m_nodeAngles[nodeId]) * Vector2.right * ROTATE_ARROW_LENGTH);
		Rect nodeRotateRect = new Rect( rotateHandlePos.x-8, rotateHandlePos.y-8, 16,16);

		//
		// Handle Events
		//

		if ( m_playing == false )
		{
			if ( m_dragState == eDragState.None )
			{
				if ( e.button == 0 && rect.Contains(e.mousePosition) )
				{
					// Event rect
					EditorGUIUtility.AddCursorRect( nodeRect, MouseCursor.MoveArrow );

					// Node
					if ( e.type == EventType.MouseDown && nodeRect.Contains(e.mousePosition) )
					{
						ClearSelection();
						m_selectedNode = nodeId;
						m_dragState = eDragState.MoveNode;
						GUI.FocusControl("none");
						e.Use();
					}
					else if ( e.type == EventType.MouseDrag && m_selectedNode == nodeId && nodeRect.Contains(e.mousePosition) )
					{
						m_dragState = eDragState.MoveNode;
						e.Use();
					}
					// Node rotate Handle
					else if ( m_selectedNode == nodeId )
					{
						EditorGUIUtility.AddCursorRect( nodeRotateRect, MouseCursor.RotateArrow );
						if ( e.type == EventType.MouseDown && nodeRotateRect.Contains(e.mousePosition) )
						{
							m_dragState = eDragState.RotateNode;
							GUI.FocusControl("none");
							e.Use();
						}
						if ( e.type == EventType.MouseDrag  )
						{
							m_dragState = eDragState.RotateNode;
							e.Use();
						}
					}
				}
			}
			else if ( m_dragState == eDragState.MoveNode )
			{	
												
				EditorGUIUtility.AddCursorRect( rect, MouseCursor.MoveArrow );

				if ( e.button == 0 && m_selectedNode == nodeId && e.type == EventType.MouseDrag )
				{
					MoveNode( m_selectedNode, 
						(e.mousePosition - rect.center - m_previewOffset) / m_previewScale, 
						(e.modifiers & (EventModifiers.Shift)) == 0 ); // only snap if not holding shift
					e.Use();
				}
				else if ( e.button == 0 && m_selectedNode == nodeId && e.type == EventType.MouseUp )
				{
					m_dragState = eDragState.None;
					e.Use();
					ApplyChanges();
				}
			}
			else if ( m_dragState == eDragState.RotateNode )
			{
				EditorGUIUtility.AddCursorRect( rect, MouseCursor.RotateArrow );
				if ( e.button == 0 && m_selectedNode == nodeId && e.type == EventType.MouseDrag )
				{
					Vector2 endPos = (e.mousePosition - rect.center - m_previewOffset) / m_previewScale;
					float angle = -(endPos - m_nodePositions[m_selectedNode]).normalized.GetDirectionAngle();

					// Snap to 15 if holding control/command
					if ( (e.modifiers & (EventModifiers.Control | EventModifiers.Command)) > 0 )
						angle = Utils.Snap(angle, 15.0f);
					// Snap to 1 (if not holding shift)
					else if ( (e.modifiers & (EventModifiers.Shift)) == 0 )
						angle = Utils.Snap(angle, 1.0f); 
					
					RotateNode( m_selectedNode, angle );
					e.Use();
				}
				else if ( e.button == 0 && m_selectedNode == nodeId && e.type == EventType.MouseUp )
				{
					m_dragState = eDragState.None;
					e.Use();
					ApplyChanges();
				}
			}
		}
	}

	// Node data editor
	void LayoutBottomBarNodeData( Rect rect )
	{
		AnimFrame currentFrame = GetCurrentFrame();
		bool frameHasNode = ( currentFrame != null && currentFrame.m_nodes != null && currentFrame.m_nodes.Exists(item=>item.m_nodeId == m_selectedNode) );

		GUIContent labelContent = null;

		float xOffset = 10;
		float width = 60;
		GUI.Label( new Rect(xOffset,1, width,rect.height), "Node: "+ m_selectedNode, EditorStyles.boldLabel );

		Vector2 pos = m_nodePositions[m_selectedNode];
		float angle = m_nodeAngles[m_selectedNode];

		// Position
		EditorGUI.BeginChangeCheck();

		xOffset += width;
		width = 10;
		labelContent = new GUIContent("X");
		width = EditorStyles.miniLabel.CalcSize(labelContent).x;
		GUI.Label(new Rect(xOffset,1,width,rect.height), labelContent, EditorStyles.miniLabel);

		xOffset += width;
		width = 40;
		GUI.SetNextControlName("OffsetX");
		pos.x = EditorGUI.FloatField( new Rect(xOffset,2, width,rect.height-3), Mathf.Abs(pos.x) <= 0.00011f ? 0 : pos.x, EditorStyles.miniTextField );

		xOffset += width;
		width = 10;
		labelContent = new GUIContent("Y");
		width = EditorStyles.miniLabel.CalcSize(labelContent).x;
		GUI.Label(new Rect(xOffset,0,width,rect.height),labelContent, EditorStyles.miniLabel);

		xOffset += width;
		width = 40;
		GUI.SetNextControlName("OffsetY");
		pos.y = EditorGUI.FloatField( new Rect(xOffset,2, width,rect.height-3), Mathf.Abs(pos.y) <= 0.00011f ? 0 : pos.y, EditorStyles.miniTextField );
		if ( EditorGUI.EndChangeCheck() )
		{
			MoveNode( m_selectedNode, pos );
			ApplyChanges();
		}

		// Angle
		EditorGUI.BeginChangeCheck();
		xOffset += width + 1;
		labelContent = new GUIContent("Angle:");
		width = EditorStyles.miniLabel.CalcSize(labelContent).x;
		GUI.Label(new Rect(xOffset,1,width,rect.height), labelContent, EditorStyles.miniLabel);

		xOffset += width;
		GUI.SetNextControlName("Angle");
		width = Mathf.Max( 25, EditorStyles.miniTextField.CalcSize(new GUIContent(angle.ToString())).x + 2 );
		angle = EditorGUI.FloatField( new Rect(xOffset,2, width,rect.height-3), angle, EditorStyles.miniTextField );

		if ( EditorGUI.EndChangeCheck() )
		{
			RotateNode(m_selectedNode, angle);
			ApplyChanges();
		}

		// Display Name
		xOffset += width+15;
		labelContent = new GUIContent("Preview name:");
		width =  EditorStyles.miniLabel.CalcSize(labelContent).x;
		EditorGUI.LabelField(new Rect(xOffset,1,width,rect.height-3), labelContent, EditorStyles.miniLabel );
		xOffset += width;

		width = 60;
		GUI.SetNextControlName("NodeName");
		width =  Mathf.Max( 25, EditorStyles.miniTextField.CalcSize(new GUIContent(m_defaultNodeNames[m_selectedNode])).x + 2 );
		m_defaultNodeNames[m_selectedNode] = EditorGUI.TextField( new Rect(xOffset, 2, width, rect.height-3), m_defaultNodeNames[m_selectedNode], EditorStyles.miniTextField );
		xOffset += width;

		labelContent = new GUIContent("/ sprite");
		width =  EditorStyles.miniLabel.CalcSize(labelContent).x;
		EditorGUI.LabelField(new Rect(xOffset,1,width,rect.height-3), labelContent, EditorStyles.miniLabel );
		xOffset += width;

		width = 70;
		labelContent = new GUIContent(m_defaultNodeSprites[m_selectedNode] == null ? "None" : m_defaultNodeSprites[m_selectedNode].name);
		width =  Mathf.Min( 120, EditorStyles.miniTextField.CalcSize(labelContent).x + 38 );
		//width =  Mathf.Max( 25, EditorStyles.miniTextField.CalcSize(new GUIContent(m_defaultNodeSprites[m_selectedNode])).x + 2 );
		m_defaultNodeSprites[m_selectedNode] = EditorGUI.ObjectField( new Rect(xOffset, 2, width, rect.height-3), m_defaultNodeSprites[m_selectedNode], typeof(Sprite), false ) as Sprite;
		xOffset += width;

		//width = 120;
		//GUI.Label( new Rect(xOffset,1, width,rect.height), "(editor only)", EditorStyles.miniLabel );

		// Align to right
		xOffset = rect.width;

		// Reset button
		width = 50;
		xOffset -= width + 5;
		if ( GUI.Button(new Rect(xOffset,2, width,rect.height-4), "Delete", EditorStyles.miniButton) )
		{
			DeleteNodeAllFrames(m_selectedNode);
		}

		
		#if UNITY_2019_1_OR_NEWER
		width = 125;
		#else 
		width = 105;
		#endif
		xOffset -= width + 5;
		GUI.enabled = frameHasNode;
		if ( GUI.Button(new Rect(xOffset,2, width,rect.height-4), "Clear Frame Offset", EditorStyles.miniButton) )
		{
			DeleteNode(m_selectedNode);
		}
		GUI.enabled = true;


	}

	#endregion
	#region Funcs: Changes to Anim


	void AddNode( Vector2 offset )
	{		
		if ( m_playing )
			return;

		if ( m_nodePositions == null || m_nodePositions.Length == 0 )
			m_nodePositions = new Vector2[SpriteAnimNodes.NUM_NODES];
		if ( m_nodeAngles == null || m_nodeAngles.Length == 0 )
			m_nodeAngles = new float[SpriteAnimNodes.NUM_NODES];

		int newNodeId = GetUnusedNodeId();
		m_nodePositions[newNodeId] = offset;

		if ( Utils.BitMask.IsSet(m_nodeMask, newNodeId) == false )
		{
			/* Disabled this for now, might return it if it's an issue having the first node being unspecified causing problems /
			// The first time a node is added, add a zero offset node to the first frame of the anim
			if ( GetCurrentFrameId() > 0 )
				SetNodeAtFrame(GetFrameAtTime(0),0,Vector2.zero);
			*/
			m_nodeMask = Utils.BitMask.SetAt(m_nodeMask, newNodeId);
		}
		SetNodeAtFrame(GetCurrentFrame(), newNodeId, offset);

		ApplyChanges();
	}


	void MoveNode( int id, Vector2 newOffset, bool snapToPixel = true )
	{
		if ( m_playing )
			return;

		AnimFrame frame = GetCurrentFrame();
		if ( frame == null )
			return;

		Sprite sprite = GetSpriteAtTime(m_animTime);
		if ( sprite != null && snapToPixel )
		{
			// Snap offset to sprite pixels
			newOffset = new Vector2(Utils.Snap(newOffset.x, 0.5f), Utils.Snap(newOffset.y, 0.5f));
		}

		SetNodeOffsetAtFrame(frame, id, newOffset);
		m_nodePositions[id] = newOffset;

		// NB: Don't wanna apply changes while dragging, just apply on lift. So no ApplyChanges() here.
	}

	void RotateNode( int id, float newAngle )
	{
		if ( m_playing )
			return;

		AnimFrame frame = GetCurrentFrame();
		if ( frame == null )
			return;

		SetNodeAngleAtFrame(frame, id, newAngle);
		m_nodeAngles[id] = Mathf.Repeat(newAngle,360);

		// NB: Don't wanna apply changes while dragging, just apply on lift. So no ApplyChanges() here.
	}

	// Delete the node form the current frame, if there are no frames left, deletes the node
	void DeleteNode( int id )
	{
		if ( m_playing )
			return;

		// check if on other frames
		bool onOtherFrames = false;
		AnimFrame currentFrame = GetCurrentFrame();
		if ( currentFrame == null )
			return;
		foreach ( AnimFrame frame in m_frames )
		{
			if ( frame != currentFrame && frame.m_nodes != null && frame.m_nodes.Exists( item => item.m_nodeId == id && (item.m_offset != Vector2.zero || item.m_angle != 0) ) )
			{
				onOtherFrames = true;
				break;
			}				
		}
		if ( onOtherFrames == false ) 
		{
			m_nodePositions[id] = Vector2.zero;
			m_nodeAngles[id] = 0;
			m_nodeMask = Utils.BitMask.UnsetAt( m_nodeMask, id );
		}

		if (  currentFrame.m_nodes != null )
			currentFrame.m_nodes.RemoveAll( item => item.m_nodeId == id );

		ApplyChanges();

	}

	// Deletes the node from all frames
	void DeleteNodeAllFrames( int id )
	{
		foreach ( AnimFrame frame in m_frames )
		{	
			if ( frame.m_nodes != null )		
				frame.m_nodes.RemoveAll( item => item.m_nodeId == id );	
		}

		m_nodePositions[id] = Vector2.zero;
		m_nodeAngles[id] = 0;
		m_nodeMask = Utils.BitMask.UnsetAt( m_nodeMask, id );

		ApplyChanges();
	}

	#endregion
	#region Funcs: Private

	// Sets the offset and angle of a node on a particular anim frame, adding it if it didn't exist.
	void SetNodeAtFrame(AnimFrame frame, int nodeId, Vector2 offset )
	{
		Node node = FindOrCreateNodeAtFrame(frame, nodeId);
		if ( node == null )
			return;
		if ( offset == Vector2.zero )
			offset = new Vector2(0.0001f,0.0001f); // Zero nodes are stripped, so when setting to zero, actually set to a real low value
		node.m_offset = offset;
	}

	// Sets the offset of a node on a particular anim frame, adding it if it didn't exist.
	void SetNodeOffsetAtFrame(AnimFrame frame, int nodeId, Vector2 offset )
	{
		Node node = FindOrCreateNodeAtFrame(frame, nodeId);
		if ( offset == Vector2.zero )
			offset = new Vector2(0.0001f,0.0001f); // Zero nodes are stripped, so when setting to zero, actually set to a real low value
		if ( node != null )
			node.m_offset = offset;
	}

	// Sets the angle of a node on a particular anim frame, adding it if it didn't exist.
	void SetNodeAngleAtFrame(AnimFrame frame, int nodeId, float angle )
	{
		if ( frame != null )
			FindOrCreateNodeAtFrame(frame, nodeId).m_angle = Mathf.Repeat(angle,360f);
	}

	// Sets the x offset of a node on a particular anim frame, adding it if it didn't exist. Called on load
	void SetNodeXAtFrame(AnimFrame frame, int nodeId, float value )
	{
		Node node = FindOrCreateNodeAtFrame(frame, nodeId);
		// if (value == 0 && onLoad == false ) - This function is only called on load
		// 	value = 0.0001f; // Zero nodes are stripped, so when setting to zero, actually set to a real low value
		if ( node != null )
			node.m_offset = new Vector2( value, node.m_offset.y );
	}

	// Sets the y offset of a node on a particular anim frame, adding it if it didn't exist.
	void SetNodeYAtFrame(AnimFrame frame, int nodeId, float value )
	{
		Node node = FindOrCreateNodeAtFrame(frame, nodeId);
		// if (value == 0 && onLoad == false ) - This function is only called on load
		// 	value = 0.0001f; // Zero nodes are stripped, so when setting to zero, actually set to a real low value
		if ( node != null )
			node.m_offset = new Vector2( node.m_offset.x, value );
	}

	// Gets node at frame, creating it if it doesn't exist
	Node FindOrCreateNodeAtFrame(AnimFrame frame, int nodeId)
	{
		if ( frame == null )
			return null;

		Node node = null;
		if ( frame.m_nodes == null )
			frame.m_nodes = new List<Node>(1);
		else 
			node = frame.m_nodes.Find( item => item.m_nodeId == nodeId );

		if ( node == null ) 
		{	
			// Adding node first time, set it's id, and set offset/angle to whatever the cached one is (if it's set on a previous frame it'll use that).

			node = new Node() { m_nodeId = nodeId };
			frame.m_nodes.Add( node );
			// Set defaults for frame (based on previous frames)
			InitNodeValues( node, frame );
		}
		return node;
	}

	/// Read from node curves and apply to nodes
	void ReadNodesFromClip()
	{
		// Clear nodes
		m_nodeMask = 0;
		System.Array.ForEach( m_nodePositions, item=>item=Vector2.zero );
		System.Array.ForEach( m_nodeAngles, item=>item=0 );

		CreateStaticBindings();

		// Loop through all nodes
		for ( int nodeId = 0; nodeId < SpriteAnimNodes.NUM_NODES; ++nodeId )
		{
			// Add x, y and angle data in order. This means that ommitted data can be read from previous keyframe, so it doesn't need to be stored in the animation.
			// They're stored in seperate curves, so first put them in the same list and sort before reading the data from each
			List<KeyframeReadInData> keyframes = new List<KeyframeReadInData>();

			// Read data into the keyframes list, x, y, and angle
			ReadNodesToKeyFrameReadInData( NODE_BINDINGS_X, SetNodeXAtFrame, keyframes, nodeId );
			ReadNodesToKeyFrameReadInData( NODE_BINDINGS_Y, SetNodeYAtFrame, keyframes, nodeId );
			ReadNodesToKeyFrameReadInData( NODE_BINDINGS_ANGLE, SetNodeAngleAtFrame, keyframes, nodeId );

			// Sort all keyframes so we read data in correct order
			keyframes.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );

			// Store in the data into nodes of AnimFrames
			keyframes.ForEach( keyframe => keyframe.m_functionSetNodeAtFrame( GetFrameAtTime(keyframe.m_time), nodeId, keyframe.m_value ) );
		}
	}

	// Helper for ReadNodesFromClip, stores keyframes from curve into passed in keyframes list. Also sets they node mask.
	void ReadNodesToKeyFrameReadInData( EditorCurveBinding[] bindings, System.Action<AnimFrame, int, float> functionSetNodeAtFrame, List<KeyframeReadInData> keyframes, int nodeId )
	{
		AnimationCurve curve = AnimationUtility.GetEditorCurve( m_clip, bindings[nodeId] );
		if  ( curve != null )
		{
			foreach (Keyframe keyframe in curve.keys ) 
			{
				keyframes.Add( new KeyframeReadInData( functionSetNodeAtFrame, keyframe) );
			}
			if ( curve.keys.Length > 0 )
				m_nodeMask = Utils.BitMask.SetAt(m_nodeMask, nodeId);
		}
	}

	/// Create curves for nodes in the clip
	void ApplyNodeChanges()
	{
		CreateStaticBindings();

		// Create curves from nodes in the animation
		AnimationCurve[] curvesX = new AnimationCurve[SpriteAnimNodes.NUM_NODES];
		AnimationCurve[] curvesY = new AnimationCurve[SpriteAnimNodes.NUM_NODES];
		AnimationCurve[] curvesAngle = new AnimationCurve[SpriteAnimNodes.NUM_NODES];

		// Cached node values to check 
		float[] prevValueX = new float[SpriteAnimNodes.NUM_NODES];
		float[] prevValueY = new float[SpriteAnimNodes.NUM_NODES];
		float[] prevValueAngle = new float[SpriteAnimNodes.NUM_NODES];

		foreach ( AnimFrame frame in m_frames )
		{
			if ( frame.m_nodes == null )
				continue;
			foreach ( Node node in frame.m_nodes )
			{				
				AddNodeKey(ref curvesX, frame.m_time, node.m_nodeId, node.m_offset.x, ref prevValueX[node.m_nodeId], false );
				AddNodeKey(ref curvesY, frame.m_time, node.m_nodeId, node.m_offset.y, ref prevValueY[node.m_nodeId], false );
				AddNodeKey(ref curvesAngle, frame.m_time, node.m_nodeId, node.m_angle, ref prevValueAngle[node.m_nodeId], true );
			}
		}

		// Set Editor Curves - if a curve is null, the SetEditorCurve function will remove any existing curve, so that's fine.
		for ( int nodeId = 0; nodeId < SpriteAnimNodes.NUM_NODES; ++nodeId )
		{
			SetKeyModeConstant(curvesX[nodeId]);
			SetKeyModeConstant(curvesY[nodeId]);
			SetKeyModeConstant(curvesAngle[nodeId]);
			AnimationUtility.SetEditorCurve( m_clip, NODE_BINDINGS_X[nodeId], curvesX[nodeId] );
			AnimationUtility.SetEditorCurve( m_clip, NODE_BINDINGS_Y[nodeId], curvesY[nodeId] );		
			AnimationUtility.SetEditorCurve( m_clip, NODE_BINDINGS_ANGLE[nodeId], curvesAngle[nodeId] );		
		}
	}

	// Adds a node key to the curve (if the value has changed since last key)
	void AddNodeKey( ref AnimationCurve[] curves, float time, int nodeId, float value, ref float lastValue, bool angle )
	{		
		if ( Mathf.Approximately(value, lastValue) )
		{			
			// Value hasn't changed, so we dont add unnecessary keys or curves
			lastValue = value;
			return;
		}

		if ( curves[nodeId] == null )
		{
			// Curve doesn't exist yet, add it
			curves[nodeId] = new AnimationCurve();
			if ( time > 0 )
			{
				// Add zero keyframe- Perhaps this shouldn't be set?
				curves[nodeId].AddKey( 0, 0 );
			}
		}
		curves[nodeId].AddKey( time, value );

		lastValue = value;
	}

	void SetKeyModeConstant( AnimationCurve curve )
	{
		if ( curve == null )
			return;

		// Set keyframe to constant so it's not a smooth curve
		#if !UNITY_5_3

			int keys = curve.keys.Length;
			for ( int i = 0; i < keys; ++ i)
			{
				AnimationUtility.SetKeyBroken(curve, i, true);
				AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
				AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Constant);
			}

		#else

			int numKeys = curve.keys.Length;

			for ( int i = 0; i < numKeys; ++ i)
			{
				Keyframe frame = curve.keys[i];
				frame.tangentMode = KEY_MODE_CONSTANT_RIGHT_LEFT;
				frame.inTangent = float.PositiveInfinity;
				frame.outTangent = float.PositiveInfinity;
				curve.MoveKey(i, frame);
			} 
		#endif
	}

	// Creates static array of node bindings if it didn't exist yet
	void CreateStaticBindings()
	{
		if ( NODE_BINDINGS_X == null || NODE_BINDINGS_Y == null || NODE_BINDINGS_ANGLE == null )
		{
			NODE_BINDINGS_X = new EditorCurveBinding[SpriteAnimNodes.NUM_NODES];
			NODE_BINDINGS_Y = new EditorCurveBinding[SpriteAnimNodes.NUM_NODES];
			NODE_BINDINGS_ANGLE = new EditorCurveBinding[SpriteAnimNodes.NUM_NODES];

			for ( int nodeId = 0; nodeId < SpriteAnimNodes.NUM_NODES; ++nodeId )
			{
				NODE_BINDINGS_X[nodeId] = new EditorCurveBinding() { type = TYPE_SPRITEANIMNODES, propertyName = string.Format(PROPERTY_NODE_X, nodeId), path = m_spritePath };
				NODE_BINDINGS_Y[nodeId] = new EditorCurveBinding() { type = TYPE_SPRITEANIMNODES, propertyName = string.Format(PROPERTY_NODE_Y, nodeId), path = m_spritePath };
				NODE_BINDINGS_ANGLE[nodeId] = new EditorCurveBinding() { type = TYPE_SPRITEANIMNODES, propertyName = string.Format(PROPERTY_NODE_ANGLE, nodeId), path = m_spritePath };
			}
		}
	}

	int GetUnusedNodeId()
	{
		for ( int i = 0; i < SpriteAnimNodes.NUM_NODES; ++i )
		{
			if ( Utils.BitMask.IsSet(m_nodeMask, i) == false )
				return i;
		}
		return -1;
	}

	// Updates the current frame's node pos/rot, if node isn't overriden on current frame it's set to last set position
	void UpdateCurrentFrameNodePositions()
	{
		// NB:  Could cashe stuff to do this more efficiently than looping through every node every frame.
		int currFrame = GetCurrentFrameId();
		if ( currFrame < 0 )
			return;

		// Work backwards through nodes to find the latest node with a value
		for ( int nodeId = 0; nodeId < SpriteAnimNodes.NUM_NODES; ++nodeId )
		{	
			m_nodePositions[nodeId] = Vector2.zero;
			m_nodeAngles[nodeId] = 0;

			for ( int frameId = currFrame; frameId >= 0; --frameId )
			{
				Node node = null;
				AnimFrame frame = m_frames[frameId];
				if ( frame.m_nodes != null )
					node = frame.m_nodes.Find(item=>item.m_nodeId == nodeId);
									
				if ( node != null )
				{
					m_nodePositions[nodeId] = node.m_offset;
					m_nodeAngles[nodeId] = Mathf.Repeat(node.m_angle, 360f);
					break; // brake from inner loop
				}
			}
		}
	}

	// Sets node values based on what nodes have on previous frames
	void InitNodeValues( Node node, AnimFrame frame )
	{
		// Work backwards to find last node
		for ( int frameId = m_frames.FindIndex(item=>item == frame)-1; frameId >= 0; --frameId )
		{
			Node prevFrameNode = null;
			AnimFrame prevFrame = m_frames[frameId];
			if ( prevFrame.m_nodes != null )
				prevFrameNode = prevFrame.m_nodes.Find(item=>item.m_nodeId == node.m_nodeId);
			if ( prevFrameNode != null )
			{
				node.m_offset = prevFrameNode.m_offset;
				node.m_angle = prevFrameNode.m_angle;
				break;
			}
		}
	}

	#endregion
}

}

