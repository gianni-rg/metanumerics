using System;
using System.Diagnostics;

namespace Meta.Numerics.Functions {

	public static partial class AdvancedMath {

		// one-argument functions

        /// <summary>
        /// Computes the natural logrithm of the Gamma function.
        /// </summary>
        /// <param name="x">The argument, which must be positive.</param>
        /// <returns>The log Gamma function ln(&#x393;(x)).</returns>
        /// <remarks>
        /// <para>Because &#x393;(x) grows rapidly for increasing positive x, it is often necessary to
        /// work with its logarithm in order to avoid overflow. This function returns accurate
        /// values of ln(&#x393;(x)) even for values of x which would cause &#x393;(x) to overflow.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> is negative or zero.</exception>
        /// <seealso cref="Gamma(double)" />
		public static double LogGamma (double x) {
            if (x <= 0.0) {
                throw new ArgumentOutOfRangeException(nameof(x));
            } else if (x < 16.0) {
                // For small arguments, use the Lanczos approximation.
                return (Lanczos.LogGamma(x));
            } else {
                // For large arguments, the asymptotic series is even faster than the Lanczos approximation.
                return (Stirling.LogGamma(x));
            }
		}

        /// <summary>
        /// Computes the Gamma function.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns>The value of &#x393;(x).</returns>
        /// <remarks>
        /// <para>The Gamma function is a generalization of the factorial (see <see cref="AdvancedIntegerMath.Factorial"/>) to arbitrary real values.</para>
        /// <img src="../images/GammaIntegral.png" />
        /// <para>For positive integer arguments, this integral evaluates to &#x393;(n+1)=n!, but it can also be evaluated for non-integer z.</para>
        /// <para>Like the factorial, &#x393;(x) grows rapidly with increasing x; &#x393;(x) overflows <see cref="System.Double" /> 
        /// for all x larger than ~171. For arguments in this range, you may find it useful to work with the <see cref="LogGamma" /> method, which
        /// returns accurate values for ln(&#x393;(x)) even in the range for which &#x393;(x) overflows.</para>
        /// <para>To evaluate the Gamma function for a complex argument, use <see cref="AdvancedComplexMath.Gamma" />.</para>
        /// <h2>Domain, Range, and Accuracy</h2>
        /// <para>The function is defined for all x. It has poles at all negative integers and at zero; the method returns <see cref="Double.NaN"/> for these arguments. For positive
        /// arguments, the value of the function increases rapidly with increasing argument. For values of x greater than about 170, the value of the function exceeds
        /// <see cref="Double.MaxValue"/>; for these arguments the method returns <see cref="Double.PositiveInfinity"/>. The method is accurate to full precision over its entire
        /// domain.</para>
        /// </remarks>
        /// <seealso cref="AdvancedIntegerMath.Factorial" />
        /// <seealso cref="LogGamma" />
        /// <seealso cref="AdvancedComplexMath.Gamma" />
        /// <seealso href="http://en.wikipedia.org/wiki/Gamma_function" />
        /// <seealso href="http://mathworld.wolfram.com/GammaFunction.html" />
        /// <seealso href="http://dlmf.nist.gov/5">DLMF on the Gamma Function</seealso>
        public static double Gamma (double x) {
            if (x < 0.25) {
                // Use \Gamma(x) \Gamma(1-x) = \frac{\pi}{\sin(\pi x)} to move values close to and left of origin to x > 0
                return (Math.PI / MoreMath.SinPi(x) / Gamma(1.0 - x));
            } else if (x < 16.0) {
                return (Lanczos.Gamma(x));
            } else {
                return (Stirling.Gamma(x));
            }
		}

        /// <summary>
        /// Computes the digamma function.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <returns>The value of &#x3C8;(x).</returns>
        /// <remarks>
        /// <para>The psi function, also called the digamma function, is the logrithmic derivative of the &#x393; function.</para>
        /// <img src="../images/DiGamma.png" />
        /// <para>Because it is defined as a <i>logarithmic</i> derivative, the digamma function does not overflow <see cref="System.Double"/>
        /// even for arguments for which <see cref="Gamma(double)"/> does.</para>
        /// <para>To evaluate the psi function for complex arguments, use <see cref="AdvancedComplexMath.Psi" />.</para>
        /// </remarks>
        /// <seealso cref="Gamma(double)"/>
        /// <seealso cref="AdvancedComplexMath.Psi"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Digamma_function" />
        /// <seealso href="http://mathworld.wolfram.com/DigammaFunction.html" />
		public static double Psi (double x) {
            if (x < 0.25) {
                return (Psi(1.0 - x) - Math.PI / MoreMath.TanPi(x));
            } else if (x < 16.0) {
                return (Lanczos.Psi(x));
            } else {
                // For large arguments, the Stirling asymptotic expansion is faster than the Lanzcos approximation
                return (Stirling.Psi(x));
            }
		}

