using IO.DTO;
using System.Collections.Generic;
using Validation;
using Xunit;
using ActionType = IO.DTO.ActionType;
using StateType = IO.DTO.StateType;

namespace ValidationTests
{
    public class RepositoryTests
    {
        private static FsmRepository CreateExampleLampRepository()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "h1", Parent = "_", Name = "powered off", Type = StateType.INITIAL },
                new() { Identifier = "h2", Parent = "_", Name = "Powered up", Type = StateType.COMPOUND },
                new() { Identifier = "h3", Parent = "h2", Name = "Lamp is off", Type = StateType.SIMPLE },
                new() { Identifier = "h4", Parent = "h2", Name = "Lamp is on", Type = StateType.SIMPLE },
                new() { Identifier = "h5", Parent = "_", Name = "powered off", Type = StateType.FINAL }
            };

            var triggers = new List<TriggerDto>
            {
                new() { Identifier = "power_on", Description = "turn power on" },
                new() { Identifier = "push_switch", Description = "Push switch" },
                new() { Identifier = "power_off", Description = "turn power off" }
            };

            var actions = new List<ActionDto>
            {
                new() { Identifier = "h4", Description = "Turn lamp on", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "h4", Description = "Turn lamp off", Type = ActionType.EXIT_ACTION },
                new() { Identifier = "h3", Description = "Start off timer", Type = ActionType.ENTRY_ACTION },
                new() { Identifier = "t2", Description = "reset off timer", Type = ActionType.TRANSITION_ACTION }
            };

            var transitions = new List<TransitionDto>
            {
                new() { Identifier = "t1", SourceStateIdentifier = "h1", DestinationStateIdentifier = "h3", TriggerIdentifier = "power_on", GuardCondition = "" },
                new() { Identifier = "t2", SourceStateIdentifier = "h3", DestinationStateIdentifier = "h4", TriggerIdentifier = "push_switch", GuardCondition = "time off > 10s" },
                new() { Identifier = "t3", SourceStateIdentifier = "h4", DestinationStateIdentifier = "h3", TriggerIdentifier = "push_switch", GuardCondition = "" },
                new() { Identifier = "t4", SourceStateIdentifier = "h2", DestinationStateIdentifier = "h5", TriggerIdentifier = "power_off", GuardCondition = "" }
            };

            var dto = new FsmDto
            {
                States = states,
                Triggers = triggers,
                Actions = actions,
                Transitions = transitions
            };

            return new FsmRepository(dto);
        }

        [Fact]
        public void StatesDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.Equal("h1", repo.RawStates["h1"].Id);
            Assert.Equal("h2", repo.RawStates["h2"].Id);
            Assert.Equal("h5", repo.RawStates["h5"].Id);
        }

        [Fact]
        public void ChildrenDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            var poweredChildren = repo.RawChildren["h2"];
            Assert.Contains(poweredChildren, s => s.Id == "h3");
            Assert.Contains(poweredChildren, s => s.Id == "h4");
        }

        [Fact]
        public void TriggersDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.Equal("power_on", repo.Triggers["power_on"].Identifier);
            Assert.Equal("push_switch", repo.Triggers["push_switch"].Identifier);
            Assert.Equal("power_off", repo.Triggers["power_off"].Identifier);
        }

        [Fact]
        public void ActionsDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.True(repo.Actions.TryGetValue("h4", out var onActions));
            Assert.Contains(onActions, a => a.Description == "Turn lamp on" && a.Type == Validation.ActionType.ENTRY_ACTION);
            Assert.Contains(onActions, a => a.Description == "Turn lamp off" && a.Type == Validation.ActionType.EXIT_ACTION);

            Assert.True(repo.Actions.TryGetValue("h3", out var offActions));
            Assert.Contains(offActions, a => a.Description == "Start off timer" && a.Type == Validation.ActionType.ENTRY_ACTION);

            Assert.True(repo.Actions.TryGetValue("t2", out var t2Actions));
            Assert.Contains(t2Actions, a => a.Description == "reset off timer" && a.Type == Validation.ActionType.TRANSITION_ACTION);
        }

        [Fact]
        public void SourceTransitionsDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.True(repo.SourceTransitions.TryGetValue("h1", out var initialTransitions));
            Assert.Contains(initialTransitions, t => t.Id == "t1" && t.DestinationStateId == "h3");

            Assert.True(repo.SourceTransitions.TryGetValue("h3", out var offTransitions));
            Assert.Contains(offTransitions, t => t.Id == "t2" && t.DestinationStateId == "h4");

            Assert.True(repo.SourceTransitions.TryGetValue("h4", out var onTransitions));
            Assert.Contains(onTransitions, t => t.Id == "t3" && t.DestinationStateId == "h3");

            Assert.True(repo.SourceTransitions.TryGetValue("h2", out var poweredTransitions));
            Assert.Contains(poweredTransitions, t => t.Id == "t4" && t.DestinationStateId == "h5");
        }

        [Fact]
        public void DestinationTransitionsDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.True(repo.DestinationTransitions.TryGetValue("h3", out var destOffTransitions));
            Assert.Contains(destOffTransitions, t => t.Id == "t1" || t.Id == "t3");

            Assert.True(repo.DestinationTransitions.TryGetValue("h4", out var destOnTransitions));
            Assert.Contains(destOnTransitions, t => t.Id == "t2");

            Assert.True(repo.DestinationTransitions.TryGetValue("h5", out var destFinalTransitions));
            Assert.Contains(destFinalTransitions, t => t.Id == "t4");
        }
    }
}
