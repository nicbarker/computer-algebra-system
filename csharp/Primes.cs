using System.Collections;

public static class Primes
{
	public static int ApproximateNthPrime(int nn)
	{
		double n = (double)nn;
		double p;
		if (nn >= 7022)
		{
			p = n * Math.Log(n) + n * (Math.Log(Math.Log(n)) - 0.9385);
		}
		else if (nn >= 6)
		{
			p = n * Math.Log(n) + n * Math.Log(Math.Log(n));
		}
		else if (nn > 0)
		{
			p = new int[] { 2, 3, 5, 7, 11 }[nn - 1];
		}
		else
		{
			p = 0;
		}
		return (int)p;
	}

	public static BitArray SieveOfSundaram(int limit)
	{
		limit /= 2;
		BitArray bits = new BitArray(limit + 1, true);
		for (int i = 1; 3 * i + 1 < limit; i++)
		{
			for (int j = 1; i + j + 2 * i * j <= limit; j++)
			{
				bits[i + j + 2 * i * j] = false;
			}
		}
		return bits;
	}

	public static List<int> GeneratePrimesSieveOfSundaram(int n)
	{
		int limit = ApproximateNthPrime(n);
		BitArray bits = SieveOfSundaram(limit);
		List<int> primes = new List<int>();
		primes.Add(2);
		for (int i = 1, found = 1; 2 * i + 1 <= limit && found < n; i++)
		{
			if (bits[i])
			{
				primes.Add(2 * i + 1);
				found++;
			}
		}
		return primes;
	}

	public static readonly int[] primesGenerated = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541 };
}