        /// <summary>
        /// Computes the polygamma function.
        /// </summary>
        /// <param name="n">The order, which must be non-negative.</param>
        /// <param name="x">The argument.</param>
        /// <returns>The value of &#968;<sub>n</sub>(x).</returns>
        /// <remarks>
        /// <para>The polygamma function gives higher logarithmic derivatives of the Gamma function.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="n"/> is negative.</exception>
        /// <seealso href="http://en.wikipedia.org/wiki/Polygamma_function"/>
        /// <seealso href="http://mathworld.wolfram.com/PolygammaFunction.html"/>
        /// <seealso href="http://dlmf.nist.gov/5.15">DLMF on the Polygamma Function</seealso>
        public static double Psi (int n, double x) {

            if (n < 0) throw new ArgumentOutOfRangeException(nameof(x));

            // for n=0, use normal digamma algorithm
            if (n == 0) return (Psi(x));

            // for small x, use the reflection formula
            if (x <= 0.0) {
                if (x == Math.Ceiling(x)) {
                    return (Double.NaN);
                } else {
                    // use the reflection formula
                    // this requires that we compute the nth derivative of cot(pi x)
                    // i was able to do this, but my algorithm is O(n^2), so for large n and not-so-negative x this is probably sub-optimal
                    double y = Math.PI * x;
                    double d = -EvaluateCotDerivative(ComputeCotDerivative(n), y) * MoreMath.Pow(Math.PI, n + 1);
                    if (n % 2 == 0) {
                        return (d + Psi(n, 1.0 - x));
                    } else {
                        return (d - Psi(n, 1.0 - x));
                    }
                }
            }

            // compute the minimum x required for our asymptotic series to converge
            // my original approximation was to say that, for 1 << k << n, the factorials approach e^(2k),
            // so to achieve convergence to 10^(-16) at the 8'th term we should need x ~ 10 e ~ 27
            // unfortunately this estimate is both crude and an underestimate; for now i use an emperical relationship instead
            double xm = 16.0 + 2.0 * n;

            // by repeatedly using \psi(n,z) = \psi(n,z+1) - (-1)^n n! / z^(n+1),
            // increase x until it is large enough to use the asymptotic series
            // keep track of the accumulated shifts in the variable s
            double s = 0.0;
            while (x < xm) {
                s += 1.0 / MoreMath.Pow(x, n + 1);
                x += 1.0;
            }

            // now that x is big enough, use the asymptotic series
            // \psi(n,z) = - (-1)^n (n-1)! / z^n [ 1 + n / 2 z + \sum_{k=1}^{\infty} (2k+n-1)! / (2k)! / (n-1)! * B_{2k} / z^{2k} ]
            double t = 1.0 + n / (2.0 * x);
            double x2 = x * x;
            double t1 = n * (n + 1) / 2.0 / x2;
            for (int i = 1; i < AdvancedIntegerMath.Bernoulli.Length; i++) {
                double t_old = t;
                t += AdvancedIntegerMath.Bernoulli[i] * t1;
                if (t == t_old) {
                    double g = AdvancedIntegerMath.Factorial(n - 1) * (t / MoreMath.Pow(x, n) + n * s);
                    if (n % 2 == 0) g = -g;
                    return (g);
                }
                int i2 = 2 * i;
                t1 *= 1.0 * (n + i2) * (n + i2 + 1) / (i2 + 2) / (i2 + 1) / x2;
            }
            throw new NonconvergenceException();

            // for small x, the s-part strongly dominates the t-part, so it isn't actually necessary for us to determine t
            // very accurately; in the future, we should modify this code to allow the t-series to converge when its overall
            // contribution no longer matters, rather than requiring t to converge to full precision

        }

        // The nth derivative of cot(x) can be obtained by noting that cot(x) = cos(x) / sin(x)
        // Define r_n = (c/s)^n. Then by explicit differentiation D_x r_n = - n (r_{n-1} + r_{n+1})
        // Since the result is expressed in terms of r's, differentiation can be repeated using the same formula
        // The next two methods simply implement this machinery. Is there a way we can turn this into a recursion formula so it is O(n) instead of O(n^2)?

        private static double[] ComputeCotDerivative (int n) {

            // make an array to hold coefficients of 1, r, r^2, ..., r_{n+1}
            double[] p = new double[n + 2];

            // start with one power of r
            p[1] = 1.0;

            // differentiate n times
            for (int i = 1; i <= n; i++) {

                // only even or odd powers of r appear at any given order; this fact allows us to use just one array:
                // the entries of one parity are the source (and are set to zero after being used), the of the other parity are the target
                for (int j = i; j >= 0; j -= 2) {
                    // add -j times our coeffcient to the coefficient above
                    p[j + 1] += -j * p[j];
                    // same for the coefficient below; we need not add since no one else has addressed it yet (since we are moving down)
                    if (j > 0) p[j - 1] = -j * p[j];
                    // we are done with this coefficeint; make it zero for the next time
                    p[j] = 0.0;
                }

            }

            // return the resulting array coefficients
            return (p);
        }

        private static double EvaluateCotDerivative (double[] p, double x) {

            // compute cot(x), which is what our polynomial is in powers of
            double r = 1.0 / Math.Tan(x);

            // compute its square, which we will multiply by to move ahead by powers of two as we evaluate
            double r2 = r * r;

            // compute the first term and set the index of the next term
            int i; double rp;
            if (p.Length % 2 == 0) {
                i = 1;
                rp = r;
            } else {
                i = 0;
                rp = 1.0;
            }

            double f = 0.0;
            while (i < p.Length) {
                // add the current term
                f += p[i] * rp;
                // prepare for next term
                i += 2;
                rp *= r2;
            }

            return (f);

        }

        // This function computes x^{\nu} / \Gamma(\nu + 1), which can easily become Infinity/Infinity=NaN for large \nu if computed naively.

        internal static double PowOverGammaPlusOne (double x, double nu) {
            if (nu < 16.0) {
                return (Math.Pow(x, nu) / AdvancedMath.Gamma(nu + 1.0));
            } else {
                return(Stirling.PowOverGammaPlusOne(x, nu));
            }
        }


        /// <summary>
        /// Computes the Pochammer symbol (x)<sub>y</sub>.
        /// </summary>
        /// <param name="x">The first argument.</param>
        /// <param name="y">The second argument.</param>
        /// <returns>The value of (x)<sub>y</sub>.</returns>
        /// <remarks>
        /// <para>The Pochhammer symbol is defined as a ratio of Gamma functions.</para>
        /// <para>For positive integer y, this is equal to a rising factorial product.</para>
        /// <para>Note that while the Pochhammer symbol notation is very common, combinatorialists sometimes
        /// use the same notation for a falling factorial.</para>
        /// <para>If you need to compute a ratio of Gamma functions, in most cases you should compute it by calling this
        /// function instead of taking the quotient of two calls to the <see cref="Gamma(double)"/> function,
        /// because there is a large fraction of the parameter space where Gamma(x+y) and Gamma(x) overflow,
        /// but their quotient does not, and is correctly computed by this method.</para>
        /// <para>This function accepts both positive and negative values for both x and y.</para>
        /// </remarks>
        /// <seealso href="http://mathworld.wolfram.com/PochhammerSymbol.html"/>
        public static double Pochhammer (double x, double y) {

            // Handle simplest cases quickly
            if (y == 0.0) {
                return (1.0);
            } else if (y == 1.0) {
                return (x);
            }

            // Should we also handle small integer y explicitly?

            // The key case of x and y positive is pretty simple. Both the Lanczos and Stirling forms of \Gamma
            // admit forms that preserve accuracy in \Gamma(x+y) / \Gamma(x) - 1 as y -> 0. It turns
            // out, however, to be ridiculous complicated to handle all the corner cases that appear in
            // other regions of the x-y plane.

            double z = x + y;
            if (x < 0.25) {

                bool xNonPositiveInteger = (Math.Round(x) == x);
                bool zNonPositiveInteger = (z <= 0.0) && (Math.Round(z) == z);

                if (xNonPositiveInteger) {
                    if (zNonPositiveInteger) {
                        long m;
                        double p;
                        if (z >= x) {
                            m = (long) (z - x);
                            p = Pochhammer(-z + 1, m);
                        } else {
                            m = (long) (x - z);
                            p = 1.0 / Pochhammer(-x + 1, m);
                        }
                        if (m % 2 != 0) p = -p;
                        return (p);
                    } else {
                        return (0.0);
                    }
                }

                if (y < 0.0) {
                    // x negative, y negative; we can transform to make x and y positive
                    return (1.0 / Pochhammer(1.0 - x, -y) * MoreMath.SinPi(x) / MoreMath.SinPi(z));
                } else if (z <= 0.25) {
                    // x negative, y positive, but not positive enough, so z still negative
                    // we can transform to make x and y positive
                    return (Pochhammer(1.0 - z, y) * MoreMath.SinPi(x) / MoreMath.SinPi(z));
                } else {
                    // x negative, y positive enough to make z positive
                    return (Gamma(1.0 - x) * Gamma(z) * MoreMath.SinPi(x) / Math.PI);
                }

            } else {

                if (z >= 0.25) {
                    // This is the standard case: x positive, y may be negative, but not negative enough to make z negative
                    double P;
                    if ((x > 16.0) && (z > 16.0)) {
                        P = Stirling.ReducedLogPochhammer(x, y);
                    } else {
                        P = Lanczos.ReducedLogPochhammer(x, y);
                    }
                    return (Math.Exp(y * P));
                } else {
                    // if y is very negative, z will also be negative, so we can transform one gamma
                    if (Math.Round(z) == z) {
                        return (Double.PositiveInfinity);
                    } else {
                        return (Math.PI / (Gamma(x) * Gamma(1.0 - z) * MoreMath.SinPi(z)));
                    }
                }

            }

        }

