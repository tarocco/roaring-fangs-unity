using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoaringFangs.Utility;
using UnityEngine.Events;
using RoaringFangs.ASM;

namespace RoaringFangs
{
    public class NavigationBreadcrumbInfo : StateController
    {
        [SerializeField]
        private string _Tag;
        public override string Tag
        {
            get { return _Tag; }
            set { _Tag = value; }
        }

        [SerializeField]
        private string _TriggerName;
        public string TriggerName
        {
            get { return _TriggerName; }
            private set { _TriggerName = value; }
        }
    }
}