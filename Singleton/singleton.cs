public sealed class ServerInfoHelper
    {
        #region AsiteArea_ID ------------
        /// <summary>
        /// 100001: Total, 100002: Today, 100003: Live
        /// </summary>
        private readonly string G_ID = "100001,100002,100003";
        /// <summary>
        /// SiteASmart:101111,101112
        /// SiteBSmart:102111
        /// </summary>
        private readonly string Member_SmartID = "101111,101112,102111";
        /// <summary>
        /// 101:SiteA, 102:SiteB, 103:SiteC, 104:SiteD, 105:SiteE    
        /// </summary>
        private readonly string Member_ID = "101,101888,102,103,104,105";
        #endregion -----------------------
        
        #region  BsiteArea_Name_ID -------
        ///<summary>
        ///display in this sequence
        ///</summary>
        private readonly string[,] MemberB_Name_ID = new string[,] { 
        { "Site1", "200001" },      { "Site1 Wap", "200101" },      { "Site1 Smart", "200103" },        
        { "Site2", "201000" },      { "Site2 Wap", "201101" },      { "Site2 Smart", "201103" },
        { "Site3", "203001" },      { "Site3 Wap", "202101" },      { "Site3 Smart", "202103" },         
        { "Site4", "204001" },      { "Site4 Wap", "303101" },      { "Site4 Smart", "303103" }
        };
        #endregion -----------------------

        #region ID or IP ReadOnlyCollection<int> ------
        private ReadOnlyCollection<int> G_IDList;
        private ReadOnlyCollection<int> Member_SmartIDList;
        private ReadOnlyCollection<int> Member_IDList;
        private ReadOnlyCollection<int> MemberB_IDList;
        private ReadOnlyCollection<int> AllSiteServerIDList;
        private readonly Dictionary<string, int> MemberB_Dic; 
        #endregion  -------------------------------------
        
        private ServerInfoHelper()
        {
            List<int> _G_IDList = new List<int>();
            List<int> _Member_SmartIDList = new List<int>();
            List<int> _Member_IDList = new List<int>();
            List<int> _MemberB_IDList = new List<int>();
            List<int> _AllSiteServerIDList = new List<int>();
            Dictionary<string, int> _MemberB_Dic = new Dictionary<string, int>(); 

            //Initial _G_ID List.
            string[] _tmp = G_ID.Split(',');
            for (int i = 0; i < _tmp.Length; i++)
            {
                _G_IDList.Add(Convert.ToInt32(_tmp[i]));
            }
            G_IDList = new ReadOnlyCollection<int>(_G_IDList);

            //Initial Member_SmartID List.
            _tmp = Member_SmartID.Split(',');
            for (int i = 0; i < _tmp.Length; i++)
            {
                _Member_SmartIDList.Add(Convert.ToInt32(_tmp[i]));
            }
            Member_SmartIDList = new ReadOnlyCollection<int>(_Member_SmartIDList);

            //Initial Member_ID List.
            _tmp = Member_ID.Split(',');
            for (int i = 0; i < _tmp.Length; i++)
            {
                _Member_IDList.Add(Convert.ToInt32(_tmp[i]));
            }
            _Member_IDList.AddRange(_Member_SmartIDList);
            Member_IDList = new ReadOnlyCollection<int>(_Member_IDList);

            //Initial MemberB List.
            for (int i = 0; i < MemberB_Name_ID.GetLength(0); i++) 
            {
                string Name = MemberB_Name_ID[i, 0];
                int S_ID = Convert.ToInt32(MemberB_Name_ID[i, 1]);

                if (!_MemberB_Dic.ContainsKey(SeverName))
                {
                    _MemberB_Dic.Add(Name, S_ID);
                    _MemberB_IDList.Add(S_ID);  
                }
            }
            MemberB_Dic = new Dictionary<string, int>();
            MemberB_Dic = _MemberB_Dic;
            MemberB_IDList = new ReadOnlyCollection<int>(_MemberB_IDList);

            //Initial AllSiteServerID List.
            _AllSiteServerIDList.AddRange(_Member_IDList);
            _AllSiteServerIDList.AddRange(_MemberB_IDList);
            AllSiteServerIDList = new ReadOnlyCollection<int>(_AllSiteServerIDList);
        }

        public static ServerInfoHelper Instance
        {
            get { return ServerInfoHelperNested.instance; }
        }

        private class ServerInfoHelperNested
        { 
            static ServerInfoHelperNested()
            {}

            internal static readonly ServerInfoHelper instance = new ServerInfoHelper();
        }

        public Dictionary<string, int> GetMemberB()
        {
            return MemberB_Dic;
        }

        #region  ServerID List ----------
        public ReadOnlyCollection<int> GetAllSiteServerIDList()
        {
            return AllSiteServerIDList;
        }
        #endregion  ---------------------

        #region IsContain ServerID   ------
        public bool IsG_ID(int ID)
        {
            return G_IDList.Contains(ID);
        }

        public bool IsMember_ID(int ID)
        {
            return Member_IDList.Contains(ID);
        }

        public bool IsMember_SmartID(int ID)
        {
            return Member_SmartIDList.Contains(ID);
        }

        public bool IsMemberB_ID(int ID)
        {
            return MemberB_IDList.Contains(ID);
        }
        #endregion  ----------------------

    }

}
