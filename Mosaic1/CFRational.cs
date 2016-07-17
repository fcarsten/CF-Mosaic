/**
 * Copyright 2006-2016 Carsten Friedrich (Carsten.Friedrich@gmail.com)
 *
 * License: GNU GENERAL PUBLIC LICENSE 3.0 (https://www.gnu.org/copyleft/gpl.html)
 *
 */
using System;

namespace org.carsten
{
	/// <summary>
	/// Summary description for CFRational.
	/// </summary>
	public struct CFRational
	{
		long numerator;
		long denominator;

		public CFRational(long n, long d)
		{
			denominator=d;
			numerator=n;
#if (DEBUG)
			if(d==0 && n!=0)
			{
				Console.WriteLine("Dodgy denominator: {0}/{1}", n, d); 
			}
			if(n==0 && d!=0)
			{
				Console.WriteLine("Dodgy numerator in: {0}/{1}", n, d); 
			}
#endif
		}
		
		public static implicit operator CFRational(int l) 
		{
			return new CFRational(l, 1);
		}

		public static explicit operator CFRational(long l) 
		{
			return new CFRational((int)l, 1);
		}

		public static explicit operator string(CFRational r) 
		{
			return r.ToString();
		}

		public static explicit operator double(CFRational r) 
		{
			if(r.denominator == 0)
			{
				if( r.numerator==0) return double.NaN;
				else if(r.numerator>0) return double.PositiveInfinity;
				else return double.NegativeInfinity;
			}
			return r.numerator/(double)r.denominator;
		}

		public void normalize() 
		{
			if(denominator==0||denominator==1)
				return;
			if(numerator==0) 
			{
				denominator=1;
				return;
			}
			long g = gcd(numerator, denominator);
			denominator/=g;
			numerator/=g;
		}

		long gcd(long a, long b) 
		{
			long c;
			while (b != 0) 
			{
				c = b;
				b = a % b;
				a = c;
			}
			return a;
		}

		public override string ToString()
		{
			normalize();
			return numerator.ToString()+ (denominator==1 ? "": "/"+denominator.ToString());
		}

		public bool isNaN() 
		{
			return numerator==0 && denominator==0;
		}

		public override bool Equals(object o) 
		{
			CFRational r = (CFRational)o;
			if(isNaN() || r.isNaN())
				return false;
			return r.denominator* numerator==denominator * r.numerator;
		}

		public override int GetHashCode()
		{
			return (int)(denominator*numerator);
		}

		public static double operator *(CFRational r, double v) 
		{
			return v*(double)r;
		}

		public static CFRational operator *(CFRational r, int v) 
		{
			return new CFRational(r.numerator*v, r.denominator);
		}

		public static double operator *(double v, CFRational r) 
		{
			return v*(double)r;
		}

		public static CFRational operator *(int v, CFRational r) 
		{
			return new CFRational(r.numerator*v, r.denominator);
		}

		public static CFRational operator /(CFRational r1, CFRational r2)
		{
			return new CFRational(r1.numerator *r2.denominator, r1.denominator*r2.numerator);
		}

		public static bool operator ==(CFRational r, int v) 
		{
			if(r.denominator==0)return false;
			return r.numerator==v*r.denominator;
		}

		public static bool operator !=(CFRational r, int v) 
		{
			return ! (r==v);
		}

	}
}
