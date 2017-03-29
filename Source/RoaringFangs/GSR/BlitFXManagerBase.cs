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

using RoaringFangs.Attributes;
using RoaringFangs.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RoaringFangs.GSR
{
    public abstract class BlitFxManagerBase : MonoBehaviour, ISerializationCallbackReceiver
    {
        public abstract IEnumerable<UpdateDirectiveBase> Directives();

        public void OnBeforeSerialize()
        {
            EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public void OnAfterDeserialize()
        {
        }

        [Serializable]
        public class UpdateDirectiveBase : ISerializationCallbackReceiver
        {
            [SerializeField]
            private String _FieldName;

            public String FieldName
            {
                get { return _FieldName; }
                set { _FieldName = value; }
            }

            [SerializeField]
            private BlitFx[] _Effects;

            public BlitFx[] Effects
            {
                get { return _Effects; }
                set { _Effects = value; }
            }

            public virtual void ApplyAll()
            {
                foreach (BlitFx effect in Effects)
                {
                    if (effect == null)
                        break;
                    if (effect.enabled)
                        Apply(effect);
                }
            }

            protected virtual void Apply(BlitFx effect)
            {
            }

            public virtual void OnBeforeSerialize()
            {
                EditorUtilities.OnBeforeSerializeAutoProperties(this);
            }

            public void OnAfterDeserialize()
            {
            }
        }

        [Serializable]
        public class UpdateDirectiveFloat : UpdateDirectiveBase
        {
            public float Value;

            public UpdateDirectiveFloat()
            {
                Value = default(float);
            }

            protected override void Apply(BlitFx effect)
            {
                effect.Material.SetFloat(FieldName, Value);
            }
        }

        [Serializable]
        public class UpdateDirectiveColor : UpdateDirectiveBase
        {
            public Color Value;

            public UpdateDirectiveColor()
            {
                Value = default(Color);
            }

            protected override void Apply(BlitFx effect)
            {
                effect.Material.SetColor(FieldName, Value);
            }
        }

        [Serializable]
        public class UpdateDirectiveTexture : UpdateDirectiveBase
        {
            [SerializeField, AutoProperty]
            private MonoBehaviour _Source;

            public ITexturable Source
            {
                get { return _Source as ITexturable; }
                set
                {
                    _Texture = null;
                    _Source = value as MonoBehaviour;
                }
            }

            [SerializeField, AutoProperty]
            private Texture _Texture;

            public Texture Texture
            {
                get
                {
                    return _Texture;
                }
                set
                {
                    _Source = null;
                    _Texture = value;
                }
            }

            public Texture ActiveTexture
            {
                get
                {
                    if (Source != null)
                        return Source.Texture;
                    return Texture;
                }
            }

            [SerializeField, AutoProperty]
            private MonoBehaviour[] _Destinations;

            public ITexturable[] Destinations
            {
                get { return _Destinations.Cast<ITexturable>().ToArray(); }
                private set { _Destinations = value.Cast<MonoBehaviour>().ToArray(); }
            }

            public override void OnBeforeSerialize()
            {
                //Source = Source;
                EditorUtilities.OnBeforeSerializeAutoProperties(this);
            }

            //public Vector2 Scale = Vector2.one;
            //public Vector2 Offset = Vector2.zero;
            public UpdateDirectiveTexture()
            {
                //SourceTexture = default(Texture);
            }

            protected override void Apply(BlitFx effect)
            {
                if (effect.Material.HasProperty(FieldName))
                {
                    var current_texture = effect.Material.GetTexture(FieldName);
                    if (current_texture != ActiveTexture)
                    {
                        effect.Material.SetTexture(FieldName, ActiveTexture);
                    }
                    //effect.Material.SetTextureScale(FieldName, Scale);
                    //effect.Material.SetTextureOffset(FieldName, Offset);
                }
            }

            protected void Apply(ITexturable texturable)
            {
                texturable.Texture = ActiveTexture;
            }

            public override void ApplyAll()
            {
                base.ApplyAll();
                foreach (ITexturable texturable in Destinations)
                {
                    if (texturable == null)
                        break;
                    Apply(texturable);
                }
            }
        }
    }
}