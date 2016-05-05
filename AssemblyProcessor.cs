using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using nTsMapper.TypeScript;

namespace nTsMapper
{
	public class AssemblyProcessor
	{
		private readonly CommandLineArgs mCommandLineArgs;
		// Define other methods and classes here
		private List<TypeMapping> mAllMappings;
		private readonly Dictionary<Type, TsType> mTypeToTsTypeMap;

		/// <summary>
		/// Instantiates a new instance of the AssemblyProcessor with the parameters given.
		/// </summary>
		/// <param name="commandLineArgs">Use this to provide all the command line arguments needed to process and generate TypeScript.</param>
		/// <param name="customMappings">Provide custom type mappings.</param>
		public AssemblyProcessor(CommandLineArgs commandLineArgs, List<TypeMapping> customMappings)
		{
			mCommandLineArgs = commandLineArgs;
			mTypeToTsTypeMap = new Dictionary<Type, TsType>();

			FillAllMappings(customMappings);
		}

		public void ProcessAssembly()
		{
			var assembly = Assembly.LoadFrom(mCommandLineArgs.AssemblyPath);
			AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;

			var types = assembly.GetTypes();
			var controllers = types.Where(IsApiController);
			var commandParamtersAndDtoTypes = FindCommandParamtersAndDtoTypes(controllers);
			
			// We've gathered a distinct list of all the not-directly-mapped top-level types that are used for input and output of the WebAPI action methods.
			// Now with our defined mappings set up, we need to iterate the types we're going to generate proxies for and iterate their properties.
			//   We'll add the types of the properties to our working list according to the same logic as before: distinct and not-directly-mapped, skipping to the item type of enumerables
			var typesToInspect = new List<Type>(commandParamtersAndDtoTypes);
			
			foreach (var candidateType in typesToInspect)
			{
				MapToTsType(candidateType);
			}

			GenerateTypeScriptCode();
		}

		private void GenerateTypeScriptCode()
		{
			var typesToGenerate = mTypeToTsTypeMap
				.Values.OfType<TsTypeWithProperties>()
				.OrderBy(t => t.InheritanceHierarchyLevel)
				.ThenBy(t => t.ModuleName)
				.ToList();

			// We are doing a distinct on the lists of enums in each grouping
			// to prevent duplicate Enums from being generated.
			var enumsToGenerate = mTypeToTsTypeMap
				.Values.OfType<TsEnum>()
				.Distinct(new EnumComparer())
				.GroupBy(t => t.ModuleName)
				.ToList();

			var tsGenerator = new TypeScriptGenerator(typesToGenerate, enumsToGenerate, mCommandLineArgs.Debug);
			var typeScriptText = tsGenerator.TransformText();
			if (mCommandLineArgs.OutputToFile)
			{
				File.WriteAllText(mCommandLineArgs.OutputFilename, typeScriptText);
			}
			else
			{
				Console.Out.WriteLine(typeScriptText);
			}
		}

		private static HashSet<Type> FindCommandParamtersAndDtoTypes(IEnumerable<Type> controllers)
		{
			var commandParamtersAndDtoTypes = new HashSet<Type>();
			foreach (var controller in controllers)
			{
				foreach (
					var method in
						controller.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
							.Where(IsActionMethod))
				{
					foreach (var type in method.CustomAttributes
						.Where(cat => cat.AttributeType.FullName == "System.Web.Http.Description.ResponseTypeAttribute")
						.Select(cat => ((Type) cat.ConstructorArguments[0].Value))
						)
					{
						commandParamtersAndDtoTypes.Add(type);
					}
					foreach (var type in method.GetParameters()
						.Select(p => p.ParameterType)
						)
					{
						if (type.Name != "CancellationToken")
						{
							commandParamtersAndDtoTypes.Add(type);
						}
					}
				}
			}
			return commandParamtersAndDtoTypes;
		}

		/*
mapping:
 These type mappings need to correspond with the JSON serialization including any customization that might have been done.
 	
	custom mappings will be attempted first.
 
	void <-
		System.Void
	number <-
		System.Byte
		System.SByte
		System.Int16
		System.UInt16
		System.Int32
		System.UInt32
		System.Int64
		System.UInt64
		System.Double
		System.Single
		System.Decimal
		(and all of their nullable counterparts)
	string <-
		System.Char (and nullable)
		System.String
		System.DateTime (and nullable)
		System.DateTimeOffset (and nullable)
		System.TimeSpan (and nullable)
		System.Guid (and nullable)
		System.Uri
	boolean <-
		System.Boolean
		(and its nullable counterpart)
 	Enum (custom) <-
		System.Enum
	T[] <-
		IEnumerable<T> (excluding the special-case of string which is IEnumerable<char>)
		T[]
	Custom class <-
		Anything else
		 * 
	
*/

