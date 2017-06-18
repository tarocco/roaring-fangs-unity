using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoaringFangs.Motion
{
    public interface IHasMovementVector
    {
        Vector3 MovementVector { get; }
    }
}
