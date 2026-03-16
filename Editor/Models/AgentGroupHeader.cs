namespace Editor.Models
{
    public class AgentGroupHeader
    {
        public int GroupIndex { get; }
        public string SystemName { get; }
        public string Label => string.IsNullOrEmpty(SystemName)
            ? $"Group {GroupIndex}"
            : $"Group {GroupIndex} — {SystemName}";
        public AgentGroupHeader(int groupIndex, string systemName = "")
        {
            GroupIndex = groupIndex;
            SystemName = systemName;
        }
    }
}
