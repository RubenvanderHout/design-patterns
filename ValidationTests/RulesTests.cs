using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation;
using Validation.ValidationRules;

namespace ValidationTests
{
    public class RulesTests
    {
        [Fact]
        public void CanBuildValidFsm()
        {   
            // Arrange
            var rules = new IValidationRule[]
            {
                new IdentifierRule()
            };
            var parser = new FsmRuleParser(rules);

            // Act
            parser.AddState("S1", "_", "Start", StateType.INITIAL);
            parser.AddState("S2", "S1", "Middle", StateType.SIMPLE);
            parser.AddTrigger("T1", "User presses button");
            parser.AddTransition("TR1", "S1", "S2", "T1", "x > 0");
           
            var fsm = parser.Build();

            // Assert
            Assert.Equal(2, fsm.States.Count);
            Assert.Single(fsm.Triggers);
            Assert.Single(fsm.Transitions);
        }

        [Fact]
        public void InvalidIdentifier_ShouldThrow()
        {
            var rules = new IValidationRule[] { new IdentifierRule() };
            var parser = new FsmRuleParser(rules);

            Assert.Throws<ArgumentException>(() =>
                parser.AddState("1234567890", "_", "Bad", StateType.SIMPLE));
        }

      
    }
}