        // two-argument functions

        /// <summary>
        /// Computes the Beta function.
        /// </summary>
        /// <param name="a">The first parameter, which must be positive.</param>
        /// <param name="b">The second parameter, which must be positive.</param>
        /// <returns>The beta function B(a,b).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is non-positive.</exception>
        /// <remarks>
        /// <para>The Beta function can be defined by the integral:</para>
        /// <img src="..\images\BetaIntegral.png" />
        /// <para>Equivalently, it can be defined as a commonly occuring ratio of Gamma functions:</para>
        /// <img src="..\images\BetaGammaRelation.png" />
        /// <para>When evaluating such a ratio of Gamma functions, it is better to use this method than to call
        /// <see cref="Gamma(double)"/> three times and form the ratio explicitly. One reason is that this method
        /// will be faster. Another reason is that, for many values, the individual Gamma functions will overflow
        /// even though the Beta function does not; this method will not overflow in such cases. There are still
        /// other cases in which the value of Beta does overflow or underflow a <see cref="double"/>; in such cases,
        /// the method <see cref="LogBeta(double, double)"/> will still return an accurate value of its logarithm.</para>
        /// <para>One place this ratio occurs is in the expression for a binomial coefficient in terms of factorials, so the Beta
        /// function can used to generalize binomial coefficients (<see cref="AdvancedIntegerMath.BinomialCoefficient(int, int)"/>)
        /// to non-integer values.</para>
        /// </remarks>
        /// <seealso href="http://en.wikipedia.org/wiki/Beta_function"/>
        /// <seealso href="http://mathworld.wolfram.com/BetaFunction.html"/>
        /// <seealso href="http://dlmf.nist.gov/5.12">DLMF on the Beta Function</seealso>
        public static double Beta (double a, double b) {
            if (a <= 0.0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b <= 0.0) throw new ArgumentOutOfRangeException(nameof(b));
            if ((a > 16.0) && (b > 16.0)) {
                return (Stirling.Beta(a, b));
            } else {
                return (Lanczos.Beta(a, b));
            }
		}

        /// <summary>
        /// Computes the lograrithm of the Beta function.
        /// </summary>
        /// <param name="a">The first parameter, which must be positive.</param>
        /// <param name="b">The second parameter, which must be positive.</param>
        /// <returns>The value of ln(B(a,b)).</returns>
        /// <remarks>
        /// <para>This function accurately computes ln(B(a,b)) even for values of a and b for which B(a,b) is
        /// too small or large to be represented by a double.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is negative or zero.</exception>
        /// <seealso cref="Beta(System.Double,System.Double)"/>
        public static double LogBeta (double a, double b) {
            if (a <= 0.0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b <= 0.0) throw new ArgumentOutOfRangeException(nameof(b));
            if ((a > 16.0) && (b > 16.0)) {
                return (Stirling.LogBeta(a, b));
            } else {
                return (Lanczos.LogBeta(a, b));
            }
        }

        /// <summary>
        /// Computes the normalized lower (left) incomplete Gamma function.
        /// </summary>
        /// <param name="a">The shape parameter, which must be positive.</param>
        /// <param name="x">The argument, which must be non-negative.</param>
        /// <returns>The value of &#x3B3;(a,x)/&#x393;(a).</returns>
        /// <remarks><para>The incomplete Gamma function is obtained by carrying out the Gamma function integration from zero to some
        /// finite value x, instead of to infinity. The function is normalized by dividing by the complete integral, so the
        /// function ranges from 0 to 1 as x ranges from 0 to infinity.</para>
        /// <para>For large values of x, this function becomes 1 within floating point precision. To determine its deviation from 1
        /// in this region, use the complementary function <see cref="RightRegularizedGamma"/>.</para>
        /// <para>For a=&#x3BD;/2 and x=&#x3C7;<sup>2</sup>/2, this function is the CDF of the &#x3C7;<sup>2</sup> distribution with &#x3BD; degrees of freedom.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> is negative or zero, or <paramref name="x"/> is negative.</exception>
        /// <seealso cref="RightRegularizedGamma" />
        public static double LeftRegularizedGamma (double a, double x) {
			if (a <= 0) throw new ArgumentOutOfRangeException(nameof(a));
			if (x < 0) throw new ArgumentOutOfRangeException(nameof(x));
            if ((a > 128.0) && (Math.Abs(x - a) < 0.25 * a)) {
                double P, Q;
                Gamma_Temme(a, x, out P, out Q);
                return (P);
            } else if (x < (a + 1.0)) {
				return( GammaP_Series(a, x) );
			} else {
				return( 1.0 - GammaQ_ContinuedFraction(a, x) );
			}
		}

