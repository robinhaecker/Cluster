using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Universe.LivingThings;

namespace Cluster.Mathematics
{
    public class Counter<T>
    {
        readonly Dictionary<T, int> dictionary = new Dictionary<T, int>();

        public Counter(IEnumerable<T> toCount)
        {
            foreach (T t in toCount)
            {
                increment(t);
            }
        }

        public int getCount(T t)
        {
            return dictionary[t];
        }

        public void increment(T t)
        {
            if (dictionary.ContainsKey(t))
            {
                dictionary[t]++;
            }
            else
            {
                dictionary.Add(t, 1);
            }
        }

        public void reset()
        {
            dictionary.Clear();
        }

        public IEnumerable<T> getKeys()
        {
            return dictionary.Keys;
        }
    }
}