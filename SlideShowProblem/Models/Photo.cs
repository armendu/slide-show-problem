using System.Collections.Generic;

namespace SlideShowProblem.Models
{
    public class Photo
    {
        public int ID { get; set; }
        public string[] Tags { get; set; }
        public Orientation Orientation { get; set; }
        public int TagCount { get; set; }
        public bool IsUsed { get; set; }
    }
}