//-----------------------------------------
//          PowerSprite Animator
//  Copyright © 2017 Powerhoof Pty Ltd
//			  powerhoof.com
//----------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PowerTools.Anim;

namespace PowerTools
{

public partial class SpriteAnimator
{
	#region Definitions

	static readonly float TIMELINE_SCRUBBER_HEIGHT = 16;
	static readonly float TIMELINE_EVENT_HEIGHT = 12;
	static readonly float TIMELINE_BOTTOMBAR_HEIGHT = 24;
	static readonly float TIMELINE_OFFSET_MIN = -10;

    static readonly float SCRUBBER_INTERVAL_TO_SHOW_LABEL = 60.0f;
    static readonly float SCRUBBER_INTERVAL_WIDTH_MIN = 10.0f;
    static readonly float SCRUBBER_INTERVAL_WIDTH_MAX = 80.0f;

	static readonly Color COLOR_UNITY_BLUE = new Color(0.3f,0.5f,0.85f,1);


	static readonly Color COLOR_INSERT_FRAMES_LINE = COLOR_UNITY_BLUE;
	static readonly Color COLOR_EVENT_BAR_BG = new Color(0.2f,0.2f,0.2f);
	static readonly Color COLOR_EVENT_LABEL_BG = (COLOR_EVENT_BAR_BG*0.8f) + (Color.grey * 0.2f);	// Fake alpha'd look while stillmasking things behind it
	static readonly Color COLOR_EVENT_LABEL_BG_SELECTED = (COLOR_EVENT_BAR_BG*0.8f) + (COLOR_UNITY_BLUE * 0.2f);

	static readonly float FRAME_RESIZE_RECT_WIDTH = 8;

	//static readonly float EVENT_WIDTH = 2;
	static readonly float EVENT_CLICK_OFFSET = -2;
	static readonly float EVENT_CLICK_WIDTH = 10;

	enum eDragState
	{
		None,
		Scrub,
		ResizeFrame,
		MoveFrame,
		SelectFrame,
		MoveEvent,
		SelectEvent,
		MoveNode,
		RotateNode
	}

	#endregion
	#region Vars: Private

	[SerializeField] List<AnimEvent> m_acListAll = new List<AnimEvent>();
	List<AnimEvent> m_acList = new List<AnimEvent>();
	string m_acRemaining = string.Empty;
	int m_acSelectedIndex = 0;
	bool m_acCanceled = false;

	int m_resizeFrameId = 0;
	float m_selectionMouseStart = 0;
	float m_timelineEventBarHeight = TIMELINE_EVENT_HEIGHT;

	#endregion
	#region Funcs: Init


	#endregion
	#region Funcs: Layout


	void LayoutTimeline( Rect rect )
	{
		Event e = Event.current;


		// Handle autocomplete
		bool doAutoComplete = false;

		if ( focusedWindow == this && m_selectedEvents.Count == 1 && GUI.GetNameOfFocusedControl() == "EventFunctionName" )
		{
			if ( e.type == EventType.KeyDown )
			{
				if ( HasAutoComplete() && (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow) )
				{
					// Select autocomplete line
					m_acSelectedIndex += e.keyCode == KeyCode.UpArrow ? -1 : 1;
					m_acSelectedIndex = Mathf.Clamp(m_acSelectedIndex,0,m_acList.Count-1);
					e.Use();
				}
				else if ( HasAutoComplete() && (e.keyCode == KeyCode.Delete) )
				{
					// Remove selected autocomplete line
					if ( m_acSelectedIndex >=0 && m_acSelectedIndex < m_acList.Count )
					{
						m_acListAll.RemoveAll(item=>item.m_functionName == m_acList[m_acSelectedIndex].m_functionName);
						ClearAutoComplete();
					}
					e.Use();
				}
				else if ( e.control == false && e.keyCode == KeyCode.None && HasAutoComplete() && (e.character == '\n' || e.character == '\t'))				
				{
					// Do Autocomplete
					doAutoComplete = true;
					//e.Use();
				}
				else if ( HasAutoComplete() && e.keyCode == KeyCode.Escape )
				{
					// Cancel autocomplete
					m_acCanceled = true;
					e.Use();
				}
			}
		}
		else if ( e.type != EventType.Layout )
		{
			//Debug.Log("Control: "+GUI.GetNameOfFocusedControl()+" evnts: "+m_selectedEvents.Count.ToString() );
			ClearAutoComplete();
		}

		// Store mouse x offset when ever button is pressed for selection box
		if ( m_dragState == eDragState.None && e.rawType == EventType.MouseDown && e.button == 0 )
		{
			m_selectionMouseStart = e.mousePosition.x;
		}

		// Select whatever's in the selection box
		if ( (m_dragState == eDragState.SelectEvent || m_dragState == eDragState.SelectFrame) && e.rawType == EventType.MouseDrag && e.button == 0 )
		{			
			float dragTimeStart = GuiPosToAnimTime(rect, m_selectionMouseStart);
			float dragTimeEnd = GuiPosToAnimTime(rect, e.mousePosition.x);			
			if ( dragTimeStart > dragTimeEnd )
					Utils.Swap( ref dragTimeStart, ref dragTimeEnd );
			if ( m_dragState == eDragState.SelectEvent )
			{
				m_selectedEvents = m_events.FindAll( animEvent => animEvent.m_time >= dragTimeStart && animEvent.m_time <= dragTimeEnd );
				m_selectedEvents.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );
			}
			else 
			{
				m_selectedFrames = m_frames.FindAll( frame => frame.m_time+frame.m_length >= dragTimeStart && frame.m_time <= dragTimeEnd );	
				m_selectedFrames.Sort( (a,b) => a.m_time.CompareTo(b.m_time) );
			}

			GUI.FocusControl("none");
		}

        m_timelineScale = Mathf.Clamp( m_timelineScale, 10, 10000 );

        //
        // Update timeline offset
        //
        m_timelineAnimWidth = m_timelineScale * GetAnimLength();
        if ( m_timelineAnimWidth > rect.width/2.0f ) 
        {
			m_timelineOffset = Mathf.Clamp( m_timelineOffset, rect.width - m_timelineAnimWidth - rect.width/2.0f, -TIMELINE_OFFSET_MIN );
        }
        else 
        {
			m_timelineOffset = -TIMELINE_OFFSET_MIN;
        }

        //
        // Layout stuff
		//
		// Draw scrubber bar
		float elementPosY = rect.yMin;
		float elementHeight = TIMELINE_SCRUBBER_HEIGHT;
		LayoutScrubber( new Rect(rect) { yMin = elementPosY, height = elementHeight } );
		elementPosY += elementHeight;

