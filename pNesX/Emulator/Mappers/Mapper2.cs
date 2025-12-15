

namespace pNesX
{
    class Mapper2 : Mapper
    {

        private int prgBankNo;


        protected override void WritePRG(int address, byte data)
        {
            prgBankNo = data & 0xF;
        }

        protected override byte ReadPRG(int address)
        {
            if(address < 0xC000)
            {
                return _rom.prgRom[(address & (prgRomBankSize16k - 1)) + (prgBankNo * prgRomBankSize16k)];
            }
            else
            {
                return _rom.prgRom[(address & (prgRomBankSize16k - 1)) + (_rom.prgRom.Length - prgRomBankSize16k)];
            }  
        }

        public override void WriteSaveState(ref Savestate state)
        {
            state.prgBankNo = prgBankNo;
            base.WriteSaveState(ref state);
        }
        public override void LoadSaveState(ref Savestate state)
        {
            prgBankNo = state.prgBankNo;
            base.LoadSaveState(ref state);
        }

    }
}