		private void FillAllMappings(List<TypeMapping> customMappings)
		{
			var mappings = new List<TypeMapping>
			{
				new TypeMapping
				{
					MatchesType = tr => (new[] {"System.String", "System.Char"}).Contains(tr.FullName),
					DestinationType = "string",
					DestinationAssignmentTemplate = "{0}"
				},
				new TypeMapping
				{
					MatchesType = tr => tr == typeof (char?),
					DestinationType = "string",
					DestinationAssignmentTemplate = "{0}"
				},
				new TypeMapping
				{
					MatchesType = tr => (new[]
					{
						"System.Byte", "System.SByte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32", "System.Int64",
						"System.UInt64", "System.Double", "System.Single", "System.Decimal"
					}).Contains(tr.FullName),
					DestinationType = "number",
					DestinationAssignmentTemplate = "{0}"
				},
				// System.Byte, System.SByte, System.Int16, System.UInt16, System.Int32, System.UInt32, System.Int64, System.UInt64, System.Double, System.Single, System.Decimal
				new TypeMapping { MatchesType = tr => (new[]
				{
					typeof (Byte?), typeof (Byte?), typeof (SByte?), typeof (Int16?), typeof (UInt16?), typeof (Int32?), typeof (UInt32?), typeof (Int64?),
					typeof (UInt64?), typeof (Double?), typeof (Single?), typeof (Decimal?)
				}).Contains(tr), DestinationType = "number", DestinationAssignmentTemplate = "{0}" },
				
				// Sytem.Void
				
				// System.DateTime (and nullable), System.DateTimeOffset (and nullable)
				// System.TimeSpan (and nullable)
				new TypeMapping { MatchesType = tr => (new[] {typeof (DateTime), typeof(DateTime?)}).Contains(tr), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },
				new TypeMapping { MatchesType = tr => (new[] {typeof (DateTimeOffset), typeof(DateTimeOffset?)}).Contains(tr), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },
				new TypeMapping { MatchesType = tr => (new[] {typeof (TimeSpan), typeof(TimeSpan?)}).Contains(tr), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },

				// System.Guid (and nullable)
				new TypeMapping { MatchesType = tr => tr == typeof (Guid), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },
				new TypeMapping { MatchesType = tr => tr == typeof (Guid?), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },
				
				// System.Uri
				new TypeMapping { MatchesType = tr => tr == typeof (Uri), DestinationType = "string", DestinationAssignmentTemplate = "{0}" },

				// 
				// System.Boolean (and nullable)
				new TypeMapping { MatchesType = tr => tr == typeof (Boolean), DestinationType = "boolean", DestinationAssignmentTemplate = "{0}" },
				new TypeMapping { MatchesType = tr => tr == typeof (Boolean?), DestinationType = "boolean", DestinationAssignmentTemplate = "{0}" },
			};
			
			mAllMappings = customMappings.Concat(mappings).ToList();
		}

		/// <summary>
		/// This method doesn't seem to be necessary anymore now that we are NOT loading the dlls in Reflection Only mode.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyName = new AssemblyName(args.Name);
			try
			{
				var found = Assembly.Load(args.Name);

				if (found != null)
					return found;
			}
			catch (FileNotFoundException)
			{
				// TODO: write errors to standard error
				//Console.Error.WriteLine();
			}
			catch (FileLoadException)
			{
				
			}

			var dllPath = Path.Combine(mCommandLineArgs.AssemblyPath, assemblyName.Name + ".dll");
			return Assembly.LoadFrom(dllPath);
		}