		// Draw frames
		elementHeight = rect.height - ((elementPosY - rect.yMin) + m_timelineEventBarHeight + TIMELINE_BOTTOMBAR_HEIGHT);
		Rect rectFrames = new Rect(rect) { yMin = elementPosY, height = elementHeight };
		LayoutFrames(rectFrames);
		elementPosY += elementHeight;

		// Draw events bar background
		elementHeight = m_timelineEventBarHeight;
		LayoutEventsBarBack( new Rect(rect) { yMin = elementPosY, height = elementHeight } );

		// Draw playhead (in front of events bar background, but behind events
		LayoutPlayhead(new Rect(rect) { height = rect.height - TIMELINE_BOTTOMBAR_HEIGHT } );

		// Draw events bar
		elementHeight = m_timelineEventBarHeight;
		LayoutEvents( new Rect(rect) { yMin = elementPosY, height = elementHeight } );
		elementPosY += elementHeight;

		// Draw bottom
		elementHeight = TIMELINE_BOTTOMBAR_HEIGHT;
		LayoutBottomBar( new Rect(rect) { yMin = elementPosY, height = elementHeight } );

		if ( e.type == EventType.Repaint )
			LayoutAutoCompleteList(new Rect(rect) { xMin = rect.x+65, yMin = elementPosY });

		// Draw Frame Reposition
		LayoutMoveFrame(rectFrames);

		// Draw Insert
		LayoutInsert(rectFrames);

		//
		// Handle events
		//

		if ( rect.Contains( e.mousePosition ) )
		{
			
			if ( e.type == EventType.ScrollWheel )
			{
				float scale = 10000.0f;
				while ( (m_timelineScale/scale) < 1.0f || (m_timelineScale/scale) > 10.0f ) {
                    scale /= 10.0f;
                }
                				
                float oldCursorTime = GuiPosToAnimTime(rect, e.mousePosition.x);

				m_timelineScale -= e.delta.y * scale * 0.05f;
				m_timelineScale = Mathf.Clamp(m_timelineScale,10.0f,10000.0f);

				// Offset to time at old cursor pos is same as at new position (so can zoom in/out of current cursor pos)
				m_timelineOffset += ( e.mousePosition.x - AnimTimeToGuiPos( rect, oldCursorTime ) );

				Repaint();
				e.Use();
			}
			else if ( e.type == EventType.MouseDrag ) 
			{
				if (  e.button == 1 || e.button == 2 )
				{					
					m_timelineOffset += e.delta.x;
					Repaint();
					e.Use();
				}
			}
		}


		if ( e.rawType == EventType.MouseUp && e.button == 0 && ( m_dragState == eDragState.SelectEvent || m_dragState == eDragState.SelectFrame ) )
		{
			m_dragState = eDragState.None;
			Repaint();
		}

		if ( doAutoComplete )
		{
			AnimEvent autoCompleteEvent = DoAutoComplete(m_selectedEvents[0].m_functionName);
			if ( autoCompleteEvent != null )
			{	
				AnimEvent selectedEvent = m_selectedEvents[0];

				// Copy data we want from the auto-complete event
				selectedEvent.m_functionName = autoCompleteEvent.m_functionName;
				selectedEvent.m_paramType = autoCompleteEvent.m_paramType;
				selectedEvent.m_paramInt = autoCompleteEvent.m_paramInt;
				selectedEvent.m_paramFloat = autoCompleteEvent.m_paramFloat;
				selectedEvent.m_paramString = autoCompleteEvent.m_paramString;
				//selectedEvent.m_paramObjectReference = autoCompleteEvent.m_paramObjectReference; // Don't think we want this?
				selectedEvent.m_messageOptions = autoCompleteEvent.m_messageOptions;
				selectedEvent.m_sendUpwards = autoCompleteEvent.m_sendUpwards;
				selectedEvent.m_usePrefix = autoCompleteEvent.m_usePrefix;

				// Replace with current event so any changes will be reflected.
				int listIndexToReplace =  m_acListAll.FindIndex(item => item == autoCompleteEvent);
				m_acListAll[listIndexToReplace] = selectedEvent;
			}
			ClearAutoComplete();
			Repaint();
			ApplyChanges();
		}

	}

