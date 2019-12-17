using UnityEditor;
using UnityEngine.UI;

namespace EasyUI.UGuiExtension.Editor
{
    public static class RecyclerScrollRectMenuItems
    {
        [MenuItem("CONTEXT/ScrollRect/ToRecyclerScrollRect")]
        public static void ScrollRect2RecyclerScrollRect(MenuCommand menuCommand)
        {
            var scrollRect = menuCommand.context as ScrollRect;
            var parent = scrollRect.gameObject;

            var content = scrollRect.content;
            var viewport = scrollRect.viewport;
            var hbar = scrollRect.horizontalScrollbar;
            var hbarVisibility = scrollRect.horizontalScrollbarVisibility;
            var hbarSpacing = scrollRect.horizontalScrollbarSpacing;
            var vbar = scrollRect.verticalScrollbar;
            var vbarVisibility = scrollRect.verticalScrollbarVisibility;
            var vbarSpacing = scrollRect.verticalScrollbarSpacing;
            Undo.DestroyObjectImmediate(scrollRect);

            var rsr = Undo.AddComponent<RecyclerScrollRect>(parent);
            rsr.content = content;
            rsr.viewport = viewport;
            
            rsr.horizontalScrollbar = hbar;
            rsr.horizontalScrollbarVisibility = hbarVisibility;
            rsr.horizontalScrollbarSpacing = hbarSpacing;
            
            rsr.verticalScrollbar = vbar;
            rsr.verticalScrollbarVisibility = vbarVisibility;
            rsr.verticalScrollbarSpacing = vbarSpacing;
        }

        [MenuItem("CONTEXT/ScrollRect/ToRecyclerScrollRect", true)]
        public static bool ScrollRect2RecyclerScrollRectValidate(MenuCommand menuCommand)
        {
            return menuCommand.context is ScrollRect && !(menuCommand.context is RecyclerScrollRect);
        }
    }
}