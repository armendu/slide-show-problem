using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlideShowProblem.Extensions;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    public class Solution
    {
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; }

        public Solution()
        {
            InterestFactor = 0;
            Slides = new List<Slide>();
        }

        public Solution(List<Slide> slides, int interestFactor)
        {
            InterestFactor = interestFactor;
            Slides = new List<Slide>(slides);
        }

        public void PrintSolution()
        {
            Console.WriteLine($"Number of slides: {this.Slides.Count}\n");
            Console.WriteLine($"Interest Factor: {this.InterestFactor}\n");
        }

    }

}