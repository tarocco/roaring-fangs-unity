/*
The MIT License (MIT)

Copyright (c) 2017 Roaring Fangs LLC

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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoaringFangs.Editor
{
    public static class BuildManager
    {
        public enum ConfigurationType
        {
            /// <summary>
            /// Build the game, ready to play FOR REAL
            /// </summary>
            Release,

            /// <summary>
            /// Development build mode of the game for testing and debugging
            /// </summary>
            DevRelease,

            /// <summary>
            /// Development build mode of the development scenes for internal testing
            /// </summary>
            Development
        }

        public struct BuildSettings
        {
            /// <summary>
            /// Which configuration to use for the build
            /// </summary>
            public ConfigurationType ConfigurationType;
        }

        public static BuildTarget GetPrimaryBuildTarget(string platform_name)
        {
            switch (platform_name.ToLower())
            {
                case "win32":
                    return BuildTarget.StandaloneWindows;

                case "win64":
                    return BuildTarget.StandaloneWindows64;

                case "linux64":
                    return BuildTarget.StandaloneLinux64;

                case "mac":
                    return BuildTarget.StandaloneOSXIntel64;

                case "webgl":
                    return BuildTarget.WebGL;

                case "switch":
                    return BuildTarget.Switch;

                default:
                    return BuildTarget.NoTarget;
            }
        }

        [Serializable]
        public struct ConfigurationOptions
        {
            public string MainScene;
            public string[] Scenes;

            public override string ToString()
            {
                return "<ConfigurationOptions MainScene: \"" + MainScene + "\">";
            }
        }

        // TODO: use a 3rd party JSON parser so we can use a dictionary instead...
        [Serializable]
        public class ConfigurationsFile
        {
            public ConfigurationOptions Release, DevRelease, Development;

            public ConfigurationOptions GetConfiguration(ConfigurationType type)
            {
                switch (type)
                {
                    default:
                    case ConfigurationType.Release:
                        return Release;

                    case ConfigurationType.DevRelease:
                        return DevRelease;

                    case ConfigurationType.Development:
                        return Development;
                }
            }
        }

        public const string BuildLogPrefix = "Build Log - ";

        public static IEnumerable<FileInfo> GetFileInfos(
            string path_prefix,
            string pattern,
            params string[] paths)
        {
            foreach (var relative_path in paths)
            {
                var path = path_prefix + "/" + relative_path;
                // TODO: implement globbing or similar path pattern matching
                if (File.Exists(path))
                {
                    yield return new FileInfo(path);
                }
                else if (Directory.Exists(path))
                {
                    var directory_info = new DirectoryInfo(path);
                    foreach (var file_info in directory_info.GetFiles(pattern))
                        yield return file_info;
                }
                else
                {
                    throw new FileNotFoundException("File not found: " + path, path);
                }
            }
        }

        public static IEnumerable<string> GetFilePaths(
            string path_prefix,
            string pattern,
            params string[] paths)
        {
            var pwd_uri = new Uri(Environment.CurrentDirectory + "/");
            return GetFileInfos(path_prefix, pattern, paths)
                .Select(i => Uri.UnescapeDataString(pwd_uri.MakeRelativeUri(new Uri(i.FullName)).OriginalString));
        }

        public static string[] FindScenes(params string[] paths)
        {
            return GetFilePaths("Assets", "*.unity", paths).ToArray();
        }

        /// <exception cref="ArgumentException">
        /// Thrown when argument is not found in <paramref name="args"/>
        /// </exception>
        private static string GetArgumentValue(
            IList<string> args,
            string argument)
        {
            for (var i = 0; i < args.Count - 1; i++)
            {
                if (args[i] != argument)
                    continue;
                return args[i + 1];
            }
            throw new ArgumentException("Argument not found", argument);
        }

        private static bool TryGetArgumentValue(
            IList<string> args,
            string argument,
            out string value)
        {
            for (var i = 0; i < args.Count - 1; i++)
            {
                if (args[i] != argument)
                    continue;
                value = args[i + 1];
                return true;
            }
            value = null;
            return false;
        }

        private static string GetArgumentValueOrDefault(
            IList<string> args,
            string argument,
            string default_value = default(string))
        {
            string value;
            if (TryGetArgumentValue(args, argument, out value))
                return value;
            return default_value;
        }

        private static ConfigurationsFile ParseConfigurationsJSON(
            string configurations_json)
        {
            try
            {
                return JsonUtility.FromJson<ConfigurationsFile>(configurations_json);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not read configurations file", ex);
            }
        }

        private static ConfigurationsFile ParseConfigurationsFile(
            string configurations_file_path)
        {
            string configurations_file_text;
            try
            {
                configurations_file_text = File.ReadAllText(configurations_file_path);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("Could not find configurations file: " + configurations_file_path, ex);
            }
            return ParseConfigurationsJSON(configurations_file_text);
        }

        private static string GetProductFileName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return PlayerSettings.productName + ".exe";

                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
                case BuildTarget.StandaloneOSXUniversal:
                    return PlayerSettings.productName + ".app";

                default:
                    return PlayerSettings.productName;
            }
        }

        public static void ParseOptionsAndFindScenes(
            string[] args,
            out BuildPlayerOptions player_options,
            out BuildSettings settings)
        {
            try
            {
                ConfigurationType configuration_type;
                string main_scene;
                string[] scene_paths;
                {
                    var path = GetArgumentValueOrDefault(args, "-config", "configurations.json");
                    var file = ParseConfigurationsFile(path);
                    var type_str = GetArgumentValue(args, "-configuration");
                    configuration_type = (ConfigurationType)Enum.Parse(typeof(ConfigurationType), type_str);
                    //var configuration_options = file.Configurations[configuration_type];
                    var configuration_options = file.GetConfiguration(configuration_type);
                    main_scene = configuration_options.MainScene;
                    scene_paths = configuration_options.Scenes;
                }

                var platform = GetArgumentValue(args, "-platform");
                var target = GetPrimaryBuildTarget(platform);
                var product_file_name = GetProductFileName(target);
                var build_path = "Build/" + platform + "/" + configuration_type;
                var location_path_name =
                    GetArgumentValueOrDefault(args, "-buildPath", build_path) +
                    "/" +
                    product_file_name;

                // Locate the scenes to be used for this build configuration
                string[] configuration_scenes;
                try
                {
                    configuration_scenes =
                        FindScenes(main_scene)
                            .Concat(FindScenes(scene_paths)).
                            ToArray();
                }
                catch (FileNotFoundException ex)
                {
                    throw new Exception("Scene file or directory not found: " + ex.FileName);
                }

                player_options = new BuildPlayerOptions()
                {
                    locationPathName = location_path_name,
                    target = target,
                    scenes = configuration_scenes,
                };

                settings = new BuildSettings()
                {
                    ConfigurationType = configuration_type,
                };
            }
            catch (ArgumentException ex)
            {
                throw new Exception("Missing required build argument: " + ex.ParamName, ex);
            }
        }

        public static void BuildWithOptions(
            BuildPlayerOptions player_options,
            BuildSettings settings)
        {
            var target = player_options.target;
            var configuration = settings.ConfigurationType;
            // Level names without .unity extension
            var scene_names = player_options.scenes
                .Select(s => s.Remove(s.IndexOf(".unity", StringComparison.Ordinal)));

            Debug.Log(DateTime.UtcNow.ToLongDateString());
            Debug.Log("Starting build process for " + PlayerSettings.productName);
            Debug.Log("-----");
            Debug.Log("Configuration: " + configuration);
            Debug.Log("Target: " + target);
            Debug.Log("Number of scenes: " + player_options.scenes.Length);
            Debug.Log("Scenes");
            Debug.Log("-----");

            foreach (var scene_name in scene_names)
                Debug.Log(" - " + scene_name);

            Debug.Log("");

            var error = BuildPipeline.BuildPlayer(player_options);
            if (error.Length != 0)
                throw new Exception("Build Error:" + error);
        }

        public static void Build()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                BuildPlayerOptions player_options;
                BuildSettings settings;
                ParseOptionsAndFindScenes(args, out player_options, out settings);
                BuildWithOptions(player_options, settings);
                Debug.Log("Build successful");
            }
            catch (Exception ex)
            {
                Debug.LogError("ERROR");
                Debug.LogError("-----");
                Debug.LogError(ex.GetType() + ": " + ex.Message);
                Debug.LogError(ex.StackTrace);
            }
        }
    }
}
