using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Fclp;

namespace XmlCommentMerge
{
    class Program
    {
        static void Main(string[] args)
        {
            // create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<Options>();

            // specify which property the value will be assigned too.
            p.Setup(arg => arg.DirName)
                .As('d', "directory") // define the short and long option name
                .WithDescription("The directory where the xml files are located")
                .SetDefault(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            p.Setup(arg => arg.FileName)
                .As('f', "filename")
                .WithDescription("Set the name for the generated merged xml file")
                .SetDefault("mergedXmlDocuments.xml");

            p.Setup(arg => arg.Files)
                .As('s', "specificfiles")
                .WithDescription("Enter specific filenames to merge");

            var result = p.Parse(args);

            if (result.HasErrors == false)
            {
                // use the instantiated ApplicationArguments object from the Object property on the parser.
                Run(p.Object);
            }


        }

        public class Options
        {
            public string DirName { get; set; }
            public string FileName { get; set; }
            public List<string> Files { get; set; }
        }

        public static void Run(Options options)
        {
            XDocument xdoc = MergeDir(options);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                NewLineOnAttributes = true,
                OmitXmlDeclaration = false
            };
            using (XmlWriter writer = XmlWriter.Create($"{options.DirName}\\{options.FileName}", settings))
            {
                xdoc.Save(writer);
            }
        }

        public static XDocument MergeDir(Options options)
        {
            XDocument xdoc = XDocument.Parse("<doc></doc>");
            xdoc.Root.Add(new XElement("members"));
            DirectoryInfo directory = new DirectoryInfo(options.DirName);
            if (directory.Exists)
            {
                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Extension == ".xml")
                    {
                        // Don't merge the previously merged file
                        if(file.Name == options.FileName) continue;

                        // If file is not part of specified file names
                        if(options.Files != null && options.Files.Any() && !options.Files.Contains(file.Name)) continue;

                        // Add members
                        xdoc.Root.Descendants("members").FirstOrDefault()?.Add(XDocument.Load(file.FullName).Descendants("members").FirstOrDefault()?.Nodes());
                    }
                }
            }

            return xdoc;
        }
    }
}
