using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;

namespace ZFrameWork
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class AutoUGUI : Attribute
    {
        protected String path;
        public AutoUGUI(string path = "")
        {
            this.path = path;
        }

        public String Path
        {
            get
            {
                return this.path;
            }
        }
    }
}

