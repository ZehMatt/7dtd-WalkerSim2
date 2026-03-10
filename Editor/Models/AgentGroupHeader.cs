namespace Editor.Models
{
    public class AgentGroupHeader
    {
        public int GroupIndex { get; }
        public string Label => $"Group {GroupIndex}";
        public AgentGroupHeader(int groupIndex) => GroupIndex = groupIndex;
    }
}
