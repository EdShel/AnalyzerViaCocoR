using System;

string fileName = args.Length > 1 ? args[1] : "input.txt";

Scanner scanner = new Scanner(fileName);
Parser parser = new Parser(scanner);
parser.Parse();

Console.WriteLine(parser.errors.count + " errors detected");