        /// <summary>
        /// Computes the normalized upper (right) incomplete Gamma function.
        /// </summary>
        /// <param name="a">The shape paraemter, which must be positive.</param>
        /// <param name="x">The argument, which must be non-negative.</param>
        /// <returns>The value of &#x393;(a,x)/&#x393;(a).</returns>
        /// <remarks>
        /// <para>This function is the complement of the left incomplete Gamma function <see cref="LeftRegularizedGamma"/>.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> is negative or zero, or <paramref name="x"/> is negative.</exception>
        /// <seealso cref="LeftRegularizedGamma"/>
		public static double RightRegularizedGamma (double a, double x) {
			if (a <= 0) throw new ArgumentOutOfRangeException(nameof(a));
			if (x < 0) throw new ArgumentOutOfRangeException(nameof(x));
            if ((a > 128.0) && (Math.Abs(x - a) < 0.25 * a)) {
                double P, Q;
                Gamma_Temme(a, x, out P, out Q);
                return (Q);
            } else if (x < (a + 1.0)) {
				return( 1.0 - GammaP_Series(a, x) );
			} else {
				return( GammaQ_ContinuedFraction(a, x) );
			}
		}

        /// <summary>
        /// Computes the upper incomplete Gamma function.
        /// </summary>
        /// <param name="a">The shape parameter, which must be positive.</param>
        /// <param name="x">The argument, which must be non-negative.</param>
        /// <returns>The value of &#x393;(a,x).</returns>
        /// <remarks>
        /// <para>The incomplete Gamma function is defined by the same integrand as the Gamma function (<see cref="Gamma(double)"/>),
        /// but the integral is not taken over the full positive real axis.</para>
        /// <img src="../images/UpperIncompleteGammaIntegral.png" />
        /// <para>Like the &#x393; function itself, this function gets large very quickly. For many
        /// purposes, you will prefer to use the regularized incomplete gamma functions <see cref="LeftRegularizedGamma"/> and
        /// <see cref="RightRegularizedGamma"/>, which are accurately computed even in regions for which this function overflows.</para>
        /// </remarks>
        /// <seealso cref="Gamma(double)"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Incomplete_Gamma_function"/>
        public static double Gamma (double a, double x) {
            return (RightRegularizedGamma(a, x) * Gamma(a));
        }



        // three-argument functions

        /// <summary>
        /// Computes the incomplete Beta function.
        /// </summary>
        /// <param name="a">The left shape parameter, which must be non-negative.</param>
        /// <param name="b">The right shape paraemter, which must be non-negative.</param>
        /// <param name="x">The integral endpoint, which must lie in [0,1].</param>
        /// <returns>The value of B<sub>x</sub>(a, b).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is negative, or
        /// <paramref name="x"/> lies outside [0, 1].</exception>
        /// <remarks>
        /// <para>This function is defined by the same integral that defines the Beta function (<see cref="Beta(double, double)"/>, but
        /// with the integral taken from 0 to x instead of from 0 to 1.</para>
        /// <para>Note that in most mathematical literature, <paramref name="x"/> is regarded as the first argument. We, however,
        /// follow the programming convention that optional arguments follow required ones, so <paramref name="x"/> is the last
        /// argument of the method.</para>
        /// <para>If you require the regularized function, use <see cref="LeftRegularizedBeta(double, double, double)"/>
        /// to obtain it directly.</para>
        /// </remarks>
        /// <seealso href="https://en.wikipedia.org/wiki/Beta_function#Incomplete_beta_function"/>
        /// <seealso href="http://mathworld.wolfram.com/IncompleteBetaFunction.html"/>
        public static double Beta (double a, double b, double x) {
            if (a < 0.0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b < 0.0) throw new ArgumentOutOfRangeException(nameof(b));
            if ((x < 0.0) || (x > 1.0)) throw new ArgumentOutOfRangeException(nameof(x));
			if (x == 0.0) return(0.0);
            double xtp = (a + 1.0) / (a + b + 2.0);
            if (x > xtp) {
                return (Beta(a, b) - Beta(b, a, 1.0 - x));
            } else {
                return (Math.Pow(x, a) * Math.Pow(1.0 - x, b) * RegularizedBeta_ContinuedFraction(a, b, x));
            }
		}

        internal static double RegularizedBeta_ContinuedFraction (double a, double b, double x) {

            // Use the continued fraction (DLMF 8.17.22,  http://dlmf.nist.gov/8.17#v)
            //   B(a, b, x) = x^a (1-x)^b BCF(a, b, x)
            //   BCF(a, b, x) = \frac{1}{a} \left[ \frac{1}{1+} \frac{d_1}{1+} \frac{d_2}{1+} \cdots \right]
            // where
            //   d_{2m} = \frac{x m (b-m)}{(a + 2m - 1)(a + k)}
            //   d_{2m+1} = -\frac{x (a + m) (a + b + m)}{(a + m)(a + 2m + 1)}
            // which is good for x < (a + 1) / (a + b + 2). For larger x, compute compliment and subtract.

            // Evaluate via Steed's method.
            // We can simplify slightly due to this being a Stieltjes CF (denominator always 1).
            // Do k = 0 and k = 1 explicitly, since simplifications are possible for those terms.

            double ab = a + b;
            double p = -x * ab / (a + 1.0);
            double D = 1.0 / (1.0 + p);
            double Df = -p * D;
            double f = 1.0 + Df;
            for (int k = 2; k < Global.SeriesMax; k++) {
                double f_old = f;
                int m = k / 2;
                p = x / ((a + (k - 1)) * (a + k));
                if ((k % 2) == 0) {
                    p *= m * (b - m);
                } else {
                    p *= -(a + m) * (ab + m);
                }
                D = 1.0 / (1.0 + p * D);
                Df = (D - 1.0) * Df;
                f += Df;
                 if (f == f_old) {
                    return (f / a);
                }
            }
            throw new NonconvergenceException();
        }

        /// <summary>
        /// Computes the regularized incomplete Beta function.
        /// </summary>
        /// <param name="a">The left shape parameter, which must be non-negative.</param>
        /// <param name="b">The right shape paraemter, which must be non-negative.</param>
        /// <param name="x">The integral endpoint, which must lie in [0,1].</param>
        /// <returns>The value of I<sub>x</sub>(a, b) = B<sub>x</sub>(a, b) / B(a, b).</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="a"/> or <paramref name="b"/> is negative or zero,
        /// or <paramref name="x"/> lies outside [0, 1].</exception>
        public static double LeftRegularizedBeta (double a, double b, double x) {
            if (a <= 0.0) throw new ArgumentOutOfRangeException(nameof(a));
            if (b <= 0.0) throw new ArgumentOutOfRangeException(nameof(b));
            if ((x < 0.0) || (x > 1.0)) throw new ArgumentOutOfRangeException(nameof(x));
            double xtp = (a + 1.0) / (a + b + 2.0);
            if (x > xtp) {
                return (1.0 - LeftRegularizedBeta(b, a, 1.0 - x));
            } else {
                return (RegularizedBeta_ContinuedFraction(a, b, x) * PowOverBeta(a, b, x));
            }
        }

