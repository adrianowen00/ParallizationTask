using System;
using System.IO;
using NAudio.Wave;
using DigitalMusicAnalysis;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Xml;
using System.Threading;
using System.Collections.Concurrent;

namespace DMParallel
{
    public class DMParallel
    {
        public String filename;
        public wavefile waveIn;
        public WaveFileReader waveReader;
        public timefreq stftRep;
        public musicNote[] sheetmusic;
        public float[] pixelArray;
        public Complex[] twiddles;
        public Complex[] compX;
        public enum pitchConv { C, Db, D, Eb, E, F, Gb, G, Ab, A, Bb, B };
        public double bpm = 70;
        public string[] alignedStrings = new string[2];
        private readonly object lockObj = new object();

        public DMParallel()
        {
            filename = "E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\Jupiter.wav";
            loadWave(filename);
            string xmlfile = "E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\Jupiter.xml";
            sheetmusic = readXML(xmlfile);
        }

        public void freqDomain()
        {
            stftRep = new timefreq(waveIn.wave, 2048);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            pixelArray = new float[stftRep.timeFreqData[0].Length * stftRep.wSamp / 2];

            Parallel.For(0, stftRep.wSamp / 2, jj =>
            {
                for (int ii = 0; ii < stftRep.timeFreqData[0].Length; ii++)
                {
                    pixelArray[jj * stftRep.timeFreqData[0].Length + ii] = stftRep.timeFreqData[jj][ii];
                }
            });

            //for (int jj = 0; jj < stftRep.wSamp / 2; jj++)
            //{
            //    for (int ii = 0; ii < stftRep.timeFreqData[0].Length; ii++)
            //    {
            //        pixelArray[jj * stftRep.timeFreqData[0].Length + ii] = stftRep.timeFreqData[jj][ii];
            //    }
            //}

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine("mainWindowPar.freqDomain():");
            Console.WriteLine(ts.TotalMilliseconds);
        }

        // Unparallizable / Worthless
        public void loadWave(string filename)
        {
            // Sound File
            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (file == null)
            {
                System.Console.Write("Failed to Open File!");
            }
            else
            {
                waveIn = new wavefile(file);
                waveReader = new WaveFileReader(filename);
            }
        }

        public void loadHistogram()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // HISTOGRAM

            float fs = waveIn.SampleRate;
            float divisor = fs / stftRep.wSamp;

            noteGraph LOWEST = new noteGraph(110, divisor);
            noteGraph LOW = new noteGraph(220, divisor);
            noteGraph MIDDLE = new noteGraph(440, divisor);
            noteGraph HIGH = new noteGraph(880, divisor);
            noteGraph HIGHEST = new noteGraph(1760, divisor);

            int rows = stftRep.wSamp / 2;

            float[] column = new float[rows];

            for (int i = 0; i < rows; i++)
            {
                column[i] = stftRep.timeFreqData[i][(int)Math.Floor(10.00000)];
            }

            LOWEST.setRectHeights(column);
            LOW.setRectHeights(column);
            MIDDLE.setRectHeights(column);
            HIGH.setRectHeights(column);
            HIGHEST.setRectHeights(column);

            double[] maxi = new double[5];
            
            

            // DYNAMIC RECTANGLES


            //Lowest Octif

            for (int ii = 0; ii < (int)Math.Floor(110 / divisor); ii++)
            {
                double lowestMarg = Math.Log(((110 + ii * divisor) / 110), 2) * 240;

            }

            //Low Octif

            for (int ii = 0; ii < (int)Math.Floor(220 / divisor); ii++)
            {
                double lowMarg = Math.Log(((220 + ii * divisor) / 220), 2) * 240;

            }

            //Middle Octif
            

            for (int ii = 0; ii < (int)Math.Floor(440 / divisor); ii++)
            {
                double midMarg = Math.Log(((440 + ii * divisor) / 440), 2) * 240;
                
            }

            for (int ii = 0; ii < (int)Math.Floor(880 / divisor); ii++)
            {
                double highMarg = Math.Log(((880 + ii * divisor) / 880), 2) * 240;
                
            }

            //Highest Octif
            

            for (int ii = 0; ii < (int)Math.Floor(1760 / divisor); ii++)
            {
                double highestMarg = Math.Log(((1760 + ii * divisor) / 1760), 2) * 240;
                
            }

            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            Console.WriteLine("mainPar.loadHisto():");
            Console.WriteLine(ts.TotalMilliseconds);
        }

