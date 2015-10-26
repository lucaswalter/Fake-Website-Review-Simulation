using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebsiteReviewSimulation
{
    class Program
    {
        public static readonly int EXIT = 4;
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

            int userInput = 0;

            // Simulation Menu
            do
            {
                userInput = DisplayMenu();

                switch (userInput)
                {
                    case 1:
                    {
                        ProcessRealReviewData();
                        GenerateSameLengthReviews();
                        GenerateDifferentLengthReviews();
                        break;
                    }
                    case 2:
                    {
                        break;
                    }
                    case 3:
                    {
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
        }

        /** Helper Methods **/
        static public int DisplayMenu()
        {
            Console.WriteLine("Fake Website Review Simulation");
            Console.WriteLine();
            Console.WriteLine("1. Generate/Regenerate Random Dataset");
            Console.WriteLine("2. Run Simulation With Dynamic Programming Algorithm");
            Console.WriteLine("3. Run Simulation With Greedy Algorithm");
            Console.WriteLine("4. Exit");

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

            int[] P = new int[100];

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

            // TODO: Remove For Final Version
            Console.WriteLine(LSym);
            return LSym.Length / 2;
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

    /** Helper CLass **/
    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
        public void Add(T1 item, T2 item2)
        {
            Add(new Tuple<T1, T2>(item, item2));
        }
    }
}
