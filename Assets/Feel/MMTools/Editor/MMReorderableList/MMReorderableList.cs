using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Tools
{
	public class MMReorderableList {

		private const float ELEMENT_EDGE_TOP = 1;
		private const float ELEMENT_EDGE_BOT = 3;
		private const float ELEMENT_HEIGHT_OFFSET = ELEMENT_EDGE_TOP + ELEMENT_EDGE_BOT;

		private static int selectionHash = "ReorderableListSelection".GetHashCode();
		private static int dragAndDropHash = "ReorderableListDragAndDrop".GetHashCode();

		public enum ElementDisplayType {
			Auto,
			Expandable,
			SingleLine
		}

		public delegate void DrawHeaderDelegate(Rect rect, GUIContent label);
		public delegate void DrawFooterDelegate(Rect rect);
		public delegate void DrawElementDelegate(Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused);
		public delegate void ActionDelegate(MMReorderableList list);
		public delegate bool ActionBoolDelegate(MMReorderableList list);
		public delegate void AddDropdownDelegate(Rect buttonRect, MMReorderableList list);
		public delegate Object DragDropReferenceDelegate(Object[] references, MMReorderableList list);
		public delegate void DragDropAppendDelegate(Object reference, MMReorderableList list);
		public delegate float GetElementHeightDelegate(SerializedProperty element);
		public delegate float GetElementsHeightDelegate(MMReorderableList list);
		public delegate string GetElementNameDelegate(SerializedProperty element);

		public event DrawHeaderDelegate drawHeaderCallback;
		public event DrawFooterDelegate drawFooterCallback;
		public event DrawElementDelegate drawElementCallback;
		public event DrawElementDelegate drawElementBackgroundCallback;
		public event GetElementHeightDelegate getElementHeightCallback;
		public event GetElementsHeightDelegate getElementsHeightCallback;
		public event GetElementNameDelegate getElementNameCallback;
		public event DragDropReferenceDelegate onValidateDragAndDropCallback;
		public event DragDropAppendDelegate onAppendDragDropCallback;
		public event ActionDelegate onReorderCallback;
		public event ActionDelegate onSelectCallback;
		public event ActionDelegate onAddCallback;
		public event AddDropdownDelegate onAddDropdownCallback;
		public event ActionDelegate onRemoveCallback;
		public event ActionDelegate onMouseUpCallback;
		public event ActionBoolDelegate onCanRemoveCallback;
		public event ActionDelegate onChangedCallback;

		public bool canAdd;
		public bool canRemove;	
		public bool draggable;
		public bool expandable;
		public bool multipleSelection;
		public GUIContent label;
		public float headerHeight;
		public float footerHeight;
		public float slideEasing;
		public float verticalSpacing;
		public bool showDefaultBackground;
		public ElementDisplayType elementDisplayType;
		public string elementNameProperty;
		public string elementNameOverride;
		public Texture elementIcon;

		internal readonly int id;

		private SerializedProperty list;
		private int controlID = -1;
		private Rect[] elementRects;
		private GUIContent elementLabel;		
		private ListSelection selection;
		private SlideGroup slideGroup;
		private int pressIndex;

		private float elementSpacing {

			get { return Mathf.Max(0, verticalSpacing - 2); }
		}

		private bool dragging;
		private float pressPosition;
		private float dragPosition;
		private int dragDirection;
		private DragElement[] dragList;
		private ListSelection beforeDragSelection;
		
		private int dragDropControlID = -1;		

		public MMReorderableList(SerializedProperty list)
			: this(list, true, true, true) {
		}

		public MMReorderableList(SerializedProperty list, bool canAdd, bool canRemove, bool draggable)
			: this(list, canAdd, canRemove, draggable, ElementDisplayType.Auto, null, null, null) {
		}

		public MMReorderableList(SerializedProperty list, bool canAdd, bool canRemove, bool draggable, ElementDisplayType elementDisplayType, string elementNameProperty, Texture elementIcon) 
			: this(list, canAdd, canRemove, draggable, elementDisplayType, elementNameProperty, null, elementIcon) {
		}

		public MMReorderableList(SerializedProperty list, bool canAdd, bool canRemove, bool draggable, ElementDisplayType elementDisplayType, string elementNameProperty, string elementNameOverride, Texture elementIcon) {

			if (list == null) {

				throw new MissingListExeption();
			}
			else if (!list.isArray) {

				//check if user passed in a ReorderableArray, if so, that becomes the list object

				SerializedProperty array = list.FindPropertyRelative("array");

				if (array == null || !array.isArray) {

					throw new InvalidListException();
				}

				this.list = array;
			}
			else {

				this.list = list;
			}

			this.canAdd = canAdd;
			this.canRemove = canRemove;
			this.draggable = draggable;
			this.elementDisplayType = elementDisplayType;
			this.elementNameProperty = elementNameProperty;
			this.elementNameOverride = elementNameOverride;
			this.elementIcon = elementIcon;

			id = GetHashCode();
			list.isExpanded = true;
			label = new GUIContent(list.displayName);

			#if UNITY_5_6_OR_NEWER
			verticalSpacing = EditorGUIUtility.standardVerticalSpacing;
			#else
			verticalSpacing = 2f;
			#endif
			headerHeight = 18f;
			footerHeight = 13f;
			slideEasing = 0.15f;
			expandable = true;
			showDefaultBackground = true;
			multipleSelection = true;
			elementLabel = new GUIContent();

			selection = new ListSelection();
			slideGroup = new SlideGroup();
			elementRects = new Rect[0];
		}

		//
		// -- PROPERTIES --
		//

		public SerializedProperty List {

			get { return list; }
			internal set { list = value; }
		}

		public bool HasList {

			get { return list != null && list.isArray; }
		}

		public int Length {

			get { return HasList ? list.arraySize : 0; }
		}

		public int[] Selected {

			get { return selection.ToArray(); }
			set { selection = new ListSelection(value); }
		}

		public int Index {

			get { return selection.First; }
			set { selection.Select(value); }
		}

		public bool IsDragging {

			get { return dragging; }
		}

		//
		// -- PUBLIC --
		//

		public float GetHeight() {

			if (HasList) {

				return list.isExpanded ? headerHeight + GetElementsHeight() + footerHeight : headerHeight;
			}
			else {

				return EditorGUIUtility.singleLineHeight;
			}
		}

		public void DoLayoutList() {

			Rect position = EditorGUILayout.GetControlRect(false, GetHeight(), EditorStyles.largeLabel);

			DoList(EditorGUI.IndentedRect(position), label);
		}

		public void DoList(Rect rect, GUIContent label) {

			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			Rect headerRect = rect;
			headerRect.height = headerHeight;			

			if (!HasList) {

				DrawEmpty(headerRect, label.text + " is not an Array!", GUIStyle.none, EditorStyles.helpBox);
			}
			else {

				controlID = GUIUtility.GetControlID(selectionHash, FocusType.Keyboard, rect);
				dragDropControlID = GUIUtility.GetControlID(dragAndDropHash, FocusType.Passive, rect);

				DrawHeader(headerRect, label);

				if (list.isExpanded) {					

					Rect elementBackgroundRect = rect;
					elementBackgroundRect.yMin = headerRect.yMax;
					elementBackgroundRect.yMax = rect.yMax - footerHeight;		

					Event evt = Event.current;

					if (selection.Length > 1) {

						if (evt.type == EventType.ContextClick && CanSelect(evt.mousePosition)) {

							HandleMultipleContextClick(evt);
						}
					}

					if (list.arraySize > 0) {

						//update element rects if not dragging. Dragging caches draw rects so no need to update

						if (!dragging) {

							UpdateElementRects(elementBackgroundRect, evt);
						}

						if (elementRects.Length > 0) {

							Rect selectableRect = elementBackgroundRect;
							selectableRect.yMin = elementRects[0].yMin;
							selectableRect.yMax = elementRects[elementRects.Length - 1].yMax;

							HandlePreSelection(selectableRect, evt);
							DrawElements(elementBackgroundRect, evt);
							HandlePostSelection(selectableRect, evt);
						}
					}
					else {

						DrawEmpty(elementBackgroundRect, "List is Empty", Style.boxBackground, Style.verticalLabel);
					}

					Rect footerRect = rect;
					footerRect.yMin = elementBackgroundRect.yMax;
					footerRect.xMin = rect.xMax - 58;

					DrawFooter(footerRect);
				}
			}

			EditorGUI.indentLevel = indent;
		}

		public SerializedProperty AddItem<T>(T item) where T : Object {

			SerializedProperty property = AddItem();

			if (property != null) {

				property.objectReferenceValue = item;
			}

			return property;
		}

		public SerializedProperty AddItem() {

			if (HasList) {

				list.arraySize++;
				selection.Select(list.arraySize - 1);

				DispatchChange();

				return list.GetArrayElementAtIndex(selection.Last);
			}
			else {

				throw new InvalidListException();
			}
		}		

		public void Remove(int[] selection) {

			System.Array.Sort(selection);

			int i = selection.Length;

			while (--i > -1) {

				RemoveItem(selection[i]);
			}
		}

		public void RemoveItem(int index) {

			if (HasList && index >= 0 && index < list.arraySize) {

				SerializedProperty property = list.GetArrayElementAtIndex(index);

				if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue) {

					property.objectReferenceValue = null;
				}

				list.DeleteArrayElementAtIndex(index);
				selection.Remove(index);

				if (list.arraySize > 0) {

					selection.Select(Mathf.Max(0, index - 1));
				}

				DispatchChange();
			}
		}

		public SerializedProperty GetItem(int index) {

			if (HasList && index >= 0 && index < list.arraySize) {

				return list.GetArrayElementAtIndex(index);
			}
			else {

				return null;
			}
		}

		public int IndexOf(SerializedProperty element) {

			if (element != null) {

				int i = list.arraySize;

				while (--i > -1) {

					if (SerializedProperty.EqualContents(element, list.GetArrayElementAtIndex(i))) {

						return i;
					}
				}
			}

			return -1;
		}

		public void GrabKeyboardFocus() {

			GUIUtility.keyboardControl = id;
		}

		public bool HasKeyboardControl() {

			return GUIUtility.keyboardControl == id;
		}

		public void ReleaseKeyboardFocus() {

			if (GUIUtility.keyboardControl == id) {

				GUIUtility.keyboardControl = 0;
			}
		}

		//
		// -- PRIVATE --
		//

		private float GetElementsHeight() {

			if (getElementsHeightCallback != null) {

				return getElementsHeightCallback(this);
			}

			int i, len = list.arraySize;

			if (len == 0) {

				return 28;
			}

			float totalHeight = 0;
			float spacing = elementSpacing;

			for (i = 0; i < len; i++) {

				totalHeight += GetElementHeight(list.GetArrayElementAtIndex(i)) + spacing;
			}

			return totalHeight + 7 - spacing;
		}

		private float GetElementHeight(SerializedProperty element) {

			if (getElementHeightCallback != null) {

				return getElementHeightCallback(element) + ELEMENT_HEIGHT_OFFSET;
			}
			else {

				return EditorGUI.GetPropertyHeight(element, GetElementLabel(element), IsElementExpandable(element)) + ELEMENT_HEIGHT_OFFSET;
			}
		}

		private Rect GetElementDrawRect(int index, Rect desiredRect) {

			if (slideEasing <= 0) {

				return desiredRect;
			}
			else {

				//lerp the drag easing toward slide easing, this creates a stronger easing at the start then slower at the end
				//when dealing with large lists, we can

				return dragging ? slideGroup.GetRect(dragList[index].startIndex, desiredRect, slideEasing) : slideGroup.SetRect(index, desiredRect);
			}
		}

		/*
		private Rect GetElementHeaderRect(SerializedProperty element, Rect elementRect) {

			Rect rect = elementRect;
			rect.height = EditorGUIUtility.singleLineHeight + verticalSpacing;

			return rect;
		}
		*/

		private Rect GetElementRenderRect(SerializedProperty element, Rect elementRect) {

			float offset = draggable ? 20 : 5;

			Rect rect = elementRect;
			rect.xMin += IsElementExpandable(element) ? offset + 10 : offset;
			rect.xMax -= 5;
			rect.yMin += ELEMENT_EDGE_TOP;
			rect.yMax -= ELEMENT_EDGE_BOT;

			return rect;
		}

		private void DrawHeader(Rect rect, GUIContent label) {

			if (showDefaultBackground && Event.current.type == EventType.Repaint) {

				Style.headerBackground.Draw(rect, false, false, false, false);
			}
			
			HandleDragAndDrop(rect, Event.current);

			Rect titleRect = rect;
			titleRect.xMin += 6f;
			titleRect.xMax -= 55f;
			titleRect.height -= 2f;
			titleRect.y++;

			label = EditorGUI.BeginProperty(titleRect, label, list);

			if (drawHeaderCallback != null) {

				drawHeaderCallback(titleRect, label);
			}
			else if (expandable) { 

				titleRect.xMin += 10;

				EditorGUI.BeginChangeCheck();

				bool isExpanded = EditorGUI.Foldout(titleRect, list.isExpanded, label, true);

				if (EditorGUI.EndChangeCheck()) {

					list.isExpanded = isExpanded;
				}
			}
			else {

				GUI.Label(titleRect, label, EditorStyles.label);
			}

			EditorGUI.EndProperty();

			if (elementDisplayType != ElementDisplayType.SingleLine) {

				Rect bRect1 = rect;
				bRect1.xMin = rect.xMax - 25;
				bRect1.xMax = rect.xMax - 5;

				if (GUI.Button(bRect1, Style.expandButton, Style.preButton)) {

					ExpandElements(true);
				}

				Rect bRect2 = rect;
				bRect2.xMin = bRect1.xMin - 20;
				bRect2.xMax = bRect1.xMin;

				if (GUI.Button(bRect2, Style.collapseButton, Style.preButton)) {

					ExpandElements(false);
				}
			}		
		}

		private void ExpandElements(bool expand) {

			if (!list.isExpanded && expand) {

				list.isExpanded = true;
			}

			for (int i = 0; i < list.arraySize; i++) {

				list.GetArrayElementAtIndex(i).isExpanded = expand;
			}
		}

		private void DrawEmpty(Rect rect, string label, GUIStyle backgroundStyle, GUIStyle labelStyle) {

			if (showDefaultBackground && Event.current.type == EventType.Repaint) {

				backgroundStyle.Draw(rect, false, false, false, false);
			}

			EditorGUI.LabelField(rect, label, labelStyle);
		}

		private void UpdateElementRects(Rect rect, Event evt) {			

			//resize array if elements changed

			int i, len = list.arraySize;

			if (len != elementRects.Length) {

				System.Array.Resize(ref elementRects, len);
			}

			if (evt.type == EventType.Repaint) {

				//start rect

				Rect elementRect = rect;
				elementRect.yMin = elementRect.yMax = rect.yMin + 2;

				float spacing = elementSpacing;

				for (i = 0; i < len; i++) {

					SerializedProperty element = list.GetArrayElementAtIndex(i);

					//update the elementRects value for this object. Grab the last elementRect for startPosition

					elementRect.y = elementRect.yMax;
					elementRect.height = GetElementHeight(element);
					elementRects[i] = elementRect;

					elementRect.yMax += spacing;
				}
			}
		}

		private void DrawElements(Rect rect, Event evt) {

			//draw list background

			if (showDefaultBackground && evt.type == EventType.Repaint) {

				Style.boxBackground.Draw(rect, false, false, false, false);
			}
			
			//if not dragging, draw elements as usual

			if (!dragging) {

				int i, len = list.arraySize;

				for (i = 0; i < len; i++) { 

					bool selected = selection.Contains(i);

					DrawElement(list.GetArrayElementAtIndex(i), GetElementDrawRect(i, elementRects[i]), selected, selected && GUIUtility.keyboardControl == controlID);
				}
			}
			else if (evt.type == EventType.Repaint) {

				//draw dragging elements only when repainting

				int i, s, len = dragList.Length;
				int sLen = selection.Length;

				//first, find the rects of the selected elements, we need to use them for overlap queries

				for (i = 0; i < sLen; i++) {

					DragElement element = dragList[i];

					//update the element desiredRect if selected. Selected elements appear first in the dragList, so other elements later in iteration will have rects to compare

					element.desiredRect.y = dragPosition - element.dragOffset;
					dragList[i] = element;
				}

				//draw elements, start from the bottom of the list as first elements are the ones selected, so should be drawn last

				i = len;

				while (--i > -1) {

					DragElement element = dragList[i];					

					//draw dragging elements last as the loop is backwards

					if (element.selected) {

						DrawElement(element.property, element.desiredRect, true, true);
						continue;
					}

					//loop over selection and see what overlaps
					//if dragging down we start from the bottom of the selection
					//otherwise we start from the top. This helps to cover multiple selected objects

					Rect elementRect = element.rect;
					int elementIndex = element.startIndex;

					int start = dragDirection > 0 ? sLen - 1 : 0;
					int end = dragDirection > 0 ? -1 : sLen;

					for (s = start; s != end; s -= dragDirection) { 

						DragElement selected = dragList[s];

						if (selected.Overlaps(elementRect, elementIndex, dragDirection)) {

							elementRect.y -= selected.rect.height * dragDirection;
							elementIndex += dragDirection;
						}
					}

					//draw the element with the new rect

					DrawElement(element.property, GetElementDrawRect(i, elementRect), false, false);

					//reassign the element back into the dragList

					element.desiredRect = elementRect;
					dragList[i] = element;
				}
			}
		}

		private void DrawElement(SerializedProperty element, Rect rect, bool selected, bool focused) {

			Event evt = Event.current;

			if (drawElementBackgroundCallback != null) {

				drawElementBackgroundCallback(rect, element, null, selected, focused);
			}
			else if (evt.type == EventType.Repaint) {

				Style.elementBackground.Draw(rect, false, selected, selected, focused);
			}
			
			if (evt.type == EventType.Repaint && draggable) {

				Style.draggingHandle.Draw(new Rect(rect.x + 5, rect.y + 6, 10, rect.height - (rect.height - 6)), false, false, false, false);
			}			

			GUIContent label = GetElementLabel(element);

			Rect renderRect = GetElementRenderRect(element, rect);

			if (drawElementCallback != null) {

				drawElementCallback(renderRect, element, label, selected, focused);
			}
			else {

				EditorGUI.PropertyField(renderRect, element, label, true);
			}

			//handle context click

			int controlId = GUIUtility.GetControlID(label, FocusType.Passive, rect);

			switch (evt.GetTypeForControl(controlId)) {

				case EventType.ContextClick:

					if (rect.Contains(evt.mousePosition)) {

						HandleContextClick(evt, element);
					}

					break;
			}
		}

		private GUIContent GetElementLabel(SerializedProperty element) {

			string name;

			if (getElementNameCallback != null) {

				name = getElementNameCallback(element);
			}
			else {

				name = GetElementName(element, elementNameProperty, elementNameOverride);
			}

			elementLabel.text = !string.IsNullOrEmpty(name) ? name : element.displayName;
			elementLabel.tooltip = element.tooltip;
			elementLabel.image = elementIcon;

			return elementLabel;
		}

		private static string GetElementName(SerializedProperty element, string nameProperty, string nameOverride) {

			if (!string.IsNullOrEmpty(nameOverride)) {

				string path = element.propertyPath;

				if (path.EndsWith("]")) {

					int startIndex = path.LastIndexOf('[') + 1;

					return string.Concat(nameOverride, " ", path.Substring(startIndex, path.Length - startIndex - 1));
				}

				return nameOverride;
			}
			else if (string.IsNullOrEmpty(nameProperty)) {

				return null;
			}
			else if (element.propertyType == SerializedPropertyType.ObjectReference && nameProperty == "name") {

				return element.objectReferenceValue ? element.objectReferenceValue.name : null;
			}

			SerializedProperty prop = element.FindPropertyRelative(nameProperty);

			if (prop != null) {

				switch (prop.propertyType) {

					case SerializedPropertyType.ObjectReference:

						return prop.objectReferenceValue ? prop.objectReferenceValue.name : null;

					case SerializedPropertyType.Enum:

						return prop.enumDisplayNames[prop.enumValueIndex];

					case SerializedPropertyType.Integer:
					case SerializedPropertyType.Character:

						return prop.intValue.ToString();

					case SerializedPropertyType.LayerMask:

						return GetLayerMaskName(prop.intValue);

					case SerializedPropertyType.String:

						return prop.stringValue;

					case SerializedPropertyType.Float:

						return prop.floatValue.ToString();
				}

				return prop.displayName;
			}

			return null;
		}

		private static string GetLayerMaskName(int mask) {

			if (mask == 0) {

				return "Nothing";
			}
			else if (mask < 0) {

				return "Everything";
			}

			string name = string.Empty;
			int n = 0;

			for (int i = 0; i < 32; i++) {

				if (((1 << i) & mask) != 0) {

					if (n == 4) {

						return "Mixed ...";
					}

					name += (n > 0 ? ", " : string.Empty) + LayerMask.LayerToName(i);
					n++;
				}
			}

			return name;
		}

		private void DrawFooter(Rect rect) {

			if (drawFooterCallback != null) {

				drawFooterCallback(rect);
				return;
			}

			if (Event.current.type == EventType.Repaint) {

				Style.footerBackground.Draw(rect, false, false, false, false);
			}

			Rect addRect = new Rect(rect.xMin + 4f, rect.y - 3f, 25f, 13f);
			Rect subRect = new Rect(rect.xMax - 29f, rect.y - 3f, 25f, 13f);

			EditorGUI.BeginDisabledGroup(!canAdd);

			if (GUI.Button(addRect, onAddDropdownCallback != null ? Style.iconToolbarPlusMore : Style.iconToolbarPlus, Style.preButton)) {

				if (onAddDropdownCallback != null) {

					onAddDropdownCallback(addRect, this);
				}
				else if (onAddCallback != null) {

					onAddCallback(this);
				}
				else {

					AddItem();
				}
			}

			EditorGUI.EndDisabledGroup();

			EditorGUI.BeginDisabledGroup(!CanSelect(selection) || !canRemove || (onCanRemoveCallback != null && !onCanRemoveCallback(this)));

			if (GUI.Button(subRect, Style.iconToolbarMinus, Style.preButton)) {

				if (onRemoveCallback != null) {

					onRemoveCallback(this);
				}
				else {

					Remove(selection.ToArray());
				}
			}

			EditorGUI.EndDisabledGroup();
		}

		private void DispatchChange() {

			if (onChangedCallback != null) {

				onChangedCallback(this);
			}
		}

		private void HandleContextClick(Event evt, SerializedProperty element) {

			selection.Select(IndexOf(element));

			GenericMenu menu = new GenericMenu();

			if (element.isInstantiatedPrefab) {

				menu.AddItem(new GUIContent("Revert " + GetElementLabel(element).text + " to Prefab"), false, selection.RevertValues, list);
				menu.AddSeparator(string.Empty);
			}

			menu.AddItem(new GUIContent("Duplicate Array Element"), false, HandleDuplicate, list);
			menu.AddItem(new GUIContent("Delete Array Element"), false, HandleDelete, list);
			menu.ShowAsContext();

			evt.Use();
		}

		private void HandleMultipleContextClick(Event evt) {

			GenericMenu menu = new GenericMenu();

			if (selection.CanRevert(list)) {

				menu.AddItem(new GUIContent("Revert Values to Prefab"), false, selection.RevertValues, list);
				menu.AddSeparator(string.Empty);
			}

			menu.AddItem(new GUIContent("Duplicate Array Elements"), false, HandleDuplicate, list);
			menu.AddItem(new GUIContent("Delete Array Elements"), false, HandleDelete, list);
			menu.ShowAsContext();

			evt.Use();
		}

		private void HandleDelete(object userData) {

			selection.Delete(userData as SerializedProperty);

			DispatchChange();
		}

		private void HandleDuplicate(object userData) {

			selection.Duplicate(userData as SerializedProperty);

			DispatchChange();
		}

		private void HandleDragAndDrop(Rect rect, Event evt) {

			switch (evt.GetTypeForControl(dragDropControlID)) {

				case EventType.DragUpdated:
				case EventType.DragPerform:

					if (GUI.enabled && rect.Contains(evt.mousePosition)) {

						Object[] objectReferences = DragAndDrop.objectReferences;
						Object[] references = new Object[1];

						bool acceptDrag = false;

						foreach (Object object1 in objectReferences) {
							
							references[0] = object1;
							Object object2 = ValidateObjectDragAndDrop(references);
							
							if (object2 != null) {
								
								DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
								
								if (evt.type == EventType.DragPerform) {

									if (onAppendDragDropCallback != null) {

										onAppendDragDropCallback(object2, this);
									}
									else {

										AppendDragAndDropValue(object2);
									}

									acceptDrag = true;
									DragAndDrop.activeControlID = 0;
								}
								else {
									
									DragAndDrop.activeControlID = dragDropControlID;
								}
							}
						}

						if (acceptDrag) {
							
							GUI.changed = true;
							DragAndDrop.AcceptDrag();
						}
					}

					break;

				case EventType.DragExited:

					if (GUI.enabled) {

						HandleUtility.Repaint();
					}
					
					break;
			}
		}

		private Object ValidateObjectDragAndDrop(Object[] references) {

			if (onValidateDragAndDropCallback != null) {

				return onValidateDragAndDropCallback(references, this);
			}

			return Internals.ValidateObjectDragAndDrop(references, list);
		}

		private void AppendDragAndDropValue(Object obj) {

			Internals.AppendDragAndDropValue(obj, list);

			DispatchChange();
		}

		private void HandlePreSelection(Rect rect, Event evt) {

			if (evt.type == EventType.MouseDrag && draggable && GUIUtility.hotControl == controlID) {

				if (selection.Length > 0 && UpdateDragPosition(evt.mousePosition, rect, dragList)) {

					GUIUtility.keyboardControl = controlID;
					dragging = true;
				}

				evt.Use();
			}

			/* This is buggy. The reason for this is to allow selection and dragging of an element using the header, or top row (if any)
			 * The main issue here is determining whether the element has an "expandable" drop down arrow, which if it does, will capture the mouse event *without* the code below
			 * Because of property drawers and certain property types, it's impossible to know this automatically (without dirty reflection)
			 * So if the below code is active and we determine that the property is expandable but isn't actually. Then we'll accidently capture the mouse focus and prevent anything else from receiving it :(
			 * So for now, in order to drag or select a row, the user must select empty space on the row. Not a huge deal, and doesn't break functionality.
			 * What needs to happen is the drag event needs to occur independent of the event type. But that's messy too, as some controls have horizontal drag sliders :(
			if (evt.type == EventType.MouseDown) {

				//check if we contain the mouse press
				//we also need to check what has current focus. If nothing we can assume control
				//if there's something, check if the header has been pressed if the element is expandable
				//if we did press the header, then override the control

				if (rect.Contains(evt.mousePosition) && IsSelectionButton(evt)) {

					int index = GetSelectionIndex(evt.mousePosition);

					if (CanSelect(index)) {

						SerializedProperty element = list.GetArrayElementAtIndex(index);

						if (IsElementExpandable(element)) {

							Rect elementHeaderRect = GetElementHeaderRect(element, elementRects[index]);
							Rect elementRenderRect = GetElementRenderRect(element, elementRects[index]);

							Rect elementExpandRect = elementHeaderRect;
							elementExpandRect.xMin = elementRenderRect.xMin - 10;
							elementExpandRect.xMax = elementRenderRect.xMin;

							if (elementHeaderRect.Contains(evt.mousePosition) && !elementExpandRect.Contains(evt.mousePosition)) {

								DoSelection(index, true, evt);
								HandleUtility.Repaint();
							}
						}
					}
				}
			}
			*/
		}

		private void HandlePostSelection(Rect rect, Event evt) {

			switch (evt.GetTypeForControl(controlID)) {

				case EventType.MouseDown:

					if (rect.Contains(evt.mousePosition) && IsSelectionButton(evt)) {

						int index = GetSelectionIndex(evt.mousePosition);

						if (CanSelect(index)) {

							DoSelection(index, GUIUtility.keyboardControl == 0 || GUIUtility.keyboardControl == controlID || evt.button == 2, evt);
						}
						else {

							selection.Clear();
						}

						HandleUtility.Repaint();
					}

					break;

				case EventType.MouseUp:

					if (!draggable) {

						//select the single object if no selection modifier is being performed

						selection.SelectWhenNoAction(pressIndex, evt);

						if (onMouseUpCallback != null && IsPositionWithinElement(evt.mousePosition, selection.Last)) {

							onMouseUpCallback(this);
						}
					}
					else if (GUIUtility.hotControl == controlID) {

						evt.Use();

						if (dragging) {

							dragging = false;

							//move elements in list
							//sort the drag list

							ReorderDraggedElements(dragList);

							//apply changes

							list.serializedObject.ApplyModifiedProperties();
							list.serializedObject.Update();

							if (onReorderCallback != null) {

								onReorderCallback(this);
							}

							DispatchChange();
						}
						else {

							//if we didn't drag, then select the original pressed object

							selection.SelectWhenNoAction(pressIndex, evt);

							if (onMouseUpCallback != null) {

								onMouseUpCallback(this);
							}
						}

						GUIUtility.hotControl = 0;
					}

					HandleUtility.Repaint();

					break;

				case EventType.KeyDown:

					if (GUIUtility.keyboardControl == controlID) {

						if (evt.keyCode == KeyCode.DownArrow && !dragging) {

							selection.Select(Mathf.Min(selection.Last + 1, list.arraySize - 1));
							evt.Use();
						}
						else if (evt.keyCode == KeyCode.UpArrow && !dragging) {

							selection.Select(Mathf.Max(selection.Last - 1, 0));
							evt.Use();
						}
						else if (evt.keyCode == KeyCode.Escape && GUIUtility.hotControl == controlID) {

							GUIUtility.hotControl = 0;

							if (dragging) {

								dragging = false;
								selection = beforeDragSelection;
							}

							evt.Use();
						}
					}

					break;
			}
		}

		private bool IsSelectionButton(Event evt) {

			return evt.button == 0 || evt.button == 2;
		}

		private void DoSelection(int index, bool setKeyboardControl, Event evt) {

			//append selections based on action, this may be a additive (ctrl) or range (shift) selection

			if (multipleSelection) {

				selection.AppendWithAction(pressIndex = index, evt);
			}
			else {

				selection.Select(pressIndex = index);
			}

			if (onSelectCallback != null) {

				onSelectCallback(this);
			}

			if (draggable) {

				dragging = false;
				dragPosition = pressPosition = evt.mousePosition.y;
				dragList = GetDragList(dragPosition);

				beforeDragSelection = selection.Clone();

				GUIUtility.hotControl = controlID;
			}

			if (setKeyboardControl) {

				GUIUtility.keyboardControl = controlID;
			}

			evt.Use();
		}

		private DragElement[] GetDragList(float dragPosition) {

			int i, len = list.arraySize;

			if (dragList == null) {

				dragList = new DragElement[len];
			}
			else if (dragList.Length != len) {

				System.Array.Resize(ref dragList, len);
			}

			for (i = 0; i < len; i++) {

				SerializedProperty property = list.GetArrayElementAtIndex(i);
				Rect elementRect = elementRects[i];

				DragElement dragElement = new DragElement() {
					property = property,
					dragOffset = dragPosition - elementRect.y,
					rect = elementRect,
					desiredRect = elementRect,
					selected = selection.Contains(i),
					startIndex = i
				};

				dragList[i] = dragElement;
			}

			//finally, sort the dragList by selection, selected objects appear first in the list
			//selection order is preserved as well

			System.Array.Sort(dragList, (a, b) => {

				if (b.selected) {

					return a.selected ? a.startIndex.CompareTo(b.startIndex) : 1;
				}
				else if (a.selected) {

					return b.selected ? b.startIndex.CompareTo(a.startIndex) : -1;
				}

				return a.startIndex.CompareTo(b.startIndex);
			});

			return dragList;
		}

		private bool UpdateDragPosition(Vector2 position, Rect bounds, DragElement[] dragList) {

			//find new drag position

			int startIndex = 0;
			int endIndex = selection.Length - 1;

			float minOffset = dragList[startIndex].dragOffset;
			float maxOffset = dragList[endIndex].rect.height - dragList[endIndex].dragOffset;

			dragPosition = Mathf.Clamp(position.y, bounds.yMin + minOffset, bounds.yMax - maxOffset);

			if (Mathf.Abs(dragPosition - pressPosition) > 1) {

				dragDirection = (int)Mathf.Sign(dragPosition - pressPosition);
				return true;
			}

			return false;
		}

		private void ReorderDraggedElements(DragElement[] dragList) {

			//save the current expanded states on all elements. I don't see any other way to do this
			//MoveArrayElement does not move the foldout states, so... fun.

			for (int i = 0; i < dragList.Length; i++) {

				dragList[i].RecordState();
			}

			//sort list based on positions

			System.Array.Sort(dragList, (a, b) => a.desiredRect.center.y.CompareTo(b.desiredRect.center.y));

			selection.Sort((a, b) => {

				int d1 = GetDragIndexFromSelection(a);
				int d2 = GetDragIndexFromSelection(b);

				return dragDirection > 0 ? d1.CompareTo(d2) : d2.CompareTo(d1);
			});

			//swap the selected elements in the List

			int s = selection.Length;

			while (--s > -1) { 

				int newIndex = GetDragIndexFromSelection(selection[s]);

				selection[s] = newIndex;

				list.MoveArrayElement(dragList[newIndex].startIndex, newIndex);
			}

			//restore expanded states on items

			for (int i = 0; i < dragList.Length; i++) {

				dragList[i].RestoreState(list.GetArrayElementAtIndex(i));
			}
		}

		private int GetDragIndexFromSelection(int index) {

			return System.Array.FindIndex(dragList, t => t.startIndex == index);
		}

		private int GetSelectionIndex(Vector2 position) {

			int i, len = elementRects.Length;

			for (i = 0; i < len; i++) {

				Rect rect = elementRects[i];

				if (rect.Contains(position) || (i == 0 && position.y <= rect.yMin) || (i == len - 1 && position.y >= rect.yMax)) {

					return i;
				}
			}

			return -1;
		}

		private bool CanSelect(ListSelection selection) {

			return selection.Length > 0 ? selection.All(s => CanSelect(s)) : false;
		}

		private bool CanSelect(int index) {

			return index >= 0 && index < list.arraySize;
		}

		private bool CanSelect(Vector2 position) {

			return selection.Length > 0 ? selection.Any(s => IsPositionWithinElement(position, s)) : false;
		}

		private bool IsPositionWithinElement(Vector2 position, int index) {

			return CanSelect(index) ? elementRects[index].Contains(position) : false;
		}

		private bool IsElementExpandable(SerializedProperty element) {

			switch (elementDisplayType) {

				case ElementDisplayType.Auto:

					return element.hasVisibleChildren && IsTypeExpandable(element.propertyType);

				case ElementDisplayType.Expandable: return true;
				case ElementDisplayType.SingleLine: return false;
			}

			return false;
		}

		private bool IsTypeExpandable(SerializedPropertyType type) {
			
			switch (type) {

				case SerializedPropertyType.Generic:
				case SerializedPropertyType.Vector4:
				case SerializedPropertyType.Quaternion:
				case SerializedPropertyType.ArraySize:

					return true;

				default:

					return false;
			}
		}

		//
		// -- LIST STYLE --
		//

		static class Style {
			
			public static GUIContent iconToolbarPlus;
			public static GUIContent iconToolbarPlusMore;
			public static GUIContent iconToolbarMinus;
			public static GUIStyle draggingHandle;
			public static GUIStyle headerBackground;
			public static GUIStyle footerBackground;
			public static GUIStyle boxBackground;
			public static GUIStyle preButton;
			public static GUIStyle elementBackground;
			public static GUIStyle verticalLabel;
			public static GUIContent expandButton;
			public static GUIContent collapseButton;

			static Style() {

				iconToolbarPlus = EditorGUIUtility.IconContent("Toolbar Plus", "Add to list");
				iconToolbarPlusMore = EditorGUIUtility.IconContent("Toolbar Plus More", "Choose to add to list");
				iconToolbarMinus = EditorGUIUtility.IconContent("Toolbar Minus", "Remove selection from list");
				draggingHandle = new GUIStyle("RL DragHandle");
				headerBackground = new GUIStyle("RL Header");
				footerBackground = new GUIStyle("RL Footer");
				elementBackground = new GUIStyle("RL Element");
				elementBackground.border = new RectOffset(2, 3, 2, 3);
				verticalLabel = new GUIStyle(EditorStyles.label);
				verticalLabel.alignment = TextAnchor.MiddleLeft;
				verticalLabel.contentOffset = new Vector2(10, -3);
				boxBackground = new GUIStyle("RL Background");
				boxBackground.border = new RectOffset(6, 3, 3, 6);
				preButton = new GUIStyle("RL FooterButton");
				expandButton = EditorGUIUtility.IconContent("winbtn_win_max");
				collapseButton = EditorGUIUtility.IconContent("winbtn_win_min");
			}
		}

		//
		// -- DRAG ELEMENT --
		//

		struct DragElement {

			internal SerializedProperty property;
			internal int startIndex;
			internal float dragOffset;
			internal bool selected;
			internal Rect rect;
			internal Rect desiredRect;

			private bool isExpanded;
			private Dictionary<int, bool> states;

			internal bool Overlaps(Rect value, int index, int direction) {

				if (direction < 0 && index < startIndex) {

					return desiredRect.yMin < value.center.y;
				}
				else if (direction > 0 && index > startIndex) {

					return desiredRect.yMax > value.center.y;
				}

				return false;
			}

			internal void RecordState() {

				states = new Dictionary<int, bool>();
				isExpanded = property.isExpanded;

				Iterate(this, property, (DragElement e, SerializedProperty p, int index) => { e.states[index] = p.isExpanded; });
			}

			internal void RestoreState(SerializedProperty property) {

				property.isExpanded = isExpanded;

				Iterate(this, property, (DragElement e, SerializedProperty p, int index) => { p.isExpanded = e.states[index]; });
			}

			private static void Iterate(DragElement element, SerializedProperty property, System.Action<DragElement, SerializedProperty, int> action) {

				SerializedProperty copy = property.Copy();
				SerializedProperty end = copy.GetEndProperty();

				int index = 0;

				while (copy.NextVisible(true) && !SerializedProperty.EqualContents(copy, end)) {

					if (copy.hasVisibleChildren) {

						action(element, copy, index);
						index++;
					}
				}
			}
		}

		//
		// -- SLIDE GROUP --
		//

		class SlideGroup {

			private Dictionary<int, Rect> animIDs;

			public SlideGroup() {

				animIDs = new Dictionary<int, Rect>();
			}

			public Rect GetRect(int id, Rect r, float easing) {
				
				if (Event.current.type != EventType.Repaint) {
					
					return r;
				}

				if (!animIDs.ContainsKey(id)) {

					animIDs.Add(id, r);
					return r;
				}
				else {

					Rect rect = animIDs[id];

					if (rect.y != r.y) {

						float delta = r.y - rect.y;
						float absDelta = Mathf.Abs(delta);

						//if the distance between current rect and target is too large, then move the element towards the target rect so it reaches the destination faster

						if (absDelta > (rect.height * 2)) {

							r.y = delta > 0 ? r.y - rect.height : r.y + rect.height;
						}
						else if (absDelta > 0.5) {

							r.y = Mathf.Lerp(rect.y, r.y, easing);
						}

						animIDs[id] = r;
						HandleUtility.Repaint();
					}

					return r;
				}
			}

			public Rect SetRect(int id, Rect rect) {

				if (animIDs.ContainsKey(id)) {

					animIDs[id] = rect;
				}
				else {

					animIDs.Add(id, rect);
				}

				return rect;
			}
		}

		//
		// -- SELECTION --
		//

		class ListSelection : IEnumerable<int> {

			private List<int> indexes;

			internal int? firstSelected;

			public ListSelection() {

				indexes = new List<int>();
			}

			public ListSelection(int[] indexes) {

				this.indexes = new List<int>(indexes);
			}

			public int First {

				get { return indexes.Count > 0 ? indexes[0] : -1; }
			}

			public int Last {

				get { return indexes.Count > 0 ? indexes[indexes.Count - 1] : -1; }
			}

			public int Length {

				get { return indexes.Count; }
			}

			public int this[int index] {

				get { return indexes[index]; }
				set {

					int oldIndex = indexes[index];
					
					indexes[index] = value;

					if (oldIndex == firstSelected) {

						firstSelected = value;
					}
				}
			}

			public bool Contains(int index) {

				return indexes.Contains(index);
			}

			public void Clear() {

				indexes.Clear();
				firstSelected = null;
			}

			public void SelectWhenNoAction(int index, Event evt) {

				if (!EditorGUI.actionKey && !evt.shift) {

					Select(index);
				}
			}

			public void Select(int index) {

				indexes.Clear();
				indexes.Add(index);

				firstSelected = index;
			}
			
			public void Remove(int index) {

				if (indexes.Contains(index)) {

					indexes.Remove(index);
				}
			}			

			public void AppendWithAction(int index, Event evt) {

				if (EditorGUI.actionKey) {

					if (Contains(index)) {

						Remove(index);
					}
					else {

						Append(index);
						firstSelected = index;
					}
				}
				else if (evt.shift && indexes.Count > 0 && firstSelected.HasValue) {

					indexes.Clear();

					AppendRange(firstSelected.Value, index);
				}
				else if (!Contains(index)) {

					Select(index);
				}
			}

			public void Sort() {

				if (indexes.Count > 0) {

					indexes.Sort();
				}
			}

			public void Sort(System.Comparison<int> comparison) {

				if (indexes.Count > 0) {

					indexes.Sort(comparison);
				}
			}

			public int[] ToArray() {

				return indexes.ToArray();
			}

			public ListSelection Clone() {

				ListSelection clone = new ListSelection(ToArray());
				clone.firstSelected = firstSelected;

				return clone;
			}

			internal bool CanRevert(SerializedProperty list) {

				if (list.serializedObject.targetObjects.Length == 1) {

					for (int i = 0; i < Length; i++) {

						if (list.GetArrayElementAtIndex(this[i]).isInstantiatedPrefab) {

							return true;
						}
					}
				}

				return false;
			}

			internal void RevertValues(object userData) {

				SerializedProperty list = userData as SerializedProperty;

				for (int i = 0; i < Length; i++) {

					SerializedProperty property = list.GetArrayElementAtIndex(this[i]);

					if (property.isInstantiatedPrefab) {

						property.prefabOverride = false;						
					}
				}
				
				list.serializedObject.ApplyModifiedProperties();
				list.serializedObject.Update();

				HandleUtility.Repaint();
			}

			internal void Duplicate(SerializedProperty list) {

				int offset = 0;

				for (int i = 0; i < Length; i++) {
					
					this[i] += offset;

					list.GetArrayElementAtIndex(this[i]).DuplicateCommand();				
					list.serializedObject.ApplyModifiedProperties();
					list.serializedObject.Update();
					
					offset++;
				}				

				HandleUtility.Repaint();
			}

			internal void Delete(SerializedProperty list) {

				Sort();
				
				int i = Length;

				while (--i > -1) {

					list.GetArrayElementAtIndex(this[i]).DeleteCommand();
				}

				Clear();

				list.serializedObject.ApplyModifiedProperties();
				list.serializedObject.Update();

				HandleUtility.Repaint();
			}

			private void Append(int index) {

				if (index >= 0 && !indexes.Contains(index)) {

					indexes.Add(index);
				}
			}

			private void AppendRange(int from, int to) {

				int dir = (int)Mathf.Sign(to - from);

				if (dir != 0) {

					for (int i = from; i != to; i += dir) {

						Append(i);
					}
				}

				Append(to);
			}

			public IEnumerator<int> GetEnumerator() {

				return ((IEnumerable<int>)indexes).GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() {

				return ((IEnumerable<int>)indexes).GetEnumerator();
			}
		}

		//
		// -- EXCEPTIONS --
		//

		class InvalidListException : System.InvalidOperationException {

			public InvalidListException() : base("ReorderableList serializedProperty must be an array") {
			}
		}

		class MissingListExeption : System.ArgumentNullException {

			public MissingListExeption() : base("ReorderableList serializedProperty is null") {
			}
		}

		//
		// -- INTERNAL --
		//

		static class Internals {

			private static MethodInfo dragDropValidation;
			private static object[] dragDropValidationParams;
			private static MethodInfo appendDragDrop;
			private static object[] appendDragDropParams;

			static Internals() {

				dragDropValidation = System.Type.GetType("UnityEditor.EditorGUI, UnityEditor").GetMethod("ValidateObjectFieldAssignment", BindingFlags.NonPublic | BindingFlags.Static);
				appendDragDrop = typeof(SerializedProperty).GetMethod("AppendFoldoutPPtrValue", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			internal static Object ValidateObjectDragAndDrop(Object[] references, SerializedProperty property) {

				#if UNITY_2017_1_OR_NEWER
				dragDropValidationParams = GetParams(ref dragDropValidationParams, 4);
				dragDropValidationParams[0] = references;
				dragDropValidationParams[1] = null;
				dragDropValidationParams[2] = property;
				dragDropValidationParams[3] = 0;
				#else
				dragDropValidationParams = GetParams(ref dragDropValidationParams, 3);
				dragDropValidationParams[0] = references;
				dragDropValidationParams[1] = null;
				dragDropValidationParams[2] = property;
				#endif
				return dragDropValidation.Invoke(null, dragDropValidationParams) as Object;
			}

			internal static void AppendDragAndDropValue(Object obj, SerializedProperty list) {

				appendDragDropParams = GetParams(ref appendDragDropParams, 1);
				appendDragDropParams[0] = obj;
				appendDragDrop.Invoke(list, appendDragDropParams);
			}

			private static object[] GetParams(ref object[] parameters, int count) {

				if (parameters == null) {

					parameters = new object[count];
				}

				return parameters;
			}
		}
	}
}