	void LayoutScrubber(Rect rect )
	{

        //
        // Calc time scrubber lines
        //
		float minUnitSecond = 1.0f/m_clip.frameRate;		
        float curUnitSecond = 1.0f;
        float curCellWidth = m_timelineScale;
        int intervalId;
		List<int> intervalScales = CreateIntervalSizeList(out intervalId);

        // get curUnitSecond and curIdx
        if ( curCellWidth < SCRUBBER_INTERVAL_WIDTH_MIN ) 
        {
            while ( curCellWidth < SCRUBBER_INTERVAL_WIDTH_MIN ) 
            {
                curUnitSecond = curUnitSecond * intervalScales[intervalId];
                curCellWidth = curCellWidth * intervalScales[intervalId];

                intervalId += 1;
                if ( intervalId >= intervalScales.Count ) 
                {
                    intervalId = intervalScales.Count - 1;
                    break;
                }
            }
        }
        else if ( curCellWidth > SCRUBBER_INTERVAL_WIDTH_MAX ) 
        {
            while ( (curCellWidth > SCRUBBER_INTERVAL_WIDTH_MAX) && 
                    (curUnitSecond > minUnitSecond) ) 
            {
                intervalId -= 1;
                if ( intervalId < 0 ) 
                {
                    intervalId = 0;
                    break;
                }

                curUnitSecond = curUnitSecond / intervalScales[intervalId];
                curCellWidth = curCellWidth / intervalScales[intervalId];
            }
        }

        // check if prev width is good to show
        if ( curUnitSecond > minUnitSecond ) 
        {
            int intervalIdPrev = intervalId - 1;
            if ( intervalIdPrev < 0 )
                intervalIdPrev = 0;
            float prevCellWidth = curCellWidth / intervalScales[intervalIdPrev];
            float prevUnitSecond = curUnitSecond / intervalScales[intervalIdPrev];
            if ( prevCellWidth >= SCRUBBER_INTERVAL_WIDTH_MIN ) {
                intervalId = intervalIdPrev;
                curUnitSecond = prevUnitSecond;
                curCellWidth = prevCellWidth;
            }
        }

        // get lod interval list
        int[] lodIntervalList = new int[intervalScales.Count+1];
        lodIntervalList[intervalId] = 1;
        for ( int i = intervalId-1; i >= 0; --i ) 
        {
            lodIntervalList[i] = lodIntervalList[i+1] / intervalScales[i];
        }
        for ( int i = intervalId+1; i < intervalScales.Count+1; ++i ) 
        {
            lodIntervalList[i] = lodIntervalList[i-1] * intervalScales[i-1];
        }

        // Calc width of intervals
        float[] lodWidthList = new float[intervalScales.Count+1];
        lodWidthList[intervalId] = curCellWidth;
        for ( int i = intervalId-1; i >= 0; --i ) 
        {
            lodWidthList[i] = lodWidthList[i+1] / intervalScales[i];
        }
        for ( int i = intervalId+1; i < intervalScales.Count+1; ++i ) 
        {
            lodWidthList[i] = lodWidthList[i-1] * intervalScales[i-1];
        }

        // Calc interval id to start from
        int idxFrom = intervalId;
        for ( int i = 0; i < intervalScales.Count+1; ++i ) 
        {
            if ( lodWidthList[i] > SCRUBBER_INTERVAL_WIDTH_MAX ) 
            {
                idxFrom = i;
                break;
            }
        }

        // NOTE: +50 here can avoid us clip text so early 
        int iStartFrom = Mathf.CeilToInt( -(m_timelineOffset + 50.0f)/curCellWidth );
        int cellCount = Mathf.CeilToInt( (rect.width - m_timelineOffset)/curCellWidth );

        // draw the scrubber bar
		GUI.BeginGroup(rect, EditorStyles.toolbar);

        for ( int i = iStartFrom; i < cellCount; ++i ) 
        {
            float x = m_timelineOffset + i * curCellWidth + 1;
            int idx = idxFrom;

            while ( idx >= 0 ) 
            {
                if ( i % lodIntervalList[idx] == 0 ) 
                {
                    float heightRatio = 1.0f - (lodWidthList[idx] / SCRUBBER_INTERVAL_WIDTH_MAX);

                    // draw scrubber bar
                    if ( heightRatio >= 1.0f ) 
                    {                           
                        DrawLine ( new Vector2(x, 0 ), 
                                   new Vector2(x, TIMELINE_SCRUBBER_HEIGHT), 
                                   Color.gray); 
                        DrawLine ( new Vector2(x+1, 0 ), 
                                   new Vector2(x+1, TIMELINE_SCRUBBER_HEIGHT), 
                                   Color.gray);
                    }
                    else
                    {
						DrawLine ( new Vector2(x, TIMELINE_SCRUBBER_HEIGHT * heightRatio ), 
                                   new Vector2(x, TIMELINE_SCRUBBER_HEIGHT ), 
                                   Color.gray);
                    }

                    // draw lable
                    if ( lodWidthList[idx] >= SCRUBBER_INTERVAL_TO_SHOW_LABEL ) 
                    {
						GUI.Label ( new Rect( x + 4.0f, -2, 50, 15 ), 
							ToTimelineLabelString(i*curUnitSecond, m_clip.frameRate), EditorStyles.miniLabel );
                    }

                    //
                    break;
                }
                --idx;
            }
        }

        GUI.EndGroup();

        //
        // Scrubber events
        //

		Event e = Event.current;
		if ( rect.Contains( e.mousePosition ) )
		{
			if ( e.type == EventType.MouseDown ) 
			{
				if ( e.button == 0 )
				{
					m_dragState = eDragState.Scrub;
					m_animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
					GUI.FocusControl("none");
					e.Use();
				}
			}
		}
		if ( m_dragState == eDragState.Scrub && e.button == 0 )
		{
			if ( e.type == EventType.MouseDrag )
			{
				m_animTime = GuiPosToAnimTime(rect, e.mousePosition.x);
				e.Use();
			}
			else if ( e.type == EventType.MouseUp )
			{
				m_dragState = eDragState.None;
				e.Use();
			}
		}
	}

	void LayoutEventsBarBack(Rect rect)
	{

		GUI.BeginGroup(rect, Styles.TIMELINE_KEYFRAME_BG);
		GUI.EndGroup();
	}

	void LayoutEvents(Rect rect)
	{
		Event e = Event.current;
		if ( e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) && m_playing == false )
		{
			// Move timeline
			m_animTime = SnapTimeToFrameRate(GuiPosToAnimTime(rect, e.mousePosition.x));	
		}
		if ( e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2 && rect.Contains(e.mousePosition) )
		{
			// Double click for new event at that time
		
			// New event
			InsertEvent(GuiPosToAnimTime(rect,e.mousePosition.x), true);
			e.Use();			
		}

		GUI.BeginGroup(rect);

		if ( m_events.Count == 0 )
		{
			GUI.Label(new Rect(0,0,rect.width,rect.height),"Double click to insert event", EditorStyles.centeredGreyMiniLabel);	
		}

		// Layout events. This is done in 4 stages so that selected items are drawn on top of (after) non-selected ones, but have their gui events handled first.

		// Calc some metadata about each event (start/end position on timeline, etc). This is stored in a temporary array, parallel to events
		AnimEventLayoutData[] eventTimelineData = new AnimEventLayoutData[m_events.Count];

		// First loop over and calculate start/end positions of events
		for ( int i = 0; i < m_events.Count; ++i )
		{
			AnimEvent animEvent = m_events[i];
			AnimEventLayoutData eventData = new AnimEventLayoutData();
			eventTimelineData[i] = eventData;

			eventData.start = AnimTimeToGuiPos(rect, SnapTimeToFrameRate( animEvent.m_time ) );
			eventData.text = animEvent.m_functionName;//.Replace(ANIM_EVENT_PREFIX,null);
			eventData.textWidth = Styles.TIMELINE_EVENT_TEXT.CalcSize(new GUIContent(eventData.text)).x;
			eventData.end = eventData.start + eventData.textWidth + 4;
			eventData.selected = m_selectedEvents.Contains(m_events[i]);
		}

		int maxEventOffset = 0;

		// Now loop over events and calculate the vertical offset of events so that they don't overlap
		for ( int i = 0; i < m_events.Count; ++i )
		{
			// Store the offset of everything we're overlapping with in a mask, so we can get the first available offset.
			int usedOffsetsMask = 0;
			AnimEventLayoutData data = eventTimelineData[i];
			for ( int j = i-1; j >= 0; --j )
			{
				// check for overlap of items before this one. A
				AnimEventLayoutData other = eventTimelineData[j];
				if ( (data.start > other.end || data.end < other.start) == false )
				{
					// overlaps!
					usedOffsetsMask |= 1<<(other.heightOffset);
				}
			}

			// Loop through mask to find first available offset.
			while ( data.heightOffset < 32 && (usedOffsetsMask & (1 << data.heightOffset) ) != 0 )
			{
				data.heightOffset++;
			}

			if ( data.heightOffset > maxEventOffset )
				maxEventOffset = data.heightOffset;
		}

		// Draw vertical lines where there's an event
		for ( int i = 0; i < m_events.Count; ++i )
		{
			DrawRect( new Rect( eventTimelineData[i].start, 0, 1, rect.height), Color.grey );
		}