        public void onsetDetection()
        {
            float[] HFC;
            int starts = 0;
            int stops = 0;
            //Complex[] Y;
            //double[] absY;
            List<int> lengths;
            List<int> noteStarts;
            List<int> noteStops;
            //List<double> pitches;
            double[] pitches;
            //int ll;
            double pi = 3.14159265;
            Complex i = Complex.ImaginaryOne;

            noteStarts = new List<int>(100);
            noteStops = new List<int>(100);
            lengths = new List<int>(100);
            //pitches = new List<double>(100);

            HFC = new float[stftRep.timeFreqData[0].Length];

            Parallel.For(0, stftRep.timeFreqData[0].Length, jj =>
            {
                for (int ii = 0; ii < stftRep.wSamp / 2; ii++)
                {
                    HFC[jj] += (float)Math.Pow((double)stftRep.timeFreqData[ii][jj] * ii, 2);
                }
            });

            //var newBag = new ConcurrentBag<float>();
            //var bag = new ConcurrentBag<float>();

            //for (int jj = 0; jj < stftRep.timeFreqData[0].Length; jj++)
            //{
            //    Parallel.For(0, stftRep.wSamp / 2, () => 0f, (ii, loop, subtotal) =>
            //    {
            //        return subtotal + (float)Math.Pow((double)stftRep.timeFreqData[ii][jj] * ii, 2);
            //    },
            //    (subtotal) => bag.Add(subtotal)
            //    );

            //    //for (int ii = 0; ii < stftRep.wSamp / 2; ii++)
            //    //{
            //    //    HFC[jj] = HFC[jj] + (float)Math.Pow((double)stftRep.timeFreqData[ii][jj] * ii, 2);
            //    //}
            //    HFC[jj] = bag.Sum();
            //    Interlocked.Exchange<ConcurrentBag<float>>(ref bag, newBag);
            //}

            float maxi = HFC.Max();

            for (int jj = 0; jj < stftRep.timeFreqData[0].Length; jj++)
            {
                HFC[jj] = (float)Math.Pow((HFC[jj] / maxi), 2);
            }

            for (int jj = 0; jj < stftRep.timeFreqData[0].Length; jj++)
            {
                if (starts > stops)
                {
                    if (HFC[jj] < 0.001)
                    {
                        noteStops.Add(jj * ((stftRep.wSamp - 1) / 2));
                        stops = stops + 1;
                    }
                }
                else if (starts - stops == 0)
                {
                    if (HFC[jj] > 0.001)
                    {
                        noteStarts.Add(jj * ((stftRep.wSamp - 1) / 2));
                        starts = starts + 1;
                    }

                }
            }

            if (starts > stops)
            {
                noteStops.Add(waveIn.data.Length);
            }


            // DETERMINES START AND FINISH TIME OF NOTES BASED ON ONSET DETECTION       

            ///*

            for (int ii = 0; ii < noteStops.Count; ii++)
            {
                lengths.Add(noteStops[ii] - noteStarts[ii]);
            }

            //added to provide safety.
            pitches = new double[lengths.Count];
            //ConcurrentDictionary<int, double> pitches_c = new ConcurrentDictionary<int, double>();
            Parallel.For(0, lengths.Count, mm =>
            {
                int nearest = (int)Math.Pow(2, Math.Ceiling(Math.Log(lengths[mm], 2)));
                Complex[] twiddles = new Complex[nearest];
                for (int ll = 0; ll < nearest; ll++)
                {
                    double a = 2 * pi * ll / (double)nearest;
                    twiddles[ll] = Complex.Pow(Complex.Exp(-i), (float)a);
                }

                Complex[] compX = new Complex[nearest];
                for (int kk = 0; kk < nearest; kk++)
                {
                    if (kk < lengths[mm] && (noteStarts[mm] + kk) < waveIn.wave.Length)
                    {
                        compX[kk] = waveIn.wave[noteStarts[mm] + kk];
                    }
                    else
                    {
                        compX[kk] = Complex.Zero;
                    }
                }

                Complex[] Y = new Complex[nearest];

                Y = fft(compX, nearest, twiddles);

                double[] absY = new double[nearest];

                double maximum = 0;
                int maxInd = 0;

                for (int jj = 0; jj < Y.Length; jj++)
                {
                    absY[jj] = Y[jj].Magnitude;
                    if (absY[jj] > maximum)
                    {
                        maximum = absY[jj];
                        maxInd = jj;
                    }
                }

                for (int div = 6; div > 1; div--)
                {

                    if (maxInd > nearest / 2)
                    {
                        if (absY[(int)Math.Floor((double)(nearest - maxInd) / div)] / absY[(maxInd)] > 0.10)
                        {
                            maxInd = (nearest - maxInd) / div;
                        }
                    }
                    else
                    {
                        if (absY[(int)Math.Floor((double)maxInd / div)] / absY[(maxInd)] > 0.10)
                        {
                            maxInd = maxInd / div;
                        }
                    }
                }

                if (maxInd > nearest / 2)
                {
                    //pitches.Add((nearest - maxInd) * waveIn.SampleRate / nearest);
                    pitches[mm] = (nearest - maxInd) * waveIn.SampleRate / nearest;
                    //pitches_c.TryAdd(mm, (nearest - maxInd) * waveIn.SampleRate / nearest);
                }
                else
                {
                    pitches[mm] = maxInd * waveIn.SampleRate / nearest;
                    //pitches.Add(maxInd * waveIn.SampleRate / nearest);
                    //pitches_c.TryAdd(mm, maxInd * waveIn.SampleRate / nearest);
                }
            });

            musicNote[] noteArray;
            noteArray = new musicNote[noteStarts.Count()];

            for (int ii = 0; ii < noteStarts.Count(); ii++)
            {
                noteArray[ii] = new musicNote(pitches[ii], lengths[ii]);
               // double value;
               // pitches_c.TryGetValue(ii, out value);
                //noteArray[ii] = new musicNote(value, lengths[ii]);
            }

            int[] sheetPitchArray = new int[sheetmusic.Length];
            int[] notePitchArray = new int[noteArray.Length];

            for (int ii = 0; ii < sheetmusic.Length; ii++)
            {
                sheetPitchArray[ii] = sheetmusic[ii].pitch % 12;
            }

            for (int jj = 0; jj < noteArray.Length; jj++)
            {
                notePitchArray[jj] = noteArray[jj].pitch % 12;
            }

            //removed aligned strings init from here to constructor to verify equality.

            alignedStrings = stringMatch(sheetPitchArray, notePitchArray);
        }

