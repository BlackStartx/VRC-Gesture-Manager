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
#endif
                    return false;
                }
            }

            public static event UnityEditor.Undo.UndoRedoCallback UndoRedoPerformed
            {
                add
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.undoRedoPerformed += value;
#endif
                }
                remove
                {
#if UNITY_EDITOR
                    UnityEditor.Undo.undoRedoPerformed -= value;
#endif
                }
            }
        }
    }
}