namespace Validation.ValidationRules
{
    public interface IRuleComponent
    {
        string Name { get; }
        bool AppliesTo(IFsmElement element);
        IEnumerable<string> Validate(IFsmElement element);
    }

    public sealed class RuleLeaf : IRuleComponent
    {
        private readonly IValidationRule _rule;
        public RuleLeaf(IValidationRule rule) { _rule = rule; }
        public string Name => _rule.RuleName;
        public bool AppliesTo(IFsmElement e) => _rule.ShouldValidate(e);
        public IEnumerable<string> Validate(IFsmElement e) => e switch
        {
            State s => _rule.Validate(s),
            Transition t => _rule.Validate(t),
            Trigger tr => _rule.Validate(tr),
            Action a => _rule.Validate(a),
            _ => Enumerable.Empty<string>()
        };
    }

    public sealed class RuleGroup : IRuleComponent
    {
        private readonly List<IRuleComponent> _children = new();
        public string Name { get; }
        public RuleGroup(string name) { Name = name; }
        public RuleGroup Add(IRuleComponent child) { _children.Add(child); return this; }

        public bool AppliesTo(IFsmElement e) => _children.Any(c => c.AppliesTo(e));

        public IEnumerable<string> Validate(IFsmElement e) =>
            _children.Where(c => c.AppliesTo(e)).SelectMany(c => c.Validate(e));
    }
}