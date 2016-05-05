namespace nTsMapper.TypeScript
{
	public class TsMappedType : TsType
	{
		private readonly string mTsTypeName;

		public string AssignmentTemplate { get; private set; }

		public TsMappedType(string tsTypeName, string assignmentTemplate)
		{
			mTsTypeName = tsTypeName;
			AssignmentTemplate = assignmentTemplate;
		}

		public override string TsTypeName
		{
			get { return mTsTypeName; }
		}
	}
}