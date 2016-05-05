using System;
using System.Collections.Generic;

namespace nTsMapper.TypeScript
{
	public class TsEnum : TsType
	{
		private readonly string mTsTypeName;

		public List<KeyValuePair<string, long>> Members { get; private set; }
		public string ModuleName { get; private set; }
		

		public TsEnum(string moduleName, string tsTypeName)
		{
			mTsTypeName = tsTypeName;
			ModuleName = moduleName;
			Members = new List<KeyValuePair<string, long>>();
		}

		public override string TsTypeName
		{
			get { return mTsTypeName; }
		}

		public override string TsTypeReferenceName
		{
			get { return string.Format("{0}.{1}",ModuleName, TsTypeName); }
		}
	}

	class EnumComparer : IEqualityComparer<TsEnum>
	{
		// Products are equal if their names and product numbers are equal.
		public bool Equals(TsEnum x, TsEnum y)
		{
			//Check whether the compared objects reference the same data.
			if (Object.ReferenceEquals(x, y)) return true;

			//Check whether any of the compared objects is null.
			if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
				return false;

			//Check whether the products' properties are equal.
			return x.TsTypeReferenceName == y.TsTypeReferenceName;
		}

		// If Equals() returns true for a pair of objects 
		// then GetHashCode() must return the same value for these objects.

		public int GetHashCode(TsEnum tsEnum)
		{
			//Check whether the object is null
			if (Object.ReferenceEquals(tsEnum, null)) return 0;

			//Get hash code for the Name field if it is not null.
			int hashProductName = tsEnum.TsTypeReferenceName == null ? 0 : tsEnum.TsTypeReferenceName.GetHashCode();

			//Calculate the hash code for the product.
			return hashProductName;
		}
	}
}