        private static double PowOverBeta (double a, double b, double x) {
            if ((a > 16.0) && (b > 16.0)) {
                return (Stirling.PowOverBeta(a, b, x));
            } else {
                return (Math.Pow(x, a) * Math.Pow(1.0 - x, b) / Beta(a, b));
            }
        }


		// Compute GammaP(a,x) for x < a+1
		private static double GammaP_Series (double a, double x) {
            if (x == 0.0) return (0.0);
			double ap = a;
			double ds = Math.Exp( a * Math.Log(x) - x - LogGamma(a + 1.0) );
			double s = ds;
            for (int i=0; i<Global.SeriesMax; i++) {
				ap += 1.0;
				ds *= (x / ap);
                double s_old = s;
				s += ds;
                if (s == s_old) {
                    return (s);
                }
			}
            throw new NonconvergenceException();
		}

		// Compute GammaQ(a,x) for x > a+1
		private static double GammaQ_ContinuedFraction (double a, double x) {
            if (Double.IsPositiveInfinity(x)) return (0.0);
			double aa = 1.0;			// a_1
			double bb = x - a + 1.0;	// b_1
			double D = 1.0/bb;		    // D_1 = b_0/b_1
			double Df = aa/bb;		    // Df_1 = f_1 - f_0
			double f = 0.0 + Df;		// f_1 = f_0 + Df_1 = b_0 + Df_1
            // entering this loop with bb infinite (as caused e.g. by infinite x) will cause a
            // NonconvergenceException instead of the expected convergence to zero
			for (int k=1; k<Global.SeriesMax; k++) {
				double f_old = f;
				aa = -k * (k-a);
				bb += 2.0;
				D = 1.0 / (bb + aa * D);
				Df = (bb * D - 1.0) * Df;
				f += Df;
                if (f == f_old) {
                    return (Math.Exp(a * Math.Log(x) - x - LogGamma(a)) * f);
                }
			}
			throw new NonconvergenceException();
		}

        // For large a, for x ~ a, the convergence of both the series and the continued fraction for the incomplete gamma
        // function is very slow; the problem is that the kth term goes like x/(a+k), and for large a, adding k
        // makes little difference until k ~ a.

        // In this region, NR uses ~15-point Gaussian quadrature of the peaked integrand, which should be about as good
        // as one iteration of the 15-point Gauss-Kronrod integrator used by our adaptive integrator. But when I tried
        // our adaptive integrator, it wanted to subdivide and repeat, requiring hundreds of evaluations to achieve full
        // accuracy. This leads me to believe that the NR algorithm is unlikely to achieve full accuracy.

        // So in this region we use instead a rather strange expansion due to Temme. It looks simple enough at first:
        //   P = erfc(-z) / 2 -  R     Q = erfc(z) / 2 + R
        // where the erfc term is (nearly) the Normal(a,sqrt(a)) approximation and R is a correction term.

        // The first odditity is that z is not quite (x-a)/sqrt(2a). Instead
        //   z^2 / a = eta^2 / 2 = (x-a)/a - log(x/a) = e - log(1+e) = exp(u) - 1 - u
        // where e = (x-a)/a and u = x/a. Note for x ~ a, this makes z have nearly the expected value, but with O(e)~O(u) corrections.

        // R can be expressed as a double power series in 1/a and u (or e).
        //   R = exp(-z^2) / sqrt(2 pi a) \sum_{ij} D_{ij} u^{j} / a_{i}
        // To obtain the coefficients, expand inverse powers of eta in powers of e
        //   C_0 = -1/eta = singular -1/3 + 1/12 e - 23/540 e^2 + ...
        //   C_1 = 1/eta^3 = singular - 1/540 - 1/288 e + ...
        //   C_2 = -3/eta^5 = singular + 25/6048 + ...
        //   C_k = (-1)^(k+1) (2k-1)!! / eta^(2k+1)
        // Discard the singular terms. The remaining terms give the coefficients for R. This weird prescription is the
        // second oddity.

        // Since the coefficients decrease faster in terms of u than in terms of e, we convert to a power series in u after
        // dropping the singular terms. Note this is not the same as dropping the singular terms in a direct expansion in u.

        // We record enough terms to obtain full accuracy when a > 100 and |e| < 0.25.

        private static readonly double[][] TemmeD = new double[][] {
            new double[] { - 1.0 / 3.0, 1.0 / 12.0, - 1.0 / 1080.0, - 19.0 / 12960.0, 1.0 / 181440.0, 47.0 / 1360800.0,
                1.0 / 32659200.0, - 221.0 / 261273600.0, - 281.0 / 155196518400.0,  857.0 / 40739086080.0, 1553.0 / 40351094784000.0 },
            new double[] { -1.0 / 540.0, - 1.0 / 288.0, 25.0 / 12096.0, - 223.0 / 1088640.0, - 89.0 / 1088640.0,
                757.0 / 52254720.0,  445331.0 / 155196518400.0, - 1482119.0 / 2172751257600.0, - 7921307.0 / 84737299046400.0 },
            new double[] { 25.0 / 6048.0, - 139.0 / 51840.0, 101.0 / 311040.0, 1379.0 / 7464960.0, - 384239.0 / 7390310400.0,
                - 1007803.0 / 155196518400.0, 88738171.0 / 24210656870400.0, 48997651.0 / 484213137408000.0 },
            new double[] { 101.0 / 155520.0, 571.0 / 2488320.0, - 3184811.0 / 7390310400.0, 36532751.0 / 310393036800.0,
                10084279.0 / 504388684800.0, - 82273493.0 / 5977939968000.0 },
            new double[] { - 3184811.0 / 3695155200.0, 163879.0 / 209018880.0, - 2745493.0 / 16303472640.0,
                - 232938227.0 / 2934625075200.0, 256276123.0 / 5869250150400.0 },
            new double[] { - 2745493.0 / 8151736320.0, - 5246819.0 / 75246796800.0, 119937661.0 / 451480780800.0,
                - 294828209.0 / 2708884684800.0 },
            new double[] { 119937661.0 / 225740390400.0, - 534703531.0 / 902961561600.0 },
            new double[] { 8325705316049.0 / 24176795811840000.0 , 4483131259.0 / 86684309913600.0 }
        };

