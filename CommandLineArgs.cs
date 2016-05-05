using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nTsMapper
{
	public class CommandLineArgs
	{
		public bool Debug { get; private set; }
		public bool Help { get; private set; }
		public string AssemblyPath { get; private set; }
		public string OutputFilename { get; private set; }
		public bool OutputToFile { get; private set; }
		public bool AreArgsValid { get; private set; }
		public List<string> ErrorMessages { get; private set; }
		
		public CommandLineArgs(string[] args)
		{
			ErrorMessages = new List<string>();
			
			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				switch (arg.ToLower())
				{
					case "/help":
					case "-help":
						Help = true;
						break;

					case "/debug":
					case "-debug":
						Debug = true;
						break;

					case "/assemblypath":
					case "-assemblypath":
						AssemblyPath = args[i + 1];
						i += 1;
						break;

					case "/outputfilename":
					case "-outputfilename":
						OutputFilename = args[i + 1];
						if (!string.IsNullOrEmpty(OutputFilename))
						{
							OutputToFile = true;
						}
						i += 1;
						break;
				}
			}

			if (string.IsNullOrEmpty(AssemblyPath))
			{
				ErrorMessages.Add("Assembly path parameter is required.");
			}

			AreArgsValid = !ErrorMessages.Any();
		}

		public string AssemblyDirectory
		{
			get
			{
				var path = Path.GetDirectoryName(AssemblyPath);
				return path;
			}
		}

		public string GetForDisplay()
		{
			var toReturn =
				string.Format("Command line parameter values:\n\r\tDebug: {0}\n\r\tAssemblyPath: {1}\n\r\tOutputFilename: {2}",
					Debug, AssemblyPath, OutputFilename);

			return toReturn;
		}
	}
}