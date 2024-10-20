namespace PSSharedVariables
{
    public sealed class VariableValue
    {
        public VariableValue(string name, string value) => (Name, Value) = (name, value);
        public string Name { get; }
        public string Value { get; }
    }
}