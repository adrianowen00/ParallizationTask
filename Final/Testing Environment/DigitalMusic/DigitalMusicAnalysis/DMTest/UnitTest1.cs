using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DMTest
{
    [TestClass]
    public class UnitTest1
    {
        DMParallel.DMParallel mainParallel;
        DigitalMusicAnalysis.MainWindow mainSeq;

        [TestInitialize]
        public void TestInit()
        {
            Console.WriteLine("TestClass Init");
            mainParallel = new DMParallel.DMParallel();
            mainSeq = new DigitalMusicAnalysis.MainWindow();
        }

        [TestMethod]
        public void TestFileLoad()
        {
            mainParallel.filename = "E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\cinder.wav";
            mainSeq.filename = "E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\cinder.wav";
            mainParallel.loadWave(mainParallel.filename);
            mainSeq.loadWave(mainSeq.filename);
            Console.WriteLine("wavefile");
            Console.WriteLine(mainParallel.waveIn.Subchunk2Size);
            Console.WriteLine(mainParallel.waveIn.NumChannels);
            Console.WriteLine(mainParallel.waveIn.BitsPerSample);
            CollectionAssert.AreEqual(mainParallel.waveIn.data, mainSeq.waveIn.data);
        }

        [TestMethod]
        public void TimeFileLoadSeq()
        {
            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();

            mainSeq.loadWave(mainSeq.filename);

            stopwatch_init.Stop();
            TimeSpan tsInit = stopwatch_init.Elapsed;
            double total = tsInit.TotalMilliseconds;

            for (int i = 0; i < 100; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                mainSeq.loadWave(mainSeq.filename);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("SeqLoadWave");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void TimeFileLoadParallel()
        {
            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();

            mainParallel.loadWave(mainSeq.filename);

            stopwatch_init.Stop();
            TimeSpan tsInit = stopwatch_init.Elapsed;
            double total = tsInit.TotalMilliseconds;

            for (int i = 0; i < 100; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                mainParallel.loadWave(mainSeq.filename);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("ParLoadWave");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void CheckFreqDomainPixelArray() {
            mainParallel.freqDomain();
            mainSeq.freqDomain();
            Console.WriteLine("Values: ");
            Console.WriteLine(mainSeq.pixelArray[0]);
            Console.WriteLine(mainParallel.pixelArray[0]);
            CollectionAssert.AreEqual(mainParallel.pixelArray, mainSeq.pixelArray);
        }

        //timefreq.cs
        [TestMethod]
        public void CheckTimeFreqConstruct()
        {
            DMParallel.timefreq stftRepParallel = new DMParallel.timefreq(mainParallel.waveIn.wave, 2048);
            DigitalMusicAnalysis.timefreq stftRepSeq = new DigitalMusicAnalysis.timefreq(mainSeq.waveIn.wave, 2048);

            for (int x = 0; x < stftRepSeq.timeFreqData.GetLength(0); x++)
            {
                CollectionAssert.AreEqual(stftRepParallel.timeFreqData[x], stftRepSeq.timeFreqData[x]);
            }

        }

        [TestMethod]
        public void OnlyTimeFreqPar()
        {
            DMParallel.timefreq stftRepParallel = new DMParallel.timefreq(mainParallel.waveIn.wave, 2048);
            Complex[] compX = stftRepParallel.compXG;

            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();
            stftRepParallel.stft(compX, 2048);

            stopwatch_init.Stop();
            TimeSpan tsInit = stopwatch_init.Elapsed;
            double total = tsInit.TotalMilliseconds;
            
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("new stopwatch");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                stftRepParallel.stft(compX, 2048);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("ADADAS");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void OnlyTimeFreqConstructorPar()
        {
            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();

            DMParallel.timefreq stftRepParallel = new DMParallel.timefreq(mainParallel.waveIn.wave, 2048);
            stopwatch_init.Stop();
            TimeSpan tsInit = stopwatch_init.Elapsed;
            double total = tsInit.TotalMilliseconds;

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine("new stopwatch");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                stftRepParallel = new DMParallel.timefreq(mainParallel.waveIn.wave, 2048);

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("Total construction time");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void OnlyTimeFreqSeq()
        {
            //mainSeq.loadWave("E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\cinder.wav");
            mainSeq.loadWave("E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\Jupiter.wav");
            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();
            mainSeq.freqDomain();
            stopwatch_init.Stop();
            TimeSpan tsInit = stopwatch_init.Elapsed;
            double total = tsInit.TotalMilliseconds;

            for (int i = 0; i < 15; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                mainSeq.freqDomain();

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;

                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine(total);
        }

        [TestMethod]
        public void CheckNoteGraphConstruct()
        {
            float fs = mainSeq.waveIn.SampleRate;
            float divisor = fs / mainSeq.stftRep.wSamp;
            DigitalMusicAnalysis.noteGraph HIGHEST = new DigitalMusicAnalysis.noteGraph(1760, divisor);

            mainParallel.freqDomain();
            float fsPar = mainParallel.waveIn.SampleRate;
            float divisorPar = fs / mainParallel.stftRep.wSamp;

            DMParallel.noteGraph HIGHESTPar = new DMParallel.noteGraph(1760, divisor);
        }

        [TestMethod]
        public void CheckLoadHisto()
        {
            mainParallel.loadHistogram();
        }


        [TestMethod]
        public void CheckOnSetDetectSeq()
        {
            mainSeq.loadWave("E:\\University\\2018\\CAB401\\Assignmnet\\DigitalMusic\\MusicTestFiles\\Jupiter.wav");
            mainSeq.freqDomain();
            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();
            mainSeq.onsetDetection();
            stopwatch_init.Stop();
            TimeSpan ts_init = stopwatch_init.Elapsed;
            double total = ts_init.TotalMilliseconds;

            for (int i = 0; i < 15; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainSeq.onsetDetection();

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("onsetdetection seq runtime: ");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void CheckOnSetDetectParallel()
        {
            mainParallel.freqDomain();

            Stopwatch stopwatch_init = new Stopwatch();
            stopwatch_init.Start();
            mainParallel.onsetDetection();
            stopwatch_init.Stop();
            TimeSpan ts_init = stopwatch_init.Elapsed;
            double total = ts_init.TotalMilliseconds;

            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("new stopwatch");
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                mainParallel.onsetDetection();

                stopwatch.Stop();
                TimeSpan ts = stopwatch.Elapsed;
                total = (total + ts.TotalMilliseconds) / 2;
            }
            Console.WriteLine("onsetdetection parallel runtime: ");
            Console.WriteLine(total);
        }

        [TestMethod]
        public void CheckOnSetDetectEquality()
        {
            mainParallel.freqDomain();
            mainParallel.onsetDetection();
            mainSeq.onsetDetection();

            CollectionAssert.AreEqual(mainParallel.alignedStrings, mainSeq.alignedStrings);
        }
    }
}
