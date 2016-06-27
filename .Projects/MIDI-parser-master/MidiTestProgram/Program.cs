using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Midi;

namespace MidiTestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string file_path = args[0];
                var file_stream = File.OpenRead(file_path);
                var midi = Midi.FileParser.parse(file_stream);
                foreach (var track in midi.tracks)
                {
                    foreach (var e in track.events)
                    {
                        Debug.WriteLine(e);
                    }
                }
            }
            else
            {
                Debug.WriteLine(String.Format("usage: {0} file.mid", args[0]));
            }
            //Console.ReadKey();
        }
    }
}
