using System;

namespace lp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string correlationId = null;
            string inputFile = null;
            string outputFile = null;

            switch (args.Length)
            {
                case 1:
                    correlationId = args[0];
                    inputFile = @"d:\Data\Logs\pimrunner\pimrunner.log";
                    outputFile = @$"d:\Data\Logs\pimrunner\{correlationId}.log";
                    break;

                case 3:
                    correlationId = args[0];
                    inputFile = args[1];
                    outputFile = args[2];
                    break;

                default:
                    PrintUsage();
                    break;
            }

            PimLoggerParser parser = new PimLoggerParser(correlationId, inputFile, outputFile);

            parser.Parse();
        }

        static void PrintUsage()
        {
            Console.WriteLine($"Usage: {System.AppDomain.CurrentDomain.FriendlyName} <correlationId> <inputFile> <outputFile>");
            Environment.Exit(-1);
        }
    }
}
