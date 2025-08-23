using System;
using System.Linq;
using System.Text.RegularExpressions;
using UmlViewer.UI;
using UmlViewer.UI.ModelView;
using Xunit;

namespace UmlViewer.Tests
{
    public class TextRendererTests
    {
        [Fact]
        public void Renders_Header_And_Title()
        {
            var view = new FsmView
            {
                Title = "Demo",
                Initial = new StateView { Identifier = "_i", DisplayName = "init", IsCompound = false },
                Final = new StateView { Identifier = "_f", DisplayName = "final", IsCompound = false }
            };
            var txt = new TextRenderer().Render(view);

            Assert.Contains("######################################################################", txt);
            Assert.Contains("# Diagram: Demo", txt);
        }

        [Fact]
        public void Renders_Initial_TopLevel_Compound_Internal_Then_Other_TopLevel_Then_Final()
        {
            var view = BuildTimedLightView();
            var txt = new TextRenderer().Render(view);

            // Order assertions using index positions
            int iInitialLine = txt.IndexOf("O Initial state (powered off)", StringComparison.Ordinal);
            int iT1 = txt.IndexOf("---turn power on---> Lamp is off", StringComparison.Ordinal);
            int iCompoundStart = txt.IndexOf("|| Compound state: Powered up", StringComparison.Ordinal);
            int iT2 = txt.IndexOf("---Push switch [time off > 10s] / reset off timer---> Lamp is on", StringComparison.Ordinal);
            int iT3 = txt.IndexOf("---Push switch---> Lamp is off", StringComparison.Ordinal);
            int iT4 = txt.IndexOf("---turn power off---> Final state (powered off)", StringComparison.Ordinal);
            int iFinalLine = txt.IndexOf("(O) Final state (powered off)", StringComparison.Ordinal);

            Assert.True(iInitialLine >= 0);
            Assert.True(iT1 > iInitialLine);
            Assert.True(iCompoundStart > iT1);
            Assert.True(iT2 > iCompoundStart);
            Assert.True(iT3 > iT2);
            Assert.True(iT4 > iCompoundStart);      // after compound block
            Assert.True(iFinalLine > iT4);
        }

        [Fact]
        public void Renders_Compound_Block_Structure_And_Child_States()
        {
            var view = BuildTimedLightView();
            var txt = new TextRenderer().Render(view);

            Assert.Contains("|| Compound state: Powered up", txt);
            Assert.Contains("| Lamp is off", txt);
            Assert.Contains("| Lamp is on", txt);

            // Match: 3 spaces + at least 10 dashes, OR 3 spaces + "| Lamp is off"
            var hasIndentedDelimiter = Regex.IsMatch(txt, @"(?m)^\s{3}-{10,}\s*$");
            var hasIndentedChildName = Regex.IsMatch(txt, @"(?m)^\s{3}\|\sLamp is off\s*$");

            Assert.True(hasIndentedDelimiter || hasIndentedChildName,
                "Expected an indented child-state box delimiter or an indented child title line.");
        }


        [Fact]
        public void Renders_State_Actions()
        {
            var view = BuildTimedLightView();
            var txt = new TextRenderer().Render(view);

            Assert.Contains("| On Entry / Start off timer", txt); // on "Lamp is off"
            Assert.Contains("| On Entry / Turn lamp on", txt);     // on "Lamp is on"
            Assert.Contains("| On Exit / Turn lamp off", txt);      // on "Lamp is on"
        }

        [Fact]
        public void Renders_Internal_Transitions_With_Guard_And_Effect()
        {
            var view = BuildTimedLightView();
            var txt = new TextRenderer().Render(view);

            Assert.Contains("---Push switch [time off > 10s] / reset off timer---> Lamp is on", txt);
            Assert.Contains("---Push switch---> Lamp is off", txt);
        }

        [Fact]
        public void Renders_Empty_View_With_Placeholder_InitialAndFinal()
        {
            var view = new FsmView
            {
                Title = "Empty",
                Initial = new StateView { Identifier = "_i", DisplayName = "init", IsCompound = false },
                Final = new StateView { Identifier = "_f", DisplayName = "final", IsCompound = false }
            };

            var txt = new TextRenderer().Render(view);

            // Header + title
            Assert.Contains("# Diagram: Empty", txt);

            // With required placeholders, initial/final SHOULD appear
            Assert.Contains("O Initial state (init)", txt);
            Assert.Contains("(O) Final state (final)", txt);
        }


        // ----------------- helpers -----------------

        private static FsmView BuildTimedLightView()
        {
            // State views
            var initial = new StateView { Identifier = "h1", DisplayName = "powered off", IsCompound = false };
            var final = new StateView { Identifier = "h5", DisplayName = "powered off", IsCompound = false };

            var powered = new StateView { Identifier = "h2", DisplayName = "Powered up", IsCompound = true };
            var off = new StateView { Identifier = "h3", DisplayName = "Lamp is off", IsCompound = false };
            var on = new StateView { Identifier = "h4", DisplayName = "Lamp is on", IsCompound = false };


            off.EntryActions.Add("Start off timer");
            on.EntryActions.Add("Turn lamp on");
            on.ExitActions.Add("Turn lamp off");


            powered.Children.Add(off);
            powered.Children.Add(on);


            var t1 = new TransitionView
            {
                FromIdentifier = "h1",
                ToIdentifier = "h3",
                ToDisplayName = off.DisplayName,
                TriggerIdentifier = "turn power on",
                GuardCondition = ""
            };


            var t2 = new TransitionView
            {
                FromIdentifier = "h3",
                ToIdentifier = "h4",
                ToDisplayName = on.DisplayName,
                TriggerIdentifier = "Push switch",
                GuardCondition = "time off > 10s"
            };
            t2.TransitionActions.Add("reset off timer");


            var t3 = new TransitionView
            {
                FromIdentifier = "h4",
                ToIdentifier = "h3",
                ToDisplayName = off.DisplayName,
                TriggerIdentifier = "Push switch",
                GuardCondition = ""
            };


            var t4 = new TransitionView
            {
                FromIdentifier = "h2",
                ToIdentifier = "h5",
                ToDisplayName = "Final state (powered off)",
                TriggerIdentifier = "turn power off",
                GuardCondition = ""
            };


            off.Outgoing.Add(t2);
            on.Outgoing.Add(t3);

            var view = new FsmView
            {
                Title = "Timed Light",
                Initial = initial,
                Final = final
            };

            view.RootStates.Add(powered);
            view.TopLevelTransitions.Add(t1);
            view.TopLevelTransitions.Add(t4);

            return view;
        }
    }
}
