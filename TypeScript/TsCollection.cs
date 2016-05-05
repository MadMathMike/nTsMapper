namespace nTsMapper.TypeScript
{
	public class TsCollection : TsType
	{
		public TsType TsItemType { get; private set; }

		public TsCollection(TsType tsItemType)
		{
			TsItemType = tsItemType;
		}

		public override string TsTypeName
		{
			get { return TsItemType.TsTypeName + "[]"; }
		}

		public override string TsTypeReferenceName
		{
			get { return TsItemType.TsTypeReferenceName + "[]"; }
		}
	}
}