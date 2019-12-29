using System.Collections.Generic;

namespace SlideShowProblem.Models
{
    public class Slide
    {
        public int ID;
        public List<Photo> Photos { get; set; }
        public HashSet<string> Tags { get; set; }
        
        public Slide(List<Photo> photos, int _ID)
        {
            ID = _ID;
            Photos = new List<Photo>();
            foreach (var photo in photos)
            {
                Photos.Add(photo);
            }

            List<string> _initTags = new List<string>();
            foreach (var photo in photos)
            {
                foreach (var tg in photo.Tags)
                {
                    _initTags.Add(tg);
                }
            }

            Tags = new HashSet<string>(_initTags);
        }
    }
}