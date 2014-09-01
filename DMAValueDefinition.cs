using System;

namespace LiveSplit.ASL
{
    public class DMAValueDefinition
    {
        public String Type { get; set; }
        public String Identifier { get; set; }
        public ASLValueDefinition ASLValue { get; set; }    
        public PokemonFireRedLeafGreen.EmulatorRamArea RamArea { get; set; }     
        public int Length { get; set; }
        public int Address { get; set; }
        public System.Type Typea { get; set; }
    }
}
