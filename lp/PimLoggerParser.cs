using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace lp
{
    public class PimLoggerParser
    {
        private string myCorrelationId;
        private string myInputFile;
        private string myOutputFile;
        private int myLogEntryCount = 0;
        private int myMatchingLogEntryCount = 0;
        private string myLastEntry = null;

        public PimLoggerParser(string correlationId, string inputFile, string outputFile)
        {
            this.myCorrelationId = correlationId;
            this.myInputFile = inputFile;
            this.myOutputFile = outputFile;

        }


        public void Parse()
        {
            Stopwatch sw = Stopwatch.StartNew();

            MemoryStream memStream = new MemoryStream();

            using (StreamWriter memWriter = new StreamWriter(memStream)) 


            using (StreamWriter fileWriter = new StreamWriter(this.myOutputFile))
            using (StreamReader fileReader = new StreamReader(this.myInputFile))
            {
                bool matching = false;
                List<string> buf = new List<string>();
                string line;
                while ((line = fileReader.ReadLine()) != null)
                {
                    // Look for timestamp - if found set matching == true marking beginning of log entry
                    if (IsLogTimestamp(line))
                    {
                        matching = true;
                    }

                    if (matching)
                    {
                        memWriter.WriteLine(line);

                        if (this.IsLogEntryEnd(line))
                        {
                            this.myLogEntryCount++;
                            matching = false;

                            if (this.IsMatchingLogEntry(line))
                            {
                                // This is a matching log entry - flush the memWriter, write to output
                                this.myMatchingLogEntryCount++;
                                memWriter.Flush();
                                memStream.WriteTo(fileWriter.BaseStream);
                                memStream.SetLength(0);
                            }
                            else
                            {
                                memWriter.Flush();
                                memStream.SetLength(0);
                            } 
                        }
                    }

                    this.myLastEntry = line;
                }
            }

            sw.Stop();

            Console.WriteLine($"Elapsed: {sw.Elapsed.TotalSeconds} seconds");
            Console.WriteLine($"Log entry count: {this.myLogEntryCount}");
            Console.WriteLine($"Matching log entry count: {this.myMatchingLogEntryCount}");
        }

        private bool IsMatchingLogEntry(string buf)
        {
            if ((this.myLastEntry != null) && (this.myLastEntry.StartsWith($"  CorrelationId: {this.myCorrelationId}")))
            {
                return (!string.IsNullOrEmpty(buf)) && (buf[0] == '}');
            }

            return false;
        }

        private bool IsLogEntryEnd(string buf)
        {
            if ((this.myLastEntry != null) && (this.myLastEntry.StartsWith($"  CorrelationId:")))
            {
                return (!string.IsNullOrEmpty(buf)) && (buf[0] == '}');
            }

            return false;
        }

        private static Regex reLogTimestamp = new Regex(@"^\d{4}", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private bool IsLogTimestamp(string buf)
        {
            if (this.IsLeadingDigit(buf))
            {
                return reLogTimestamp.IsMatch(buf);
            }

            return false;
        }


        private bool IsLeadingDigit(string buf)
        {
            return (!string.IsNullOrEmpty(buf)) && (char.IsDigit(buf[0]));
        }
    }
}
