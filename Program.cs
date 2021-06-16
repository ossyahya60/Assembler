using System;
using System.IO;

namespace Assembler
{
    class Program
    {
        private static string WriteFilePath = "GeneratedCode.txt";
        private static string ReadFilePath = "AssemblyCode.asm";

        static void Main(string[] args)
        {
            using (StreamReader SR = new StreamReader(ReadFilePath, false))
            {
                using (StreamWriter SW = new StreamWriter(WriteFilePath, false))
                {
                    int CurrentMemLocation = 0;

                    while (!SR.EndOfStream)
                    {
                        string Line = SR.ReadLine();
                        int IndexOfComment = Line.IndexOf('#');
                        while (IndexOfComment != -1)
                        {
                            Line = Line.Remove(IndexOfComment);
                            IndexOfComment = Line.IndexOf('#');
                        }

                        Line = Line.Trim();
                        if (string.IsNullOrEmpty(Line))
                            continue;

                        //Line here is clean of comments
                        if (Line[0] == '.') //This is a .ORG
                        {
                            int MemLocation = int.Parse(Line.Substring(Line.LastIndexOf(' ') + 1));

                            for (int i = CurrentMemLocation; i < MemLocation; i++)
                                SW.WriteLine("\"0000000000000000\",");

                            CurrentMemLocation = MemLocation;
                        }
                        else if(Line.IndexOf(',') != -1) //binary operator
                        {
                            string ArgType = Line.Substring(0, Line.IndexOf(' '));
                            Line = Line.Replace(" ", "");
                            Line = Line.Remove(0, ArgType.Length);

                            string[] Arguments = Line.Split(',');

                            switch(ArgType.ToLower())
                            {
                                case "mov":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00011110" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[1].Remove(0, 1))) + "\",");
                                    break;
                                case "add":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00011111" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[1].Remove(0, 1))) + "\",");
                                    break;
                                case "sub":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00011000" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[1].Remove(0, 1))) + "\",");
                                    break;
                                case "and":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00011001" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[1].Remove(0, 1))) + "\",");
                                    break;
                                case "or":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00011010" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[1].Remove(0, 1))) + "\",");
                                    break;
                                case "shl":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    SW.WriteLine("\"10010110" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "1111" + "\",\n" + "\"" + "00000" + Ensure5Bit(Convert.ToString(int.Parse(Arguments[1]), 2)) + "000000" + "\",");
                                    break;
                                case "shr":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    SW.WriteLine("\"10010111" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "1111" + "\",\n" + "\"" + "00000" + Ensure5Bit(Convert.ToString(int.Parse(Arguments[1]), 2)) + "000000" + "\",");
                                    break;
                                case "iadd":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    SW.WriteLine("\"11100111" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "\",\n" + "\"" + Ensure16Bit(Convert.ToString(int.Parse(Arguments[1], System.Globalization.NumberStyles.HexNumber), 2).ToString()) + "\",");
                                    break;
                                case "ldm":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    SW.WriteLine("\"11100100" + "1111" + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "\",\n" + "\"" + Ensure16Bit(Convert.ToString(int.Parse(Arguments[1]), 2).ToString()) + "\",");
                                    break;
                                case "ldd":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    string[] OffsetAndRsrc = Arguments[1].Split('(');
                                    SW.WriteLine("\"11101101" + IntToBinary(int.Parse(OffsetAndRsrc[1].Remove(OffsetAndRsrc[1].Length - 1, 1).Remove(0, 1))) + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "\",\n" + "\"" + Ensure16Bit(Convert.ToString(int.Parse(OffsetAndRsrc[0]), 2).ToString()) + "\",");
                                    break;
                                case "std":
                                    CurrentMemLocation = CurrentMemLocation + 2;
                                    string[] OffsetAndRsrc2 = Arguments[1].Split('(');
                                    SW.WriteLine("\"11101110" + IntToBinary(int.Parse(OffsetAndRsrc2[1].Remove(OffsetAndRsrc2[1].Length - 1, 1).Remove(0, 1))) + IntToBinary(int.Parse(Arguments[0].Remove(0, 1))) + "\",\n" + "\"" + Ensure16Bit(Convert.ToString(int.Parse(OffsetAndRsrc2[0]), 2).ToString()) + "\",");
                                    break;
                            }
                        }
                        else if(Line.IndexOf(' ') == -1) //Unary operator which doesn't take arguments
                        {
                            switch(Line.ToLower())
                            {
                                case "ret":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"0011011011111111" + "\",");
                                    break;
                                case "nop":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"0000000011111111" + "\",");
                                    break;
                                case "setc":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"0000000111111111" + "\",");
                                    break;
                                case "clrc":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"0000001011111111" + "\",");
                                    break;
                            }
                        }
                        else //Unary operator
                        {
                            string ArgType = Line.Substring(0, Line.IndexOf(' '));
                            int RegNum = int.Parse((Line.Substring(Line.LastIndexOf(' ') + 1)).Remove(0, 1));
                            
                            switch(ArgType.ToLower())
                            {
                                case "not":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"000100011111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "inc":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"000100101111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "dec":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"000100111111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "out":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00010100" + IntToBinary(RegNum) + "1111" + "\",");
                                    break;
                                case "in":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"010101011111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "push":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001000101111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "pop":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001000111111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "jz":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001110011111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "jn":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001110101111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "jc":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001110111111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "jmp":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"001111001111" + IntToBinary(RegNum) + "\",");
                                    break;
                                case "call":
                                    CurrentMemLocation++;
                                    SW.WriteLine("\"00110101" + IntToBinary(RegNum) + IntToBinary(RegNum) + "\",");
                                    break;
                            }
                        }
                    }
                }
            }
        }
            
        private static string IntToBinary(int N)
        {
            string Output = "";

            for (int i = 0; i < 4; i++)
            {
                Output = (N % 2).ToString() + Output;
                N = N / 2;
            }

            return Output;
        }

        private static string Ensure16Bit(string H)
        {
            string Output = H;
            for (int i = H.Length; i < 16; i++)
                Output = "0" + Output;

            return Output;
        }

        private static string Ensure5Bit(string H)
        {
            string Output = H;
            for (int i = H.Length; i < 5; i++)
                Output = "0" + Output;

            return Output;
        }
    }
}
