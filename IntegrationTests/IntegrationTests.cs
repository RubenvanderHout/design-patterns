using IO;
using IO.DTO;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Data;
using System.Fabric.Testability.Scenario;
using System.Globalization;
using System.IO;
using Validation;
using Validation.ValidationRules;
using Xunit.Abstractions;
using static IO.FileLoader;
using static Validation.ValidationRules.ValidationRuleBuilder;

namespace IntegrationTests
{
    public class IntegrationTests
    {
        private readonly string _testDataPath = "Test_fsm";
        private readonly ITestOutputHelper _output;
       
        public IntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // Utils
        // -----------------------------------------------------------------------------------------------------------------------------
        public static FsmRuleParser LoadFile(String path, IRuleComponent rootRules)
        {
            ILoaderFactory loaderFactory = new FileLoaderFactory();
            var fileLoader = loaderFactory.CreateLoader();
            var raw = fileLoader.Load(path);
            var parser = new FsmFileParser();
            var dto = parser.Parse(raw);
            var ruleparser = new FsmRuleParser(rootRules, dto);

            return ruleparser;
        }

        private static void NoErrors(ValidationResult result)
        {
            Assert.True(result.IsValid && result.Errors.Count == 0);
        }
        private void ExpectErrors(ValidationResult result)
        {
            Assert.True(!result.IsValid && result.Errors.Count > 0);
            _output.WriteLine($"Validation errors: {string.Join(", ", result.Errors)}");
        }
        // -----------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void ExampleUserAccount_NoErrors()
        {
            var path = Path.Combine(_testDataPath, "example_user_account.fsm");
            var rules = ValidationRuleBuilder.BuildCompleteComposite();

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();
            
            NoErrors(result);
        }

        [Fact]
        public void Example_Lamp_NoErrors()
        {
            var path = Path.Combine(_testDataPath, "example_lamp.fsm");
            var rules = ValidationRuleBuilder.BuildCompleteComposite();

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            NoErrors(result);
        }

        [Fact]
        public void Deterministic_NoErrors()
        {
            var path = Path.Combine(_testDataPath, "valid_deterministic.fsm");
            var rules = new RuleLeaf(new NonDeterministicTransitionsRule());

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            NoErrors(result);
        }

        [Fact]
        public void Deterministic_Triggers_Bad_ExpectErrors()
        {
            var path = Path.Combine(_testDataPath, "invalid_deterministic1.fsm");
            var rules = new RuleLeaf(new NonDeterministicTransitionsRule());

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Deterministic_Guards_Bad_ExpectErrors()
        {
            var path = Path.Combine(_testDataPath, "invalid_deterministic2.fsm");
            var rules = new RuleLeaf(new NonDeterministicTransitionsRule());


            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Deterministic_Unreachable_Because_Automatic_Transaction_ExpectErrors()
        {
            var path = Path.Combine(_testDataPath, "invalid_deterministic3.fsm");
            var rules = new RuleLeaf(new NonDeterministicTransitionsRule());


            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Invalid_Compound_State_ExcpectErrors()
        {
            var path = Path.Combine(_testDataPath, "invalid_compound.fsm");
            var rules = BuildCompleteComposite();

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void Valid_Compound_State_NoErrors()
        {
            var path = Path.Combine(_testDataPath, "valid_compound.fsm");
            var rules = BuildCompleteComposite();

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            NoErrors(result);
        }

        [Fact]
        public void Unreachable_State_ExpectErrors()
        {
            var path = Path.Combine(_testDataPath, "invalid_unreachable.fsm");
            var rules = BuildCompleteComposite();

            var ruleParser = LoadFile(path, rules);
            var result = ruleParser.Validate();

            ExpectErrors(result);
        }
    }
}