        private static void Gamma_Temme (double a, double x, out double P, out double Q) {

            double u = Math.Log(x / a);
            
            // compute argument of error function, which is almost (x-a)/sqrt(a)

            double dz = 1.0;
            double z = dz;
            for (int i = 3; true; i++) {
                if (i > Global.SeriesMax) throw new NonconvergenceException();
                double z_old = z;
                dz *= u / i;
                z += dz;
                if (z == z_old) break;
            }
            z = u * Math.Sqrt(a * z / 2.0);

            // the first approximation is just the almost-Gaussian one

            if (z > 0) {
                Q = AdvancedMath.Erfc(z) / 2.0;
                P = 1.0 - Q;
            } else {
                P = AdvancedMath.Erfc(-z) / 2.0;
                Q = 1.0 - P;
            }

            // compute Temme's correction to the Gaussian approximation

            double R0 = Math.Exp(-z*z) / Math.Sqrt(Global.TwoPI * a);

            double S0 = 0.0;
            double ai = 1.0;
            for (int i=0; i < TemmeD.Length; i++) {
                double dS = 0.0;
                double uj = 1.0;
                for (int j = 0; j < TemmeD[i].Length; j++) {
                    dS += TemmeD[i][j] * uj;
                    uj *= u;
                }
                S0 += dS / ai;
                ai *= a;
            }

            double R = R0 * S0;
            Q = Q + R;
            P = P - R;

        }

	}

    /// <summary>
    /// Contains methods that compute advanced functions of complex arguments.
    /// </summary>
    public static partial class AdvancedComplexMath {

        /// <summary>
        /// Computes the complex Gamma function.
        /// </summary>
        /// <param name="z">The complex argument.</param>
        /// <returns>The complex value of &#x393;(z).</returns>
        /// <remarks>
        /// <para>The image below shows the complex &#x393; function near the origin using domain coloring.</para>
        /// <img src="../images/ComplexGammaPlot.png" />
        /// </remarks>
        /// <seealso cref="AdvancedMath.Gamma(double)"/>
        /// <seealso href="http://en.wikipedia.org/wiki/Gamma_function" />
        /// <seealso href="http://mathworld.wolfram.com/GammaFunction.html" />
        public static Complex Gamma (Complex z) {
            if (z.Re < 0.5) {
                // 1-z form
                return (Math.PI / Gamma(1.0 - z) / ComplexMath.Sin(Math.PI * z));
            } else {
                return (ComplexMath.Exp(LogGamma(z)));
            }
        }

        /// <summary>
        /// Compute the complex log Gamma function.
        /// </summary>
        /// <param name="z">The complex argument, which must have a non-negative real part.</param>
        /// <returns>The complex value ln(&#x393;(z)).</returns>
        /// <exception cref="ArgumentOutOfRangeException">The real part of <paramref name="z"/> is negative.</exception>
        /// <seealso cref="AdvancedMath.LogGamma" />
        public static Complex LogGamma (Complex z) {
            if (z.Re < 0.0) {
                throw new ArgumentOutOfRangeException(nameof(z));
            } else if (ComplexMath.Abs(z) < 16.0) {
                return (Lanczos.LogGamma(z));
            } else {
                return (LogGamma_Stirling(z));
            }
        }


        private static Complex LogGamma_Stirling (Complex z) {

            // work in the upper complex plane; i think this isn't actually necessary
            if (z.Im < 0.0) return (LogGamma_Stirling(z.Conjugate).Conjugate);

            Complex f = (z - 0.5) * ComplexMath.Log(z) - z + Math.Log(Global.TwoPI) / 2.0;

            // reduce f.Im modulo 2*PI
            // result is cyclic in f.Im modulo 2*PI, but if f.Im starts off too big, the corrections
            // applied below will be lost because they are being added to a big number
            f = new Complex(f.Re, AdvancedMath.Reduce(f.Im, 0.0));

            Complex zz = z * z;
            Complex zp = z;
            for (int i = 1; i < AdvancedIntegerMath.Bernoulli.Length; i++) {
                Complex f_old = f;
                f += AdvancedIntegerMath.Bernoulli[i] / (2 * i) / (2 * i - 1) / zp;
                if (f == f_old) return (f);
                zp *= zz;
            }
            throw new NonconvergenceException();
        }

        /// <summary>
        /// Computes the complex digamma (&#x3C8;) function.
        /// </summary>
        /// <param name="z">The complex argument.</param>
        /// <returns>The value of &#x3C8;(z).</returns>
        /// <remarks>
        /// <para>The image below shows the complex &#x3C8; function near the origin using domain coloring.</para>
        /// <img src="../images/ComplexPsiPlot.png" />
        /// </remarks>
        /// <seealso cref="AdvancedMath.Psi(double)" />
        /// <seealso href="https://en.wikipedia.org/wiki/Digamma_function"/>
        public static Complex Psi (Complex z) {
            if (z.Re < 0.5) {
                // reduce z.Re in order to handle large real values!
                return (Psi(1.0 - z) - Math.PI / ComplexMath.Tan(Math.PI * z));
            } else {
                // add Stirling for large z
                return (Lanczos.Psi(z));
            }
        }

    }



    // This class handles the Lanczos approximation to the \Gamma function and the correspoding approximations to associated functions.
    // For basic background to the Lanczos approximation, see http://en.wikipedia.org/wiki/Lanczos_approximation and
    // http://mathworld.wolfram.com/LanczosApproximation.html and http://www.boost.org/doc/libs/1_53_0/libs/math/doc/sf_and_dist/html/math_toolkit/backgrounders/lanczos.html.
    // The basic Lanczos formula is:
    //   \Gamma(z+1) = \sqrt{2 \pi} (z + g + 1/2)^(z+1/2) e^{-(z + g + 1/2)} \left[ c_0 + \frac{c_1}{z+1} + \frac{c_2}{z+2} + \cdots + \frac{c_N}{z+N} \right]
    // Given a value of g, the c-values can be computed using a complicated set of matrix equations that require high precision.
    // We write this as:
    //   \Gamma(z) = \sqrt{2 \pi} (z + g - 1/2)^(z-1/2) e^{-(z + g - 1/2)} \left[ c_0 + \frac{c_1}{z} + \frac{c_2}{z+1} + \cdots + \frac{c_N}{z+N-1} \right]
    //             = \sqrt{2 \pi} (\frac{z + g - 1/2}{e})^{z-1/2} e^{-g} \left[ c_0 + \frac{c_1}{z} + \frac{c_2}{z+1} + \cdots + \frac{c_N}{z+N-1} \right]

    internal static class Lanczos {

