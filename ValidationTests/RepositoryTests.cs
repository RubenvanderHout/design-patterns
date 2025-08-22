using IO.DTO;
using System.Collections.Generic;
using Validation;
using Xunit;
using ActionType = IO.DTO.ActionType;
using StateType = IO.DTO.StateType;

namespace ValidationTests
{
    // These tests test if the indexes and tranformation of the raw data went any good.
    // At this point in time no validation is done on this data. So things like duplicate keys can throw exceptions.

    public class RepositoryTests
    {
        private static FsmRepository CreateExampleLampRepository()
        {
            var states = new List<StateDto>
            {
                new() { Identifier = "h1", Parent = null, Name = "powered off", Type = StateType.INITIAL },
                new() { Identifier = "h2", Parent = null, Name = "Powered up", Type = StateType.COMPOUND },
                new() { Identifier = "h3", Parent = "h2", Name = "Lamp is off", Type = StateType.SIMPLE },
                new() { Identifier = "h4", Parent = "h2", Name = "Lamp is on", Type = StateType.SIMPLE },
                new() { Identifier = "h5", Parent = null, Name = "powered off", Type = StateType.FINAL }
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
            Assert.Equal("h1", repo.RootStates["h1"].Identifier);
            Assert.Equal("h2", repo.RootStates["h2"].Identifier);
            Assert.Equal("h5", repo.RootStates["h5"].Identifier);
        }

        [Fact]
        public void StatesDictionary_Nested_Transaction_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            repo.AllStates.TryGetValue("h3", out var state);
            if(state == null)
            {
                throw new Exception("Error");
            }
            Assert.Contains(state.SourceTransitions, t => t.Identifier == "t2");
            Assert.Contains(state.SourceTransitions, t => t.SourceState.Identifier == "h3");
        }

        [Fact]
        public void ChildStates_Can_Access_Parent()
        {
            var repo = CreateExampleLampRepository();
            repo.ChildStates.TryGetValue("h2", out var states);
            if (states == null)
            {
                throw new Exception("Should have one");
            } else if(states.Count == 0)
            {
                throw new Exception("Should not be empty");
            }

            foreach (var state in states)
            {
                Assert.True(state.Parent!.Identifier == "h2");
            }
        }

        [Fact]
        public void ChildrenDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            var poweredChildren = repo.ChildStates["h2"];
            Assert.Contains(poweredChildren, s => s.Identifier == "h3");
            Assert.Contains(poweredChildren, s => s.Identifier == "h4");
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
            Assert.Contains(initialTransitions, t => t.Identifier == "t1" && t.DestinationState.Identifier == "h3");

            Assert.True(repo.SourceTransitions.TryGetValue("h3", out var offTransitions));
            Assert.Contains(offTransitions, t => t.Identifier == "t2" && t.DestinationState.Identifier == "h4");

            Assert.True(repo.SourceTransitions.TryGetValue("h4", out var onTransitions));
            Assert.Contains(onTransitions, t => t.Identifier == "t3" && t.DestinationState.Identifier == "h3");

            Assert.True(repo.SourceTransitions.TryGetValue("h2", out var poweredTransitions));
            Assert.Contains(poweredTransitions, t => t.Identifier == "t4" && t.DestinationState.Identifier == "h5");
        }

        [Fact]
        public void DestinationTransitionsDictionary_IsCorrect()
        {
            var repo = CreateExampleLampRepository();
            Assert.True(repo.DestinationTransitions.TryGetValue("h3", out var destOffTransitions));
            Assert.Contains(destOffTransitions, t => t.Identifier == "t1" || t.Identifier == "t3");

            Assert.True(repo.DestinationTransitions.TryGetValue("h4", out var destOnTransitions));
            Assert.Contains(destOnTransitions, t => t.Identifier == "t2");

            Assert.True(repo.DestinationTransitions.TryGetValue("h5", out var destFinalTransitions));
            Assert.Contains(destFinalTransitions, t => t.Identifier == "t4");
        }
    }
}
