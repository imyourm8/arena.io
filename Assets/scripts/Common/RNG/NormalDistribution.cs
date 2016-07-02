using System;

/*
    Converts uniform double value to Gaussian value (Normal)
 */

public class NormalDistribution
{
    private static double[] cdf_a = new double[] { 
			1.161110663653770e-002,
			3.951404679838207e-001,
			2.846603853776254e+001,
			1.887426188426510e+002,
			3.209377589138469e+003
		};
    private static double[] cdf_b = new double[] { 
			1.767766952966369e-001,
			8.344316438579620e+000,
			1.725514762600375e+002,
			1.813893686502485e+003,
			8.044716608901563e+003
		};
    private static double[] cdf_c = new double[] {
			2.15311535474403846e-8,
			5.64188496988670089e-1,
			8.88314979438837594e00,
			6.61191906371416295e01,
			2.98635138197400131e02,
			8.81952221241769090e02,
			1.71204761263407058e03,
			2.05107837782607147e03,
			1.23033935479799725E03	
		};

    private static double[] cdf_d = new double[] {
			1.00000000000000000e00,
			1.57449261107098347e01,
			1.17693950891312499e02,
			5.37181101862009858e02,
			1.62138957456669019e03,
			3.29079923573345963e03,
			4.36261909014324716e03,
			3.43936767414372164e03,
			1.23033935480374942e03
		};

    private static double[] cdf_p = new double[] {
			1.63153871373020978e-2,
			3.05326634961232344e-1,
			3.60344899949804439e-1,
			1.25781726111229246e-1,
			1.60837851487422766e-2,
			6.58749161529837803e-4
		};

    private static double[] cdf_q = new double[] {
			1.00000000000000000e00,
			2.56852019228982242e00,
			1.87295284992346047e00,
			5.27905102951428412e-1,
			6.05183413124413191e-2,
			2.33520497626869185e-3
		};
    public static double stdnormal_cdf(double u)
    {
        const double m_1_sqrtpi = 0.564189583547756286948;
        double msqrt2 = 1.41421356237309504880;
        double z, y;

        if (Double.IsNaN(u))
        {
            return (Double.NaN);
        }

        if (Double.IsInfinity(u))
        {
            return (u < 0 ? 0.0 : 1.0);
        }

        y = Math.Abs(u);

        if (y <= 0.46875 * msqrt2)
        {
            /* evaluate erf() for |u| <= sqrt(2)*0.46875 */
            z = y * y;
            y = u * ((((cdf_a[0] * z + cdf_a[1]) * z + cdf_a[2]) * z + cdf_a[3]) * z + cdf_a[4])
                / ((((cdf_b[0] * z + cdf_b[1]) * z + cdf_b[2]) * z + cdf_b[3]) * z + cdf_b[4]);
            return 0.5 + y;
        }

        z = Math.Exp(-y * y / 2) / 2;
        if (y <= 4.0)
        {
            /* evaluate erfc() for sqrt(2)*0.46875 <= |u| <= sqrt(2)*4.0 */
            y = y / msqrt2;
            y = ((((((((cdf_c[0] * y + cdf_c[1]) * y + cdf_c[2]) * y + cdf_c[3]) * y + cdf_c[4]) * y + cdf_c[5]) * y + cdf_c[6]) * y + cdf_c[7]) * y + cdf_c[8])
            / ((((((((cdf_d[0] * y + cdf_d[1]) * y + cdf_d[2]) * y + cdf_d[3]) * y + cdf_d[4]) * y + cdf_d[5]) * y + cdf_d[6]) * y + cdf_d[7]) * y + cdf_d[8]);
            y = z * y;
        }
        else
        {
            /* evaluate erfc() for |u| > sqrt(2)*4.0 */
            z = z * msqrt2 / y;
            y = 2 / (y * y);
            y = y * (((((cdf_p[0] * y + cdf_p[1]) * y + cdf_p[2]) * y + cdf_p[3]) * y + cdf_p[4]) * y + cdf_p[5])
            / (((((cdf_q[0] * y + cdf_q[1]) * y + cdf_q[2]) * y + cdf_q[3]) * y + cdf_q[4]) * y + cdf_q[5]);
            y = z * (m_1_sqrtpi - y);
        }

        return (u < 0.0 ? y : 1 - y);

    }

