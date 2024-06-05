using TokenizationHelperTools;
using LogProject;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ex6Assembly
{
    public sealed class Multiplication
    {
        public static void Muiltiple(int rows1, int columns1, Func<int, int, decimal> func1, int rows2, int columns2, Func<int, int, decimal> func2, string output)
        {
            using Logger logger = new(output, false);
            StringBuilder stringBuilder = new StringBuilder("Muiltiplication report:\n")
                .Append("Matrix1 size: ")
                .Append(rows1)
                .Append("x")
                .Append(columns1)
                .Append("\n")
                .Append("Matrix2 size: ")
                .Append(rows2)
                .Append("x")
                .Append(columns2)
                .Append("\n\nInput data:\nMatrix1:");
            logger.Log(stringBuilder.ToString());
            stringBuilder.Clear();
           
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (columns1 != rows2) throw new ArgumentException("Incorrect size of input matrix");


            for(int i =0; i<rows1; ++i)
            {
                for (int j = 0; j < columns1; ++j)
                {
                    stringBuilder.Append(func1(i, j)).Append("\t");
                }
                logger.Log(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            logger.Log("Load first matrix executed", true);

            logger.Log("\nMatrix2:");
            for (int i = 0; i < rows2; ++i)
            {
                for (int j = 0; j < columns2; ++j)
                {
                    stringBuilder.Append(func2(i, j)).Append("\t");
                }
                logger.Log(stringBuilder.ToString());
                stringBuilder.Clear();
            }
            logger.Log("Load second matrix executed", true);

            using Mutex mutex = new Mutex(); 
            logger.Log("\nResult:");
            for (int i=0; i<rows1; ++i)
            {
                for(int j=0; j<columns2; ++j)
                {
                    decimal result = 0;
                    for(int k=0; k< rows2; ++k)
                    {
                        try
                        {
                            mutex.WaitOne();
                            result += func1(i, k) * func2(k, j);
                            
                        }
                        finally
                        { 
                            mutex.ReleaseMutex(); 
                        }

                    }
                    stringBuilder.Append(result.ToString()).Append("\t");
                }
                logger.Log(stringBuilder.ToString());
                stringBuilder.Clear();
            }

            logger.Log("Method executed", true);
            stopwatch.Stop();
            stringBuilder
                .Append("\nExecution time: ")
                .Append(stopwatch.ElapsedMilliseconds)
                .Append("ms\n")
                .Append("Threads: ")
                .Append(rows1 * columns2 * columns1);
            logger.Log(stringBuilder.ToString());
        }

        static char[] separators = { '\t', '\n', '\r', ' ' };
        public static void Muiltiple(string pathToMatrix1, string pathToMatrix2, string output)
        {
            
            using Logger logger = new Logger(output); //в логгере disposable + using 
            //всегда переопределять disp вместе с дестр если у используемых объектов / полей есть disposable (те их необх возвращать)
            StringBuilder stringBuilder = new StringBuilder("\nMuiltiplication report:\nResult:");
            logger.Log(stringBuilder.ToString());
            stringBuilder.Clear();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            using TokenizationHelper token = new(pathToMatrix1, new HashSet<char>(separators));

            string? rowMatrix2;
          
            List<double> rowMatrix1 = new List<double>();
            bool isFirstIteration = true;
            //using разворачивается в try finally. В finally вызывается disp.
            int columns2 = 0;

            foreach (string str in token)
            {
                if (str != "\n")
                {
                     try 
                     {
                        rowMatrix1.Add(double.Parse(str));
                     }
                     catch (ArgumentException)
                     {
                        throw new ArgumentException("Incorrect input file format");
                     }
                     catch (OverflowException)
                     {
                          throw new ArgumentException("Incorrect input file format");
                     }
                }
                else
                {
                    Action<List<double>> action = (rowMatrix1) =>
                    {
                        StringBuilder sb = new();
                        int i = 0;
                        while (i < columns2 || isFirstIteration)
                        {

                            using var reader = new StreamReader(pathToMatrix2); //!!!!!!!!!!!!! using. 
                            //если локальный объект - using. если объект в котором есть поле с disposable
                            double val = 0;

                            foreach (var v in rowMatrix1)
                            {

                                rowMatrix2 = reader.ReadLine();
                                if (rowMatrix2 == null) throw new ArgumentException("Incorrect input file format");
                                try
                                {
                                    if (isFirstIteration)
                                    {
                                        columns2 = rowMatrix2.Split(separators).Length;
                                        isFirstIteration = false;
                                    }
                                    val += double.Parse(rowMatrix2.Split(separators)[i]) * v;

                                }
                                catch (ArgumentException)
                                {
                                    throw new ArgumentException("Incorrect input file format");
                                }
                                catch (OverflowException)
                                {
                                    throw new ArgumentException("Incorrect input file format");
                                }

                            }
                            ++i;
                            sb.Append(val).Append("\t");
                        }

                        logger.Log(sb.ToString());
                    };


                    action(rowMatrix1);
                    rowMatrix1.Clear();
                }
            }


            stopwatch.Stop();
            stringBuilder
                .Append("\nExecution time: ")
                .Append(stopwatch.ElapsedMilliseconds)
                .Append("ms\n");
            logger.Log(stringBuilder.ToString());
        }
    }
}