		// First draw events
		for ( int i = 0; i < m_events.Count; ++i )
		{
			LayoutEvent(rect, m_events[i], eventTimelineData[i], true );
		}

		// Then handle gui events in reverse order
		for (int i = m_events.Count - 1; i >= 0; --i)
		{
			LayoutEvent(rect, m_events[i], eventTimelineData[i], false );
		}

		GUI.EndGroup();


		// Draw selection rect
		if ( m_dragState == eDragState.SelectEvent && Mathf.Abs(m_selectionMouseStart-e.mousePosition.x) > 1.0f  )
		{
			// Draw selection rect
			Rect selectionRect = new Rect(rect){ xMin = Mathf.Min(m_selectionMouseStart, e.mousePosition.x), xMax = Mathf.Max(m_selectionMouseStart, e.mousePosition.x) };
			DrawRect(selectionRect,COLOR_UNITY_BLUE.WithAlpha(0.1f),COLOR_UNITY_BLUE.WithAlpha(0.6f));			
		}

		// Check for unhandled mouse left click. It should deselect any selected events
		if ( e.type == EventType.MouseDown && e.button == 0  )
		{		
			if ( rect.Contains(e.mousePosition) )
			{
				ClearSelection();
				e.Use();
			}
		}

		// Check for unhanlded drag, it should start a select
		if ( m_dragState == eDragState.None && e.type == EventType.MouseDrag && e.button == 0  )
		{		
			if ( rect.Contains(e.mousePosition) )
			{
				m_dragState = eDragState.SelectEvent;
				e.Use();
			}
		}

