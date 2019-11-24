using System;
using System.Collections.Generic;
using System.Linq;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    public class Solution
    {
        public const int NO_ITERATIONS = 500;

        public List<Slide> slides { get; set; }
        public int interestFactor { get; set; }

        public Solution()
        {
            this.interestFactor = 0;
            this.slides = new List<Slide>();
        }

        // Generates first solution on random basis
        public void TrySolution(List<Photo> photos)
        {
            Random random = new Random();

            // Change photos position
            photos.OrderBy(x => random.Next());

            // Number of photos
            int noPhotos = photos.Count;

            List<Photo> slidePhotos;

            int skippedPhoto = -1;

            // Iterate into photos and put them in list
            for (int i = 0; i < noPhotos; i++)
            {
                // If we have selected this photo before
                // Continue
                if (photos[i].ID == skippedPhoto)
                    continue;

                slidePhotos = new List<Photo>();


                // If current photo has horizontal orientation
                // Put that photo into one slide
                if (photos[i].Orientation == Orientation.H)
                {
                    slidePhotos.Add(photos[i]);
                }

                // If current photo has vertical orientation
                else
                {
                    // Add that photo
                    slidePhotos.Add(photos[i]);

                    // Try to get the next one with vertical orientation
                    Photo nextVerticalPhoto = GetNextVerticalPhoto(photos, i + 1);

                    // If there is one
                    // Add that on the list
                    // And mark that one as visited
                    if(nextVerticalPhoto != null)
                    {
                        slidePhotos.Add(nextVerticalPhoto);
                        skippedPhoto = nextVerticalPhoto.ID;
                    }
                }

                Slide oneSlide = new Slide(slidePhotos);

                this.slides.Add(oneSlide);
            }


        }

        // Returns the next vertical photo in list
        private Photo GetNextVerticalPhoto(List<Photo> photos, int start)
        {
            int noPhotos = photos.Count;

            // Control to not make index out of bounds
            if (start >= noPhotos)
                return null;

            for (int i = start; i < noPhotos; i++)
            {
                if (photos[i].Orientation == Orientation.V)
                    return photos[i];
            }

            return null;
        }


        // Hill Climbing Algorithm
        public void  HillClimbing()
        {
            Random random = new Random();

           

            // Try NO_ITERATIONS solutions
            for (int i = 0; i < NO_ITERATIONS; i++)
            {

                List<Slide> tmpSlides = this.slides;

                // For each round generate two positions to swap slides
                int position1, position2;

                position1 = random.Next(0, this.slides.Count);

                do
                {
                    position2 = random.Next(0, this.slides.Count);

                } while (position2 == position1);

                Slide firstSlide = this.slides[position1];
                Slide secondSlide = this.slides[position2];


                //if(firstSlide.Photos.Count == 1 || secondSlide.Photos.Count == 1)
                //{
                    tmpSlides[position1].Photos = secondSlide.Photos;
                    tmpSlides[position1].Tags= secondSlide.Tags;

                    tmpSlides[position2].Photos = firstSlide.Photos;
                    tmpSlides[position2].Tags = firstSlide.Tags;
                //}
                //else
                //{

                //TODO
                // When we pick two slides with two photos
                // Change photos between slides
                //}

                int currentInterestFactor = CalculateInterestFactor(tmpSlides);

                if(currentInterestFactor >= this.interestFactor)
                {
                    this.interestFactor = currentInterestFactor;
                    this.slides = tmpSlides;
                }

            }


        }

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int _interestFactor = 0;

            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = CommonTagsFactor(slides[i], slides[i + 1]);

                int slideAnotB = DifferentTagsFactor(slides[i], slides[i + 1]);
                int slideBnotA = DifferentTagsFactor(slides[i + 1], slides[i]);

                _interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            return _interestFactor;
        }

        private int CommonTagsFactor(Slide firstSlide, Slide secondSlide)
        {
            return firstSlide.Tags.Where(x => secondSlide.Tags.Contains(x)).Count();
        }

        private int DifferentTagsFactor(Slide firstSlide, Slide secondSlide)
        {
            return firstSlide.Tags.Where(x => !secondSlide.Tags.Contains(x)).Count();
        }

        public void PrintSolution()
        {
            Console.WriteLine($"Number of slides: { this.slides.Count() }\n");
            Console.WriteLine($"Interest Factor: { this.interestFactor }");
        }

    }
}
