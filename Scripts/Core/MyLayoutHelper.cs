using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GestureManager.Scripts.Core
{
	/**
	 * 	Hi!
	 * 	You're a curious one? :3
	 * 
	 *  Anyway, you're looking at some of the methods of my Core Unity Library OwO
	 *  I didn't included all of them (files and methods) because otherwise the UnityPackage would have been so much bigger >.<
	 */
	public static class MyLayoutHelper
	{
		public static void ObjectField<T>(string label, T unityObject, Action<T> onObjectSet) where T : Object
		{
			ObjectField(label, unityObject, onObjectSet, (oldObject, newObject) => { onObjectSet(newObject); }, oldObject => { onObjectSet(null); });
		}

		private static void ObjectField<T>(string label, T unityObject, Action<T> onObjectSet, Action<T, T> onObjectChange, Action<T> onObjectRemove) where T : Object
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
		
		/*
		 * 	Toggle
		 * 	Toggle
		 * 	Toggle
		 */

		public delegate void OnToggle(bool newValue);

        public static void Toggle(bool value, string label, OnToggle onToggle)
        {
            var newValue = GUILayout.Toggle(value, label);
            if (newValue != value)
            {
                onToggle(newValue);
            }
        }
		
		/**
		 * 	ToolBar
		 * 	ToolBar
		 * 	ToolBar
		 */

		private static readonly Dictionary<string, MyToolbarHeader> ToolbarDictionary = new Dictionary<string, MyToolbarHeader>();
		
		public static void MyToolbar(string id, MyToolbarRow[] rows)
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
			private int selected;
			private readonly string[] titles;
			private readonly MyToolbarRow[] rows;

			public MyToolbarHeader(MyToolbarRow[] rows)
			{
				selected = 0;
				titles = rows.Select(row => row.GetTitle()).ToArray();
				this.rows = rows;
			}

			public int GetSelected()
			{
				return selected;
			}

			public void SetSelected(int newSelected)
			{
				if (selected != newSelected)
					rows[newSelected].OnStartFocus();
				selected = newSelected;
			}

			public string[] GetTitles()
			{
				return titles;
			}

			public void ShowSelected()
			{
				rows[selected].Show();
			}
		}
		
		public class MyToolbarRow
		{
			private readonly string title;
			private readonly Action willShow;
			private readonly Action onStartFocus;

			private MyToolbarRow(string title, Action willShow, Action onStartFocus)
			{
				this.title = title;
				this.willShow = willShow;
				this.onStartFocus = onStartFocus;
			}

			public MyToolbarRow(string title, Action willShow) : this(title, willShow, () => { })
			{
			}

			public void Show()
			{
				willShow();
			}

			public void OnStartFocus()
			{
				onStartFocus();
			}

			internal string GetTitle()
			{
				return title;
			}
		}
	}
}
