using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    class Program
    {
        private static int _numberOfPhotosInCollection;

        static void Main(string[] args)
        {
            var fileLines = File.ReadAllLines("./input/a_example.txt");

            _numberOfPhotosInCollection = int.Parse(fileLines[0]);

            List<Photo> photos = new List<Photo>();

            // Get all the info from the input file
            for (int i = 1; i < fileLines.Length; i++)
            {
                var strippedLines = fileLines[i].Split(' ').Where(l => l != "").ToArray();
                photos.Add(new Photo
                {
                    ID = i-1,
                    Orientation = Enum.Parse<Orientation>(strippedLines[0]),
                    TagCount = int.Parse(strippedLines[1]),
                    Tags = strippedLines[2..]
                });
            }


            Solution solution = new Solution();
            solution.TrySolution(photos);

            solution.HillClimbing();

            solution.PrintSolution();

            // Get horizontal and vertical photos
            var horizontalPhotos = photos.Where(p => p.Orientation == Orientation.H).ToList().OrderBy(p => p.TagCount).ToList();
            var verticalPhotos = photos.Except(horizontalPhotos).ToList();


            Console.ReadLine();
        }
    }
}