        // These are listed at http://www.mrob.com/pub/ries/lanczos-gamma.html as the values used by GSL, although I don't know if they still are.
        // Measured deviations at integers 2, 2, 4, 11, 1, 17, 22, 21 X 10^(-16) so this is clearly worse than Godfrey's coefficients, although it
        // does manage with slightly fewer terms.
        /*
        private const double LanczosG = 7.0;
        private static readonly double[] LanczosC = new double[] {
            0.99999999999980993, 676.5203681218851, -1259.1392167224028,
            771.32342877765313, -176.61502916214059, 12.507343278686905,
            -0.13857109526572012, 9.9843695780195716e-6, 1.5056327351493116e-7
        };
        */

        // Godfrey's coefficients, claimed relative error < 10^(-15), documented at http://my.fit.edu/~gabdo/gamma.txt and in NR 3rd edition section 6.1.
        // Measured relative deviation at integers 1, 1, 4, 1, 4, 5, 6, 3 X 10^(-16) so this appears about right.
        // These improves to 1, 1, 2, 1, 3, 3, 3 X 10^(-16) when we pull the 1/e into Math.Pow(t/e, z+1/2) instead of calling Math.Exp(-t) seperately.

        private const double LanczosG = 607.0 / 128.0;

        private static readonly double[] LanczosC = new double[] {
            0.99999999999999709182,
            57.156235665862923517,
            -59.597960355475491248,
            14.136097974741747174,
            -0.49191381609762019978,
            3.3994649984811888699e-5,
            4.6523628927048575665e-5,
            -9.8374475304879564677e-5,
            1.5808870322491248884e-4,
            -2.1026444172410488319e-4,
            2.1743961811521264320e-4,
            -1.6431810653676389022e-4,
            8.4418223983852743293e-5,
            -2.6190838401581408670e-5,
            3.6899182659531622704e-6
        };


        // These coefficients are given by Pugh in his thesis (http://web.viu.ca/pughg/phdThesis/phdThesis.pdf) table 8.5, p. 116
        // (We record them in his form; to get the usual form, multiply by Exp(-\gamma+1/2) \sqrt{2} / \pi.)
        // He claims that they "guarantee 16 digit floating point accuracy in the right-half plane", and since he uses fewer coefficients
        // than Godfrey that would be fantastic. But we measure relative deviations at the integers of 2, 0, 35, 23, 45, 42, 6 X 10^(-16),
        // making this relatively bad.
        // Unfortunately, we didn't do these measurements ourselves at first, so we actually used these coefficients until version 3.
        // Perhaps this really does give 53 bits of accuracy if you do the calculation with more bits. The fact that G is relatively large
        // makes the initial coefficients relatively large, which probably leads to cancellation errors.
        /*
        private const double LanczosG = 10.900511;

        private static readonly double[] LanczosC = new double[] {
            +2.48574089138753565546e-5,
            1.05142378581721974210,
            -3.45687097222016235469,
            +4.51227709466894823700,
            -2.98285225323576655721,
            +1.05639711577126713077,
            -1.95428773191645869583e-1,
            +1.70970543404441224307e-2,
            -5.71926117404305781283e-4,
            +4.63399473359905636708e-6,
            -2.71994908488607703910e-9,
        };
        */

        // From LanczosG, we derive several values that we need only compute once.

        private static readonly double LanczosGP = LanczosG - 0.5;
        private static readonly double LanczosExpG = Math.Exp(-LanczosG);
        private static readonly double LanczosExpGP = Math.Exp(-LanczosGP);

        private static double Sum (double x) {
            double s = LanczosC[0] + LanczosC[1] / x;
            for (int i = 2; i < LanczosC.Length; i++) {
                x += 1.0;
                s += LanczosC[i] / x;
            }
            return (s);
        }

        private static Complex Sum (Complex z) {
            Complex s = LanczosC[0] + LanczosC[1] / z;
            for (int i = 2; i < LanczosC.Length; i++) {
                z += 1.0;
                s += LanczosC[i] / z;
            }
            return (s);
        }

        private static double LogSumPrime (double x) {
            double q = LanczosC[0] + LanczosC[1] / x;
            double p = LanczosC[1] / (x * x);
            for (int i = 2; i < LanczosC.Length; i++) {
                x += 1.0;
                q += LanczosC[i] / x;
                p += LanczosC[i] / (x * x);
            }
            return (-p / q);
        }

        private static Complex LogSumPrime (Complex z) {
            Complex q = LanczosC[0] + LanczosC[1] / z;
            Complex p = LanczosC[1] / (z * z);
            for (int i = 2; i < LanczosC.Length; i++) {
                z += 1.0;
                q += LanczosC[i] / z;
                p += LanczosC[i] / (z * z);
            }
            return (-p / q);
        }

        // To calculate \frac{\Gamma(x + y)}{\Gamma(x)}, we need \frac{S(x+y)}{S(x)}. A little bit of algebra shows
        //   \frac{S(x+y)}{S(x)} = 1 - y \left[ \sum_{k=1} \frac{c_k}{(x + y + (k-1))(x + (k-1))} \right]
        //                               \left[ c_0 + \sum_{k=1} \frac{c_k}{x + (k-1)} \right]
        // Expressing the result as the rate at which the ratio moves away from 1 as y increases allows us to
        // derive accurate results for small y.

        private static double RatioOfSumsResidual (double x, double y) {
            double z = x + y;
            double p = LanczosC[1] / (x * z);
            double q = LanczosC[0] + LanczosC[1] / x;
            for (int i = 2; i < LanczosC.Length; i++) {
                x += 1.0;
                z += 1.0;
                p += LanczosC[i] / (x * z);
                q += LanczosC[i] / x;
            }
            return (p / q);
        }

        public static double Gamma (double x) {
            double t = x + LanczosGP;
            return (
                Global.SqrtTwoPI *
                Math.Pow(t / Math.E, x - 0.5) * LanczosExpG *
                Sum(x)
            );
        }

        public static double LogGamma (double x) {
            double t = x + LanczosGP;
            return (
                Math.Log(Global.SqrtTwoPI * Sum(x)) +
                (x - 0.5) * Math.Log(t) - t
            );
        }

        public static Complex LogGamma (Complex z) {
            Complex t = z + LanczosGP;
            return (
                Math.Log(Global.SqrtTwoPI) +
                (z - 0.5) * ComplexMath.Log(t) - t +
                ComplexMath.Log(Sum(z))
            );
        }

        public static double Psi (double x) {
            double t = x + LanczosGP;
            return (Math.Log(t) - LanczosG / t + LogSumPrime(x));
        }

        public static Complex Psi (Complex z) {
            Complex t = z + LanczosGP;
            return (ComplexMath.Log(t) - LanczosG / t + LogSumPrime(z));
        }

