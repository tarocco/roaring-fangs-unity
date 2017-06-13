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

        [Serializable]
        public abstract class UpdateDirectiveBase : ISerializationCallbackReceiver
        {
            [SerializeField, AutoProperty]
            private string _FieldName;

            public string FieldName
            {
                get { return _FieldName; }
                set { _FieldName = value; }
            }

            [SerializeField]
            private BlitFxBase[] _Effects = {};

            public BlitFxBase[] Effects
            {
                get { return _Effects; }
                set { _Effects = value; }
            }

            public virtual void ApplyAll()
            {
                foreach (BlitFxBase effect in Effects)
                {
                    if (effect == null)
                        break;
                    if (effect.enabled)
                        Apply(effect);
                }
            }

            protected abstract void Apply(BlitFxBase effect);

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

            protected override void Apply(BlitFxBase effect)
            {
                var material = effect.Material;
                if(material)
                    material.SetFloat(FieldName, Value);
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

            protected override void Apply(BlitFxBase effect)
            {
                var material = effect.Material;
                if(material)
                    material.SetColor(FieldName, Value);
            }
        }

        [Serializable]
        public class UpdateDirectiveTexture : UpdateDirectiveBase
        {
            [SerializeField, AutoProperty]
            private MonoBehaviour _Source;

            public ITexturable<RenderTexture> Source
            {
                get { return (ITexturable<RenderTexture>)_Source; }
                set
                {
                    //_Texture = null;
                    _Source = (MonoBehaviour)value;
                }
            }

            //[SerializeField, AutoProperty]
            //private RenderTexture _Texture;

            //public RenderTexture Texture
            //{
            //    get
            //    {
            //        return _Texture;
            //    }
            //    set
            //    {
            //        _Source = null;
            //        _Texture = value;
            //    }
            //}

            public RenderTexture ActiveTexture
            {
                get
                {
                    if (Source != null)
                        return Source.Texture;
                    return null;
                    //return Texture;
                }
            }

            [SerializeField, AutoProperty]
            private MonoBehaviour[] _Destinations;

            public ITexturable<RenderTexture>[] Destinations
            {
                get { return _Destinations.Cast<ITexturable<RenderTexture>>().ToArray(); }
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

            protected override void Apply(BlitFxBase effect)
            {
                var material = effect.Material;
                if (material == null)
                    return;
                if (!material.HasProperty(FieldName))
                    return;
                var current_texture = material.GetTexture(FieldName);
                if (current_texture != ActiveTexture)
                {
                    material.SetTexture(FieldName, ActiveTexture);
                }
                //effect.Material.SetTextureScale(FieldName, Scale);
                //effect.Material.SetTextureOffset(FieldName, Offset);
            }

            protected void Apply(ITexturable<RenderTexture> texturable)
            {
                texturable.Texture = ActiveTexture;
            }

            public override void ApplyAll()
            {
                base.ApplyAll();
                foreach (var texturable in Destinations)
                {
                    if (texturable == null)
                        break;
                    Apply(texturable);
                }
            }
        }

        public virtual void OnBeforeSerialize()
        {
            EditorUtilities.OnBeforeSerializeAutoProperties(this);
        }

        public virtual void OnAfterDeserialize()
        {
        }
    }
}