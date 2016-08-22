using System;
using System.Collections.Generic;

namespace nTsMapper
{
	public class Program
	{
		public static int Main(string[] args)
		{
			var commandLineArgs = new CommandLineArgs(args);
			// Display the help message before we validate the parameters, so the user can always see the help message
			CheckAndDisplayCommandLineArgumentHelp(commandLineArgs.Help);

			if (!commandLineArgs.AreArgsValid)
			{
				Console.Out.WriteLine("The following errors were encountered with the command line arguments:\n\r");
				foreach (var errorMessage in commandLineArgs.ErrorMessages)
				{
					Console.Out.WriteLine("\t{0}", errorMessage);
				}
				return 1;
			}

			// For debugging purposes
			//Console.Out.WriteLine(commandLineArgs.GetForDisplay());

			var customMappings = new List<TypeMapping>
			{
				new TypeMapping
				{
					MatchesType = tr => tr.FullName == "NodaTime.LocalDate" || tr.IsGenericType && tr.GetGenericTypeDefinition() == typeof(Nullable<>) && tr.GenericTypeArguments[0].FullName == "NodaTime.LocalDate",
					DestinationType = "LocalDate",
					DestinationAssignmentTemplate = "LocalDate.fromJSON({0})"
				},
				new TypeMapping
				{
					MatchesType = tr => tr.FullName == "NodaTime.Instant" || tr.IsGenericType && tr.GetGenericTypeDefinition() == typeof(Nullable<>) && tr.GenericTypeArguments[0].FullName == "NodaTime.Instant",
					DestinationType = "Instant",
					//DestinationAssignmentTemplate = "Instant.fromJSON({0}, tenantClockService)"
					DestinationAssignmentTemplate = "TenantClockService.instantFromJSON({0})"
				},
				new TypeMapping
				{
					MatchesType = tr => typeof(DateTime) == tr,
					DestinationType = "Date",
					DestinationAssignmentTemplate = "{0}"
				},
				new TypeMapping
				{
					MatchesType = tr => typeof(DateTime?) == tr,
					DestinationType = "Date",
					DestinationAssignmentTemplate = "{0}"
				},
				new TypeMapping
				{
					MatchesType = tr => typeof(Object) == tr,
					DestinationType = "any",
					DestinationAssignmentTemplate = "{0}"
				}
			};

			var assemblyInsepctor = new AssemblyProcessor(commandLineArgs, customMappings);
			assemblyInsepctor.ProcessAssembly();

			return 0;
		}

		private static void CheckAndDisplayCommandLineArgumentHelp(bool help)
		{
			if (help)
			{
				// print help
				Console.Out.WriteLine(
					@"

This application will inspect the specified .NET dll for WebAPI controller
methods to produce TypeScript code for all the command parameter and DTO
classes used.

Usage: 
[-assemblypath] [-outputfilename] [-help] [-debug]

  -help                Displays this help message.

  -debug               Produces helpful debug information as comments in the 
                       generated TypeScript code.

  -assemblypath        Specifies the full path and filename to the .NET library
                       to be inspected. Note: this parameter is REQUIRED and
                       must be wrapped in quotation marks if the path contains
                       a space.

  -outputfilename      Specifies the full path and file name of the output
                       file where generated TypeScript code will be saved.  If
                       this parameter is omitted, generated TypeScript code
                       will be sent to standard output.  Note: this parameter
                       must be wrapped in quotation marks if the path contains
                       a space.
");
			}
		}
	}
}