    private static double[] a = new double[] {
			-3.969683028665376e+01,  2.209460984245205e+02,
			-2.759285104469687e+02,  1.383577518672690e+02,
			-3.066479806614716e+01,  2.506628277459239e+00
		};

    private static double[] b = new double[] {
			-5.447609879822406e+01,  1.615858368580409e+02,
			-1.556989798598866e+02,  6.680131188771972e+01,
			-1.328068155288572e+01
		};

    private static double[] c = new double[] {
			-7.784894002430293e-03, -3.223964580411365e-01,
			-2.400758277161838e+00, -2.549732539343734e+00,
			4.374664141464968e+00,  2.938163982698783e+00
		};

    private static double[] d = new double[] {
			7.784695709041462e-03,  3.224671290700398e-01,
			2.445134137142996e+00,  3.754408661907416e+00
		};

    public static double stdnormal_inv(double p)
    {
        const double m_sqrt2pi = 2.50662827463100;

        double q, t, u;

        if (Double.IsNaN(p) || p > 1.0 || p < 0.0)
        {
            return (Double.NaN);
        }
        if (p == 0.0)
        {
            return Double.NegativeInfinity;
        }
        if (p == 1.0)
        {
            return Double.PositiveInfinity;
        }

        q = (p < (1 - p)) ? p : (1 - p);

        if (q > 0.02425)
        {
            /* Rational approximation for central region. */
            u = q - 0.5;
            t = u * u;
            u = u * (((((a[0] * t + a[1]) * t + a[2]) * t + a[3]) * t + a[4]) * t + a[5])
            / (((((b[0] * t + b[1]) * t + b[2]) * t + b[3]) * t + b[4]) * t + 1);
        }
        else
        {
            /* Rational approximation for tail region. */
            t = Math.Sqrt(-2 * Math.Log(q));
            u = (((((c[0] * t + c[1]) * t + c[2]) * t + c[3]) * t + c[4]) * t + c[5])
            / ((((d[0] * t + d[1]) * t + d[2]) * t + d[3]) * t + 1);
        }

        /* The relative error of the approximation has absolute value less
           than 1.15e-9.  One iteration of Halley's rational method (third
           order) gives full machine precision... */

        t = stdnormal_cdf(u) - q;    /* error */
        t = t * m_sqrt2pi * Math.Exp(u * u / 2);   /* f(u)/df(u) */
        u = u - t / (1 + u * t / 2);     /* Halley's method */

        return (p > 0.5 ? -u : u);
    }

    public static double toNormalDistribution(double rn, float temperature)
    {
        // Trasformiamo questo numero in una distribuzione normale (CDF inversa, nota anche come Gaussiana, vedi http://en.wikipedia.org/wiki/Normal_distribution)
        // facendo l'inversa della cumulative density function su una distribuzione uniforme [0,1] otteniamo una distribuzione normale (-inf,+inf) centrata a zero
        // e con deviazione standard 1
        double localVal = stdnormal_inv(rn);

        // A questo punto passiamo attraverso una funzione sigmoide (o curva logistica) con Lateratura 5 
        // Questo ci riporta a un campo (0,1), il valore 5 fa si che circa il 90% dei valori restituiti sia tra 0.45 e 0.55, aumentandolo
        // la distribuzione si stringe (piu' valori vicini a 0.5), abbassandolo si allarga, ma a differenza di una gaussiana in ogni caso
        // il valore restituito e' tra zero e uno.
        localVal = localVal / temperature;
        localVal = 1.0 / (1.0 + Math.Exp(-localVal));
        return (localVal);
    }
}