		private void MapToTsType(Type type)
		{
			if (mTypeToTsTypeMap.ContainsKey(type))
				return;
			var foundMapping = mAllMappings.FirstOrDefault(m => m.MatchesType.Invoke(type));
			if (foundMapping != null)
			{
				mTypeToTsTypeMap[type] = new TsMappedType(foundMapping.DestinationType, foundMapping.DestinationAssignmentTemplate);
			}
			else
			{
				Type itemType;
				Type keyItemType;
				Type valueItemType;

				if (IsDictionary(type, out keyItemType, out valueItemType))
				{
					MapToTsType(keyItemType);
					MapToTsType(valueItemType);
					mTypeToTsTypeMap[type] = new TsDictionary(mTypeToTsTypeMap[keyItemType], mTypeToTsTypeMap[valueItemType]);
				}
				else if (IsIEnumerableType(type, out itemType))
				{
					MapToTsType(itemType);
					mTypeToTsTypeMap[type] = new TsCollection(mTypeToTsTypeMap[itemType]);
				}
				else if (type.IsEnum)
				{
					var enumType = new TsEnum(type.Namespace, type.Name);
					var names = Enum.GetNames(type);
					var valuesArray = Enum.GetValues(type);
					var values = valuesArray.Cast<object>().Select(v => Convert.ToInt64(v)).ToArray();
					for (int i = 0; i < names.Length; i++)
					{
						enumType.Members.Add(new KeyValuePair<string, long>(names[i], values[i]));
					}
					mTypeToTsTypeMap[type] = enumType;
				}
				// nullable enum
				else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
							type.GetGenericArguments()[0].IsEnum)
				{
					var enumType = new TsEnum(type.GetGenericArguments()[0].Namespace, type.GetGenericArguments()[0].Name);
					var names = Enum.GetNames(type.GetGenericArguments()[0]);
					var valuesArray = Enum.GetValues(type.GetGenericArguments()[0]);
					var values = valuesArray.Cast<object>().Select(v => Convert.ToInt64(v)).ToArray();
					for (int i = 0; i < names.Length; i++)
					{
						enumType.Members.Add(new KeyValuePair<string, long>(names[i], values[i]));
					}
					mTypeToTsTypeMap[type] = enumType;
				}
				else
				{
					// Check if current TypeWithProperties has a base class that is not object
					TsType baseTsType = null;
					if (type.IsClass && type != typeof(Object) && type.BaseType != typeof(Object))
					{
						MapToTsType(type.BaseType);
						baseTsType = mTypeToTsTypeMap[type.BaseType];
					}

					var tsType = new TsTypeWithProperties(type.Namespace, type.Name, baseTsType);
					mTypeToTsTypeMap[type] = tsType;

					var propertyInfos = type
						.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
						.Where(p => p.GetMethod.IsPublic && !p.GetMethod.GetParameters().Any());

					// iterate properties.
					foreach (var prop in propertyInfos)
					{
						MapToTsType(prop.PropertyType);
						tsType.Properties.Add(new TsProperty { Name = prop.Name, TsType = mTypeToTsTypeMap[prop.PropertyType] });
					}
				}
			}
		}

		private bool IsDictionary(Type type, out Type keyItemType, out Type valueItemType)
		{
			keyItemType = null;
			valueItemType = null;
			if (type == null)
				return false;

			var isDictionary = type.IsGenericType &&
				(type.GetGenericTypeDefinition() == typeof(Dictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
			if (isDictionary)
			{
				keyItemType = type.GetGenericArguments()[0];
				valueItemType = type.GetGenericArguments()[1];
			}

			return isDictionary;
		}

		private static bool IsIEnumerableType(Type type, out Type itemType)
		{
			itemType = null;
			if (type == null)
				return false;
			var enumerableInterface = typeof(IEnumerable<>);
			if (type.IsGenericType && type.GetGenericTypeDefinition() == enumerableInterface)
			{
				itemType = type.GenericTypeArguments[0];
				return true;
			}
			var enumerable = type.GetInterface(enumerableInterface.FullName);
			if (enumerable != null)
			{
				itemType = enumerable.GenericTypeArguments[0]; // type; //TODO: fill in
				return true;
			}
			return false;
		}

		private static bool IsActionMethod(MethodInfo method)
		{
			if (!method.IsStatic && method.IsPublic && !method.IsSpecialName)
				return true;
			return false;
		}

		public bool IsApiController(Type type)
		{
			if (type.BaseType == null)
				return false;
			if (type.BaseType.Namespace == "System.Web.Http" && type.BaseType.Name == "ApiController")
				return true;
			
			return IsApiController(type.BaseType);
		} 
	}
}