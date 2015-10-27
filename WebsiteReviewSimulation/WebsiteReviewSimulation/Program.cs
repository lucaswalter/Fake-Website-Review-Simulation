using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WebsiteReviewSimulation
{
    class Program
    {
        public static readonly string resultPath = Environment.CurrentDirectory + "/Results/";
        public static readonly double SCORE_THRESHOLD = 0.7;
        public static readonly int EXIT = 8;
        public static readonly int[] STRING_LENGTH = { 10, 100, 150, 200, 250, 300, 350, 400, 450, 500 };
        public static readonly TupleList<int, int> RANGES = new TupleList<int,int>
        {
            { 10, 100 },
            { 101, 200 },
            { 201, 300 },
            { 301, 400 },
            { 401, 500 },
            { 501, 600 },
            { 601, 700 },
            { 701, 800 },
            { 801, 900 },
            { 901, 1000 }
        };

        static void Main(string[] args)
        {
            // Initial Directory Setup
            var appPath = Environment.CurrentDirectory;
            System.IO.Directory.CreateDirectory(appPath + "/Random/Same/");
            System.IO.Directory.CreateDirectory(appPath + "/Random/Different/");
            System.IO.Directory.CreateDirectory(appPath + "/Processed/");
            System.IO.Directory.CreateDirectory(resultPath);

            int userInput = 0;

            // Simulation Menu
            do
            {
                userInput = DisplayMenu();

                switch (userInput)
                {
                    case 1: // Generate Datasets
                    {
                        ProcessRealReviewData();
                        GenerateSameLengthReviews();
                        GenerateDifferentLengthReviews();
                        File.Delete(appPath + "/Processed/" + "MASTER" + ".txt");
                        break;
                    }
                    case 2:
                    {
                        RunDymanicSimulationRandomSetSameLength();
                        break;
                    }
                    case 3: 
                    {
                        RunDynamicSimulationRandomSetDifferentLengths();
                        break;
                    }
                    case 4:
                    {
                        RunDynamicSimulationRealWorldDataset();
                        break;
                    }
                    case 5:
                    {
                        RunGreedySimulationRandomSetSameLength();
                        break;
                    }
                    case 6:
                    {
                        RunGreedySimulationRandomSetDifferentLengths();
                        break;
                    }
                    case 7:
                    {
                        RunGreedySimulationRealWorldDataset();
                        break;
                    }
                }

            } while (userInput != EXIT);
        }

        /** Module 1: Random Dataset Generation **/

        // 1A: 100 Reviews Of The Same Lengths For 10 Lengths
        static void GenerateSameLengthReviews()
        {
            var random = new Random();

            // Generate 10 Files For Review Length
            for (int i = 0; i < 10; i++)
            {
                var appPath = Environment.CurrentDirectory + "/Random/Same/" + STRING_LENGTH[i] + ".txt";
                List<string> randomReviews = new List<string>();

                // Generate 100 Strings
                for (int j = 0; j < 100; j++)
                    randomReviews.Add(RandomString(random, STRING_LENGTH[i]));
                
                // Output To Text File For Appropriate Length
                System.IO.File.WriteAllLines(appPath, randomReviews);
            }
        }

        // 1B: 100 Reviews Of Different Lengths For 10 Ranges
        static void GenerateDifferentLengthReviews()
        {
            var random = new Random();

            for (int i = 0; i < 10; i++)
            {
                // File Names 0-9 Will Correspond To The Ranges Of Generated Reviews
                var appPath = Environment.CurrentDirectory + "/Random/Different/" + i + ".txt";
                List<string> randomReviews = new List<string>();

                // Selects And Generates String Of Length Inside The Length Range
                for (int j = 0; j < 100; j++)
                {                 
                    var lengthInRange = random.Next(RANGES[i].Item1, RANGES[i].Item2);
                    randomReviews.Add(RandomString(random, lengthInRange));
                }
                    
                // Output To Text File For Appropriate Length
                System.IO.File.WriteAllLines(appPath, randomReviews);
            }
        }

        /** Module 2: Process Real World Sample Dataset **/

        static void ProcessRealReviewData()
        {
            var inputPath = Environment.CurrentDirectory + "/Input/";
            var dataPath = Environment.CurrentDirectory + "/Processed/MASTER" + ".txt";
            var reviews = new List<string>();

            foreach (var file in Directory.EnumerateFiles(inputPath, "*.txt"))
            {
                var review = File.ReadAllText(file).Trim();
                review = review + "|";
                reviews.Add(review);
            }

            // Sort And Output All Reviews To Single File
            var result = reviews.OrderBy(x => x.Length).ToList();
            System.IO.File.WriteAllLines(dataPath, result);

            CreateBuckets();
        }

        static void CreateBuckets()
        {
            var masterPath = Environment.CurrentDirectory + "/Processed/MASTER" + ".txt";
            var file = new StreamReader(masterPath);
            var data = file.ReadToEnd();

            string[] parsedReviews = data.Split('|');

            // Divide Into 10 Buckets And Create New Text Files
            for (int i = 0; i < 10; i++)
            {
                var dataPath = Environment.CurrentDirectory + "/Processed/" + i + ".txt";
                var reviews = new List<string>();

                var lowerBound = 0;
                var upperBound = 999;

                for (int j = lowerBound; j < upperBound; j++)
                    reviews.Add(parsedReviews[j].Trim() + "|");

                lowerBound += 1000;
                upperBound += 1000;

                // Output Bucket To Individual File
                System.IO.File.WriteAllLines(dataPath, reviews);
            }

            file.Close();
        }

        /** Module 3A: Run Dynamic Programming Simulation **/

        public static void RunDymanicSimulationRandomSetSameLength()
        {
            var appPathSame = Environment.CurrentDirectory + "/Random/Same/";
            var counter = 0;

            // Random Dataset-Same
            foreach (var file in Directory.EnumerateFiles(appPathSame, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                var results = new List<Result>();
                var rawReviews = File.ReadAllLines(file);
                double scoreTotal = 0;
                double avgCounter = 0;

                foreach (var review in rawReviews)
                    reviews.Add(review.Trim());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nComputing Results For Random Dataset Of Length: {0} ", STRING_LENGTH[counter]);
                Console.ForegroundColor = ConsoleColor.White;

                var timer = new Stopwatch();
                timer.Start();

                foreach (var X in reviews)
                {
                    foreach (var Y in reviews)
                    {
                        if (X == Y)
                            continue;

                        var lcsLength = CountLCSDynamic(X.ToCharArray(), Y.ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Y.Length;
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && X != Y)
                            results.Add(new Result { Left = X, Right = Y, Score = similarityScore });        
                    }              
                }

                timer.Stop();

                OutputResults(results, "DynamicRandomSameLength", counter);

                Console.WriteLine("\nBucket-Set {0} Results:", counter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);
                
                timer.Reset();
                results.Clear();
                counter++;
            }

            Console.WriteLine("\n---------------------------- END OF RANDOM DATASET ----------------------------\n");
        }

        public static void RunDynamicSimulationRandomSetDifferentLengths()
        {
            var appPathDifferent = Environment.CurrentDirectory + "/Random/Different/";
            var counter = 0;

            // Random Dataset-Different
            foreach (var file in Directory.EnumerateFiles(appPathDifferent, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                var results = new List<Result>();
                var rawReviews = File.ReadAllLines(file);
                double scoreTotal = 0;
                double avgCounter = 0;

                foreach (var review in rawReviews)
                    reviews.Add(review.Trim());

                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("\nComputing Results For Random Dataset In The Range: {0} - {1} ", RANGES[counter].Item1, RANGES[counter].Item2);
                Console.ForegroundColor = ConsoleColor.White;

                var timer = new Stopwatch();
                timer.Start();

                foreach (var X in reviews)
                {                  
                    foreach (var Y in reviews)
                    {
                        if (X == Y)
                            continue;

                        var lcsLength = CountLCSDynamic(X.ToCharArray(), Y.ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(X.Length, Y.Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && X != Y)
                            results.Add(new Result { Left = X, Right = Y, Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRandomDifferentLengths",  counter);

                Console.WriteLine("\nBucket-Set {0} Results:", counter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                results.Clear();
                counter++;
            }

            Console.WriteLine("\n---------------------------- END OF RANDOM DATASET ----------------------------\n");
        }

        public static void RunDynamicSimulationRealWorldDataset()
        {
            var appPathProcessed = Environment.CurrentDirectory + "/Processed/";

            var counter = 0;

            // Real World Dataset
            foreach (var file in Directory.EnumerateFiles(appPathProcessed, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                List<Result> results = new List<Result>();

                // Pre-Process Data Split By Pipe
                var rawFile = new StreamReader(file);
                var data = rawFile.ReadToEnd();
                rawFile.Close();

                var subCounter = 1;

                double scoreTotal = 0;
                double avgCounter = 0;

                string[] parsedReviews = data.Split('|');
                for (int i = 0; i < parsedReviews.Length; i++)
                    parsedReviews[i] = parsedReviews[i].Trim('\r', '\n');

                reviews = parsedReviews.ToList();

                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Bucket: {0}", counter);
                Console.ForegroundColor = ConsoleColor.White;

                /********** Hacky Sub-Bucket Solution **********/

                // Start Timer Diagnostics
                var timer = new Stopwatch();
                timer.Start();

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                // 0 - 100
                for (int i = 0; i < 100; i++)
                {            
                    for (int j = 0; j < 100; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });                    
                    }             
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();


                // 101 - 200
                for (int i = 101; i < 200; i++)
                {
                    for (int j = 101; j < 200; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 201 - 300
                for (int i = 201; i < 300; i++)
                {
                    for (int j = 201; j < 300; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;


                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 301 - 400
                for (int i = 301; i < 400; i++)
                {
                    for (int j = 301; j < 400; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;


                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 401 - 500
                for (int i = 401; i < 500; i++)
                {
                    for (int j = 401; j < 500; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 501 - 600
                for (int i = 501; i < 600; i++)
                {
                    for (int j = 501; j < 600; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 601 - 700
                for (int i = 601; i < 700; i++)
                {
                    for (int j = 601; j < 700; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 701 - 800
                for (int i = 701; i < 800; i++)
                {
                    for (int j = 701; j < 800; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 801 - 900
                for (int i = 801; i < 900; i++)
                {
                    for (int j = 801; j < 900; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                timer.Stop();

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 901 - 999
                for (int i = 901; i < 999; i++)
                {
                    for (int j = 901; j < 999; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSDynamic(reviews[i].ToCharArray(), reviews[j].ToCharArray());
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "DynamicRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                // Increment To Next Bucket
                counter++;
            }

            Console.WriteLine("\n-------------------------- END OF REAL-WORLD DATASET --------------------------\n");
        }

        /** Module 3B: Run Greedy Simulation **/

        public static void RunGreedySimulationRandomSetSameLength()
        {
            var appPathSame = Environment.CurrentDirectory + "/Random/Same/";
            var counter = 0;

            // Random Dataset-Same
            foreach (var file in Directory.EnumerateFiles(appPathSame, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                var results = new List<Result>();
                var rawReviews = File.ReadAllLines(file);
                double scoreTotal = 0;
                double avgCounter = 0;

                foreach (var review in rawReviews)
                    reviews.Add(review.Trim());

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nComputing Results For Random Dataset Of Length: {0} ", STRING_LENGTH[counter]);
                Console.ForegroundColor = ConsoleColor.White;

                var timer = new Stopwatch();
                timer.Start();

                foreach (var X in reviews)
                {
                    foreach (var Y in reviews)
                    {
                        if (X == Y)
                            continue;

                        var lcsLength = CountLCSGreedy(X, Y);
                        var similarityScore = Convert.ToDouble(lcsLength) / Y.Length;
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && X != Y)
                            results.Add(new Result { Left = X, Right = Y, Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRandomSameLength", counter);

                Console.WriteLine("\nBucket-Set {0} Results:", counter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                results.Clear();
                counter++;
            }

            Console.WriteLine("\n---------------------------- END OF RANDOM DATASET ----------------------------\n");
        }

        public static void RunGreedySimulationRandomSetDifferentLengths()
        {
            var appPathDifferent = Environment.CurrentDirectory + "/Random/Different/";
            var counter = 0;

            // Random Dataset-Different
            foreach (var file in Directory.EnumerateFiles(appPathDifferent, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                var results = new List<Result>();
                var rawReviews = File.ReadAllLines(file);
                double scoreTotal = 0;
                double avgCounter = 0;

                foreach (var review in rawReviews)
                    reviews.Add(review.Trim());

                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("\nComputing Results For Random Dataset In The Range: {0} - {1} ", RANGES[counter].Item1, RANGES[counter].Item2);
                Console.ForegroundColor = ConsoleColor.White;

                var timer = new Stopwatch();
                timer.Start();

                foreach (var X in reviews)
                {
                    foreach (var Y in reviews)
                    {
                        if (X == Y)
                            continue;

                        var lcsLength = CountLCSGreedy(X, Y);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(X.Length, Y.Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && X != Y)
                            results.Add(new Result { Left = X, Right = Y, Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRandomDifferentLengths", counter);

                Console.WriteLine("\nBucket-Set {0} Results:", counter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                results.Clear();
                counter++;
            }

            Console.WriteLine("\n---------------------------- END OF RANDOM DATASET ----------------------------\n");
        }

        public static void RunGreedySimulationRealWorldDataset()
        {
            var appPathProcessed = Environment.CurrentDirectory + "/Processed/";

            var counter = 0;

            // Real World Dataset
            foreach (var file in Directory.EnumerateFiles(appPathProcessed, "*.txt"))
            {
                // List Of Reviews In File
                var reviews = new List<string>();
                List<Result> results = new List<Result>();

                // Pre-Process Data Split By Pipe
                var rawFile = new StreamReader(file);
                var data = rawFile.ReadToEnd();
                rawFile.Close();

                var subCounter = 1;

                double scoreTotal = 0;
                double avgCounter = 0;

                string[] parsedReviews = data.Split('|');
                for (int i = 0; i < parsedReviews.Length; i++)
                    parsedReviews[i] = parsedReviews[i].Trim('\r', '\n');

                reviews = parsedReviews.ToList();

                Console.ForegroundColor = ConsoleColor.Green;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Bucket: {0}", counter);
                Console.ForegroundColor = ConsoleColor.White;

                /********** Hacky Sub-Bucket Solution **********/

                // Start Timer Diagnostics
                var timer = new Stopwatch();
                timer.Start();

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                // 0 - 100
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();


                // 101 - 200
                for (int i = 101; i < 200; i++)
                {
                    for (int j = 101; j < 200; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 201 - 300
                for (int i = 201; i < 300; i++)
                {
                    for (int j = 201; j < 300; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;


                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 301 - 400
                for (int i = 301; i < 400; i++)
                {
                    for (int j = 301; j < 400; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;


                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 401 - 500
                for (int i = 401; i < 500; i++)
                {
                    for (int j = 401; j < 500; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 501 - 600
                for (int i = 501; i < 600; i++)
                {
                    for (int j = 501; j < 600; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 601 - 700
                for (int i = 601; i < 700; i++)
                {
                    for (int j = 601; j < 700; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 701 - 800
                for (int i = 701; i < 800; i++)
                {
                    for (int j = 701; j < 800; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 801 - 900
                for (int i = 801; i < 900; i++)
                {
                    for (int j = 801; j < 900; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                timer.Stop();

                Console.ForegroundColor = ConsoleColor.Yellow;
                System.Console.WriteLine("\nComputing Results For Real World Dataset Sub-Bucket: {0}-{1}", counter, subCounter);
                Console.ForegroundColor = ConsoleColor.White;

                timer.Start();

                // 901 - 999
                for (int i = 901; i < 999; i++)
                {
                    for (int j = 901; j < 999; j++)
                    {
                        if (reviews[i] == reviews[j])
                            continue;

                        var lcsLength = CountLCSGreedy(reviews[i], reviews[j]);
                        var similarityScore = Convert.ToDouble(lcsLength) / Min(reviews[i].Length, reviews[j].Length);
                        scoreTotal += similarityScore;
                        avgCounter++;

                        if (similarityScore > SCORE_THRESHOLD && reviews[i] != reviews[j])
                            results.Add(new Result { Left = reviews[i], Right = reviews[j], Score = similarityScore });
                    }
                }

                timer.Stop();

                OutputResults(results, "GreedyRealWorld", counter, subCounter);
                results.Clear();

                Console.WriteLine("\nBucket-Set {0}-{1} Results:", counter, subCounter);
                Console.WriteLine("Execution Time: {0} ms", timer.ElapsedMilliseconds);
                Console.WriteLine("Average Similarity Score: {0}", scoreTotal / avgCounter);

                timer.Reset();
                subCounter++;

                // Increment To Next Bucket
                counter++;
            }

            Console.WriteLine("\n-------------------------- END OF REAL-WORLD DATASET --------------------------\n");
        }

        /** Helper Methods **/

        static public int DisplayMenu()
        {
            
            Console.WriteLine("\nFake Website Review Simulation");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n1. Generate/Regenerate Random Dataset");
            Console.ForegroundColor = ConsoleColor.White;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n2. Dynamic Programming: Random-Same-Length Dataset");
            Console.WriteLine("3. Dynamic Programming: Random-Different-Length Dataset");
            Console.WriteLine("4. Dynamic Programming: Real-World Dataset");
            Console.ForegroundColor = ConsoleColor.White;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n5. Greedy: Random-Same-Length Dataset");
            Console.WriteLine("6. Greedy: Random-Different-Length Dataset");
            Console.WriteLine("7. Greedy: Real-World Dataset");
            Console.ForegroundColor = ConsoleColor.White;

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("\n8. Exit");
            Console.ForegroundColor = ConsoleColor.White;

            var result = Console.ReadLine();
            return Convert.ToInt32(result);
        }

        public static string RandomString(Random random, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";      
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static int CountLCSDynamic(char[] str1, char[] str2)
        {
            int[,] arr = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str2.Length; i++)
            {
                arr[0, i] = 0;
            }
            for (int i = 0; i <= str1.Length; i++)
            {
                arr[i, 0] = 0;
            }

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    if (str1[i - 1] == str2[j - 1])
                    {
                        arr[i, j] = arr[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        arr[i, j] = Max(arr[i - 1, j], arr[i, j - 1]);
                    }
                }
            }

            return arr[str1.Length, str2.Length];
        }

        public static int CountLCSGreedy(string X, string Y)
        {
            X = Match(X, Y);

            if (X.Length == 0)
                return 0;

            int[] Processed_Y = new int[Y.Length];
            int y_length = Y.Length;

            for (int j = 0; j < y_length; j++)
                Processed_Y[j] = 0;

            int m = X.Length;
            int n = Y.Length;

            string L = string.Empty;
            string LSym = string.Empty;

            int R = 0;
            int i = 1;

            int[] P = new int[1000];

            P[i] = Position(X, Y, i, Processed_Y, R);
            i = 1;

            while (i <= m)
            {
                if (i != m)
                    P[i + 1] = Position(X, Y, (i + 1), Processed_Y, R);

                if (P[i + 1] == 0)
                {
                    if (P[i] > R)
                    {
                        L = L + " " + P[i].ToString();
                        LSym = LSym + " " + X.ElementAt(i - 1);
                    }

                    break;
                }

                if (P[i + 1] < R || P[i] < R)
                    R = 0;

                if (P[i] > P[i + 1])
                {
                    if (R == 0 && i > 1)
                    {
                        L = L + " " + P[i].ToString();
                        LSym = LSym + " " + X.ElementAt(i - 1);
                        Processed_Y[P[i] - 1] = 1;
                        R = P[i];
                        i = i + 1;

                        if (R == Y.Length || i > X.Length)
                            break;

                        P[i] = Position(X, Y, i, Processed_Y, R);
                    }
                    else
                    {
                        L = L + " " + P[i + 1].ToString();
                        LSym = LSym + " " + X.ElementAt(i + 1 - 1);
                        Processed_Y[P[i + 1] - 1] = 1;
                        R = P[i + 1];
                        i = (i + 1) + 1;

                        if (R == Y.Length || i > X.Length)
                            break;

                        P[i] = Position(X, Y, i, Processed_Y, R);
                    }
                }
                else
                {
                    if (R == 0 && i > 1)
                    {
                        L = L + " " + P[i + 1].ToString();
                        LSym = LSym + " " + X.ElementAt(i + 1 - 1);
                        Processed_Y[P[i + 1] - 1] = 1;
                        R = P[i + 1];
                        i = (i + 1) + 1;

                        if (R == Y.Length || i > X.Length)
                            break;

                        P[i] = Position(X, Y, i, Processed_Y, R);
                    }
                    else
                    {
                        L = L + " " + P[i].ToString();
                        LSym = LSym + " " + X.ElementAt(i - 1);
                        Processed_Y[P[i] - 1] = 1;
                        R = P[i];
                        i = i + 1;

                        if (R == Y.Length || i > X.Length)
                            break;

                        P[i] = Position(X, Y, i, Processed_Y, R);
                    }
                }
            }

            return LSym.Length / 2;
        }

        public static void OutputResults(List<Result> results, string fileName, float counter)
        {
            var path = resultPath + fileName + "[" + counter + "]" + ".txt";
            var newResults = new List<string>();

            foreach (var result in results)
            {
                var newResult = "[" + result.Score + "] " + result.Left + " <---> " + result.Right;
                newResults.Add(newResult);
            }

            File.WriteAllLines(path, newResults);
        }

        public static void OutputResults(List<Result> results, string fileName, float counter, float subCounter)
        {
            var path = resultPath + fileName + "[" + counter + "-" + subCounter + "]" + ".txt";
            var newResults = new List<string>();

            foreach (var result in results)
            {
                var newResult = "[" + result.Score + "] " + result.Left + " <---> " + result.Right;
                newResults.Add(newResult);
            }

            File.WriteAllLines(path, newResults);
        }

        /** Dynamic Programming Helper Methods **/
        private static int Max(int int1, int int2)
        {
            return int1 > int2 ? int1 : int2;
        }

        private static int Min(int int1, int int2)
        {
            return int1 < int2 ? int1 : int2;
        }  

        /** Greedy LCS Helper Methods **/
        private static string Match(string X, string Y)
        {
            var result = string.Empty;

            for (var i = 0; i < X.Length; i++)
            {
                for (int j = 0; j < Y.Length; j++)
                {
                    if (X.ElementAt(i) == Y.ElementAt(j))
                    {
                        result = result + X.ElementAt(i);
                        break;
                    }
                }
            }

            return result;
        }

        private static int Position(string X, string Y, int i, int[] Processed_Y, int R)
        {
            int n = Y.Length;
            int k = 0;
            int kr = 0;

            i = i - 1;

            for (k = 0; k < n; k++)
            {
                if ((X.ElementAt(i)) == Y.ElementAt(k) && Processed_Y[k] == 0)
                {
                    kr = k + 1;
                    break;
                }
            }

            for (k = R; k < n; k++)
            {
                if ((X.ElementAt(i) == Y.ElementAt(k)) && Processed_Y[k] == 0)
                {
                    kr = k + 1;
                    break;
                }
            }

            return kr;
        }
    }

    /** Helper CLasses **/
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }

    public class Result
    {
        public string Left { get; set; }
        public string Right { get; set; }
        public double Score { get; set; }
        public long Time { get; set; }
    }
}