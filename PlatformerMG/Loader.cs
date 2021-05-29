using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace PlatformerMG
{
    class Loader
    {
        // Stream we are going to use for file reading
        private Stream mFileStream = null;

        // Constructor
        public Loader(Stream stream)
        {
            mFileStream = stream;
        }

        public string ReadTextFileComplete()
        {
            //The "correct" (most efficient) way to concatenate strings in .NET 
            //is to make use of a StringBuilder object instead of using the 
            //addition operator (e.g. string s = s + "foo";)
            StringBuilder result = new StringBuilder();

            // Use a try-catch block to make sure any exceptions are handled
            // E.g. File not found errors etc.
            try
            {
                // The using statement here creates a single StreamReader object
                // and properly disposes of it after it has finished being used
                using (StreamReader reader = new StreamReader(mFileStream))
                {
                    // Add the text of the whole file (ReadToEnd()) to the
                    // resulting string
                    result.Append(reader.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                // If we've caught an exception, output an error message
                // describing the error
                Console.WriteLine("ERROR: File could not be read!");
                Console.WriteLine("Exception Message: " + e.Message);
            }

            // Return the resulting string
            return result.ToString();
        }

        public List<string> ReadLinesFromTextFile()
        {
            // We don't have to worry about string building here, as we are only
            // reading a line at a time
            string line = "";

            // Initialise a list to contain the results
            List<string> lines = new List<string>();

            try
            {
                using (StreamReader reader = new StreamReader(mFileStream))
                {
                    // Now we'll keep reading until the end of the file and
                    // store each line in a collection
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Add the line to the collection
                        lines.Add(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: File could not be read!");
                Console.WriteLine("Exception Message: " + e.Message);
            }

            return lines;
        }

        public void ReadXML(string filename)
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    GameInfo.Instance = (GameInfo)new XmlSerializer(typeof(GameInfo)).Deserialize(reader.BaseStream);
                }
            }
            catch (Exception e)
            {
                // If we've caught an exception, output an error message
                // describing the error
                Console.WriteLine("ERROR: XML File could not be deserialized!");
                Console.WriteLine("Exception Message: " + e.Message);
            }
        }



    }
}
