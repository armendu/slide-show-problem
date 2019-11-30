using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    static class Program
    {
        private static int _numberOfPhotosInCollection;

        static void Main(string[] args)
        {
            var fileLines = File.ReadAllLines("./input/c_memorable_moments.txt");

            _numberOfPhotosInCollection = int.Parse(fileLines[0]);

            List<Photo> photos = new List<Photo>();

            // Get all the info from the input file
            for (int i = 1; i <= _numberOfPhotosInCollection; i++)
            {
                var strippedLines = fileLines[i].Split(' ').Where(l => l != "").ToArray();
                photos.Add(new Photo
                {
                    ID = i - 1,
                    Orientation = Enum.Parse<Orientation>(strippedLines[0]),
                    TagCount = int.Parse(strippedLines[1]),
                    Tags = strippedLines[2..]
                });
            }

            Solution solution = new Solution();
            solution.TrySolution(photos);

            var watch = Stopwatch.StartNew();
            watch.Start();
            solution.HillClimbing();
            watch.Stop();

            solution.PrintSolution();
            Console.WriteLine($"Algorithm took {watch.Elapsed.TotalSeconds} seconds");

            Console.ReadLine();
        }
    }
}