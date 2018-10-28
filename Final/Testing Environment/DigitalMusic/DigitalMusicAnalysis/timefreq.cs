using System;
using System.Numerics;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;

namespace DigitalMusicAnalysis
{
    public class timefreq
    {
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;
        private readonly object lockObj = new object();

        public timefreq(float[] x, int windowSamp)
        {
            //int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;

            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];

            for (int ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }

            timeFreqData = new float[wSamp / 2][];

            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];
            for (int kk = 0; kk < nearest; kk++)
            {
                if (kk < x.Length)
                {
                    compX[kk] = x[kk];
                }
                else
                {
                    compX[kk] = Complex.Zero;
                }
            }


            int cols = 2 * nearest / wSamp;

            for (int jj = 0; jj < wSamp / 2; jj++)
            {
                timeFreqData[jj] = new float[cols];
            }

            timeFreqData = stft(compX, wSamp);

        }

        float[][] stft(Complex[] x, int wSamp)
        {
            int ii = 0;
            int jj = 0;
            int kk = 0;
            int ll = 0;
            int N = x.Length;
            float fftMax = 0;

            float[][] Y = new float[wSamp / 2][];

            for (ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }

            Complex[] temp = new Complex[wSamp];
            Complex[] tempFFT = new Complex[wSamp];

            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {

                for (jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp);

                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    if (Y[kk][ii] > fftMax)
                    {
                        fftMax = Y[kk][ii];
                    }
                }


            }

            for (ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            {
                for (kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            }

            return Y;
            ////int ii = 0;
            ////int jj = 0;
            ////int kk = 0;
            //int ll = 0;
            //int N = x.Length;
            //float fftMax = 0;

            //float[][] Y = new float[wSamp / 2][];


            //for (ll = 0; ll < wSamp / 2; ll++)
            //{
            //    Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            //}

            ////Complex[] temp = new Complex[wSamp];
            ////Complex[] tempFFT = new Complex[wSamp];

            //Parallel.For(
            //     0,
            //     2 * (int)Math.Floor((double)N / (double)wSamp) - 1,
            //     () => 0f, //The max value local var
            //     (ii, loop, fftPotentialMax) =>
            //     {
            //         Complex[] temp = new Complex[wSamp];
            //         Complex[] tempFFT = new Complex[wSamp];

            //         for (int jj = 0; jj < wSamp; jj++)
            //         {
            //             temp[jj] = x[ii * (wSamp / 2) + jj];
            //         }

            //         tempFFT = fft(temp);

            //         for (int kk = 0; kk < wSamp / 2; kk++)
            //         {
            //             Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]); //Safe since it runs in the I loop alongside KK
            //             fftPotentialMax = Math.Max(Y[kk][ii], fftPotentialMax);
            //         }

            //         return fftPotentialMax;
            //     },
            //     (fftPotentialMax) =>
            //     {
            //         lock (lockObj)
            //         {
            //             fftMax = Math.Max(fftMax, fftPotentialMax);
            //         }
            //         //Interlocked.Exchange(ref fftMax, Math.Max(fftMax, fftPotentialMax));
            //     });

            //for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            //{
            //    for (int kk = 0; kk < wSamp / 2; kk++)
            //    {
            //        Y[kk][ii] /= fftMax;
            //    }
            //}

            //return Y;
        }

        //Complex[] fft(Complex[] x)
        //{
        //    int ii = 0;
        //    int kk = 0;
        //    int N = x.Length;
        //    int n = N;
        //    Complex[] Y = new Complex[N];

        //    // NEED TO MEMSET TO ZERO?


        //    if (N == 1)
        //    {
        //        Y[0] = x[0];
        //    }
        //    else
        //    {
        //        Complex[] E = new Complex[N / 2];
        //        Complex[] O = new Complex[N / 2];
        //        Complex[] even = new Complex[N / 2];
        //        Complex[] odd = new Complex[N / 2];

        //        for (ii = 0; ii < N; ii++)
        //        {

        //            if (ii % 2 == 0)
        //            {
        //                even[ii / 2] = x[ii];
        //            }
        //            if (ii % 2 == 1)
        //            {
        //                odd[(ii - 1) / 2] = x[ii];
        //            }
        //        }

        //        E = fft(even);
        //        O = fft(odd);

        //        for (kk = 0; kk < N; kk++)
        //        {
        //            Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
        //        }
        //    }

        //    return Y;
        //}

        //public static int BitReverse(int n, int bits)
        //{
        //    int reversedN = n;
        //    int count = bits - 1;

        //    n >>= 1;
        //    while (n > 0)
        //    {
        //        reversedN = (reversedN << 1) | (n & 1);
        //        count--;
        //        n >>= 1;
        //    }

        //    return ((reversedN << count) & ((1 << bits) - 1));
        //}

        //Complex[] fft(Complex[] x)
        //{
        //    int N = x.Length;
        //    Complex[] Y = new Complex[N];

        //    int bits = (int)Math.Log(N, 2);
        //    for (int j = 0; j < N; j++)
        //    {
        //        int swapPos = BitReverse(j, bits);
        //        Y[j] = x[swapPos];
        //    }
        //    for (int j = 2; j <= N; j <<= 1)
        //    {
        //        for (int i = 0; i < N; i += j)
        //        {
        //            for (int k = 0; k < j / 2; k++)
        //            {
        //                int evenIndex = i + k;
        //                int oddIndex = i + k + (j / 2);
        //                //Set initially to prevent overwrite
        //                var even = Y[evenIndex];
        //                var odd = Y[oddIndex];

        //                //Assign seperately instead of same time
        //                Y[evenIndex] = even + odd * twiddles[k * (N / j)]; // even
        //                Y[oddIndex] = even + odd * twiddles[(k + (j / 2)) * (N / j)]; //odd
        //            }
        //        }
        //    }
        //    return Y;
        //}

        Complex[] fft(Complex[] x)
        {
            int ii = 0;
            int kk = 0;
            int N = x.Length;

            Complex[] Y = new Complex[N];

            // NEED TO MEMSET TO ZERO?

            if (N == 1)
            {
                Y[0] = x[0];
            }
            else
            {

                Complex[] E = new Complex[N / 2];
                Complex[] O = new Complex[N / 2];
                Complex[] even = new Complex[N / 2];
                Complex[] odd = new Complex[N / 2];

                for (ii = 0; ii < N; ii++)
                {

                    if (ii % 2 == 0)
                    {
                        even[ii / 2] = x[ii];
                    }
                    if (ii % 2 == 1)
                    {
                        odd[(ii - 1) / 2] = x[ii];
                    }
                }

                E = fft(even);
                O = fft(odd);

                for (kk = 0; kk < N; kk++)
                {
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * wSamp / N];
                }
            }

            return Y;
        }
    }
}
