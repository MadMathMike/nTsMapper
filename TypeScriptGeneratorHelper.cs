using System.Collections.Generic;
using System.Linq;
using nTsMapper.TypeScript;

namespace nTsMapper
{
	partial class TypeScriptGenerator
	{
		private readonly bool mDebugMode;
		private readonly List<TsTypeWithProperties> mTypesToGenerate;
		private readonly List<IGrouping<string, TsEnum>> mEnumsToGenerate;

		public TypeScriptGenerator(
			List<TsTypeWithProperties> typesToGenerate,
			List<IGrouping<string, TsEnum>> enumsToGenerate,
			bool debugMode)
		{
			mTypesToGenerate = typesToGenerate;
			mEnumsToGenerate = enumsToGenerate.OrderBy(t => t.Key).ToList();
			mDebugMode = debugMode;
		}
	}
}