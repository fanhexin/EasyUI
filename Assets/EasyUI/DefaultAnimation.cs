using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyUI
{
    public abstract class DefaultAnimation : ScriptableObject
    {
        public abstract void PlayEnterAnim(UIPanel panel, Action complete = null);
        public abstract void PlayExitAnim(UIPanel panel, Action complete = null);
        public abstract void PlayDialogMaskEnterAnim(Image mask, Action complete = null);
        public abstract void PlayDialogMaskExitAnim(Image mask, Action complete = null);
    }
}