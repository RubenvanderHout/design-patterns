using IO.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Validation;
using Validation.ValidationRules;
using ActionType = IO.DTO.ActionType;
using StateType = IO.DTO.StateType;

namespace ValidationTests
{
    // These test most likely could just be integration tests it's a lot easier to just load the files we already got.
    // Testing that seperately is not really that usefull. 

    public class RuleParserTests
    {
        public static List<StateDto> createState()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "initial", Parent = null, Name = "", Type = StateType.INITIAL },
                new() { Identifier = "created", Parent = null, Name = "Created", Type = StateType.COMPOUND },
                new() { Identifier = "inactive", Parent = "created", Name = "Inactive", Type = StateType.COMPOUND },
                new() { Identifier = "active", Parent = "created", Name = "Active", Type = StateType.COMPOUND },
                new() { Identifier = "unverified", Parent = "inactive", Name = "Unverified", Type = StateType.SIMPLE },
                new() { Identifier = "blocked", Parent = "inactive", Name = "Blocked", Type = StateType.SIMPLE },
                new() { Identifier = "deleted", Parent = "inactive", Name = "Deleted", Type = StateType.SIMPLE },
                new() { Identifier = "verified", Parent = "active", Name = "Verified", Type = StateType.SIMPLE },
                new() { Identifier = "logged_in", Parent = "active", Name = "Logged in", Type = StateType.SIMPLE },
                new() { Identifier = "final", Parent = null, Name = "Archived", Type = StateType.FINAL }
            };
            return states;
        }

        public static List<TriggerDto> createTriggers()
        {
            var triggers = new List<TriggerDto>
            {
                new() { Identifier = "create", Description = "create" },
                new() { Identifier = "timer_elapsed", Description = "timer elapsed" },
                new() { Identifier = "email_verification", Description = "email verification" },
                new() { Identifier = "blocked_by_admin", Description = "blocked by admin" },
                new() { Identifier = "unblocked_by_admin", Description = "unblocked by admin" },
                new() { Identifier = "deleted_by_admin", Description = "deleted by admin" },
                new() { Identifier = "forget_me", Description = "forget me" },
                new() { Identifier = "login", Description = "login" },
                new() { Identifier = "logout", Description = "logout" },
                new() { Identifier = "archive", Description = "archive" }
            };
            return triggers;
        }

        public static List<ActionDto> createActions()
        {
            var actions = new List<ActionDto>
            {
                new() { Identifier = "unverified", Description = "send confirmation mail", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "unverified", Description = "start timer", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "unverified", Description = "stop timer", Type = ActionType.EXIT_ACTION },
                new() { Identifier = "blocked", Description = "notify user", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "active", Description = "logout", Type = ActionType.EXIT_ACTION },
                new() { Identifier = "verified", Description = "notify user", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "deleted", Description = "anonymize", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "logged_in", Description = "reset attempts", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "logged_in", Description = "log activity", Type = ActionType.DO_ACTION },
                new() { Identifier = "t2", Description = "attempts = 0", Type = ActionType.TRANSITION_ACTION },
                new() { Identifier = "t3", Description = "attempts++", Type = ActionType.TRANSITION_ACTION }
            };
            return actions;
        }

        public static List<TransitionDto> createTransitions()
        {
            var transitions = new List<TransitionDto>
            {
                new() { Identifier = "t1", SourceStateIdentifier = "initial", DestinationStateIdentifier = "unverified", TriggerIdentifier = "create", GuardCondition = "" },
                new() { Identifier = "t2", SourceStateIdentifier = "unverified", DestinationStateIdentifier = "verified", TriggerIdentifier = "email_verification", GuardCondition = "" },
                new() { Identifier = "t3", SourceStateIdentifier = "verified", DestinationStateIdentifier = "verified", TriggerIdentifier = "login", GuardCondition = "invalid credentials" },
                new() { Identifier = "t4", SourceStateIdentifier = "verified", DestinationStateIdentifier = "logged_in", TriggerIdentifier = "login", GuardCondition = "valid credentials" },
                new() { Identifier = "t5", SourceStateIdentifier = "logged_in", DestinationStateIdentifier = "verified", TriggerIdentifier = "logout", GuardCondition = "" },
                new() { Identifier = "t6", SourceStateIdentifier = "logged_in", DestinationStateIdentifier = "deleted", TriggerIdentifier = "forget_me", GuardCondition = "valid credentials" },
                new() { Identifier = "t7", SourceStateIdentifier = "verified", DestinationStateIdentifier = "blocked", TriggerIdentifier = null, GuardCondition = "attempts >= 3" },
                new() { Identifier = "t8", SourceStateIdentifier = "active", DestinationStateIdentifier = "blocked", TriggerIdentifier = "blocked_by_admin", GuardCondition = "" },
                new() { Identifier = "t9", SourceStateIdentifier = "blocked", DestinationStateIdentifier = "verified", TriggerIdentifier = "unblocked_by_admin", GuardCondition = "" },
                new() { Identifier = "t10", SourceStateIdentifier = "blocked", DestinationStateIdentifier = "deleted", TriggerIdentifier = "deleted_by_admin", GuardCondition = "" },
                new() { Identifier = "t11", SourceStateIdentifier = "deleted", DestinationStateIdentifier = "final", TriggerIdentifier = "archive", GuardCondition = "" },
                new() { Identifier = "t12", SourceStateIdentifier = "unverified", DestinationStateIdentifier = "deleted", TriggerIdentifier = "timer_elapsed", GuardCondition = "" }
            };

            return transitions;
        }

        // Utils
        public static void NoErrors(ValidationResult result)
        {
            Assert.True(result.IsValid && result.Errors.Count == 0);
        }
        public static void ExpectErrors(ValidationResult result)
        {
            Assert.True(!result.IsValid && result.Errors.Count > 0);
        }

        [Fact]
        public void IdentifierRule_Stops_Invalid_State()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "1", Parent = null, Name = "Intial", Type = StateType.INITIAL },
                new() { Identifier = "2", Parent = null, Name = "Simple", Type = StateType.SIMPLE },
                new() { Identifier = "3", Parent = null, Name = "Final", Type = StateType.FINAL }
            };

            var transitions = new List<TransitionDto>
            {
                  new() { Identifier = "12", SourceStateIdentifier = "1", DestinationStateIdentifier = "2", TriggerIdentifier = null, GuardCondition = "" },
                  new() { Identifier = "23", SourceStateIdentifier = "2", DestinationStateIdentifier = "3", TriggerIdentifier = null, GuardCondition = "" }, 
            };

            var dto = new FsmDto
            {
               States = states,
               Transitions = transitions
            };

            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var parser = new FsmRuleParser(rules, dto);
            var result = parser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void IdentifierRule_Accepts_Valid_State()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "init", Parent = null, Name = "Intial", Type = StateType.INITIAL },
                new() { Identifier = "some", Parent = null, Name = "Simple", Type = StateType.SIMPLE },
                new() { Identifier = "fin", Parent = null, Name = "Final", Type = StateType.FINAL }
            };

            var transitions = new List<TransitionDto>
            {
                new() { Identifier = "t1", SourceStateIdentifier = "init", DestinationStateIdentifier = "some", TriggerIdentifier = null, GuardCondition = "" },
                new() { Identifier = "t2", SourceStateIdentifier = "some", DestinationStateIdentifier = "fin", TriggerIdentifier = null, GuardCondition = "" },
            };

            var dto = new FsmDto
            {
                States = states,
                Transitions = transitions
            };

            var rules = new List<IValidationRule>
            {
                new IdentifierRule()
            };

            var parser = new FsmRuleParser(rules, dto);
            var result = parser.Validate();

            NoErrors(result);
        }


        [Fact]
        public void IntialAndFinalStateRules_Stop_Invalid_State()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "init", Parent = null, Name = "Intial", Type = StateType.INITIAL },
                new() { Identifier = "fin", Parent = null, Name = "Final", Type = StateType.FINAL }
            };

            var transitions = new List<TransitionDto>
            {
                new() { Identifier = "t1", SourceStateIdentifier = "init", DestinationStateIdentifier = "fin", TriggerIdentifier = null, GuardCondition = "" },
                new() { Identifier = "t2", SourceStateIdentifier = "fin", DestinationStateIdentifier = "init", TriggerIdentifier = null, GuardCondition = "" },
            };

            var dto = new FsmDto
            {
                States = states,
                Transitions = transitions
            };

            var rules = new List<IValidationRule>
            {
                new InitialStateRule(),
                new FinalStateRule(),
            };

            var parser = new FsmRuleParser(rules, dto);
            var result = parser.Validate();

            ExpectErrors(result);
        }

        [Fact]
        public void IntialAndFinalStateRules_Accept_Valid_State()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "init", Parent = null, Name = "Intial", Type = StateType.INITIAL },
                new() { Identifier = "some", Parent = null, Name = "Simple", Type = StateType.SIMPLE },
                new() { Identifier = "fin", Parent = null, Name = "Final", Type = StateType.FINAL }
            };

            var transitions = new List<TransitionDto>
            {
                new() { Identifier = "t1", SourceStateIdentifier = "init", DestinationStateIdentifier = "some", TriggerIdentifier = null, GuardCondition = "" },
                new() { Identifier = "t2", SourceStateIdentifier = "some", DestinationStateIdentifier = "fin", TriggerIdentifier = null, GuardCondition = "" },
            };

            var dto = new FsmDto
            {
                States = states,
                Transitions = transitions
            };

            var rules = new List<IValidationRule>
            {
                new InitialStateRule(),
                new FinalStateRule()
            };

            var parser = new FsmRuleParser(rules, dto);
            var result = parser.Validate();

            NoErrors(result);
        }

    }
}