        private string[] stringMatch(int[] A, int[] B)
        {
            // SETUP SIMILARITY MATRIX
            int[][] S = new int[12][];

            for (int i = 0; i < 12; i++)
            {
                S[i] = new int[12];
            }

            for (int i = 0; i < 12; i++)
            {
                for (int j = i; j < 12; j++)
                {
                    if (i == j)
                        S[i][j] = 10;
                    else if (Math.Abs(i - j) <= 6)
                        S[i][j] = -Math.Abs(i - j);
                    else
                        S[i][j] = Math.Abs(i - j) - 12;

                    S[j][i] = S[i][j];
                }
            }

            //GAP PENALTY

            int d = -20;

            int[][] F = new int[A.Length + 1][];

            for (int i = 0; i < A.Length + 1; i++)
            {
                F[i] = new int[B.Length + 1];
            }

            for (int j = 0; j < B.Length + 1; j++)
            {
                F[0][j] = d * j;
            }

            for (int i = 0; i < A.Length + 1; i++)
            {
                F[i][0] = d * i;
            }

            for (int i = 1; i < A.Length + 1; i++)
            {
                for (int j = 1; j < B.Length + 1; j++)
                {
                    int Ai = A[i - 1];
                    int Bj = B[j - 1];

                    F[i][j] = Math.Max(Math.Max((F[i - 1][j - 1] + S[Ai][Bj]), (F[i][j - 1] + d)), (F[i - 1][j] + d));
                }
            }

            string AlignA = "";
            string AlignB = "";

            int ii = (A.Length);
            int jj = (B.Length);

            while (ii > 0 && jj > 0)
            {

                int Score = F[ii][jj];
                int ScoreDiag = F[ii - 1][jj - 1];
                int ScoreUp = F[ii][jj - 1];
                int ScoreLeft = F[ii - 1][jj];

                int Ai = (A[ii - 1]);
                int Bj = (B[jj - 1]);

                if (Score == ScoreDiag + S[Ai][Bj])
                {
                    AlignA = Enum.GetName(typeof(musicNote.notePitch), (A[ii - 1])) + AlignA;
                    AlignB = Enum.GetName(typeof(musicNote.notePitch), (B[jj - 1])) + AlignB;

                    ii = ii - 1;
                    jj = jj - 1;

                }

                else if (Score == ScoreUp + d)
                {
                    AlignA = "  " + AlignA;
                    AlignB = Enum.GetName(typeof(musicNote.notePitch), (B[jj - 1])) + AlignB;

                    jj = jj - 1;
                }

                else if (Score == ScoreLeft + d)
                {
                    AlignA = Enum.GetName(typeof(musicNote.notePitch), (A[ii - 1])) + AlignA;
                    AlignB = "  " + AlignB;

                    ii = ii - 1;

                }
            }

            while (ii > 0)
            {
                AlignA = Enum.GetName(typeof(musicNote.notePitch), (A[ii - 1])) + AlignA;
                AlignB = "  " + AlignB;

                ii = ii - 1;
            }

            while (jj > 0)
            {
                AlignA = "  " + AlignA;
                AlignB = Enum.GetName(typeof(musicNote.notePitch), (B[jj - 1])) + AlignB;

                jj = jj - 1;
            }

            string[] returnArray = new string[2];

            returnArray[0] = AlignA;
            returnArray[1] = AlignB;

            return returnArray;
        }

