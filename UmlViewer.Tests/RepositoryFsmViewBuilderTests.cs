using System.Linq;
using IO.DTO;
using UmlViewer.UI;
using UmlViewer.UI.ModelView;
using Validation;
using Xunit;
using DtoActionType = IO.DTO.ActionType;
using DtoStateType  = IO.DTO.StateType;

namespace UmlViewer.Tests
{
    public class RepositoryFsmViewBuilderTests
    {
        
        private static FsmRepository CreateExampleLampRepository()
        {
            var states = new[]
            {
                new StateDto { Identifier = "h1", Parent = null,  Name = "powered off", Type = DtoStateType.INITIAL },
                new StateDto { Identifier = "h2", Parent = null,  Name = "Powered up",  Type = DtoStateType.COMPOUND },
                new StateDto { Identifier = "h3", Parent = "h2",  Name = "Lamp is off", Type = DtoStateType.SIMPLE },
                new StateDto { Identifier = "h4", Parent = "h2",  Name = "Lamp is on",  Type = DtoStateType.SIMPLE },
                new StateDto { Identifier = "h5", Parent = null,  Name = "powered off", Type = DtoStateType.FINAL },
            };

            var triggers = new[]
            {
                new TriggerDto { Identifier = "power_on",   Description = "turn power on"   },
                new TriggerDto { Identifier = "push_switch",Description = "Push switch"     },
                new TriggerDto { Identifier = "power_off",  Description = "turn power off"  },
            };

            var actions = new[]
            {
                new ActionDto { Identifier = "h4", Description = "Turn lamp on",     Type = DtoActionType.ENTRY_ACTION },
                new ActionDto { Identifier = "h4", Description = "Turn lamp off",    Type = DtoActionType.EXIT_ACTION  },
                new ActionDto { Identifier = "h3", Description = "Start off timer",  Type = DtoActionType.ENTRY_ACTION },
                new ActionDto { Identifier = "t2", Description = "reset off timer",  Type = DtoActionType.TRANSITION_ACTION },
            };

            var transitions = new[]
            {
                new TransitionDto { Identifier = "t1", SourceStateIdentifier = "h1", DestinationStateIdentifier = "h3", TriggerIdentifier = "power_on",    GuardCondition = "" },
                new TransitionDto { Identifier = "t2", SourceStateIdentifier = "h3", DestinationStateIdentifier = "h4", TriggerIdentifier = "push_switch", GuardCondition = "time off > 10s" },
                new TransitionDto { Identifier = "t3", SourceStateIdentifier = "h4", DestinationStateIdentifier = "h3", TriggerIdentifier = "push_switch", GuardCondition = "" },
                new TransitionDto { Identifier = "t4", SourceStateIdentifier = "h2", DestinationStateIdentifier = "h5", TriggerIdentifier = "power_off",   GuardCondition = "" },
            };

            var dto = new FsmDto
            {
                States      = states.ToList(),
                Triggers    = triggers.ToList(),
                Actions     = actions.ToList(),
                Transitions = transitions.ToList(),
            };

            return new FsmRepository(dto);
        }

        private static FsmView Build(FsmRepository repo, string title = "Timed Light")
        {
            var b = new RepositoryFsmViewBuilder();
            return b.Build(repo, title);
        }

        [Fact]
        public void Step1_CollectStates_InitialAndFinalDetected()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            Assert.NotNull(view.Initial);
            Assert.NotNull(view.Final);
            Assert.Equal("powered off", view.Initial!.DisplayName);
            Assert.Equal("powered off", view.Final!.DisplayName);
        }

        [Fact]
        public void Step2_StateActions_MappedToEntryDoExit()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            var off = FindState(view, "h3");
            Assert.Contains("Start off timer", off.EntryActions);

