
using System.Globalization;
using CsvHelper.Configuration;
using CsvHelper;
using System.Data;
using System.Text.RegularExpressions;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VoiSlateParser.Utilities;

public class AlePaser : IDisposable
{
    CsvConfiguration config = new (CultureInfo.InvariantCulture)
    {
        Delimiter = "\t",
        WhiteSpaceChars = new[] { ' ' },
        NewLine = "\n"
    };
    MemoryStream stream = new();
    StreamWriter tempwriter;
    StreamReader tempreader;
    public string Heading = string.Empty;
    public string Column = string.Empty;
    public string DataTrunk = string.Empty;
    public DataTable dt = new();

    public AlePaser(string path)
    {
        this.tempwriter = new(stream);
        this.tempreader = new(stream);
        ParseAle(path);
        GenerateDataTable();
    }

    private void ParseAle(string path)
    {
        string line = string.Empty;
        using (StreamReader reader = new(path))
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("Heading"))
                {
                    stream.Position = 0;
                    line = reader.ReadLine()!;
                }
                if (line.Contains("Column"))
                {
                    ConsolidateStream();
                    Heading = tempreader.ReadToEnd();
                    Console.WriteLine("Heading end");
                    Console.WriteLine(Heading);
                    stream.Position = 0;
                    line = reader.ReadLine()!;
                }
                if (line.Contains("Data"))
                {
                    ConsolidateStream();
                    Column = tempreader.ReadToEnd();
                    Console.WriteLine("Column end");
                    Console.WriteLine(Column);
                    stream.Position = 0;
                    line = reader.ReadLine()!;
                }
                tempwriter.WriteLine(line);
            }
            ConsolidateStream();
            DataTrunk = tempreader.ReadToEnd();
            Console.WriteLine("Data end");
            stream.Position = 0;
        }
    }

    private void ConsolidateStream()
    {
        tempwriter.Flush();
        stream.Position = 0;
    }

    void CsvHelperTest()
    {

        // foreach (DataRow row in dt.Rows)
        // {
        //     foreach (DataColumn col in dt.Columns)
        //     {
        //         Console.Write(row[col] + " ");
        //     }
        //     Console.WriteLine();
        // }
        // write data to ale

        dt.Columns.Add("Fuck", typeof(string));
        DataRow[] rows = dt.Select("Name LIKE '%wav%'");

        foreach (DataRow row in rows)
        {
            // Set value of NewColumn column to "K"
            row["Fuck"] = "K";
        }
        var query = from row in dt.AsEnumerable()
                    where row.Field<string>("Circled") == "N"
                    select row;

        // var col = dt.Columns;
    }

    private void GenerateDataTable()
    {
        // clean the empty lines
        string lines = RemoveEmptyLines(Column + DataTrunk);
        tempwriter.WriteLine(lines);
        ConsolidateStream();

        // start to edit the ale
        using (CsvReader r = new(tempreader, config))
        using (var dr = new CsvDataReader(r))
        {
            dt.Load(dr);
        }
    }

    public void WriteAle(string path)
    {
        using (StreamWriter aleWriter = new(path))
        {
            aleWriter.WriteLine("Heading");
            aleWriter.WriteLine(Heading);
            aleWriter.WriteLine("Column");
            var headerList = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            var header = string.Join(config.Delimiter, headerList);
            aleWriter.WriteLine(header);
            aleWriter.WriteLine("Data");
            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                aleWriter.WriteLine(string.Join(config.Delimiter, fields));
            }
        }
    }

    private string RemoveEmptyLines(string toProcess)
    {
        return Regex.Replace(toProcess, "^\\s*$\\n|\\r", string.Empty, RegexOptions.Multiline);
    }

    public void Dispose()
    {
        tempreader.Close();
        tempwriter.Close();
        stream.Close();
        Console.WriteLine("done");
    }
}