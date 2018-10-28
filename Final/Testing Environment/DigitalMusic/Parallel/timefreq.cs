using System;
using System.Numerics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DMParallel
{
    public class timefreq
    {
        
        public float[][] timeFreqData;
        public int wSamp;
        public Complex[] twiddles;
        private readonly object lockObj = new object();

        public Complex[] compXG;
        public timefreq(float[] x, int windowSamp)
        {
            int workerThreadCount;
            int ioThreadCount;

            Console.WriteLine("Determining threadcount");
            ThreadPool.GetMinThreads(out workerThreadCount, out ioThreadCount);
            Console.WriteLine(workerThreadCount);
            ThreadPool.SetMinThreads(1, ioThreadCount);

            ThreadPool.GetMinThreads(out workerThreadCount, out ioThreadCount);
            Console.WriteLine(workerThreadCount);

            int workerThreads, completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            workerThreads = 4;
            ThreadPool.SetMaxThreads(1, completionPortThreads);

            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            Console.WriteLine(workerThreadCount);


            // int ii;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;
            this.wSamp = windowSamp;
            twiddles = new Complex[wSamp];


            //Parallel.For(0, wSamp, ii =>
            //{
            //    double a = 2 * pi * ii / (double)wSamp;
            //    twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            //});
            for (int ii = 0; ii < wSamp; ii++)
            {
                double a = 2 * pi * ii / (double)wSamp;
                twiddles[ii] = Complex.Pow(Complex.Exp(-i), (float)a);
            }

            timeFreqData = new float[wSamp / 2][];

            
            int nearest = (int)Math.Ceiling((double)x.Length / (double)wSamp);
            nearest = nearest * wSamp;

            Complex[] compX = new Complex[nearest];

            //Parallel.For(0, nearest, kk =>
            //{
            //    if (kk < x.Length)
            //    {
            //        compX[kk] = x[kk];
            //    }
            //    else
            //    {
            //        compX[kk] = Complex.Zero;
            //    }
            //});

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

            Parallel.For(0, wSamp / 2, jj =>
              {
                  timeFreqData[jj] = new float[cols];
              });


            //for (int jj = 0; jj < wSamp / 2; jj++)
            //{
            //    timeFreqData[jj] = new float[cols];
            //}
            
            compXG = compX;
            timeFreqData = stft(compX, wSamp);
        }

        public float[][] stft(Complex[] x, int wSamp)
        {
            
            //int ii = 0;
            //int jj = 0;
            //int kk = 0;
            //int ll = 0;
            int N = x.Length;
            float fftMax = 0;

            float[][] Y = new float[wSamp / 2][];

            //Parallel.For(0, wSamp/2, ll =>
            //{
            //    Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            //});
            
            for (int ll = 0; ll < wSamp / 2; ll++)
            {
                Y[ll] = new float[2 * (int)Math.Floor((double)N / (double)wSamp)];
            }

            //Complex[] temp = new Complex[wSamp];
            //Complex[] tempFFT = new Complex[wSamp];

            ////Outer with local vars
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

            //         tempFFT = fft(temp, 11);

            //         for (int kk = 0; kk < wSamp / 2; kk++)
            //         {
            //             Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]); //Safe since it runs in the I loop alongside KK
            //             fftPotentialMax = Math.Max(Y[kk][ii], fftPotentialMax);
            //         }

            //         return fftPotentialMax;
            //     },
            //     (fftPotentialMax) =>
            //     {
            //         lock (x)
            //         {
            //             fftMax = Math.Max(fftMax, fftPotentialMax);
            //         }
            //         //Interlocked.Exchange(ref fftMax, Math.Max(fftMax, fftPotentialMax));
            //     });

            ////Outer with non-locals
            Parallel.For(0, 2 * (int)Math.Floor((double)N / (double)wSamp) - 1, ii =>
            {
                Complex[] temp = new Complex[wSamp];
                Complex[] tempFFT = new Complex[wSamp];

                for (int jj = 0; jj < wSamp; jj++)
                {
                    temp[jj] = x[ii * (wSamp / 2) + jj];
                }

                tempFFT = fft(temp);

                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

                    //lock (lockObj)
                    //{
                    //    fftMax = Math.Max(fftMax, Y[kk][ii]);
                    //}
                    Interlocked.Exchange(ref fftMax, Math.Max(fftMax, Y[kk][ii]));
                }
            });

            //Complex[] temp = new Complex[wSamp];
            //Complex[] tempFFT = new Complex[wSamp];

            //for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            //{
            //    Parallel.For(0, wSamp, jj =>
            //    {
            //        temp[jj] = x[ii * (wSamp / 2) + jj];
            //    });
            //    //for (int jj = 0; jj < wSamp; jj++)
            //    //{
            //    //    temp[jj] = x[ii * (wSamp / 2) + jj];
            //    //}

            //    tempFFT = fft(temp);

            //    Parallel.For(0, wSamp / 2, kk =>
            //    {
            //        Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

            //        if (Y[kk][ii] > fftMax)
            //        {
            //            fftMax = Y[kk][ii];
            //        }
            //    });
            //    //for (int kk = 0; kk < wSamp / 2; kk++)
            //    //{
            //    //    Y[kk][ii] = (float)Complex.Abs(tempFFT[kk]);

            //    //    if (Y[kk][ii] > fftMax)
            //    //    {
            //    //        fftMax = Y[kk][ii];
            //    //    }
            //    //}


            //}

            Parallel.For(0, 2 * (int)Math.Floor((double)N / (double)wSamp) - 1, ii =>
            {
                for (int kk = 0; kk < wSamp / 2; kk++)
                {
                    Y[kk][ii] /= fftMax;
                }
            });
            //Parallel.For(0, wSamp / 2, zz =>
            //{
            //    for (int gg = 0; gg < 2 * Math.Floor((double)N / (double)wSamp) - 1; gg++)
            //    {
            //        Y[zz][gg] /= fftMax;
            //    }
            //});

            //for (int ii = 0; ii < 2 * Math.Floor((double)N / (double)wSamp) - 1; ii++)
            //{
            //    for (int kk = 0; kk < wSamp / 2; kk++)
            //    {
            //        Y[kk][ii] /= fftMax;
            //    }
            //}
            return Y;
        }

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
                //if (Interlocked.Decrement(ref maxDepth) == 0)
                //{
                //    E = fft(even, maxDepth);
                //    O = fft(odd, maxDepth);
                //}
                //else
                //{
                //    Parallel.Invoke(
                //        () => E = fft(even, maxDepth),
                //        () => O = fft(odd, maxDepth));
                //}
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
