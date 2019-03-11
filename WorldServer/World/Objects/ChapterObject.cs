using Common;

namespace WorldServer.World.Objects
{
    public class ChapterObject : Object
    {
        public Chapter_Info Info;

        public ChapterObject(Chapter_Info info)
        {
            Info = info;
            Name = Info.Name;
        }

        public override void OnLoad()
        {
            X = Info.PinX;
            Y = Info.PinY;
            Z = 16384;
            SetOffset(Info.OffX, Info.OffY);

            UpdateWorldPosition();
            IsActive = true;

            base.OnLoad();
        }

        public override void SendMeTo(Player plr)
        {
            plr.TokInterface.AddTok(Info.TokEntry);
        }
    }
}
