using System;
using System.Collections.Generic;

namespace SlideShowProblem.Extensions
{
    public static class Extensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = 0; i < list.Count - 1; i++)
                list.Swap(i, rnd.Next(i, list.Count));

            return list;
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static void SwapElementsWithAnotherList<T>(this IList<T> list1, IList<T> list2, int i, int j)
        {
            var temp = list1[j];
            list1[j] = list2[i];
            list2[i] = temp;
        }

        public static double GenerateDouble(this Random random, double minValue, double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}