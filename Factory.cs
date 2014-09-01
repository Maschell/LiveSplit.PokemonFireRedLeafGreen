using LiveSplit.Model;
using LiveSplit.PokemonFireRedLeafGreen;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(Factory))]

namespace LiveSplit.PokemonFireRedLeafGreen
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Pokemon FireRed/LeafGreen Auto Splitter"; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public string Description
        {
            get { return "Automatically splits for Pokemon FireRed/LeafGreen on VBA 1.7.2."; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new Component();
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.OcarinaOfTime.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("0.0.1"); }
        }
    }
}
