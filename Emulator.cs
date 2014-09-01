using System.Diagnostics;
using System.Linq;

namespace LiveSplit.PokemonFireRedLeafGreen
{
    public class Emulator
    {
        public Process Process { get; protected set; }
        public int[] Offset { get; protected set; }

        protected Emulator(Process process, int[] offset)
        {
            Process = process;
            Offset = offset;
        }

        public static Emulator TryConnect()
        {
            
            var process = Process.GetProcessesByName("VisualBoyAdvance").FirstOrDefault();
            if (process != null)
            {
                return BuildVBA(process);
            }         


            return null;
        }

        private static Emulator Build(Process process, int[] _base)
        {            
            int[] offset = new int[3];
            offset[(int)EmulatorRamArea.IRAM] = ~new DeepPointer<int>(process, _base[(int)EmulatorRamArea.IRAM]);
            offset[(int)EmulatorRamArea.WRAM] = ~new DeepPointer<int>(process, _base[(int)EmulatorRamArea.WRAM]);

            return new Emulator(process, offset);
        }
        
        private static Emulator BuildVBA(Process process)
        {
            int[] _base = new int[3];
            ProcessModule module = process.MainModule;
            _base[(int)EmulatorRamArea.IRAM] = (int)EmulatorBaseIRAM.VisualBoyAdvance172;
            _base[(int)EmulatorRamArea.WRAM] = (int)EmulatorBaseWRAM.VisualBoyAdvance172;     
              
            return Build(process, _base);
        }

      

        public DeepPointer<T> CreatePointer<T>(EmulatorRamArea ramarea,int address)
        {
            return CreatePointer<T>(ramarea,1, address);
        }

        public DeepPointer<T> CreatePointer<T>(EmulatorRamArea ramarea, int length, int address)
        {
            return new DeepPointer<T>(length, Process, Offset[(int)ramarea] - (int)Process.MainModule.BaseAddress + address);
        }
        public DeepPointer<T> CreatePointer<T>(EmulatorRamArea ramarea, int length, int address, params int[] offsets)
        {
            return new DeepPointer<T>(length, Process, Offset[(int)ramarea] - (int)Process.MainModule.BaseAddress + address, offsets);
        }
    }
}
