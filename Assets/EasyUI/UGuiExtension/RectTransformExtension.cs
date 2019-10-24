using UnityEngine;

namespace EasyUI.UGuiExtension.Extension
{
    public static class RectTransformExtension
    {
        static Vector3[] _rectCorners;
        
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            if (_rectCorners == null)
            {
                _rectCorners = new Vector3[4];
            }
            
            rectTransform.GetWorldCorners(_rectCorners);
            return Rect.MinMaxRect(_rectCorners[0].x, _rectCorners[0].y, _rectCorners[2].x, _rectCorners[2].y);
        }
    }
}