using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZFrameWork
{
    /// <summary>
    /// Sub UI Panel
    /// </summary>
    public abstract class BasePanel : BaseUI
    {
        protected virtual void OnShow() { }

        protected virtual void OnHide() { }
    }
}

