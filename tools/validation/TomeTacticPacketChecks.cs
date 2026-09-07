using System;
using System.IO;
using System.Reflection;
using System.Text;
using FrameWork;

// Builds the tome tactic advance packets with the real PacketOut writer and diffs them against
// bytes captured from the live 1.4.8 server (WAR-RE-Toolkit/libs/protocolservices/Packet Logs,
// "Inevitable City Shaman 40 94 Defense", the category-16 exchange for these same 27 tactics).
//
// This exists because the layout was got wrong repeatedly by reading code and inferring offsets.
// It needs no database, no server and no client: if the bytes differ, it says exactly where.
internal static class TomeTacticPacketChecks
{
    // Payload only -- the leading 3 bytes of each captured packet are the frame (size, opcode).
    private const string CapturedCategory =
        "10 01 00 00 00 00 00 00 00 00 E2 90 00 01 4F F0 10 54 6F 6D 65 20 54 61 63 74 69 63 20 43 43 20 41 " +
        "00 1B 00 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 09 00 0A 00 0B 00 0C 00 0D 00 0E 00 0F " +
        "00 10 00 11 00 12 00 13 00 14 00 15 00 16 00 17 00 18 00 19 00 1A 00 1B 00 00 00";

    private const string CapturedPackageOne =
        "10 01 00 01 00 00 00 00 00 00 00 00 00 00 18 38 00 00 00 00 00 00 00 00 01 00 00 03 2A 02 00 00 " +
        "3A FC 00 01 07 45 02 00 00 00 00 00 00 00 01 02 00 00 00 00 14 " +
        "41 65 74 68 79 72 69 63 20 50 61 6E 64 65 6D 6F 6E 69 75 6D 00 00 00 00 00";

    private static int Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", new AssemblyName(args.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };

        try
        {
            // Match the live wire format WorldServer/NetWork/TCPServer.cs configures, so the frame
            // is the same 2-byte size + 1-byte opcode the capture shows. Without this the defaults
            // give a 4-byte size and every offset appears shifted by two.
            PacketOut.SizeLen = sizeof(ushort);
            PacketOut.OpcodeInLen = false;
            PacketOut.SizeInLen = false;
            PacketOut.Struct = PackStruct.SizeAndOpcode;

            int bad = 0;
            bad += Compare("F_CAREER_CATEGORY (category 16)", BuildCategory(27), Parse(CapturedCategory));
            bad += Compare("F_CAREER_PACKAGE_INFO (package 1)", BuildPackage(1, 6200, 810, 15100, 1861, "Aethyric Pandemonium"), Parse(CapturedPackageOne));

            if (bad != 0)
            {
                Console.Error.WriteLine(bad + " packet(s) do not match the live capture.");
                return 1;
            }

            Console.WriteLine("PASS: tome tactic advance packets match the live 1.4.8 capture byte for byte.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    // Mirrors AbilityInterface.SendTomeTacticAdvances exactly. Keep the two in step.
    private static byte[] BuildCategory(int count)
    {
        PacketOut cat = new PacketOut(0xEE, 128);
        cat.WriteByte(16);
        cat.WriteByte(1);
        cat.WriteByte(0);
        cat.WriteByte(0);
        cat.WriteByte(0);
        cat.Fill(0, 3);
        cat.WriteUInt32(0xE290);
        cat.WriteByte(0);
        cat.WriteByte(1);
        cat.WriteByte(0x4F);
        cat.WriteByte(0xF0);
        cat.WritePascalString("Tome Tactic CC A");
        cat.WriteByte(0);
        cat.WriteByte((byte)count);
        cat.WriteByte(0);
        for (int i = 1; i <= count; i++)
        {
            cat.WriteByte((byte)i);
            cat.WriteByte(0);
        }
        cat.Fill(0, 2);
        return Payload(cat);
    }

    private static byte[] BuildPackage(byte index, ushort tok, ushort advance, ushort ability, ushort effect, string name)
    {
        PacketOut pkg = new PacketOut(0xF3, 128);
        pkg.WriteByte(16);
        pkg.WriteByte(1);
        pkg.WriteByte(0);
        pkg.WriteByte(index);
        pkg.Fill(0, 10);
        pkg.WriteUInt16(tok);
        pkg.Fill(0, 8);
        pkg.WriteByte(1);
        pkg.WriteByte(0);
        pkg.WriteByte(0);
        pkg.WriteUInt16(advance);
        pkg.WriteByte(2);
        pkg.Fill(0, 2);
        pkg.WriteUInt16(ability);
        pkg.WriteByte(0);
        pkg.WriteByte(1);
        pkg.WriteUInt16(effect);
        pkg.WriteByte(2);
        pkg.Fill(0, 7);
        pkg.WriteByte(1);
        pkg.WriteByte(2);
        pkg.Fill(0, 4);
        pkg.WritePascalString(name);
        pkg.WriteByte(0);
        pkg.Fill(0, 4);
        return Payload(pkg);
    }

    /// <summary>PacketOut's own buffer minus its frame header, so we compare payloads.</summary>
    private static byte[] Payload(PacketOut packet)
    {
        packet.WritePacketLength();
        byte[] all = packet.ToArray();
        byte[] payload = new byte[all.Length - 3];
        Array.Copy(all, 3, payload, 0, payload.Length);
        return payload;
    }

    private static byte[] Parse(string hex)
    {
        string[] parts = hex.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        byte[] bytes = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            bytes[i] = Convert.ToByte(parts[i], 16);
        return bytes;
    }

    private static int Compare(string what, byte[] ours, byte[] live)
    {
        if (ours.Length == live.Length)
        {
            bool same = true;
            for (int i = 0; i < ours.Length; i++)
                if (ours[i] != live[i]) { same = false; break; }

            if (same)
            {
                Console.WriteLine("  " + what + ": matches (" + ours.Length + " payload bytes)");
                return 0;
            }
        }

        Console.WriteLine("  " + what + ": MISMATCH  ours=" + ours.Length + " live=" + live.Length);
        int max = Math.Max(ours.Length, live.Length);
        int shown = 0;
        for (int i = 0; i < max && shown < 24; i++)
        {
            string a = i < ours.Length ? ours[i].ToString("X2") : "--";
            string b = i < live.Length ? live[i].ToString("X2") : "--";
            if (a != b) { Console.WriteLine("    +" + i + ": ours=" + a + " live=" + b); shown++; }
        }

        Console.WriteLine("    ours: " + Dump(ours));
        Console.WriteLine("    live: " + Dump(live));
        return 1;
    }

    private static string Dump(byte[] b)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < b.Length; i++) { if (i > 0) sb.Append(' '); sb.Append(b[i].ToString("X2")); }
        return sb.ToString();
    }
}