        public static double ReducedLogPochhammer (double x, double y) {
            double r = MoreMath.ReducedLogOnePlus(-RatioOfSumsResidual(x, y), y) +
                (x - 0.5) * MoreMath.ReducedLogOnePlus(1.0 / (x + LanczosGP), y) +
                Math.Log(x + y + LanczosGP) - 1.0;
            return (r);
        }

        // If we just compute Exp( LogGamma(x) + LogGamma(y) - LogGamma(x+y) ) then several leading terms in the sum cancel,
        // potentially introducing cancelation error. So we write out the ratios explicitly and take the opportunity
        // to write the result in terms of some naturally occuring ratios.

        public static double Beta (double x, double y) {
            double tx = x + LanczosGP;
            double ty = y + LanczosGP;
            double txy = x + y + LanczosGP;
            return (
                Global.SqrtTwoPI * LanczosExpGP *
                Math.Pow(tx / txy, x) * Math.Pow(ty / txy, y) * Math.Sqrt(txy / tx / ty) *
                Sum(x) * Sum(y) / Sum(x + y)
            );
        }

        public static double LogBeta (double x, double y) {
            double tx = x + LanczosGP;
            double ty = y + LanczosGP;
            double txy = x + y + LanczosGP;
            return(
                Math.Log(2.0 * Math.PI / txy) / 2.0 + (x - 0.5) * Math.Log( tx / txy) + (y - 0.5) * Math.Log(ty / txy) +
                Math.Log(LanczosExpGP * Sum(x) * Sum(y) / Sum(x + y))
            );
        }

    }

    // This class implements algorithms that arise from Stirling's approximation to the Gamma function, which is an asymptotic expansion good for large
    // arguments.

    // The Sterling sum converges to full double precision within 12 terms for all x > 16.

    internal static class Stirling {

        private static double Sum (double x) {

            double xx = x * x; // x^2 
            double xk = x; // tracks x^{2k - 1}
            double f = AdvancedIntegerMath.Bernoulli[1] / (2.0 * xk); // k = 1 term
            for (int k = 2; k < AdvancedIntegerMath.Bernoulli.Length; k++) {
                double f_old = f;
                xk *= xx;
                f += AdvancedIntegerMath.Bernoulli[k] / ((2 * k) * (2 * k - 1)) / xk;
                if (f == f_old) return (f);
            }

            throw new NonconvergenceException();

        }

        private static double SumPrime (double x) {

            double xx = x * x;
            double xk = xx; // tracks x^{2k}
            double f = -AdvancedIntegerMath.Bernoulli[1] / (2.0 * xk); // k=1 term
            for (int k = 2; k < AdvancedIntegerMath.Bernoulli.Length; k++) {
                double f_old = f;
                xk *= xx;
                f -= AdvancedIntegerMath.Bernoulli[k] / (2 * k) / xk;
                if (f == f_old) return (f);
            }
            throw new NonconvergenceException();

        }

        // Computes (S(x + y) - S(x)) / y, i.e. the rate at which S(x + y) - S(x) changes with y

        private static double ReducedPochhammerSum (double x, double y) {
            double L = MoreMath.ReducedLogOnePlus(1.0 / x, y);
            double xx = x * x;
            double xk = x;
            double f = AdvancedIntegerMath.Bernoulli[1] / (2.0 * xk) * MoreMath.ReducedExpMinusOne(-L, y);
            for (int k = 2; k < AdvancedIntegerMath.Bernoulli.Length; k++) {
                double f_old = f;
                xk *= xx;
                f += AdvancedIntegerMath.Bernoulli[k] / ((2 * k) * (2 * k - 1)) / xk * MoreMath.ReducedExpMinusOne((1 - 2 * k) * L, y);
                if (f == f_old) return (f);
            }
            throw new NonconvergenceException();

        }

        public static double LogGamma (double x) {
            // we-write to use (x-0.5) form to eliminate one one by storing log(2\pi)?
            return (x * Math.Log(x) - x - Math.Log(x / (2.0 * Math.PI)) / 2.0 + Sum(x));
        }

        public static double Gamma (double x) {
            // return (Math.Sqrt(2.0 * Math.PI / x) * Math.Pow(x / Math.E, x) * Math.Exp(Sum(x)));
            return (Math.Exp(LogGamma(x)));
        }

        public static double ReducedLogPochhammer (double x, double y) {
            double L = MoreMath.ReducedLogOnePlus(1.0 / x, y);
            return ((x - 0.5) * L + Math.Log(x + y) - 1.0 + ReducedPochhammerSum(x, y));
        }

        public static double Beta (double x, double y) {
            double xy = x + y;
            return (
                Math.Sqrt(2.0 * Math.PI * xy / x / y) *
                Math.Pow(x / xy, x) * Math.Pow(y / xy, y) *
                Math.Exp(Sum(x) + Sum(y) - Sum(xy))
            );
        }

        public static double LogBeta (double x, double y) {
            double xy = x + y;
            return (x * Math.Log(x / xy) + y * Math.Log(y / xy) - Math.Log(x / xy * y / (2.0 * Math.PI)) / 2.0 + Sum(x) + Sum(y) - Sum(xy));
        }

        public static double Psi (double x) {
            return (Math.Log(x) - 0.5 / x + SumPrime(x));
        }

        // Compute \frac{x^{\nu}}{\Gamma(\nu + 1)}

        public static double PowOverGammaPlusOne (double x, double nu) {
            return (Math.Pow(x * Math.E / nu, nu) / Math.Sqrt(Global.TwoPI * nu) / Math.Exp(Sum(nu)));
        }

        // Compute \frac{x^a (1-x)^b}{B(a,b)}
        // For large a, b, B(a,b) is often unrepresentably small but this ratio is still representable because x^a (1-x)^b is also small.
        // To compute this ratio, we bring x and (1-x) inside the a and b powers that we need to take anyway to compute B(a,b).
        
        // At first we tried doing this explicitly, computing (x / (a / ab))^a * ((1-x) / (b / ab))^b. While this does work over a larger range
        // than computing x^a * (1-x)^b and B(a,b) seperately, there are still many values for which one of the powers is unrepresentably small (0)
        // the other is unrepresentably large (Infinity), so the product becomes 0 * Infinity = NaN. We threfore fall back and do this in log-space.

        public static double PowOverBeta (double a, double b, double x) {
            double ab = a + b;
            double t = a * Math.Log(x / (a / ab)) + b * Math.Log((1.0 - x) / (b / ab));
            return (
                Math.Sqrt(a / ab * b / (2.0 * Math.PI)) * Math.Exp(Sum(ab) - Sum(a) - Sum(b) + t)
            );
        }

    }

}
