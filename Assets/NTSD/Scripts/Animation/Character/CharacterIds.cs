namespace NTSD.Animation
{
    /// <summary>
    /// 角色ID常量（对应 NTSD data.txt 中的 object id）
    ///
    /// 来源：J:\QQFile\NTSD 2.4.1 工具人亲测能玩\data\data.txt
    /// C++ release 中存在特判分支的角色：ID=6(Hsasori), ID=7(RockLee), ID=8(Chiyo), ID=11(Sasuke)
    /// </summary>
    public static class CharacterIds
    {
        // 主角色（ID 1-25）
        public const int Sakura      = 1;
        public const int Naruto      = 2;
        public const int Kakashi     = 3;
        public const int Sai         = 4;
        public const int Shino       = 5;
        public const int Hsasori     = 6;   // C++ release 0x00414B82: cmp [ecx+6F4h], 6
        public const int RockLee     = 7;   // C++ release 0x004085AB 附近
        public const int Chiyo       = 8;   // C++ release 0x0040AC31: cmp [ecx+6F4h], 8
        public const int Itachi      = 9;
        public const int Deidara     = 10;
        public const int Sasuke      = 11;  // C++ release 0x0040AF81: cmp [ecx+6F4h], 0Bh
        public const int Kiba        = 12;
        public const int Yamato      = 13;
        public const int Kankuro     = 14;
        public const int Temari      = 15;
        public const int Gaara       = 16;
        public const int Kisame      = 17;
        public const int Neji        = 18;
        public const int Tenten      = 19;
        public const int Orochimaru  = 20;
        public const int Jiraiya     = 21;
        public const int Shikamaru   = 22;
        public const int Kabuto      = 23;
        public const int Hidan       = 24;
        public const int Kakuzu      = 25;

        // 音忍／NPC 角色（ID 30-39）
        public const int SoundNin    = 30;
        public const int SoundNin2   = 31;
        public const int Hunter      = 32;
        public const int NarutoClone = 33;
        public const int Kidomaru    = 34;
        public const int Sakon       = 35;
        public const int Tayuya      = 36;
        public const int Jiroubou    = 37;
        public const int SasukeCS2   = 38;
        public const int SandCreature = 39;

        // 大蛇丸系／其他（ID 50-58）
        public const int Pein        = 50;
        public const int Sasori      = 51;
        public const int Kyubi       = 52;
        public const int PuppetSasori = 55;
        public const int Reaper      = 56;
        public const int NcKakuzu    = 57;
        public const int DeidaraBird = 58;
    }
}
