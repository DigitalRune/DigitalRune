// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune
{
  /// <summary>
  /// Selecting prime numbers suitable for hash table sizes.
  /// </summary>
  public static class PrimeHelper
  {
    // The following prime number are used for hash table sizes:
    private static readonly int[] Primes =
    {
      // ----- Primes used in .NET (large primes increase by ~1.2):
      //3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 
      //353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 
      //4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 
      //30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
      //187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
      //968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899,
      //4166287, 4999559, 5999471, 7199369
    
      // ----- Primes used in Mono (large primes increase by ~1.5):
      //11, 19, 37, 73, 109, 163, 251, 367, 557, 823, 1237, 1861, 2777, 4177, 
      //6247, 9371, 14057, 21089, 31627, 47431, 71143, 106721, 160073, 240101,
      //360163, 540217, 810343, 1215497, 1823231, 2734867, 4102283, 6153409, 
      //9230113, 13845163

      // ----- "Good primes" according to http://planetmath.org/encyclopedia/GoodHashTablePrimes.html:
      //53, 97, 193, 389, 769, 1543, 3079, 6151, 12289, 24593, 49157, 98317, 
      //196613, 393241, 786433, 1572869, 3145739, 6291469, 12582917, 25165843,
      //50331653, 100663319, 201326611, 402653189, 805306457, 1610612741

      // ----- Prime numbers in cern.colt (used in many other Java libraries):
      //
      //  Copyright (c) 1999 CERN - European Organization for Nuclear Research. 
      //
      //  Permission to use, copy, modify, distribute and sell this software and 
      //  its documentation for any purpose is hereby granted without fee, provided 
      //  that the above copyright notice appear in all copies and that both that 
      //  copyright notice and this permission notice appear in supporting 
      //  documentation. CERN makes no representations about the suitability of 
      //  this software for any purpose. It is provided "as is" without expressed 
      //  or implied warranty. 
      //
      // http://trac.mcs.anl.gov/projects/mpich2/browser/mpich2/trunk/src/mpe2/src/slog2sdk/src/cern/colt/map/PrimeFinder.java?rev=9050
      // Author: wolfgang.hoschek@cern.ch

      // The prime number list consists of 11 chunks.
      // Each chunk contains prime numbers.
      // A chunk starts with a prime P1. The next element is a prime P2. P2 is the smallest prime for which holds: P2 >= 2*P1.
      // The next element is P3, for which the same holds with respect to P2, and so on.
      // 
      // Chunks are chosen such that for any desired capacity >= 1000 
      // the list includes a prime number <= desired capacity * 1.11 (11%).
      // For any desired capacity >= 200 
      // the list includes a prime number <= desired capacity * 1.16 (16%).
      // For any desired capacity >= 16
      // the list includes a prime number <= desired capacity * 1.21 (21%).
      // 
      // Therefore, primes can be retrieved which are quite close to any desired capacity,
      // which in turn avoids wasting memory.
      // For example, the list includes 1039,1117,1201,1277,1361,1439,1523,1597,1759,1907,2081.
      // So if you need a prime >= 1040, you will find a prime <= 1040*1.11=1154.
      // 
      // Chunks are chosen such that they are optimized for a hashtable growthfactor of 2.0;
      // If your hashtable has such a growthfactor then, 
      // after initially "rounding to a prime" upon hashtable construction, 
      // it will later expand to prime capacities such that there exist no better primes.
      // 
      // In total these are about 32*10=320 numbers -> 1 KB of static memory needed.
      // If you are stingy, then delete every second or fourth chunk.

      //chunk #0 
      int.MaxValue, // Yes, it is prime. (See http://en.wikipedia.org/wiki/2147483647)
      
      //chunk #1 
      5,11,23,47,97,197,397,797,1597,3203,6421,12853,25717,51437,102877,205759, 
      411527,823117,1646237,3292489,6584983,13169977,26339969,52679969,105359939, 
      210719881,421439783,842879579,1685759167, 
        
      //chunk #2 
      433,877,1759,3527,7057,14143,28289,56591,113189,226379,452759,905551,1811107, 
      3622219,7244441,14488931,28977863,57955739,115911563,231823147,463646329,927292699, 
      1854585413, 
        
      //chunk #3 
      953,1907,3821,7643,15287,30577,61169,122347,244703,489407,978821,1957651,3915341, 
      7830701,15661423,31322867,62645741,125291483,250582987,501165979,1002331963, 
      2004663929, 
        
      //chunk #4 
      1039,2081,4177,8363,16729,33461,66923,133853,267713,535481,1070981,2141977,4283963, 
      8567929,17135863,34271747,68543509,137087021,274174111,548348231,1096696463, 
        
      //chunk #5 
      31,67,137,277,557,1117,2237,4481,8963,17929,35863,71741,143483,286973,573953, 
      1147921,2295859,4591721,9183457,18366923,36733847,73467739,146935499,293871013, 
      587742049,1175484103, 
        
      //chunk #6 
      599,1201,2411,4831,9677,19373,38747,77509,155027,310081,620171,1240361,2480729, 
      4961459,9922933,19845871,39691759,79383533,158767069,317534141,635068283,1270136683, 
        
      //chunk #7 
      311,631,1277,2557,5119,10243,20507,41017,82037,164089,328213,656429,1312867, 
      2625761,5251529,10503061,21006137,42012281,84024581,168049163,336098327,672196673, 
      1344393353, 
        
      //chunk #8 
      3,7,17,37,79,163,331,673,1361,2729,5471,10949,21911,43853,87719,175447,350899, 
      701819,1403641,2807303,5614657,11229331,22458671,44917381,89834777,179669557, 
      359339171,718678369,1437356741, 
        
      //chunk #9 
      43,89,179,359,719,1439,2879,5779,11579,23159,46327,92657,185323,370661,741337, 
      1482707,2965421,5930887,11861791,23723597,47447201,94894427,189788857,379577741, 
      759155483,1518310967, 
        
      //chunk #10 
      379,761,1523,3049,6101,12203,24407,48817,97649,195311,390647,781301,1562611, 
      3125257,6250537,12501169,25002389,50004791,100009607,200019221,400038451,800076929, 
      1600153859 

/* 
      // some more chunks for the low range [3..1000] 
      //chunk #11 
      13,29,59,127,257,521,1049,2099,4201,8419,16843,33703,67409,134837,269683, 
      539389,1078787,2157587,4315183,8630387,17260781,34521589,69043189,138086407, 
      276172823,552345671,1104691373, 
       
      //chunk #12 
      19,41,83,167,337,677, 
      //1361,2729,5471,10949,21911,43853,87719,175447,350899, 
      //701819,1403641,2807303,5614657,11229331,22458671,44917381,89834777,179669557, 
      //359339171,718678369,1437356741, 
       
      //chunk #13 
      53,107,223,449,907,1823,3659,7321,14653,29311,58631,117269, 
      234539,469099,938207,1876417,3752839,7505681,15011389,30022781, 
      60045577,120091177,240182359,480364727,960729461,1921458943 
*/ 
    };


    /// <summary>
    /// Initializes static members of the <see cref="PrimeHelper"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static PrimeHelper()
    {
      // The above prime numbers are formatted for human readability. 
      Array.Sort(Primes);
    }


    /// <summary>
    /// Determines whether the specified number is prime.
    /// </summary>
    /// <param name="value">The value to test.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="value"/> is prime; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPrime(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0)
      {
        // ----- Number is even.
        return value == 2;
      }

      // ----- Number is odd.
      int upper = (int)Math.Sqrt(value);
      for (int n = 3; n <= upper; n += 2)
        if ((value % n) == 0)
          return false;

      return true;
    }


    /// <summary>
    /// Gets a prime number greater than or equal to the specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// A prime number greater than or equal to <paramref name="value"/>. Returns <paramref name="value"/> 
    /// if no suitable prime number was found within the available range (up to 
    /// <see cref="int.MaxValue"/>).
    /// </returns>
    public static int NextPrime(int value)
    {
      // ----- Binary search (large list):
      Debug.Assert(Primes[Primes.Length - 1] == int.MaxValue, "Int32.MaxValue should be set as largest prime number.");
      int index = Array.BinarySearch(Primes, value);
      if (index < 0)
      {
        // No exact match:
        // index is a negative number which is the bitwise complement of the index 
        // of the first prime that is larger than x.
        index = -index - 1;
      }

      return Primes[index];

/*
      // ----- Linear search (small list):
      for (int i = 0; i < Primes.Length; i++)
      {
        int prime = Primes[i];
        if (x <= prime)
          return prime;
      }

      // ----- Brute force search:
      // Start with next odd number.
      for (int n = x | 1; n < Int32.MaxValue; n += 2)
      {
        if (IsPrime(n))
          return n;
      }

      // Failed to find prime.
      return x;
*/
    }
  }
}