            var on = FindState(view, "h4");
            Assert.Contains("Turn lamp on",  on.EntryActions);
            Assert.Contains("Turn lamp off", on.ExitActions);
        }

        [Fact]
        public void Step3_ParentChildren_WiredUnderCompound()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            // “Powered up” is a top-level root and compound
            var powered = view.RootStates.Single(s => s.Identifier == "h2");
            Assert.True(powered.IsCompound);

            var childIds = powered.Children.Select(c => c.Identifier).ToHashSet();
            Assert.Contains("h3", childIds); // Lamp is off
            Assert.Contains("h4", childIds); // Lamp is on
        }

        [Fact]
        public void Step4_Transitions_SplitIntoTopLevelAndOutgoing()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            // initial -> off should be TopLevel (from initial)
            Assert.Contains(view.TopLevelTransitions, t => t.FromIdentifier == "h1" && t.ToIdentifier == "h3");

            // powered -> final should be TopLevel (from top-level compound)
            Assert.Contains(view.TopLevelTransitions, t => t.FromIdentifier == "h2" && t.ToIdentifier == "h5");

            // off -> on should be an outgoing transition inside state h3
            var off = FindState(view, "h3");
            Assert.Contains(off.Outgoing, t => t.ToIdentifier == "h4");

            // on -> off should be an outgoing transition inside state h4
            var on = FindState(view, "h4");
            Assert.Contains(on.Outgoing, t => t.ToIdentifier == "h3");
        }

        [Fact]
        public void Step5_TriggerDescriptions_Appear_OnTransitions()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            // from initial: "turn power on"
            var t1 = view.TopLevelTransitions.Single(t => t.FromIdentifier == "h1");
            Assert.Equal("turn power on", t1.TriggerIdentifier);

            // internal: "Push switch"
            var off = FindState(view, "h3");
            var t2 = off.Outgoing.Single(t => t.ToIdentifier == "h4");
            Assert.Equal("Push switch", t2.TriggerIdentifier);
        }

        [Fact]
        public void Step6_TransitionEffects_Mapped_FromActionsDict()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            // t2 has / reset off timer
            var off = FindState(view, "h3");
            var t2 = off.Outgoing.Single(t => t.ToIdentifier == "h4");
            Assert.Contains("reset off timer", t2.TransitionActions);
        }

        [Fact]
        public void Step7_TopLevelTransitions_InitialComeFirst()
        {
            var repo = CreateExampleLampRepository();
            var view = Build(repo);

            Assert.True(view.TopLevelTransitions.Count >= 2);

            var first = view.TopLevelTransitions[0];
            Assert.Equal("h1", first.FromIdentifier); // initial first

            // somewhere after should be powered -> final
            Assert.Contains(view.TopLevelTransitions.Skip(1), t => t.FromIdentifier == "h2" && t.ToIdentifier == "h5");
        }

        [Fact]
        public void Step8_Throws_When_NoInitialOrFinal()
        {
            
            var badStates = new[]
            {
                new StateDto { Identifier = "h2", Parent = null, Name = "Powered up",  Type = DtoStateType.COMPOUND },
                new StateDto { Identifier = "h3", Parent = "h2", Name = "Lamp is off", Type = DtoStateType.SIMPLE },
                new StateDto { Identifier = "h4", Parent = "h2", Name = "Lamp is on",  Type = DtoStateType.SIMPLE },
            };

            var dto = new FsmDto
            {
                States      = badStates.ToList(),
                Triggers    = new List<TriggerDto>(),
                Actions     = new List<ActionDto>(),
                Transitions = new List<TransitionDto>(),
            };

            var repo = new FsmRepository(dto);
            var builder = new RepositoryFsmViewBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.Build(repo, "Broken"));
        }

        // --------- helper ---------

        private static StateView FindState(FsmView view, string id)
        {
            // search roots + descendants
            foreach (var r in view.RootStates)
            {
                var found = Dfs(r, id);
                if (found is not null) return found;
            }
            if (view.Initial?.Identifier == id) return view.Initial;
            if (view.Final?.Identifier   == id) return view.Final;
            throw new Xunit.Sdk.XunitException($"State '{id}' not found in view.");
        }

        private static StateView? Dfs(StateView node, string id)
        {
            if (node.Identifier == id) return node;
            foreach (var c in node.Children)
            {
                var f = Dfs(c, id);
                if (f is not null) return f;
            }
            return null;
        }
    }
}
