namespace UIQ.ViewModels
{
    public class HomeTableViewModel
    {
        public string MemberName { get; set; }

        public string Nickname { get; set; }

        public string ModelGroup { get; set; }
        
        public string Dtg { get; set; }

        public string RunType { get; set; }

        public string Status { get; set; }

        public string Comment { get; set; }

        public int MaintainerStatus { get; set; }

        public int MemberPosition { get; set; }

        public int LId { get; set; }

        public bool AlertFlag { get; set; }

        public string TdText { get { return $"{MemberName} ({Nickname})"; } }

        public string TdGroupMember { get { return $"{MemberName}({Nickname})"; } }

        public string TdClass { get; set; }

        public int Sn { get; set; }

        public string Href { get; set; }

        public bool NewTr { get; set; }

        public bool TdEnd { get; set; }

        public bool LastMember { get; set; }
    }
}
