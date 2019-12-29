using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlideShowProblem.Extensions;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    public class Solution2
    {
        private const int NO_ITERATIONS = 500;
        private readonly Random _random = new Random();
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; }

        public Solution2()
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

            int slideID = 1;
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

                Slide oneSlide = new Slide(slidePhotos, slideID++);

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

                Slide firstSlide = new Slide(Slides[position1].Photos, Slides[position1].ID);
                Slide secondSlide = new Slide(Slides[position2].Photos, Slides[position2].ID);
                
                // Create new slides
                tmpSlides[position1] = new Slide(secondSlide.Photos, secondSlide.ID);
                tmpSlides[position2] = new Slide(firstSlide.Photos, firstSlide.ID);

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
            int currentInterestFactor = 0;
            // Components
            var C = GenerateComponents(); 

            // Total Time
            var totalTime = TimeSpan.FromSeconds(120);

            // List of Times used for local optimum
            List<int> T = new List<int>{ 20, 30, 15, 25, 8, 7 };

            //List of component penalties, Initially Zero
            List<int> p = new List<int>(C.Count);

            // Initial Solution
            List<Slide> S = new List<Slide>(Slides);

            // Mark this solution as Best
            List<Slide> Best = new List<Slide>(S);

           
            var watch = Stopwatch.StartNew();
            watch.Start();

            // Start 
            while (watch.Elapsed < totalTime)
            {

                // Select random time
                int randomTimeIndex =  _random.Next(0, T.Count);

                int time = T[randomTimeIndex];

                var localWatch = Stopwatch.StartNew();
                localWatch.Start();

                // Start local optimum
                while (localWatch.Elapsed < TimeSpan.FromSeconds(time))
                {
                    // Copy S
                    var copy = new List<Slide>(S);
                    var tweakResult = Tweak(copy);

                    var R = tweakResult.Item1;
                    currentInterestFactor = tweakResult.Item2;

                    //Quality
                    if (currentInterestFactor > InterestFactor)
                    {
                        Best = new List<Slide>(R);
                        InterestFactor = currentInterestFactor;
                    }

                }

                S = new List<Slide>();
                S.AddRange(Slides);

                // For each round generate two positions to swap slides
                int position1 = _random.Next(0, this.Slides.Count);
                int position2;

                do
                {
                    position2 = _random.Next(0, this.Slides.Count);
                } while (position2 == position1);

                Slide firstSlide = new Slide(Slides[position1].Photos, Slides[position1].ID);
                Slide secondSlide = new Slide(Slides[position2].Photos, Slides[position2].ID);

                currentInterestFactor = InterestFactor - DeltaFactor(S, position1, position2);

                // Create new slides
                S[position1] = new Slide(secondSlide.Photos, secondSlide.ID);
                S[position2] = new Slide(firstSlide.Photos, firstSlide.ID);

                currentInterestFactor = currentInterestFactor + DeltaFactor(S, position1, position2);

                //TODO
                // When we pick two slides with two photos
                // Change photos between slides
                //}

                //int currentInterestFactor = CalculateInterestFactor(tmpSlides);

                if (currentInterestFactor >= InterestFactor)
                {
                    InterestFactor = currentInterestFactor;
                    Slides = new List<Slide>(S);
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

        private List<string> GenerateComponents()
        {
            List<string> Components = new List<string>();

            for (int i = 0; i < Slides.Count; i++)
            {
                for (int j = 0; j < Slides.Count; j++)
                {
                    if(i != j)
                    {
                        Components.Add(Slides[i].ID + " - " + Slides[j].ID);
                    }
                }
            }

            return Components;
        }

        private (List<Slide>, int) Tweak(List<Slide> s)
        {
            int position1 = _random.Next(0, s.Count);
            int position2;

            do
            {
                position2 = _random.Next(0, s.Count);
            } while (position2 == position1);

            // Get two slides in random positions
            Slide firstSlide = new Slide(s[position1].Photos, s[position1].ID);
            Slide secondSlide = new Slide(s[position2].Photos, s[position2].ID);

            // Remove interest factor that is related with these two slides
            int currentInterestFactor = InterestFactor - DeltaFactor(s, position1, position2);

            // Swap slides
            s[position1] = new Slide(secondSlide.Photos, secondSlide.ID);
            s[position2] = new Slide(firstSlide.Photos, firstSlide.ID);

            // Add interest factor after swaping slides
            currentInterestFactor = currentInterestFactor + DeltaFactor(s, position1, position2);


            return (s, currentInterestFactor);
        }

        
    }

}