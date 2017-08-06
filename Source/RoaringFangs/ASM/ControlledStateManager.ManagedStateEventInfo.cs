/*
The MIT License (MIT)

Copyright (c) 2016 Roaring Fangs Entertainment

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;

namespace RoaringFangs.ASM
{
    public partial class ControlledStateManager
    {
        protected struct ManagedStateEventInfo<TArgs> : IManagedStateEventInfo
        {
            private readonly ManagedStateEventType _Type;
            public ManagedStateEventType Type { get { return _Type; } }

            public readonly TArgs Args;

            // An event type is not used in this case because handlers with different event args will need to be processed
            public readonly Action<ControlledStateManager, TArgs> Handler;

            public readonly Action<object, TArgs> SharedEventHandler;

            /// <summary>
            /// Used for exception-raising state invariants (allows for transactions / rollback operations)
            /// </summary>
            public readonly Action<ControlledStateManager, TArgs> Verifier;

            public void Process(ControlledStateManager manager)
            {
                Handler(manager, Args);
                SharedEventHandler.Invoke(manager, Args);
            }

            public void Verify(ControlledStateManager manager)
            {
                Verifier(manager, Args);
            }

            public ManagedStateEventInfo(
                ManagedStateEventType type,
                Action<ControlledStateManager, TArgs> verifier,
                Action<ControlledStateManager, TArgs> handler,
                Action<object, TArgs> shared_event_handler,
                TArgs args)
            {
                _Type = type;
                Verifier = verifier;
                Handler = handler;
                SharedEventHandler = shared_event_handler;
                Args = args;
            }
        }
    }
}