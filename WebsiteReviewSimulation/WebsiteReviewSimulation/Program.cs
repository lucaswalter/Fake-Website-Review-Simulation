using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
