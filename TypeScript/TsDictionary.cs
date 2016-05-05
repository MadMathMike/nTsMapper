namespace nTsMapper.TypeScript
{
	public class TsDictionary : TsType
	{
		public TsType TsKeyItemType { get; private set; }
		public TsType TsValueItemType { get; private set; }

		public TsDictionary(TsType tsKeyItemType, TsType tsValueItemType)
		{
			TsKeyItemType = tsKeyItemType;
			TsValueItemType = tsValueItemType;
		}

		public override string TsTypeName
		{
			get { return "{ key:" + TsKeyItemType.TsTypeName + "; value:" + TsValueItemType.TsTypeName + " }[]"; }
		}

		public override string TsTypeReferenceName
		{
			get { return "{ key:" + TsKeyItemType.TsTypeReferenceName + "; value:" + TsValueItemType.TsTypeReferenceName + " }[]"; }
		}
	}
}