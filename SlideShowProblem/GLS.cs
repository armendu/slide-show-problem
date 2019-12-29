﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlideShowProblem.Extensions;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    public class GLS
    {
        private readonly Random _random = new Random();
        private double MAX_PENALIZABILITY = 100.0;

        List<Photo> _photos;

        public GLS(List<Photo> photos)
        {
            _photos = new List<Photo>(photos);
        }

        // Generates first solution on random basis
        public Solution ConfigSolution(List<Photo> photos)
        {
            Solution s = new Solution();       

            //TODO
            //Add Heuristic for initial solution 

            // Number of photos
            int noPhotos = photos.Count;

            List<Slide> slides = new List<Slide>();
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

                slides.Add(oneSlide);
            }

            // Calculate fitness and save it
            int InterestFactor = CalculateInterestFactor(slides);

            s.Slides = slides;
            s.InterestFactor = InterestFactor;

            return s;
        }

        public Solution GenerateGreedySolution(List<Photo> photos)
        {
            Solution s = new Solution();
            List<Slide> slides = new List<Slide>();
            List<Photo> slidePhotos;
            int noPhotos = photos.Count;
            int slideID = 1;

            // Put photos that have similar tags closer together
            for (int i = 0; i < noPhotos; i += (int)Math.Ceiling(noPhotos * 0.001))
            {
                //for (int j = i + 1; j < i + ((noPhotos - i) / 2); j++)
                for (int j = i + 1; j < i + (int)Math.Ceiling(noPhotos * 0.1); j++)
                {
                    if (j >= noPhotos)
                        break;

                    if (photos[i].Tags.Intersect(photos[j].Tags).Any())
                    {
                        if ((i + 1) != j)
                        {
                            photos.Swap(i + 1, j);
                        }
                        i++;
                        break;
                    }
                }
            }

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

                Slide oneSlide = new Slide(slidePhotos, slideID++);

                slides.Add(oneSlide);
            }

            // Calculate fitness and save it
            s.Slides = slides;
            s.InterestFactor = CalculateInterestFactor(slides);

            return s;
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

        // Main work
        public void Start()
        {
            //Solution FirstSolution = ConfigSolution(_photos);
            var FirstSolution = GenerateGreedySolution(_photos);

            // Components
            var C = new List<Slide>(FirstSolution.Slides);

            // Total Time
            var totalTime = TimeSpan.FromSeconds(60);

            // List of Times used for local optimum
            List<int> T = new List<int> { 20, 30, 15, 25, 8, 7 };

            //List of component penalties, Initially Zero
            List<int> p = InitializePenalties(C.Count);

            // Initial Solution
            Solution S = CopySolution(FirstSolution);

            // Mark this solution as Best
            Solution Best = CopySolution(S);

            var watch = Stopwatch.StartNew();
            watch.Start();

            // Start 
            while (watch.Elapsed < totalTime)
            {
                // Select random time
                int randomTimeIndex = _random.Next(0, T.Count);

                int time = T[randomTimeIndex];

                var localWatch = Stopwatch.StartNew();
                localWatch.Start();

                // Start local optimum
                while (localWatch.Elapsed < TimeSpan.FromSeconds(time) && watch.Elapsed < totalTime)
                {
                    // Copy S
                    var copy = CopySolution(S);

                    var R = Tweak(copy);

                    //Quality
                    if (Quality(R) > Quality(Best))
                    {
                        Best = CopySolution(R);
                    }

                    //Adjusted Quality
                    if (AdjustedQuality(R, C, p) > AdjustedQuality(S, C, p))
                    {
                        S = CopySolution(R);
                    }
                }

                localWatch.Stop();

                // List to save index of items to penalize
                List<int> C_prim = new List<int>();

                List<Slide> currentSlides = S.Slides;

                for (int i = 0; i < C.Count; i++)
                {
                    //if(HasFeature(S, C[i]))
                    //{
                        //Get penalizability
                        double firstComponentPenalizability = Penalizability(S, C[i], p[i]);

                        // Indicator to check if current component is more penalizible or eqaul than/with all others
                        bool isMorePenalizible = true;

                        for (int j = 0; j < C.Count; j++)
                        {
                            if(i != j)
                            {
                                //if(HasFeature(S, C[j]))
                                //{
                                    //Get penalizability
                                    double secondComponentPenalizability = Penalizability(S, C[j], p[j]);

                                    //If there is only one component that is more penalizible than the one we are comparing
                                    //Than break and go look others
                                    if (firstComponentPenalizability < secondComponentPenalizability)
                                    {
                                        isMorePenalizible = false;
                                        break;
                                    }
                                //}
                            }
                        }

                        //If component[i] is the most penalizible, add it to list
                        if (isMorePenalizible)
                            C_prim.Add(i);
                    }
                //}

                // Foreach component that we have seleceted as the most penalizibles
                // Increase penalty for one
                for (int i = 0; i < C_prim.Count; i++)
                {
                    p[C_prim[i]] = p[C_prim[i]] + 1;
                }

            }

            watch.Stop();

            Best.PrintSolution();
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
            if (pos1 < slides.Count - 1)
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
            if (pos2 < slides.Count - 1)
            {
                commonTags = CommonTagsFactor(slides[pos2], slides[pos2 + 1]);

                slideAnotB = DifferentTagsFactor(slides[pos2], slides[pos2 + 1]);
                slideBnotA = DifferentTagsFactor(slides[pos2 + 1], slides[pos2]);

                totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }


            //Implement delta interest factor
            return totalInteresFactor;
        }    

        private Solution CopySolution(Solution origin)
        {
            List<Slide> slides = new List<Slide>(origin.Slides);
            int interestFactor = origin.InterestFactor;

            Solution destination = new Solution(slides, interestFactor);

            return destination;
        }

        private Solution Tweak(Solution s)
        {
            int tweakOption = _random.Next(0, 3);


            switch (tweakOption)
            {
                case 0:
                    return SwapTwoSlides(s);

                case 1:
                    return SwapTwoVerticalPhotos(s);

                case 2:
                    return ShiftElement(s);

                default:
                    return ShiftElement(s);

            }           
        }

        private Solution SwapTwoSlides(Solution s)
        {
            List<Slide> slides = new List<Slide>(s.Slides);
            int interestFactor = s.InterestFactor;

            int position1 = _random.Next(0, slides.Count);
            int position2;

            do
            {
                position2 = _random.Next(0, slides.Count);
            } while (position2 == position1);

            // Get two slides in random positions
            Slide firstSlide = new Slide(slides[position1].Photos, slides[position1].ID);
            Slide secondSlide = new Slide(slides[position2].Photos, slides[position2].ID);

            // Remove interest factor that is related with these two slides
            int currentInterestFactor = interestFactor - DeltaFactor(slides, position1, position2);

            // Swap slides
            slides[position1] = new Slide(secondSlide.Photos, secondSlide.ID);
            slides[position2] = new Slide(firstSlide.Photos, firstSlide.ID);

            // Add interest factor after swaping slides
            currentInterestFactor = currentInterestFactor + DeltaFactor(slides, position1, position2);

            Solution s2 = new Solution(slides, currentInterestFactor);

            return s2;
        }

        private Solution SwapTwoVerticalPhotos(Solution s)
        {
            List<Slide> slides = new List<Slide>(s.Slides);
            int interestFactor = s.InterestFactor;

            int position1, position2;

            do
            {
                position1 = _random.Next(0, slides.Count);
            } while (slides[position1].Photos.Count != 2);

            do
            {
                position2 = _random.Next(0, slides.Count);
            } while (position2 == position1 && slides[position2].Photos.Count != 2);

            // Remove interest factor that is related with these two slides
            int currentInterestFactor = interestFactor - DeltaFactor(slides, position1, position2);

            // Swap slides
            if (slides[position1].Photos.Count == 2 && slides[position2].Photos.Count == 2)
            {
                slides[position1].Photos.SwapElementsWithAnotherList(slides[position2].Photos, 0, 1);
            }

            // Add interest factor after swaping slides
            currentInterestFactor = currentInterestFactor + DeltaFactor(slides, position1, position2);

            Solution s2 = new Solution(slides, currentInterestFactor);

            return s2;
        }

        private Solution ShiftElement(Solution s)
        {
            List<Slide> slides = new List<Slide>(s.Slides);
            int size = slides.Count;

            int interestFactor = s.InterestFactor;

            // Remove interest factor of last two slides
            int currentInterestFactor = interestFactor - RemoveFactor(slides);

            Slide lastSlide = slides[size - 1];

            slides.RemoveAt(size - 1);

            slides.Prepend(lastSlide);

            // Add interest factor of first two slides
            currentInterestFactor = currentInterestFactor + AddFactor(slides);

            Solution s2 = new Solution(slides, currentInterestFactor);

            return s2;
        }

        private int RemoveFactor(List<Slide> slides)
        {
            int totalInteresFactor = 0, commonTags = 0, slideAnotB = 0, slideBnotA = 0;

            commonTags = CommonTagsFactor(slides[slides.Count-2], slides[slides.Count - 1]);

            slideAnotB = DifferentTagsFactor(slides[slides.Count - 2], slides[slides.Count - 1]);
            slideBnotA = DifferentTagsFactor(slides[slides.Count - 1], slides[slides.Count - 2]);

            totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));    

            //Implement delta interest factor
            return totalInteresFactor;
        }

        private int AddFactor(List<Slide> slides)
        {
            int totalInteresFactor = 0, commonTags = 0, slideAnotB = 0, slideBnotA = 0;

            commonTags = CommonTagsFactor(slides[0], slides[1]);

            slideAnotB = DifferentTagsFactor(slides[0], slides[1]);
            slideBnotA = DifferentTagsFactor(slides[1], slides[0]);

            totalInteresFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));

            //Implement delta interest factor
            return totalInteresFactor;
        }


        private int Quality(Solution s)
        {
            return s.InterestFactor;
        }

        private double AdjustedQuality(Solution s, List<Slide> C, List<int> p)
        {
            // beta factor
            double beta = s.InterestFactor / s.Slides.Count;

            // calculate a sum that is depended on penalties
            double sum = 0;
            for (int i = 0; i < C.Count; i++)
            {
                //if (HasFeature(s, C[i]))
                    sum += p[i];
              
            }
           
            return s.InterestFactor + beta*sum;
        }


        private List<int> InitializePenalties(int size)
        {
            List<int> p = new List<int>();

            for (int i = 0; i < size; i++)
            {
                p.Add(0);
            }

            return p;
        }

        private double Penalizability(Solution s, Slide component, int p)
        {
            int cValue = GetComponentValue(s, component);

            if (cValue == 0)
                return MAX_PENALIZABILITY;

            double penalizability = 1.0 / ((1 + p) * cValue);

            return penalizability;
        }

        private int GetComponentValue(Solution s, Slide component)
        {
            int size = s.Slides.Count;

            int prevFactor = 0, nextFactor = 0;

            for (int i = 0; i < size; i++)
            {
                if(s.Slides[i].ID == component.ID)
                {
                    if(i > 0)
                    {
                        prevFactor = ComponentValue(s.Slides[i - 1], s.Slides[i]);
                    }

                    if (i < size-1)
                    {
                        nextFactor = ComponentValue(s.Slides[i], s.Slides[i+1]);
                    }

                    return prevFactor + nextFactor;
                }
            }

            return 0;
        }

        public int ComponentValue(Slide s1, Slide s2)
        {
            int interestFactor = 0;

            int commonTags = CommonTagsFactor(s1, s2);

            int slideAnotB = DifferentTagsFactor(s1, s2);
            int slideBnotA = DifferentTagsFactor(s2, s1);

            interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));

            return interestFactor;
        }

        public int ComponentIndex(List<string> components, String c)
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == c)
                    return i;
            }

            return -1;
        }
    }
}
