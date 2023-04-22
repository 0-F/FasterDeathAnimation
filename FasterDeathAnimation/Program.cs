using SoulsFormats;

namespace FasterDeathAnimation
{
    internal class Program
    {
        // TAE.Event.EndTime may have a very high value, but I don't know why.
        // If the value is greater to the limit, I prefer not to change the value.
        public const float EventEndTimeLimit = 100F;

        static void PatchTAE(ref TAE tae)
        {
            int nb = 0;
            int animIndex = 0;
            foreach (var anim in tae.Animations)
            {
                int eventIndex = 0;
                int killCharacterEventIndex = -1;
                int unkEventIndex = -1;
                foreach (var e in anim.Events)
                {
                    // type 0 = JumpTable
                    if (e.Type != 0)
                    {
                        eventIndex++;
                        continue;
                    }

                    var args = e.GetParameterBytes(false);
                    int jumpTableID = BitConverter.ToInt32(args, 0);

                    // JumpTableID 0x0C (12) = Kill Character
                    if (killCharacterEventIndex == -1 &&
                        jumpTableID == 12 &&
                        e.StartTime != 0 && e.EndTime != 0)
                    {
                        killCharacterEventIndex = eventIndex;
                        Console.WriteLine(anim.AnimFileName + " " + animIndex + " killCharacterEventIndex=" + eventIndex);
                    }
                    // JumpTableID 0x14 (20) = Set_0x78_10 (???)
                    else if (
                        unkEventIndex == -1 &&
                        jumpTableID == 20)
                    {
                        unkEventIndex = eventIndex;
                        Console.WriteLine(anim.AnimFileName + " " + animIndex + " unkEventIndex=" + eventIndex);
                    }

                    eventIndex++;
                }

                if (killCharacterEventIndex != -1 && unkEventIndex != -1)
                {
                    TAE.Event killCharEvent = anim.Events[killCharacterEventIndex];
                    TAE.Event unkEvent = anim.Events[unkEventIndex];

                    // unkEvent may start a little after killCharEvent (maybe not important)
                    float diff = unkEvent.StartTime - killCharEvent.StartTime;
                    if (diff < 0) { diff = 0; }

                    // move `Kill Character` event to the beginning
                    if (killCharEvent.EndTime < EventEndTimeLimit)
                    {
                        killCharEvent.EndTime -= killCharEvent.StartTime;
                    }
                    killCharEvent.StartTime = 0;

                    // move `unk` event to the beginning
                    if (unkEvent.EndTime < EventEndTimeLimit)
                    {
                        unkEvent.EndTime -= unkEvent.StartTime;
                        unkEvent.EndTime += diff;
                    }
                    unkEvent.StartTime = 0;
                    unkEvent.StartTime += diff;

                    nb++;
                }

                animIndex++;
            }

            Console.WriteLine("Animations patched: " + nb);
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: FasterDeathAnimation <anibnd_file_path> <out_anibnd_file_path> ");
                Console.WriteLine("Example: FasterDeathAnimation c0000.anibnd.dcx mod\\c0000.anibnd.dcx");

                return;
            }

            string file = args[0];
            string fileOut = args[1];

            Console.WriteLine($"Read `{file}`");

            BND4 bnd = BND4.Read(file);

            for (int i = 0; i < bnd.Files.Count; i++)
            {
                if (bnd.Files[i].Name.EndsWith(@"\chr\c0000\tae\a00.tae"))
                {
                    var tae = TAE.Read(bnd.Files[i].Bytes);
                    PatchTAE(ref tae);

                    // replace original TAE by patched TAE
                    bnd.Files[i].Bytes = tae.Write(tae.Compression);
                    if (!bnd.Validate(out Exception ex))
                    {
                        throw ex;
                    }

                    // write patched file
                    Console.WriteLine($"Write patched file `{fileOut}`");
                    Console.WriteLine("Wait...");
                    bnd.Write(fileOut, bnd.Compression);
                    Console.WriteLine("File patched.");
                    break;
                }
            }
        }
    }
}
