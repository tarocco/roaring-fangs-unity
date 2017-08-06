using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoaringFangs
{
    public class RedundantSceneException : Exception
    {
        public readonly string SceneName;
        public RedundantSceneException(string message, string scene_name) :
            base(message)
        {
            SceneName = scene_name;
        }
    }
}