		if ( m_dragState == eDragState.MoveEvent )
		{
			// While moving frame, show the move cursor
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.MoveArrow );
			if ( e.type == EventType.MouseDrag )
			{
				MoveSelectedEvents(e.delta.x);

				// move the frame
				e.Use();
			}
			if ( e.rawType == EventType.MouseUp && e.button == 0 )
			{
				// Apply the move change
				ApplyChanges();
				m_dragState = eDragState.None;
				e.Use();
			}
		}

		float newTimelineHeight = Mathf.Max( (maxEventOffset+1), 1.5f ) * TIMELINE_EVENT_HEIGHT;
		if ( newTimelineHeight != m_timelineEventBarHeight )
		{
			m_timelineEventBarHeight = newTimelineHeight;
			Repaint();
		}

	}

	// NB: if draw is true, it'll draw stuff only, otherwise it'll handle events only.
	void LayoutEvent(Rect rect, AnimEvent animEvent, AnimEventLayoutData layoutData, bool draw )
	{	
		// check if it's visible on timeline
		if ( layoutData.start > rect.xMax || layoutData.end < rect.xMin )
			return;

		float heightOffset = (layoutData.heightOffset*TIMELINE_EVENT_HEIGHT);
		Rect eventRect = new Rect( layoutData.start, heightOffset, 0, TIMELINE_EVENT_HEIGHT);
		Rect labelRect = new Rect(layoutData.start + 2, heightOffset, layoutData.textWidth, TIMELINE_EVENT_HEIGHT-2);

		if ( draw )
			LayoutEventVisuals( animEvent, layoutData.selected, eventRect, layoutData.text, labelRect );
		else 
			LayoutEventGuiEvents( rect, animEvent, layoutData.selected, eventRect, layoutData.text, labelRect );
	}


	// draws visuals for an event
	void LayoutEventVisuals(AnimEvent animEvent, bool selected, Rect eventRect, string labelText, Rect labelRect )
	{	
		// Color differently if selected
		Color eventColor = selected ? COLOR_UNITY_BLUE : Color.grey;
		Color eventBGColor = selected ? COLOR_EVENT_LABEL_BG_SELECTED : COLOR_EVENT_LABEL_BG;

		//DrawRect( new Rect(eventRect) { width = EVENT_WIDTH }, eventColor  );
		DrawRect( labelRect, eventBGColor);
		GUI.Label( new Rect( labelRect ){ yMin = labelRect.yMin-4, height = TIMELINE_EVENT_HEIGHT + 5}, labelText, new GUIStyle( Styles.TIMELINE_EVENT_TEXT ) { normal = { textColor = eventColor } } ); 
		if ( selected ) GUI.color = COLOR_UNITY_BLUE;
		GUI.Box( new Rect(eventRect.xMin-2 ,eventRect.yMin,6,20), Contents.EVENT_MARKER, new GUIStyle( Styles.TIMELINE_EVENT_TICK ) { normal = { textColor = eventColor } }  );
		GUI.color = Color.white;
	}

	// Handles gui events for an event
	void LayoutEventGuiEvents(Rect rect, AnimEvent animEvent, bool selected, Rect eventRect, string labelText, Rect labelRect )
	{	
		Rect eventClickRect = new Rect(eventRect) { xMin = eventRect.xMin + EVENT_CLICK_OFFSET, width = EVENT_CLICK_WIDTH };

		//
		// Frame clicking events
		//

		Event e = Event.current;
		bool mouseContained = eventClickRect.Contains(e.mousePosition) || labelRect.Contains(e.mousePosition); // Can click on either label or event bar

		// Move cursor (when selected, it can be dragged to move it)	
		EditorGUIUtility.AddCursorRect( eventClickRect, MouseCursor.MoveArrow );

		if ( m_dragState == eDragState.None && mouseContained && e.button == 0 )
		{
			//
			// Handle Event Selection
			//
			if ( (selected == false || e.control == true) && e.type == EventType.MouseDown )
			{
				// Started clicking unselected - start selecting
				SelectEvent(animEvent);
				GUI.FocusControl("none");
				e.Use();
			}

			if ( selected && e.control == false && m_selectedEvents.Count > 1 && e.type == EventType.MouseUp )
			{
				// Had multiple selected, and clicked on just one, deselect others. Done on mouse up so we can start the drag if we want
				SelectEvent(animEvent);
				GUI.FocusControl("none");
				e.Use();
			}

			if ( selected && e.type == EventType.MouseDown )
			{
				GUI.FocusControl("none");
				// Clicked alredy selected item, consume event so it doesn't get deseleccted when starting a move
				e.Use();
			}

			//
			// Handle start move frame drag
			//
			if ( e.type == EventType.MouseDrag )
			{
				m_dragState = eDragState.MoveEvent;
			}


		}
	}


	void LayoutFrames(Rect rect)
	{
		Event e = Event.current;

		GUI.BeginGroup(rect, Styles.TIMELINE_ANIM_BG);

		//DrawRect( new Rect(0,0,rect.width,rect.height), new Color(0.3f,0.3f,0.3f,1));

		for ( int i = 0; i < m_frames.Count; ++i ) // NB: ignore final dummy keyframe
		{
			// Calc time of next frame
			LayoutFrame(rect, i, m_frames[i].m_time, m_frames[i].EndTime, (Sprite)m_frames[i].m_sprite);
		}

		// Draw rect over area that has no frames in it
		if ( m_timelineOffset > 0 )
		{			
			// Before frames start
			DrawRect( new Rect(0,0,m_timelineOffset,rect.height), new Color(0.4f,0.4f,0.4f,0.2f) );
			DrawLine( new Vector2(m_timelineOffset,0), new Vector2(m_timelineOffset,rect.height), new Color(0.4f,0.4f,0.4f) );
		}
		float endOffset = m_timelineOffset + (GetAnimLength() * m_timelineScale);
		if ( endOffset < rect.xMax )
		{
			// After frames end
			DrawRect( new Rect(endOffset,0,rect.width-endOffset,rect.height), new Color(0.4f,0.4f,0.4f,0.2f) );
		}

		GUI.EndGroup();

		// Draw selection rect
		if ( m_dragState == eDragState.SelectFrame && Mathf.Abs(m_selectionMouseStart-e.mousePosition.x) > 1.0f )
		{
			// Draw selection rect
			Rect selectionRect = new Rect(rect){ xMin = Mathf.Min(m_selectionMouseStart, e.mousePosition.x), xMax = Mathf.Max(m_selectionMouseStart, e.mousePosition.x) };
			DrawRect(selectionRect,COLOR_UNITY_BLUE.WithAlpha(0.1f),COLOR_UNITY_BLUE.WithAlpha(0.6f));			
		}

		if ( m_dragState == eDragState.None )
		{
			//
			// Check for unhandled mouse left click. It should deselect any selected frames
			//
			if ( e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) )
			{				
				ClearSelection();
				e.Use();
			}
			// Check for unhanlded drag, it should start a select
			if ( e.type == EventType.MouseDrag && e.button == 0 && rect.Contains(e.mousePosition) )
			{		
				m_dragState = eDragState.SelectFrame;
				e.Use();				
			}
		}
		else if ( m_dragState == eDragState.ResizeFrame )
		{
			// While resizing frame, show the resize cursor
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.ResizeHorizontal );
		}
		else if ( m_dragState == eDragState.MoveFrame )
		{
			// While moving frame, show the move cursor
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.MoveArrow );
		}
	}

	void LayoutFrame( Rect rect, int frameId, float startTime, float endTime, Sprite sprite )
	{
		float startOffset = m_timelineOffset + (startTime * m_timelineScale);
		float endOffset = m_timelineOffset + (endTime * m_timelineScale);

		// check if it's visible on timeline
		if ( startOffset > rect.xMax || endOffset < rect.xMin )
			return;
		AnimFrame animFrame = m_frames[frameId];
		Rect frameRect = new Rect(startOffset,0, endOffset-startOffset, rect.height);
		bool selected = m_selectedFrames.Contains(animFrame);
		if ( selected )
		{
			// highlight selected frames
			DrawRect( frameRect, Color.grey.WithAlpha(0.3f) );
		}
		DrawLine( new Vector2(endOffset,0), new Vector2(endOffset,rect.height), new Color(0.4f,0.4f,0.4f) );
		LayoutTimelineSprite( frameRect, GetSpriteAtTime(startTime) );

		//
		// Frame clicking events
		//

		Event e = Event.current;

		if ( m_dragState == eDragState.None )
		{
			// Move cursor (when selected, it can be dragged to move it)
			if ( selected )
			{	
				EditorGUIUtility.AddCursorRect( new Rect(frameRect) { xMin = frameRect.xMin+FRAME_RESIZE_RECT_WIDTH*0.5f, xMax = frameRect.xMax-FRAME_RESIZE_RECT_WIDTH*0.5f }, MouseCursor.MoveArrow );
			}

			//
			// Resize rect
			//
			Rect resizeRect = new Rect(endOffset-(FRAME_RESIZE_RECT_WIDTH*0.5f),0,FRAME_RESIZE_RECT_WIDTH,rect.height);
			EditorGUIUtility.AddCursorRect( resizeRect, MouseCursor.ResizeHorizontal );

			//
			// Check for Start Resizing frame
			//
			if ( e.type == EventType.MouseDown && e.button == 0 && resizeRect.Contains(e.mousePosition) )
			{
				// Start resizing the frame
				m_dragState = eDragState.ResizeFrame;
				m_resizeFrameId = frameId;
				GUI.FocusControl("none");
				e.Use();
			}

			//
			// Handle Frame Selection
			//
			if ( selected == false && e.type == EventType.MouseDown && e.button == 0 && frameRect.Contains(e.mousePosition) )
			{
				// Started clicking unselected - start selecting
				m_dragState = eDragState.SelectFrame;
				SelectFrame(animFrame);
				GUI.FocusControl("none");
				e.Use();
			}

			if ( selected == true && m_selectedFrames.Count > 1 && e.type == EventType.MouseUp && e.button == 0 &&  frameRect.Contains(e.mousePosition) )
			{
				// Had multiple selected, and clicked on just one, deselect others
				SelectFrame(animFrame);
				e.Use();
			}

			//
			// Handle start move frame drag (once selected)
			//
			if ( selected && e.type == EventType.MouseDrag && e.button == 0 && frameRect.Contains(e.mousePosition) )
			{
				m_dragState = eDragState.MoveFrame;
				e.Use();
			}
			if ( selected && e.type == EventType.MouseDown && e.button == 0 && frameRect.Contains(e.mousePosition) )
			{
				// Clicked alredy selected item, consume event so it doesn't get deseleccted when starting a move
				GUI.FocusControl("none");
				e.Use();
			}
		}
		else if ( m_dragState == eDragState.ResizeFrame )
		{
			// Check for resize frame by dragging mouse
			if ( e.type == EventType.MouseDrag && e.button == 0 && m_resizeFrameId == frameId )
			{	

				if ( selected && m_selectedFrames.Count > 1 )
				{					
					// Calc frame end if adding a frame to each selected frame.
					float currEndTime = animFrame.m_time + animFrame.m_length;
					float newEndTime = animFrame.m_time + animFrame.m_length;
					float mouseTime = GuiPosToAnimTime(new Rect(0,0,position.width,position.height), e.mousePosition.x);
					float direction = Mathf.Sign(mouseTime - currEndTime);
					for ( int i = 0; i < m_selectedFrames.Count; ++i )
					{												
						if ( m_selectedFrames[i].m_time <= animFrame.m_time || i == frameId )
							newEndTime += GetMinFrameTime() * direction;
					}
					// if mouse time is closer to newEndTime than currEnd time then commit the change
					if ( Mathf.Abs(mouseTime-newEndTime) < Mathf.Abs(mouseTime-currEndTime) )
					{						
						for ( int i = 0; i < m_selectedFrames.Count; ++i )
						{
							m_selectedFrames[i].m_length = Mathf.Max(GetMinFrameTime(), SnapTimeToFrameRate(m_selectedFrames[i].m_length + (GetMinFrameTime() * direction)));							
						}	
						RecalcFrameTimes();
					}
				}
				else 
				{

					float newFrameLength = GuiPosToAnimTime(new Rect(0,0,position.width,position.height), e.mousePosition.x) - startTime;
					newFrameLength = Mathf.Max(newFrameLength, 1.0f / m_clip.frameRate);
					SetFrameLength(frameId, newFrameLength);
				}

				e.Use();
				Repaint();				
			}

			// Check for finish resizing frame
			if ( e.type == EventType.MouseUp && e.button == 0 && m_resizeFrameId == frameId)
			{
				m_dragState = eDragState.None;
				ApplyChanges();
				e.Use();			
			}
		}
		else if ( m_dragState == eDragState.SelectFrame )
		{
			if ( e.type == EventType.MouseUp && e.button == 0 )
			{
				m_dragState = eDragState.None;
				e.Use();
			}
		}
	}

	void LayoutTimelineSprite( Rect rect, Sprite sprite )
	{
		if ( sprite == null )
			return;

		float scale = 0.85f;
		// Choose recct to scale with, can't use the texture rect in some cases or unity asserts 
		Rect timelineSpriteRect = (sprite.packed && Application.isPlaying) ? sprite.rect : sprite.textureRect;
		if ( timelineSpriteRect.width > 0 && timelineSpriteRect.height > 0 )
		{
			float widthScaled = rect.width / timelineSpriteRect.width;
			float heightScaled = rect.height / timelineSpriteRect.height;
			// Finds best fit for timeline window based on sprite size
			if ( widthScaled < heightScaled)
			{
				scale *= rect.width / timelineSpriteRect.width;
			}
			else 
			{
				scale *= rect.height / timelineSpriteRect.height;
			}
		}

		LayoutFrameSprite(rect, sprite, scale, Vector2.zero, true, false );
	}


	void LayoutBottomBar(Rect rect)
	{		
		// Give bottom bar a min width
		rect = new Rect(rect) { width = Mathf.Max(rect.width, 655) };
		GUI.BeginGroup(rect, Styles.TIMELINE_BOTTOMBAR_BG);

		// Offset internal content
		rect = new Rect(rect) { height = rect.height-6, y = rect.y+3 };
		GUI.BeginGroup(new Rect(rect){ y = 3 });

		// offset content vertically a bit

		if ( m_selectedEvents.Count == 1 )
		{
			// Event data editor
			LayoutBottomBarEventData(rect);

		}
		else if ( m_selectedFrames.Count == 1 )
		{
			// Animation Frame data editor
			LayoutBottomBarFrameData(rect);
		}
		else if ( m_selectedNode >= 0 )
		{
			// Node data editor
			LayoutBottomBarNodeData(rect);
		}

		GUI.EndGroup();
		GUI.EndGroup();
	}

	void LayoutBottomBarEventData(Rect rect)
	{
		EditorGUI.BeginChangeCheck();

		float xOffset = 10;
		float width = 55;
		GUI.Label( new Rect(xOffset,1, width,rect.height), "Event:", EditorStyles.boldLabel );

		// Function Name
		AnimEvent animEvent = m_selectedEvents[0];


		xOffset += width;

		width = Mathf.Max(80, EditorStyles.miniTextField.CalcSize(new GUIContent(animEvent.m_functionName)).x+5);

		// Give gui control name using it's time and function name so it can be auto-selected when creating new evnet
		GUI.SetNextControlName("EventFunctionName");
		animEvent.m_functionName = EditorGUI.TextField( new Rect(xOffset,2, width,rect.height+5), animEvent.m_functionName, EditorStyles.toolbarTextField );

		if ( m_focusEvent == animEvent )
		{				
			if ( GUI.GetNameOfFocusedControl() == "EventFunctionName" )
				m_focusEvent = null;					
			EditorGUI.FocusTextInControl("EventFunctionName");
		}

		if ( GUI.GetNameOfFocusedControl() == "EventFunctionName" )
		{
			UpdateAutoComplete(animEvent.m_functionName);
		}

		xOffset += width + 5;
		width = 60;

		GUI.Label( new Rect(xOffset,0, width,rect.height), "Parameter:", EditorStyles.miniLabel );

		xOffset += width;
		width = 60;

		animEvent.m_paramType = (eAnimEventParameter)EditorGUI.EnumPopup( new Rect(xOffset,2, width,rect.height), animEvent.m_paramType as System.Enum );

		switch ( animEvent.m_paramType )
		{

			case eAnimEventParameter.Int:
			{
				xOffset += width + 5;
				width = 60;
				animEvent.m_paramInt = EditorGUI.IntField( new Rect(xOffset,2, width,rect.height-3), animEvent.m_paramInt ,  EditorStyles.toolbarTextField);				
			} break;

			case eAnimEventParameter.Float:
			{
				xOffset += width + 5;
				width = 60;
				animEvent.m_paramFloat = EditorGUI.FloatField( new Rect(xOffset,2, width,rect.height-3), animEvent.m_paramFloat,  EditorStyles.toolbarTextField );				
			} break;

			case eAnimEventParameter.String:
			{
				xOffset += width + 5;
				width = Mathf.Max(60, EditorStyles.miniTextField.CalcSize(new GUIContent(animEvent.m_paramString)).x+5);
				animEvent.m_paramString = EditorGUI.TextField( new Rect(xOffset,2, width,rect.height), animEvent.m_paramString,  EditorStyles.toolbarTextField );				
			} break;

			case eAnimEventParameter.Object:
			{
				xOffset += width + 5;
				width = 150;
				animEvent.m_paramObjectReference = EditorGUI.ObjectField( new Rect(xOffset,2, width,rect.height-3), animEvent.m_paramObjectReference, typeof(Object), false );				
			} break;

			default: break;
		}

		// Align options to to right

		xOffset = rect.width;
		width = 110;
		xOffset -= width;

		// Prefix
		animEvent.m_usePrefix = GUI.Toggle( new Rect(xOffset,0, width,rect.height), animEvent.m_usePrefix, "Add 'Anim' Prefix",  Styles.TIMELINE_EVENT_TOGGLE );

		width = 105;
		xOffset -= width;

		// Send updwards
		animEvent.m_messageOptions = GUI.Toggle( new Rect(xOffset,0, width,rect.height), 
			animEvent.m_messageOptions == SendMessageOptions.RequireReceiver,
			"Require Receiver", Styles.TIMELINE_EVENT_TOGGLE ) ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver;

		width = 90;
		xOffset -= width;

		// Send updwards
		animEvent.m_sendUpwards = GUI.Toggle( new Rect(xOffset,0, width,rect.height), animEvent.m_sendUpwards, "Send Upwards", Styles.TIMELINE_EVENT_TOGGLE );

		if ( EditorGUI.EndChangeCheck() )
		{
			// Set default tag options to last tag edited (when new tag is created it'll use these options)
			m_defaultEventMessageOptions = animEvent.m_messageOptions;
			m_defaultEventSendUpwards = animEvent.m_sendUpwards;
			m_defaultEventUsePrefix = animEvent.m_usePrefix;

			// Apply events
			ApplyChanges();
		}
	}

	void LayoutBottomBarFrameData(Rect rect)
	{
		AnimFrame frame = m_selectedFrames[0];

		EditorGUI.BeginChangeCheck();

		float xOffset = 10;
		float width = 60;
		GUI.Label( new Rect(xOffset,1, width,rect.height), "Frame:", EditorStyles.boldLabel );

		// Function Name

		xOffset += width;
		width = 250;

		frame.m_sprite = EditorGUI.ObjectField( new Rect(xOffset,2, width,rect.height-3), frame.m_sprite, typeof(Sprite), false ) as Sprite;

		xOffset += width+5;
		width = 50;

		// Frame length (in samples)
		EditorGUI.LabelField(new Rect(xOffset,2,width,rect.height-3), "Length");

		xOffset += width+5;
		width = 30;

		GUI.SetNextControlName("FrameLen");
		int frameLen = Mathf.RoundToInt( frame.m_length / GetMinFrameTime() );
		frameLen = EditorGUI.IntField( new Rect(xOffset,2,width,rect.height-3), frameLen );
		SetFrameLength(frame, frameLen * GetMinFrameTime() );

		xOffset += width;
		width = 100;

		if ( EditorGUI.EndChangeCheck() )
		{
			// Apply events
			ApplyChanges();
		}
	}

	void LayoutPlayhead(Rect rect)
	{		
		float offset = rect.xMin + m_timelineOffset + (m_animTime * m_timelineScale);
		DrawLine( new Vector2(offset, rect.yMin), new Vector2(offset,rect.yMax),Color.red );
	}

	// Handles moving frames
	void LayoutMoveFrame(Rect rect)
	{
		Event e = Event.current;

		if ( m_dragState == eDragState.MoveFrame )
		{
			int closestFrame = MousePosToInsertFrameIndex(rect);

			LayoutInsertFramesLine(rect, closestFrame);

			if ( e.type == EventType.MouseDrag && e.button == 0 )
			{
				e.Use();
			}

			if ( e.type == EventType.MouseUp && e.button == 0 )
			{
				// Move selected frame to before closestFrame
				MoveSelectedFrames(closestFrame);
				m_dragState = eDragState.None;
				e.Use();
			}
		}
	}


	// Handles drag/drop onto timeline
	void LayoutInsert(Rect rect)
	{	
		Event e = Event.current;
		if ( (e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && rect.Contains( e.mousePosition ) )
		{			
			if ( System.Array.Exists( DragAndDrop.objectReferences, item => item is Sprite || item is Texture2D ) )
			{

				int closestFrame = 0;
				if ( e.control )
				{
					// When control is held, the frames are replaced rather than inserted
					DragAndDrop.visualMode = DragAndDropVisualMode.Move; 
					closestFrame = MousePosToReplaceFrameIndex(rect);
					LayoutReplaceFramesBox(rect, closestFrame, DragAndDrop.objectReferences.Length ); // holding control, so show frames that'll be replaced
				}
				else 
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
					closestFrame = MousePosToInsertFrameIndex(rect);
					LayoutInsertFramesLine(rect, closestFrame);
				}
				m_dragDropHovering = true;

				if ( e.type == EventType.DragPerform )
				{
					DragAndDrop.AcceptDrag();
					List<Sprite> sprites = new List<Sprite>();
					foreach (Object obj in DragAndDrop.objectReferences)
					{
						if ( obj is Sprite )
						{
							sprites.Add(obj as Sprite);
						}
						else if ( obj is Texture2D )
						{
							// Grab all sprites associated with a texture, add to list
							string path = AssetDatabase.GetAssetPath(obj);
							Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
							foreach (Object subAsset in assets) 
							{
								if (subAsset is Sprite) 
								{
									sprites.Add((Sprite)subAsset);
								}
							}
						}
					}

					// Sort sprites by name and insert
					using ( NaturalComparer comparer = new NaturalComparer() )
					{
						sprites.Sort( (a, b) => comparer.Compare(a.name,b.name) );
					}

					if ( e.control ) // When control is held, the frames are replaced
						ReplaceFrames(sprites.ToArray(), closestFrame);
					else 
						InsertFrames( sprites.ToArray(), closestFrame );
				}

			}
		}

		// The indicator won't update while drag/dropping becuse it's not active, so we hack it using this flag
		if ( m_dragDropHovering && rect.Contains( e.mousePosition ) )
		{
			if ( e.control )
				LayoutReplaceFramesBox(rect, MousePosToReplaceFrameIndex(rect), DragAndDrop.objectReferences.Length ); // holding control, so show frames that'll be replaced
			else 
				LayoutInsertFramesLine(rect, MousePosToInsertFrameIndex(rect));
		}
		else 
		{
			m_dragDropHovering = false;
		}

		if ( e.type == EventType.DragExited )
		{
			m_dragDropHovering = false;
		}
	}

	// Draws line that shows where frames will be inserted
	void LayoutInsertFramesLine( Rect rect, int frameId )
	{
		float time = frameId < m_frames.Count ? m_frames[frameId].m_time : GetAnimLength();
		float posOnTimeline = m_timelineOffset + (time * m_timelineScale);

		// check if it's visible on timeline
		if ( posOnTimeline < rect.xMin || posOnTimeline > rect.xMax )
			return;		
		DrawLine(new Vector2(posOnTimeline, rect.yMin),new Vector2(posOnTimeline, rect.yMax), COLOR_INSERT_FRAMES_LINE);

	}

	void LayoutReplaceFramesBox( Rect rect, int frameId, int numFrames )
	{
		float time = frameId < m_frames.Count ? m_frames[frameId].m_time : GetAnimLength();
		float startPosOnTimeline = m_timelineOffset + (time * m_timelineScale);
		int finalTimeId = frameId+numFrames;
		float finalTime = finalTimeId < m_frames.Count ? m_frames[finalTimeId].m_time : GetAnimLength()+0.0f;
		float endPosOnTimeline = m_timelineOffset + (finalTime * m_timelineScale);

		Rect selectionRect = new Rect(rect){ xMin = Mathf.Max(rect.xMin,startPosOnTimeline), xMax = Mathf.Min(rect.xMax, endPosOnTimeline) };
		DrawRect(selectionRect,COLOR_UNITY_BLUE.WithAlpha(0.1f),COLOR_UNITY_BLUE.WithAlpha(0.6f));	
	}


	#endregion
	#region Funcs: Private

    public static string ToTimelineLabelString( float seconds, float sampleRate ) 
    {
		return string.Format( "{0:0}:{1:00}",Mathf.FloorToInt(seconds),(seconds%1.0f)*100.0f );
    }

    List<int> CreateIntervalSizeList(out int intervalId)
    {
		List<int> intervalSizes = new List<int>();
        int tmpSampleRate = (int)m_clip.frameRate;
        while ( true ) {
            int div = 0;
            if ( tmpSampleRate == 30 ) {
                div = 3;
            }
            else if ( tmpSampleRate % 2 == 0 ) {
                div = 2;
            }
            else if ( tmpSampleRate % 5 == 0 ) {
                div = 5;
            }
            else if ( tmpSampleRate % 3 == 0 ) {
                div = 3;
            }
            else {
                break;
            }
            tmpSampleRate /= div;
            intervalSizes.Insert(0,div);
        }
		intervalId = intervalSizes.Count;
        intervalSizes.AddRange( new int[] { 
                            5, 2, 3, 2,
                            5, 2, 3, 2,
                            } );
		return intervalSizes;
    }

    float GuiPosToAnimTime(Rect rect, float mousePosX)
    {
		float pos = mousePosX - rect.xMin;
		return ((pos-m_timelineOffset) / m_timelineScale );
    }

    float AnimTimeToGuiPos(Rect rect, float time)
    {
    	return rect.xMin + m_timelineOffset + (time*m_timelineScale);
    }

	// Returns the point- Set to m_frames.Length if should insert after final frame
	int MousePosToInsertFrameIndex(Rect rect)
	{
		if ( m_frames.Count == 0 )
			return 0;

		// Find point between two frames closest to mouse cursor so we can show indicator
		float closest = float.MaxValue;
		float animTime = GuiPosToAnimTime(rect, Event.current.mousePosition.x);
		int closestFrame = 0;
		for ( ; closestFrame < m_frames.Count+1; ++closestFrame )
		{
			// Loop through frames until find one that's further away than the last from the mouse pos
			// For final iteration it checks the end time of the last frame rather than start time
			float frameStartTime = closestFrame < m_frames.Count ? m_frames[closestFrame].m_time : m_frames[closestFrame-1].EndTime; 
			float diff = Mathf.Abs(frameStartTime-animTime);
			if ( diff > closest)
				break;
			closest = diff;
		}

		closestFrame = Mathf.Clamp(closestFrame-1, 0, m_frames.Count);
		return closestFrame;
	}

	// Returns frame mouse is hovering over
	int MousePosToReplaceFrameIndex(Rect rect)
	{
		if ( m_frames.Count == 0 )
			return 0;

		// Find point between two frames closest to mouse cursor so we can show indicator

		float animTime = GuiPosToAnimTime(rect, Event.current.mousePosition.x);
		int closestFrame = 0;
		while ( closestFrame < m_frames.Count && m_frames[closestFrame].EndTime <= animTime )
			++closestFrame;
		closestFrame = Mathf.Clamp(closestFrame, 0, m_frames.Count);
		return closestFrame;
	}

	// Clears both frame and event selection
	void ClearSelection()
	{
		m_selectedFrames.Clear();
		m_selectedEvents.Clear();	
		m_selectedNode = -1;	
	}

	#endregion
	#region AutoComplete


	void UpdateAutoComplete(string text)
	{
		// Work back until whitespace
		if ( string.IsNullOrEmpty(text) )
		{
			m_acList.Clear();
			return;
		}

		string expression = text;

		// Add keywords depending on context			

		m_acRemaining = expression;

		// Add items that match remaining expression to the list
		m_acList.Clear();
		m_acListAll.ForEach( item => { if ( item.m_functionName.StartsWith(m_acRemaining, System.StringComparison.OrdinalIgnoreCase) ) m_acList.Add(item); } );

		m_acSelectedIndex = Mathf.Clamp(m_acSelectedIndex,0,m_acList.Count-1);

		// Don't show if there's only one match and it's been compeltely typed in anyway
		if ( m_acList.Count == 1 && m_acRemaining == m_acList[0].m_functionName )
			m_acList.Clear();

		//Debug.Log( "AC: "+ m_acRemaining);


	}

	void ClearAutoComplete()
	{
		m_acList.Clear();
		m_acRemaining = string.Empty;
		m_acCanceled = false;
		m_acSelectedIndex = 0;
	}


	bool HasAutoComplete()
	{
		return m_acList.Count > 0 && m_acCanceled == false;
	}

	AnimEvent DoAutoComplete(string text)
	{
		if ( text.Length == 0 )
			return null;
		if ( m_acList.Count <= 0 || m_acCanceled )
			return null;
		
		return m_acList[ Mathf.Clamp(m_acSelectedIndex, 0, m_acList.Count) ];
		//GUI.changed = true;
		//return true;
	}


	#endregion
	#region: Functions: Layout

	void LayoutAutoCompleteList(Rect rect)
	{		
		if ( m_acList.Count <= 0 || m_acCanceled )
			return; 
		
		float lineHeight = 15;
		int numLinesShown =  Mathf.Min(m_acList.Count, 5);
		
		float offsetX = 0;
		// Adjust rect based on size of text
		rect.min = rect.min + new Vector2(offsetX,-lineHeight*numLinesShown);
		// Set x/y based on cursor pos
		rect.max = rect.min + new Vector2( 120 , lineHeight * numLinesShown );

		// Backround + border
		EditorGUI.DrawRect(new Rect(rect) { height = rect.height+2, width = rect.width+2,center = rect.center },new Color(0.616f, 0.635f, 0.651f,1)); // border

		EditorGUI.DrawRect(rect, new Color(0.93f,0.93f,0.95f,1));
		GUI.contentColor = Color.black;

		int end = Mathf.Min( m_acSelectedIndex+2, m_acList.Count-1 );
		int start = Mathf.Max( end-4, 0 );

		for ( int i = start; i < start+5 && i < m_acList.Count; ++i )
		{
			if ( m_acSelectedIndex == i )
				EditorGUI.DrawRect(new Rect(rect){yMin = rect.yMin+((i-start)*lineHeight), height = lineHeight},  GUI.skin.settings.selectionColor );			
			
			GUI.Label(new Rect(rect){ yMin=rect.yMin+((i-start)*lineHeight) }, m_acList[i].m_functionName, Styles.AUTOCOMPLETE_LABEL );
		}
		GUI.contentColor = Color.white;


	}
	#endregion
}

}