        private Complex[] fft(Complex[] x, int L, Complex[] twiddles)
        {
            int ii = 0;
            int kk = 0;
            int N = x.Length;

            Complex[] Y = new Complex[N];

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
                E = fft(even, L, twiddles);
                O = fft(odd, L, twiddles);

                for (kk = 0; kk < N; kk++)
                {
                    Y[kk] = E[(kk % (N / 2))] + O[(kk % (N / 2))] * twiddles[kk * (L / N)];
                }
            }

            return Y;
        }

        private musicNote[] readXML(string filename)
        {

            List<string> stepList = new List<string>(100);
            List<int> octaveList = new List<int>(100);
            List<int> durationList = new List<int>(100);
            List<int> alterList = new List<int>(100);
            int noteCount = 0;
            bool sharp;
            musicNote[] scoreArray;

            FileStream file = new FileStream(filename, FileMode.Open, FileAccess.Read);
            if (file == null)
            {
                System.Console.Write("Failed to Open File!");
            }

            XmlTextReader reader = new XmlTextReader(filename);

            bool finished = false;

            while (finished == false)
            {
                sharp = false;
                while ((!reader.Name.Equals("note") || reader.NodeType == XmlNodeType.EndElement) && !finished)
                {
                    reader.Read();
                    if (reader.ReadState == ReadState.EndOfFile)
                    {
                        finished = true;
                    }
                }

                reader.Read();
                reader.Read();
                if (reader.Name.Equals("rest"))
                {
                }
                else if (reader.Name.Equals("pitch"))
                {

                    while (!reader.Name.Equals("step"))
                    {
                        reader.Read();
                    }
                    reader.Read();
                    stepList.Add(reader.Value);
                    while (!reader.Name.Equals("octave"))
                    {
                        if (reader.Name.Equals("alter") && reader.NodeType == XmlNodeType.Element)
                        {
                            reader.Read();
                            alterList.Add(int.Parse(reader.Value));
                            sharp = true;
                        }
                        reader.Read();
                    }
                    reader.Read();
                    if (!sharp)
                    {
                        alterList.Add(0);
                    }
                    sharp = false;
                    octaveList.Add(int.Parse(reader.Value));
                    while (!reader.Name.Equals("duration"))
                    {
                        reader.Read();
                    }
                    reader.Read();
                    durationList.Add(int.Parse(reader.Value));
                    //System.Console.Out.Write("Note ~ Pitch: " + stepList[noteCount] + alterList[noteCount] + " Octave: " + octaveList[noteCount] + " Duration: " + durationList[noteCount] + "\n");
                    noteCount++;

                }

            }

            scoreArray = new musicNote[noteCount];

            double c0 = 16.351625;

            for (int nn = 0; nn < noteCount; nn++)
            {
                int step = (int)Enum.Parse(typeof(pitchConv), stepList[nn]);

                double freq = c0 * Math.Pow(2, octaveList[nn]) * (Math.Pow(2, ((double)step + (double)alterList[nn]) / 12));
                scoreArray[nn] = new musicNote(freq, (double)durationList[nn] * 60 * waveIn.SampleRate / (4 * bpm));

            }

            return scoreArray;
        }
    }
}
