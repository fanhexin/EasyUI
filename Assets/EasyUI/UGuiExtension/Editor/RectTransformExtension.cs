using UnityEngine;

namespace EasyUI.UGuiExtension.Editor
{
    public static class RectTransformExtension
    {
        static void SetVerticalStretchAnchor(RectTransform rectTransform, int y)
        {
            rectTransform.anchorMin = new Vector2(0, y);        
            rectTransform.anchorMax = new Vector2(1, y);        
            rectTransform.pivot = new Vector2(0.5f, y);
            rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
            rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
        }
        
        static void SetHorizontalStretchAnchor(RectTransform rectTransform, int x)
        {
            rectTransform.anchorMin = new Vector2(x, 0);        
            rectTransform.anchorMax = new Vector2(x, 1);        
            rectTransform.pivot = new Vector2(x, 0.5f);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
        }
        
        public static void SetStretchAnchorTop(this RectTransform rectTransform)
        {
            SetVerticalStretchAnchor(rectTransform, 1);
        } 
        
        public static void SetStretchAnchorBottom(this RectTransform rectTransform)
        {
            SetVerticalStretchAnchor(rectTransform, 0);
        } 
        
        public static void SetStretchAnchorLeft(this RectTransform rectTransform)
        {
            SetHorizontalStretchAnchor(rectTransform, 0);
        } 
        
        public static void SetStretchAnchorRight(this RectTransform rectTransform)
        {
            SetHorizontalStretchAnchor(rectTransform, 1);
        } 
    }
}