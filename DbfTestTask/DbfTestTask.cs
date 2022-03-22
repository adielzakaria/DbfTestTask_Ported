using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbfTests
{
    [TestClass]
    public class DbfTestTask
    {
        [TestMethod]
        public void TestTask()
        {
            //using uri with path.combine ? to benefit from cross-platform
            const string RootDir = @".\Data";
            // probably .DBF is better since we can use cross-platform feature of .Net 6 and it can still run on windows 
            const string RelevantFileName = "128.DBF";
            /*testing the existence of the root directory before trying to read its content 
              probably not useful now since I throw an exception when not found but anyway */
            var headers=GetHeaders(RootDir,RelevantFileName);
            var reader = new DbfReader();
            var values = headers.Select(x=> reader.ReadValues(x))
                                .SelectMany(x=> x)
                                .GroupBy(x=>x.Timestamp)
                                .OrderBy(x=>x.Key);

            var outputs = new List<OutputRow>();
            OutputRow.Headers=headers.ToList();
            foreach (var item in values)
            {
                var outputRow = new OutputRow();
                outputRow.Timestamp=item.Key; 
                outputRow.Values=item.Select(x=>x.Value as double?).ToList();
                outputs.Add(outputRow);  
            }
            // the following asserts should pass
           
            Assert.AreEqual(25790, outputs.Count);
            // <-- these asserts are failing on my machine for some reason 
             
            Assert.AreEqual(27, OutputRow.Headers.Count);
            Assert.AreEqual(27, outputs[0].Values.Count);
            Assert.AreEqual(27, outputs[11110].Values.Count);
            Assert.AreEqual(27, outputs[25789].Values.Count);
            
            //  these asserts are failing on my machine for some reason -->
            Assert.AreEqual(633036852000000000, outputs.Min(o => o.Timestamp).Ticks);
            Assert.AreEqual(634756887000000000, outputs.Max(o => o.Timestamp).Ticks);
            Assert.AreEqual(633036852000000000, outputs[0].Timestamp.Ticks);
            Assert.AreEqual(634756887000000000, outputs.Last().Timestamp.Ticks);

            // write into file that we can compare results later on (you don't have to do something)
            string content = "Time\t" + string.Join("\t", OutputRow.Headers) + Environment.NewLine +
                          string.Join(Environment.NewLine, outputs.Select(o => o.AsTextLine()));
            File.WriteAllText(@".\output.txt", content);
        }
        public string[] GetHeaders(string rootDir,string RelevantFileName)
        {
            if(Directory.Exists(rootDir))
            {
                return Directory.GetFiles(rootDir,RelevantFileName,SearchOption.AllDirectories);
            }
            else
            {
                throw new ArgumentException("Invalid directory");
            }
        }
    }
}
