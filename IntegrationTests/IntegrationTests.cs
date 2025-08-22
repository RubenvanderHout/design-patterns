using IO;
using IO.DTO;
using System.Globalization;
using System.IO;
using Validation;
using Validation.ValidationRules;
using static IO.FileLoader;

namespace IntegrationTests
{
    public class IntegrationTests
    {
    // Utils
    // -----------------------------------------------------------------------------------------------------------------------------
        public FsmRuleParser LoadFile(String path, List<IValidationRule> rules)
        {
            ILoaderFactory loaderFactory = new FileLoaderFactory();
            var fileLoader = loaderFactory.CreateLoader();
            var raw = fileLoader.Load(path);
            var parser = new FsmFileParser();
            var dto = parser.Parse(raw);
            var ruleparser = new FsmRuleParser(rules, dto);

            return ruleparser;
        }

        public static void NoErrors(ValidationResult result)
        {
            Assert.True(result.IsValid && result.Errors.Count == 0);
        }
        public static void ExpectErrors(ValidationResult result)
        {
            Assert.True(!result.IsValid && result.Errors.Count > 0);
        }
    // -----------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void Deterministic_NoErrors()
        {
            string path = Path.Combine("Test_fsm", "valid_deterministic.fsm");
            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var ruleParser = LoadFile(path, rules);

            var result = ruleParser.Validate();

            NoErrors(result);
        }

        [Fact]
        public void Deterministic_Triggers_ExpectErrors()
        {
            string path = Path.Combine("Test_fsm", "invalid_deterministic1.fsm");
           
            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var ruleParser = LoadFile(path, rules);

            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Deterministic_Guards_ExpectErrors()
        {
            string path = Path.Combine("Test_fsm", "invalid_deterministic2.fsm");

            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var ruleParser = LoadFile(path, rules);

            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Deterministic_Automatic_With_Others_ExpectErrors()
        {
            string path = Path.Combine("Test_fsm", "invalid_deterministic3.fsm");

            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var ruleParser = LoadFile(path, rules);

            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

    }
}