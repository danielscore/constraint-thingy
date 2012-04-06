﻿using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Intervals
{
    /// <summary>
    /// An closed interval is a set of real numbers such that any number between the lower and upper bounds, including the endpoints, is also included in the set.
    /// </summary>
    public struct Interval
    {
        public Interval(float lowerBound, float upperBound) : this()
        {
            if (lowerBound > upperBound) throw new ArgumentOutOfRangeException(String.Format("The upper bound of {0} must be at least the lower bound of {1}", upperBound, lowerBound));

            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        /// <summary>
        /// The smallest possible value of members in the interval
        /// </summary>
        public float LowerBound { get; private set; }

        /// <summary>
        /// The largest possible value of members in the interval
        /// </summary>
        public float UpperBound { get; private set; }

        /// <summary>
        /// The absolute difference of the endpoints.
        /// </summary>
        public float Range { get { return UpperBound - LowerBound; } }

        /// <summary>
        /// The center value of the interval.
        /// </summary>
        public float Center { get { return (UpperBound + LowerBound) * 0.5f; } }

        public static readonly Interval Empty = new Interval(float.NaN, float.NaN);

        /// <summary>
        /// True if the interval contains no values.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return Equals(Empty);
            }
        }

        /// <summary>
        /// True if the interval contains <param name="value"></param>
        /// </summary>
        [Pure]
        public bool Contains(float value)
        {
            return value <= UpperBound && value >= LowerBound;
        }

#region Operators

        /// <summary>
        /// Adds two intervals.
        /// </summary>
        [Pure]
        public static Interval Add(Interval a, Interval b)
        {
            return new Interval(a.LowerBound + b.LowerBound, a.UpperBound + b.UpperBound);
        }

        /// <summary>
        /// Subtracts <param name="b"></param> from <param name="a"></param>.
        /// </summary>
        [Pure]
        public static Interval Subtract(Interval a, Interval b)
        {
            return new Interval(a.LowerBound - b.UpperBound, a.UpperBound - b.LowerBound);
        }

        /// <summary>
        /// Multiplies two intervals.
        /// </summary>
        [Pure]
        public static Interval Multiply(Interval a, Interval b)
        {
            float a1b1 = a.LowerBound * b.LowerBound;
            float a1b2 = a.LowerBound * b.UpperBound;
            float a2b1 = a.UpperBound * b.LowerBound;
            float a2b2 = a.UpperBound * b.UpperBound;

            return new Interval(Math.Min(a1b1, Math.Min(a1b2, Math.Min(a2b1, a2b2))), Math.Max(a1b1, Math.Max(a1b2, Math.Max(a2b1, a2b2))));
        }

        /// <summary>
        /// Divides <param name="dividend"></param> by <param name="divisor"></param>
        /// </summary>
        [Pure]
        public static Interval Divide(Interval dividend, Interval divisor)
        {
            if (divisor.Contains(0f)) throw new ArgumentOutOfRangeException(String.Format("The divisor, {0}, cannot contain 0.", divisor));

            return Multiply(dividend, new Interval(1f / divisor.UpperBound, 1f / divisor.LowerBound));
        }

        /// <summary>
        /// True if the intervals share at least one value.
        /// </summary>
        [Pure]
        public static bool Intersects(Interval a, Interval b)
        {
            float start = Math.Max(a.LowerBound, b.LowerBound);
            float end = Math.Min(a.UpperBound, b.UpperBound);

            return start <= end;
        }

        /// <summary>
        /// Computes an interval representing the intersection of <param name="a"></param> and <param name="b"></param>
        /// </summary>
        [Pure]
        public static Interval Intersection(Interval a, Interval b)
        {
            float start = Math.Max(a.LowerBound, b.LowerBound);
            float end = Math.Min(a.UpperBound, b.UpperBound);

            if (start > end) return Empty;
            
            return new Interval(start, end);
        }

        /// <summary>
        /// Computes an interval representing the union of <param name="a"></param> and <param name="b"></param>
        /// </summary>
        [Pure]
        public static Interval Union(Interval a, Interval b)
        {
            if (a.IsEmpty && b.IsEmpty) return Empty;
            
            if (a.IsEmpty) return b;
            
            if (b.IsEmpty) return a;

            return new Interval(Math.Min(a.LowerBound, b.LowerBound), Math.Max(a.UpperBound, b.UpperBound));
        }

        /// <summary>
        /// Computes an interval representing the minimum of <param name="a"></param> and <param name="b"></param>
        /// </summary>
        [Pure]
        public static Interval Min(Interval a, Interval b)
        {
            return new Interval(Math.Min(a.LowerBound, b.LowerBound), Math.Min(a.UpperBound, b.UpperBound));
        }

        /// <summary>
        /// Computes an interval representing the maximum of <param name="a"></param> and <param name="b"></param>
        /// </summary>
        [Pure]
        public static Interval Max(Interval a, Interval b)
        {
            return new Interval(Math.Max(a.LowerBound, b.LowerBound), Math.Max(a.UpperBound, b.UpperBound));
        }

#endregion

#region Overloaded Operators

        public static Interval operator+(Interval left, Interval right)
        {
            return Add(left, right);
        }

        public static Interval operator -(Interval left, Interval right)
        {
            return Subtract(left, right);
        }

        public static Interval operator *(Interval left, Interval right)
        {
            return Multiply(left, right);
        }

        public static Interval operator /(Interval left, Interval right)
        {
            return Divide(left, right);
        }

        public static bool operator ==(Interval left, Interval right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Interval left, Interval right)
        {
            return !(left == right);
        }

        #endregion

#region Equality
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (Interval)) return false;
            return Equals((Interval) obj);
        }

        /// <summary>
        /// Intervals are equal if their lower and upper bounds are the same.
        /// </summary>
        public bool Equals(Interval other)
        {
            return other.LowerBound.Equals(LowerBound) && other.UpperBound.Equals(UpperBound);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (LowerBound.GetHashCode()*397) ^ UpperBound.GetHashCode();
            }
        }
#endregion

        public override string ToString()
        {
            return String.Format("Lower Bound: {0}, Upper Bound: {1}", LowerBound, UpperBound);
        }
    }
}