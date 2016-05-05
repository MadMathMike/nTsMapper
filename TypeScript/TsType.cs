namespace nTsMapper.TypeScript
{
	public abstract class TsType
	{
		public abstract string TsTypeName { get; }

		public virtual string TsTypeReferenceName
		{
			get { return TsTypeName; }
		}
	}
}