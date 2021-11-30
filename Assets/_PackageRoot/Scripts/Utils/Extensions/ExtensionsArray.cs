using System;

namespace Extensions.Saver.Utils
{
    public static class ExtensionsArray
    {
        public static T Random<T>(this T[] list)
        {
            return list[UnityEngine.Random.Range(0, list.Length)];
        }
        public static T[] ChangeEach<T>(this T[] array, Func<T, T> mutator)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = mutator(array[i]);
            }
            return array;
        }
        public static void ForEach(this Array array, Action<Array, int[]> action)
        {
            if (array.LongLength == 0) return;
            ArrayTraverse walker = new ArrayTraverse(array);
            do action(array, walker.Position);
            while (walker.Step());
        }

        internal class ArrayTraverse
        {
            public int[] Position;
            private int[] maxLengths;

            public ArrayTraverse(Array array)
            {
                maxLengths = new int[array.Rank];
                for (int i = 0; i < array.Rank; ++i)
                {
                    maxLengths[i] = array.GetLength(i) - 1;
                }
                Position = new int[array.Rank];
            }

            public bool Step()
            {
                for (int i = 0; i < Position.Length; ++i)
                {
                    if (Position[i] < maxLengths[i])
                    {
                        Position[i]++;
                        for (int j = 0; j < i; j++)
                        {
                            Position[j] = 0;
                        }
                        return true;
                    }
                }
                return false;
            }
        }
    }
}