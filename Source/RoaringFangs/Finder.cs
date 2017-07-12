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
using System.Linq;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace RoaringFangs
{
    public class Finder : IDisposable
    {
        public class FinderEncounteredErrorsException : Exception
        {
            public FinderEncounteredErrorsException(string message) :
                base(message)
            {
            }
        }
        public class GameObjectNotFoundException : Exception
        {
            public GameObjectNotFoundException(string message) :
                base(message)
            {
            }
        }

        public class ComponentNotFoundException : Exception
        {
            public ComponentNotFoundException(string message) :
                base(message)
            {
            }
        }

        private T FindObjectOfAnyType<T>()
            where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(UnityObject)))
                return UnityObject.FindObjectOfType(typeof(T)) as T;
            return UnityObject.FindObjectsOfType<UnityObject>()
               .OfType<T>()
               .FirstOrDefault();
        }

        private T[] FindObjectsOfAnyType<T>()
            where T : class
        {
            if (typeof(T).IsAssignableFrom(typeof(UnityObject)))
                return UnityObject.FindObjectsOfType(typeof(T)) as T[];
            return UnityObject.FindObjectsOfType<UnityObject>()
                .OfType<T>()
                .ToArray();
        }

        /// <summary>
        /// Searches all loaded scenes for the first qualifying component of
        /// type <typeparamref name="T"/>.
        /// </summary>
        ///
        /// <exception cref="ComponentNotFoundException">
        /// Thrown when no qualifying component could be found in any loaded scene.
        /// </exception>
        public T FindComponent<T>()
            where T : class
        {
            var component = FindObjectOfAnyType<T>();
            if (component == null)
                throw new ComponentNotFoundException(
                    "Could not find " + typeof(T) +
                    " component on any object in any loaded scene");
            return component;
        }

        /// <summary>
        /// Searches all loaded scenes for the first qualifying component of
        /// type <typeparamref name="T"/>.
        ///
        /// If no qualifying component is found, an error is logged by
        /// <see cref="Debug.LogError(object)"/>.
        /// </summary>
        public T FindComponentOrLogError<T>()
            where T : class
        {
            try
            {
                return FindComponent<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                HasErrors = true;
                return null;
            }
        }

        /// <summary>
        /// Searches all loaded scenes for all qualifying components of type
        /// <typeparamref name="T"/>.
        /// </summary>
        public T[] FindComponents<T>()
            where T : class
        {
            return FindObjectsOfAnyType<T>();
        }

        /// <summary>
        /// Searches all loaded scenes for the first qualifying component of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.name"/> matching <paramref name="name"/>.
        /// </summary>
        ///
        /// <exception cref="GameObjectNotFoundException">
        /// Thrown when no matching <see cref="GameObject"/> could be found.
        /// </exception>
        ///
        /// <exception cref="ComponentNotFoundException">
        /// Thrown when no qualifying component could be found on the matching
        /// <see cref="GameObject"/>.
        /// </exception>
        public T FindComponent<T>(string name)
            where T : class
        {
            var game_object = GameObject.Find(name);
            if (game_object == null)
                throw new GameObjectNotFoundException(
                    "Could not find game object named \"" + name + "\"");
            // GameObject.GetComponent is polymorphic
            var component = game_object.GetComponent<T>();
            if (component == null)
                throw new ComponentNotFoundException(
                    "Could not find " + typeof(T) +
                    " component on game object named \"" + name + "\"");
            return component;
        }

        /// <summary>
        /// Searches all loaded scenes for the first qualifying component of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.tag"/> matching <paramref name="tag"/>.
        /// </summary>
        ///
        /// <exception cref="GameObjectNotFoundException">
        /// Thrown when no matching <see cref="GameObject"/> could be found.
        /// </exception>
        ///
        /// <exception cref="ComponentNotFoundException">
        /// Thrown when no qualifying component could be found on the matching
        /// <see cref="GameObject"/>.
        /// </exception>
        public T FindComponentByTag<T>(string tag)
            where T : class
        {
            var game_object = GameObject.FindGameObjectWithTag(tag);
            if (game_object == null)
                throw new GameObjectNotFoundException(
                    "Could not find game object with tag \"" +
                    tag + "\"");
            // GameObject.GetComponent is polymorphic
            var component = game_object.GetComponent<T>();
            if (component == null)
                throw new ComponentNotFoundException(
                    "Could not find " + typeof(T) +
                    " component on game object with tag \"" + tag + "\"");
            return component;
        }

        /// <summary>
        /// Searches all loaded scenes for the first qualifying component of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.name"/> matching <paramref name="name"/>.
        ///
        /// If a matching <see cref="GameObject"/> with a qualifying component
        /// is not found, an error is logged by
        /// <see cref="Debug.LogError(object)"/>.
        /// </summary>
        public T FindComponentOrLogError<T>(string name)
            where T : class
        {
            try
            {
                return FindComponent<T>(name);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                HasErrors = true;
                return null;
            }
        }

        /// <summary>
        /// Searches loaded all scenes for the first qualifying component of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.tag"/> matching <paramref name="tag"/>.
        ///
        /// If no matching <see cref="GameObject"/> or qualifying component is
        /// found, an error is logged by <see cref="Debug.LogError(object)"/>.
        /// </summary>
        public T FindComponentByTagOrLogError<T>(string tag)
            where T : class
        {
            try
            {
                return FindComponentByTag<T>(tag);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                HasErrors = true;
                return null;
            }
        }

        /// <summary>
        /// Searches all loaded scenes for all qualifying components of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.name"/> matching <paramref name="name"/>.
        /// </summary>
        ///
        /// <exception cref="GameObjectNotFoundException">
        /// Thrown when no matching <see cref="GameObject"/> could be found.
        /// </exception>
        public T[] FindComponents<T>(string name)
            where T : class
        {
            var game_object = GameObject.Find(name);
            if (game_object == null)
                throw new GameObjectNotFoundException(
                    "Could not find game object named \"" + name + "\"");
            // GameObject.GetComponent is polymorphic
            var components = game_object.GetComponents<T>();
            return components;
        }

        /// <summary>
        /// Searches all loaded scenes for all qualifying components of
        /// type <typeparamref name="T"/> on **ALL** <see cref="GameObject"/>s
        /// with <see cref="GameObject.tag"/> matching <paramref name="tag"/>.
        /// </summary>
        public T[] FindComponentsByTag<T>(string tag)
            where T : class
        {
            var game_object = GameObject.FindGameObjectsWithTag(tag);
            // GameObject.GetComponent is polymorphic
            var components = game_object
                .SelectMany(o => o.GetComponents<T>())
                .ToArray();
            return components;
        }

        /// <summary>
        /// Searches all loaded scenes for all qualifying components of
        /// type <typeparamref name="T"/> on the first <see cref="GameObject"/>
        /// with <see cref="GameObject.name"/> matching <paramref name="name"/>.
        /// </summary>
        ///
        /// If a matching <see cref="GameObject"/> is not found, an error is
        /// logged by  <see cref="Debug.LogError(object)"/>.
        public T[] FindComponentsOrLogError<T>(string name)
            where T : class
        {
            try
            {
                return FindComponents<T>(name);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                HasErrors = true;
                return null;
            }
        }

        public bool HasErrors
        {
            get; protected set;
        }
        public Finder()
        {
            HasErrors = false;
        }

        public void Dispose()
        {
            if (HasErrors)
                throw new FinderEncounteredErrorsException(
                    "Finder encountered errors. " +
                    "Please check the error log for details.");
        }
    }
}