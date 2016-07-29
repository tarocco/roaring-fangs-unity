// * BasicFSM.cs
// * Author: Will Matterer <will.matterer@gmail.com>
// *
// * DESCRIPTION:  A basic finite state machine with approprate objects
// *               States have 3 possible functions:
// *                    OnEnter() -- called once on entering the state
// *                    WhileIn() -- called repeatedly while in the state
// *                    OnExit() -- called once leaving the state
// *
// * REQUIREMENTS: Create a inherited class, and add functionality there
// *               Inherited class needs to be called on a GameObject (does not inherit from MonoBehaviour)
// *

using System;

namespace RoaringFangs.FSM
{
    public interface IStateManager<TStateEnum> : IDisposable
        where TStateEnum : struct, IConvertible
    {
        void ChangeState(TStateEnum next_state);

        //bool CheckState(int id);
    }
}