using BluetoothLibrary.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowedApplication
{
    /// <summary>
    /// Scouting message used in 2022
    /// </summary>
    public class Message2022 : ScoutingMessageBase
    {
        [ScoutingProperty(0, "teamNumber")]
        public string TeamNumber { get; set; }

        [ScoutingProperty(1, "allianceColor")]
        public string AllianceColour { get; set; }

        [ScoutingProperty(2, "matchNumber")]
        public string MatchNumber { get; set; }

        [ScoutingProperty(3, "scoutName")]
        public string ScoutName { get; set; }

        [ScoutingProperty(4, "leftCommunity")]
        public string LeftCommunity { get; set; }

        [ScoutingProperty(5, "autoConesTop")]
        public string AutoConesTop { get; set; }

        [ScoutingProperty(6, "autoConesMid")]
        public string AutoConesMid { get; set; }

        [ScoutingProperty(7, "autoConesBot")]
        public string AutoConesBot { get; set; }

        [ScoutingProperty(8, "autoCubesTop")]
        public string AutoCubesTop { get; set; }

        [ScoutingProperty(9, "autoCubesMid")]
        public string AutoCubesMid { get; set; }

        [ScoutingProperty(10, "autoCubesBot")]
        public string AutoCubesBot { get; set; }

        [ScoutingProperty(11, "conesTop")]
        public string ConesTop { get; set; }

        [ScoutingProperty(12, "conesMid")]
        public string ConesMid { get; set; }

        [ScoutingProperty(13, "conesBot")]
        public string ConesBot { get; set; }

        [ScoutingProperty(14, "cubesTop")]
        public string CubesTop { get; set; }

        [ScoutingProperty(15, "cubesMid")]
        public string CubesMid { get; set; }

        [ScoutingProperty(16, "cubesBot")]
        public string CubesBot { get; set; }

        [ScoutingProperty(17, "autoDocked")]
        public string AutoDocked { get; set; }

        [ScoutingProperty(18, "autoEngaged")]
        public string AutoEngaged { get; set; }

        [ScoutingProperty(19, "docked")]
        public string Docked { get; set; }

        [ScoutingProperty(20, "engaged")]
        public string Engaged { get; set; }

        [ScoutingProperty(21, "park")]
        public string Park { get; set; }

        [ScoutingProperty(22, "endgameTime")]
        public string EndgameTime { get; set; }

        [ScoutingProperty(23, "opponentA")]
        public string OpponentA { get; set; }

        [ScoutingProperty(24, "opponentADefenceTime")]
        public string OpponentADefenceTime { get; set; }

        [ScoutingProperty(25, "opponentB")]
        public string OpponentB { get; set; }

        [ScoutingProperty(26, "OpponentBDefenceTime")]
        public string OpponentBDefenceTime { get; set; }

        [ScoutingProperty(27, "opponentC")]
        public string OpponentC { get; set; }

        [ScoutingProperty(28, "opponentCDefenceTime")]
        public string OpponentCDefenceTime { get; set; }

        [ScoutingProperty(28, "loadingTime")]
        public string loadingTime { get; set; }

        [ScoutingProperty(28, "transportTime")]
        public string transportTime { get; set; }

        [ScoutingProperty(28, "communityTime")]
        public string communityTime { get; set; }

        [ScoutingProperty(28, "startingPosition")]
        public string startingPosition { get; set; }
    }
}
