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
        private const int NO_ITERATIONS = 500;
        private readonly Random _random = new Random();
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; }

        public Solution()
        {
            InterestFactor = 0;
            Slides = new List<Slide>();
        }

        // Generates first solution on random basis
        public void TrySolution(List<Photo> photos)
        {
            // Change photos position
            photos.Shuffle(_random);

            //TODO
            //Add Heuristic for initial solution 

            // Number of photos
            int noPhotos = photos.Count;

            List<Photo> slidePhotos;

            int photoToSkipId = -1;

            // Iterate into photos and put them in list
            for (int i = 0; i < noPhotos; i++)
            {
                // If we have selected this photo before
                // Continue
                if (photos[i].ID == photoToSkipId)
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
                    if (nextVerticalPhoto != null)
                    {
                        slidePhotos.Add(nextVerticalPhoto);
                        photoToSkipId = nextVerticalPhoto.ID;
                    }
                }

                Slide oneSlide = new Slide(slidePhotos);

                this.Slides.Add(oneSlide);
            }

            // Calculate fitness and save it
            InterestFactor = CalculateInterestFactor(Slides);
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
        public void HillClimbing()
        {
            List<Slide> tmpSlides;

            /*
            // Try NO_ITERATIONS solutions
            for (int i = 0; i < NO_ITERATIONS; i++)
            {
                tmpSlides = new List<Slide>();
                tmpSlides.AddRange(Slides);

                // For each round generate two positions to swap slides
                int position1 = _random.Next(0, this.Slides.Count);
                int position2;

                do
                {
                    position2 = _random.Next(0, this.Slides.Count);
                } while (position2 == position1);

                Slide firstSlide = new Slide(Slides[position1].Photos);
                Slide secondSlide = new Slide(Slides[position2].Photos);

                // Create new slides
                tmpSlides[position1] = new Slide(secondSlide.Photos);
                tmpSlides[position2] = new Slide(firstSlide.Photos);

                //TODO
                // When we pick two slides with two photos
                // Change photos between slides
                //}

                int currentInterestFactor = CalculateInterestFactor(tmpSlides);

                if (currentInterestFactor >= InterestFactor)
                {
                    InterestFactor = currentInterestFactor;
                    Slides = new List<Slide>(tmpSlides);
                }
            }
            */

            var watch = Stopwatch.StartNew();
            watch.Start();
            while (watch.Elapsed < TimeSpan.FromSeconds(60))
            {
                tmpSlides = new List<Slide>();
                tmpSlides.AddRange(Slides);

                // For each round generate two positions to swap slides
                int position1 = _random.Next(0, this.Slides.Count);
                int position2;

                do
                {
                    position2 = _random.Next(0, this.Slides.Count);
                } while (position2 == position1);

                Slide firstSlide = new Slide(Slides[position1].Photos);
                Slide secondSlide = new Slide(Slides[position2].Photos);
                
                // Create new slides
                tmpSlides[position1] = new Slide(secondSlide.Photos);
                tmpSlides[position2] = new Slide(firstSlide.Photos);

                //TODO
                // When we pick two slides with two photos
                // Change photos between slides
                //}

                int currentInterestFactor = CalculateInterestFactor(tmpSlides);

                if (currentInterestFactor >= InterestFactor)
                {
                    InterestFactor = currentInterestFactor;
                    Slides = new List<Slide>(tmpSlides);
                }
            }

            watch.Stop();
        }


        // Hill Climbing Algorithm
        public void GuidedLocalSearch()
        {
            List<Slide> tmpSlides;

            var watch = Stopwatch.StartNew();
            watch.Start();
            while (watch.Elapsed < TimeSpan.FromSeconds(60))
            {
                tmpSlides = new List<Slide>();
                tmpSlides.AddRange(Slides);

                // For each round generate two positions to swap slides
                int position1 = _random.Next(0, this.Slides.Count);
                int position2;

                do
                {
                    position2 = _random.Next(0, this.Slides.Count);
                } while (position2 == position1);

                Slide firstSlide = new Slide(Slides[position1].Photos);
                Slide secondSlide = new Slide(Slides[position2].Photos);

                int currentInterestFactor = InterestFactor - DeltaFactor(tmpSlides, position1, position2);

                // Create new slides
                tmpSlides[position1] = new Slide(secondSlide.Photos);
                tmpSlides[position2] = new Slide(firstSlide.Photos);

                currentInterestFactor = currentInterestFactor + DeltaFactor(tmpSlides, position1, position2);

                //TODO
                // When we pick two slides with two photos
                // Change photos between slides
                //}

                //int currentInterestFactor = CalculateInterestFactor(tmpSlides);

                if (currentInterestFactor >= InterestFactor)
                {
                    InterestFactor = currentInterestFactor;
                    Slides = new List<Slide>(tmpSlides);
                }
            }

            watch.Stop();
        }
     

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int interestFactor = 0;

            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = CommonTagsFactor(slides[i], slides[i + 1]);

                int slideAnotB = DifferentTagsFactor(slides[i], slides[i + 1]);
                int slideBnotA = DifferentTagsFactor(slides[i + 1], slides[i]);

                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            return interestFactor;
        }

        private int CommonTagsFactor(Slide firstSlide, Slide secondSlide)
        {
            return firstSlide.Tags.Count(x => secondSlide.Tags.Contains(x));
        }

        private int DifferentTagsFactor(Slide firstSlide, Slide secondSlide)
        {
            return firstSlide.Tags.Count(x => !secondSlide.Tags.Contains(x));
        }

        public void PrintSolution()
        {
            Console.WriteLine($"Number of slides: {this.Slides.Count}\n");
            Console.WriteLine($"Interest Factor: {this.InterestFactor}");
        }

        private int DeltaFactor(List<Slide> slides, int pos1, int pos2)
        {
            int totalInteresFactor = 0, commonTags = 0, slideAnotB = 0, slideBnotA = 0;

            // Calculate interest factor between neighboors of
            // first slide that has been picked up to be changed

            // With previous neighboor
            if (pos1 != 0)
            {
                commonTags = CommonTagsFactor(slides[pos1 - 1], slides[pos1]);

                slideAnotB = DifferentTagsFactor(slides[pos1 - 1], slides[pos1]);
                slideBnotA = DifferentTagsFactor(slides[pos1], slides[pos1 - 1]);

                totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            // With next neighboor
            if (pos1 < slides.Count-1)
            {
                commonTags = CommonTagsFactor(slides[pos1], slides[pos1 + 1]);

                slideAnotB = DifferentTagsFactor(slides[pos1], slides[pos1 + 1]);
                slideBnotA = DifferentTagsFactor(slides[pos1 + 1], slides[pos1]);

                totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            // Calculate interest factor between neighboors of
            // second slide that has been picked up to be changed

            // With previous neighboor
            if (pos2 != 0)
            {
                commonTags = CommonTagsFactor(slides[pos2 - 1], slides[pos2]);

                slideAnotB = DifferentTagsFactor(slides[pos2 - 1], slides[pos2]);
                slideBnotA = DifferentTagsFactor(slides[pos2], slides[pos2 - 1]);

                totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }

            // With next neighboor
            if (pos2 < slides.Count-1)
            {
                commonTags = CommonTagsFactor(slides[pos2], slides[pos2 + 1]);

                slideAnotB = DifferentTagsFactor(slides[pos2], slides[pos2 + 1]);
                slideBnotA = DifferentTagsFactor(slides[pos2 + 1], slides[pos2]);

                totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }


            //Implement delta interest factor
            return totalInteresFactor;
        }
    }

}