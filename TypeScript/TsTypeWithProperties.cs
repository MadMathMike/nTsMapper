using System.Collections.Generic;

namespace nTsMapper.TypeScript
{
	public class TsTypeWithProperties : TsType
	{
		private readonly string mTsTypeName;

		public List<TsProperty> Properties { get; set; }
		public string ModuleName { get; private set; }
		public TsType BaseTsType { get; private set; }
		public int InheritanceHierarchyLevel { get; private set; }

		public TsTypeWithProperties(string moduleName, string tsTypeName, TsType baseTsType)
		{
			mTsTypeName = tsTypeName;
			ModuleName = moduleName;
			BaseTsType = baseTsType;
			Properties = new List<TsProperty>();

			var type = baseTsType as TsTypeWithProperties;
			if (type == null)
			{
				// this class has no base class so it's level should be 0
				InheritanceHierarchyLevel = 0;
			}
			else
			{
				// the InheritanceHierarchy needs to be greater than this class' base type InheritanceHierarchy
				InheritanceHierarchyLevel = type.InheritanceHierarchyLevel + 1;
			}
		}

		public override string TsTypeName
		{
			get { return mTsTypeName; }
		}

		public override string TsTypeReferenceName
		{
			get { return string.Format("{0}.{1}", ModuleName, TsTypeName); }
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", mTsTypeName, InheritanceHierarchyLevel);
		}
	}
}