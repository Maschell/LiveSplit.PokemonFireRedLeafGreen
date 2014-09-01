using LiveSplit.Model;
using LiveSplit.PokemonFireRedLeafGreen;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace LiveSplit.ASL
{
    public class PokemonFireRedLeafGreenScript
    {
        protected TimerModel Model { get; set; }
        protected Emulator Emulator { get; set; }
        public ASLState OldState { get; set; }
        public ASLState State { get; set; }

        public List<DMAValueDefinition> DMAValueDefinitions { get; set; }
        public PokemonFireRedLeafGreenScript()
        {
            State = new ASLState();
            DMAValueDefinitions = new List<DMAValueDefinition>();
        }

        protected void TryConnect()
        {
            if (Emulator == null || Emulator.Process.HasExited)
            {
                Emulator = Emulator.TryConnect();
                if (Emulator != null)
                {
                    Rebuild();
                    State.RefreshValues();                    
                    OldState = State;                    
                }
            }
        }

        private void Rebuild()
        {
            State.ValueDefinitions.Clear();
            var gameVersion = GetGameVersion();
            switch (gameVersion)
            {               
                case GameVersion.US: RebuildUS(); break;
                default: Emulator = null; break;
            }
           
        }
        private void RebuildBase(int offset = 0x0)
        {
            AddPointer<int>(EmulatorRamArea.IRAM,"DMAPointer", 0x500C + offset);
            AddDMAPointer<GameTime>("GameTimer", 0x010);            
        }
         private void RebuildUS()
        {
             RebuildBase();            
        }   

        private GameVersion GetGameVersion()
        {
            return GameVersion.US;
        }

        private ASLValueDefinition AddPointer<T>(EmulatorRamArea ramarea, String name, int address)
        {
            return AddPointer<T>(ramarea, 1, name, address);
        }

        private ASLValueDefinition AddPointer<T>(EmulatorRamArea ramarea, int length, String name, int address)
        {
            ASLValueDefinition ASLDef = new ASLValueDefinition()
            {
                Identifier = name,
                Pointer = Emulator.CreatePointer<T>(ramarea, length, address)
            };
            State.ValueDefinitions.Add(ASLDef);
            return ASLDef;        
        }

        private void AddDMAPointer<T>(String name, int address)
        {
            AddDMAPointer<T>(1, name, address);
        }
        private void AddDMAPointer<T>(int length, String name, int address)
        {
            AddDMAPointer<T>(length, name, address, GetDMAAddress());
        }
        private void AddDMAPointer<T>(int length, String name, int address, int DMAAddress)
        {
            ASLValueDefinition _ASLValue = AddPointer<T>(EmulatorRamArea.WRAM, length, name, DMAAddress + address);
            DMAValueDefinition _DMAValue = new DMAValueDefinition()
            {
                Identifier = name,
                Address = address,
                RamArea = EmulatorRamArea.WRAM,
                Length = length,
                ASLValue = _ASLValue,
                Typea = typeof(T)
            };
            DMAValueDefinitions.Add(_DMAValue);
        }

        protected int GetDMAAddress()
        {
            var address = ~Emulator.CreatePointer<int>(EmulatorRamArea.IRAM, 0x500C);
            return address - 0x02000000;
        }

        private void UpdateDMAAddress()
        {           
            int DMAAddress = GetDMAAddress();
            foreach (var valueDefinition in DMAValueDefinitions)
            {         
                State.ValueDefinitions.Remove(valueDefinition.ASLValue);
                MethodInfo method = Emulator.GetType().GetMethod("CreatePointer", new Type[] { typeof(EmulatorRamArea), typeof(int), typeof(int) });
                MethodInfo generic = method.MakeGenericMethod(valueDefinition.Typea);    
                           
                ASLValueDefinition foo = new ASLValueDefinition()
                {
                    Identifier = valueDefinition.Identifier,
                    Pointer =  generic.Invoke(Emulator, new Object [] { valueDefinition.RamArea, valueDefinition.Length, valueDefinition.Address + DMAAddress }) 
                };
                valueDefinition.ASLValue = foo;
                State.ValueDefinitions.Add(foo);
            }                      
        }

        public void Update(LiveSplitState lsState)
        {
           
            if (Emulator != null && !Emulator.Process.HasExited)
            {                
                OldState = State.RefreshValues();
                if (checkDMAAddress(OldState.Data, State.Data))
                {
                    UpdateDMAAddress();
                }

                if (lsState.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (Start(lsState, OldState.Data, State.Data))
                    {
                        Model.Start();
                    }
                }
                else if (lsState.CurrentPhase == TimerPhase.Running || lsState.CurrentPhase == TimerPhase.Paused)
                {
                   
                    if (Reset(lsState, OldState.Data, State.Data))
                    {
                        Model.Reset();
                        return;
                    }
                    else if (Split(lsState, OldState.Data, State.Data))
                    {
                        Model.Split();
                    }
                   
                    var isPaused = IsPaused(lsState, OldState.Data, State.Data);
                    if (isPaused != null)
                        lsState.IsGameTimePaused = isPaused;

                    //var encounter = GetEncounter(lsState, OldState.Data, State.Data);
                   
                    var gameTime = GameTime(lsState, OldState.Data, State.Data);
                    if (gameTime != null)
                        lsState.SetGameTime(gameTime);
                   
                }
            }
            else
            {
                if (Model == null)
                {
                    Model = new TimerModel() { CurrentState = lsState };
                }
                TryConnect();
            }
           
        }
        public bool checkDMAAddress(dynamic old, dynamic current)
        {
            return old.DMAPointer != current.DMAPointer;
        }
        public bool Start(LiveSplitState timer, dynamic old, dynamic current)
        {           
           return false;
        }

        public bool Split(LiveSplitState timer, dynamic old, dynamic current)
        {
            //var segment = timer.CurrentSplit.Name.ToLower();
            return false;
        }

        public bool Reset(LiveSplitState timer, dynamic old, dynamic current)
        {
            return false;
        }

        public bool IsPaused(LiveSplitState timer, dynamic old, dynamic current)
        {
            return true;
        }

        public TimeSpan? GameTime(LiveSplitState timer, dynamic old, dynamic current)
        {
            return TimeSpan.FromMilliseconds((((current.GameTimer.Hours * 60) + current.GameTimer.Minutes) * 60 + current.GameTimer.Seconds + current.GameTimer.Frames / 60.0) * 1000);
        }

       
       
    }
}
