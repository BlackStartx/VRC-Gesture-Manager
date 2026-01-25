namespace BlackStartX.GestureManager.Library
{
    public static class GmgEditorCallbacks
    {
        public static class Undo
        {
            public static bool IsProcessing
            {
                get
                {
#if UNITY_EDITOR
                    return UnityEditor.Undo.isProcessing;
#else
                    return false;
#endif
                }
            }

#if UNITY_EDITOR
            public static event UnityEditor.Undo.UndoRedoCallback UndoRedoPerformed
            {
                add => UnityEditor.Undo.undoRedoPerformed += value;
                remove => UnityEditor.Undo.undoRedoPerformed -= value;
            }
#else
            public static event System.Action UndoRedoPerformed
            {
                add => _ = value;
                remove => _ = value;
            }
#endif
        }
    }
}