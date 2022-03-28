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
        [ScoutingProperty(0, "Team_Number")]
        public string TeamNumber { get; set; }

        [ScoutingProperty(1, "Alliance_Colour")]
        public string AllianceColour { get; set; }

        [ScoutingProperty(2, "Match_Number")]
        public string MatchNumber { get; set; }

        [ScoutingProperty(3, "Scout_Name")]
        public string ScoutName { get; set; }

        [ScoutingProperty(4, "Crossed_Auto_Line")]
        public string CrossedAutoLine { get; set; }

        [ScoutingProperty(5, "Auto_Balls")]
        public string AutoBalls { get; set; }

        [ScoutingProperty(6, "Auto_Balls_Shot")]
        public string AutoBallsShot { get; set; }

        [ScoutingProperty(7, "Far_Balls")]
        public string FarBalls { get; set; }

        [ScoutingProperty(8, "Far_Balls_Shot")]
        public string FarBallsShot { get; set; }

        [ScoutingProperty(9, "Tarmac_Balls")]
        public string TarmacBalls { get; set; }

        [ScoutingProperty(10, "Tarmac_Balls_Shot")]
        public string TarmacBallsShot { get; set; }

        [ScoutingProperty(11, "Close_Balls")]
        public string CloseBalls { get; set; }

        [ScoutingProperty(12, "Close_Balls_Shot")]
        public string CloseBallsShot { get; set; }

        [ScoutingProperty(13, "Protected_Zone_Balls")]
        public string ProtectedZoneBalls { get; set; }

        [ScoutingProperty(14, "Protected_Zone_Balls_Shot")]
        public string ProtectedZoneBallsShot { get; set; }

        [ScoutingProperty(15, "Active_Defence_Time")]
        public string ActiveDefenceTime { get; set; }

        [ScoutingProperty(16, "Defence_Time")]
        public string DefenceTime { get; set; }

        [ScoutingProperty(17, "Defended_Teams")]
        public string DefendedTeams { get; set; }

        [ScoutingProperty(18, "Climb_Time")]
        public string Climb_Time { get; set; }

        [ScoutingProperty(19, "Rung_Level")]
        public string RungLevel { get; set; }

        [ScoutingProperty(20, "Foul_Count")]
        public string FoulCount { get; set; }
    }
}
