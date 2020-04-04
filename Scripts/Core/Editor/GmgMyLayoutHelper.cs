using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GestureManager.Scripts.Core.Editor
{
	/**
	 * 	Hi!
	 * 	You're a curious one? :3
	 * 
	 *  Anyway, you're looking at some of the methods of my Core Unity Library OwO
	 *  I didn't included all of them (files and methods) because otherwise the UnityPackage would have been so much bigger >.<
     *
     *  P.S: Gmg stands for GestureManager UwU
	 */
	public static class GmgMyLayoutHelper
	{
		public static void ObjectField<T>(string label, T unityObject, Action<T> onObjectSet) where T : UnityEngine.Object
		{
			ObjectField(label, unityObject, onObjectSet, (oldObject, newObject) => { onObjectSet(newObject); }, oldObject => { onObjectSet(null); });
		}

		private static void ObjectField<T>(string label, T unityObject, Action<T> onObjectSet, Action<T, T> onObjectChange, Action<T> onObjectRemove) where T : UnityEngine.Object
		{
			var oldObject = unityObject;

			unityObject = (T) EditorGUILayout.ObjectField(label, unityObject, typeof(T), true, null);
			if (oldObject != unityObject)
			{
				if (oldObject == null)
					onObjectSet(unityObject);
				else if (unityObject == null)
					onObjectRemove(oldObject);
				else
					onObjectChange(oldObject, unityObject);
			}
		}

		/**
		 * 	ToolBar
		 * 	ToolBar
		 * 	ToolBar
		 */

		private static readonly Dictionary<object, MyToolbarHeader> ToolbarDictionary = new Dictionary<object, MyToolbarHeader>();
		
		public static void MyToolbar(object id, MyToolbarRow[] rows)
		{
			if (!ToolbarDictionary.ContainsKey(id))
			{
				ToolbarDictionary[id] = new MyToolbarHeader(rows);
			}

			var selected = GUILayout.Toolbar(ToolbarDictionary[id].GetSelected(), ToolbarDictionary[id].GetTitles());
			ToolbarDictionary[id].SetSelected(selected);
			ToolbarDictionary[id].ShowSelected();
		}
		
		private class MyToolbarHeader
		{
			private int _selected;
			private readonly string[] _titles;
			private readonly MyToolbarRow[] _rows;

			public MyToolbarHeader(MyToolbarRow[] rows)
			{
				_selected = 0;
				_titles = rows.Select(row => row.GetTitle()).ToArray();
				_rows = rows;
			}

			public int GetSelected()
			{
				return _selected;
			}

			public void SetSelected(int newSelected)
			{
				if (_selected != newSelected)
					_rows[newSelected].OnStartFocus();
				_selected = newSelected;
			}

			public string[] GetTitles()
			{
				return _titles;
			}

			public void ShowSelected()
			{
				_rows[_selected].Show();
			}
		}
		
		public class MyToolbarRow
		{
			private readonly string _title;
			private readonly Action _willShow;
			private readonly Action _onStartFocus;

			private MyToolbarRow(string title, Action willShow, Action onStartFocus)
			{
				_title = title;
				_willShow = willShow;
				_onStartFocus = onStartFocus;
			}

			public MyToolbarRow(string title, Action willShow) : this(title, willShow, () => { })
			{
			}

			public void Show()
			{
				_willShow();
			}

			public void OnStartFocus()
			{
				_onStartFocus();
			}

			internal string GetTitle()
			{
				return _title;
			}
		}
	}
}
