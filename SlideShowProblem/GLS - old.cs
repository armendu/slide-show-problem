using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SlideShowProblem.Extensions;
using SlideShowProblem.Models;

namespace SlideShowProblem
{
    public class GLS2
    {
        private readonly Random _random = new Random();

        List<Photo> _photos;

        public GLS2(List<Photo> photos)
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
            Solution FirstSolution = ConfigSolution(_photos);

            // Components
            var C = GenerateComponents(FirstSolution.Slides);

            // Total Time
            var totalTime = TimeSpan.FromSeconds(20);

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
                while (localWatch.Elapsed < TimeSpan.FromSeconds(time))
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

                for (int i = 0; i < currentSlides.Count-1; i++)
                {

                    // Form component 1 as a combination between each pair of Slide ID-s
                    string firstComponent = currentSlides[i].ID + " - " + currentSlides[i + 1].ID;

                    // Calculate value of this pair
                    int firstComponentValue = ComponentValue(currentSlides[i], currentSlides[i + 1]);

                    // Get index in list
                    int firstComponnentIndex = ComponentIndex(C, firstComponent);

                    //Get component penalty
                    int firstComponentP = p[firstComponnentIndex];

                    //Get penalizability
                    double firstComponentPenalizability = Penalizability(firstComponentValue, firstComponentP);

                    // Indicator to check if current component is more penalizible or eqaul than/with all others
                    bool isMorePenalizible = true;

                    for (int j = 0; j < currentSlides.Count-1; j++)
                    {
                        if (i != j)
                        {
                            // Form component 1 as a combination between each pair of Slide ID-s
                            string secondComponent = currentSlides[j].ID + " - " + currentSlides[j + 1].ID;

                            // Calculate value of this pair                           
                            int secondComponentValue = ComponentValue(currentSlides[j], currentSlides[j + 1]);

                            // Get index in list                           
                            int secondComponnentIndex = ComponentIndex(C, secondComponent);

                            //Get component penalty
                            int secondComponentP = p[secondComponnentIndex];

                            //Get penalizability
                            double secondComponentPenalizability = Penalizability(secondComponentValue, secondComponentP);

                            //If there is only one component that is more penalizible than the one we are comparing
                            //Than break and go look others
                            if(firstComponentPenalizability < secondComponentPenalizability)
                            {
                                isMorePenalizible = false;
                                break;
                            }
                        }
                    }

                    //If component[i] is the most penalizible, add it to list
                    if (isMorePenalizible)
                        C_prim.Add(firstComponnentIndex);
                }


                // Foreach component that we have seleceted as the most penalizibles
                // Increase penalty for one
                for (int i = 0; i < C_prim.Count; i++)
                {
                    p[C_prim[i]] = p[C_prim[i]] + 1;
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

        private List<string> GenerateComponents(List<Slide> slides)
        {
            List<string> Components = new List<string>();

            for (int i = 0; i < slides.Count-1; i++)
            {
                for (int j = i+1; j < slides.Count; j++)
                {
                    Components.Add(slides[i].ID + " - " + slides[j].ID);
                 
                }
            }

            return Components;
        }

        private Solution CopySolution(Solution origin)
        {
            List<Slide> slides = new List<Slide>(origin.Slides);

            Solution destination = new Solution(slides, 0);

            return destination;
        }

        private Solution Tweak(Solution s)
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

        private int Quality(Solution s)
        {
            return s.InterestFactor;
        }

        private double AdjustedQuality(Solution s, List<string> C, List<int> p)
        {
            // beta factor
            double beta = s.InterestFactor / s.Slides.Count;

            // calculate a sum that is depended on penalties
            double sum = 0;
            for (int i = 0; i < C.Count; i++)
            {
                if (HasFeature(s, C[i]))
                    sum += p[i];
              
            }
           
            return s.InterestFactor + beta*sum;
        }

        private bool HasFeature(Solution s, string feature)
        {
            List<Slide> slides = s.Slides;

            for (int i = 0; i < slides.Count-1; i++)
            {
                for (int j = i+1; j < slides.Count; j++)
                {
                    // Control A - B
                    string currentFeature = slides[i].ID + " - " + slides[j].ID;
                    if (feature == currentFeature)
                        return true;

                    // Control B - A
                    currentFeature = slides[j].ID + " - " + slides[i].ID;
                    if (feature == currentFeature)
                        return true;
                }
            }

            return false;
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

        private double Penalizability(int cValue, int p)
        {
            return 1 / ( (1+p) * cValue);
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
