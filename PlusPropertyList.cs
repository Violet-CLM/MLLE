using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MLLE
{
    public struct PlusPropertyList
    {

        [DescriptionAttribute("If False, the Team Trigger will be Off for Blue and On for Red.\nIf True, the Team Trigger will be On for Blue and Off for Red."),
        CategoryAttribute("Triggers")]
        public bool TeamTriggerOnForBlue { get; set; }

        private static void TryToSetTrigger(ref uint internalField, string value)
        {
            uint tryParse;
            if (uint.TryParse(value, out tryParse) && tryParse <= 31)
                internalField = tryParse + 1;
            else
                internalField = 0;
        }
        private static string GetStringRepresentationOfTrigger(uint internalField)
        {
            return (internalField > 0) ?
                (internalField - 1).ToString() :
                "None"
            ;
        }
        private uint teamTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will base its status on each player's team."),
        CategoryAttribute("Triggers")]
        public string TeamTrigger
        {
            get { return GetStringRepresentationOfTrigger(teamTrigger); }
            set { TryToSetTrigger(ref teamTrigger, value); }
        }

        private uint serverTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will be On for the server and Off for all clients."),
        CategoryAttribute("Triggers")]
        public string ServerTrigger
        {
            get { return GetStringRepresentationOfTrigger(serverTrigger); }
            set { TryToSetTrigger(ref serverTrigger, value); }
        }

        private uint overtimeTrigger;
        [DescriptionAttribute("If an integer from 0-31, that trigger will turn On at the beginning of Overtime."),
        CategoryAttribute("Triggers")]
        public string OvertimeTrigger
        {
            get { return GetStringRepresentationOfTrigger(overtimeTrigger); }
            set { TryToSetTrigger(ref overtimeTrigger, value); }
        }


        public enum PitStyleEnum
        {
            FallForever = 0, InstantDeathPit = 255, StandOnPlatform = 1
        }
        [DescriptionAttribute("What should happen to players who fall to the bottom of the level?"),
        CategoryAttribute("Miscellaneous")]
        public PitStyleEnum PitStyle { get; set; }

        public class TriggerIDListConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == typeof(System.String) && value is TriggerIDList)
                {
                    var triggers = ((TriggerIDList)value).Triggers;

                    string result = "";
                    for (uint i = 0; i < 32; ++i)
                    {
                        if (triggers[i])
                        {
                            if (result.Length > 0)
                                result += ",";
                            result += i.ToString();
                        }
                    }
                    if (result.Length > 0)
                        return result;
                    return "None";
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [TypeConverterAttribute(typeof(TriggerIDListConverter))]
        public class TriggerIDList
        {
            public bool[] Triggers = new bool[32];
            public bool Trigger00 { get { return Triggers[0]; } set { Triggers[0] = value; } }
            public bool Trigger01 { get { return Triggers[1]; } set { Triggers[1] = value; } }
            public bool Trigger02 { get { return Triggers[2]; } set { Triggers[2] = value; } }
            public bool Trigger03 { get { return Triggers[3]; } set { Triggers[3] = value; } }
            public bool Trigger04 { get { return Triggers[4]; } set { Triggers[4] = value; } }
            public bool Trigger05 { get { return Triggers[5]; } set { Triggers[5] = value; } }
            public bool Trigger06 { get { return Triggers[6]; } set { Triggers[6] = value; } }
            public bool Trigger07 { get { return Triggers[7]; } set { Triggers[7] = value; } }
            public bool Trigger08 { get { return Triggers[8]; } set { Triggers[8] = value; } }
            public bool Trigger09 { get { return Triggers[9]; } set { Triggers[9] = value; } }
            public bool Trigger10 { get { return Triggers[10]; } set { Triggers[10] = value; } }
            public bool Trigger11 { get { return Triggers[11]; } set { Triggers[11] = value; } }
            public bool Trigger12 { get { return Triggers[12]; } set { Triggers[12] = value; } }
            public bool Trigger13 { get { return Triggers[13]; } set { Triggers[13] = value; } }
            public bool Trigger14 { get { return Triggers[14]; } set { Triggers[14] = value; } }
            public bool Trigger15 { get { return Triggers[15]; } set { Triggers[15] = value; } }
            public bool Trigger16 { get { return Triggers[16]; } set { Triggers[16] = value; } }
            public bool Trigger17 { get { return Triggers[17]; } set { Triggers[17] = value; } }
            public bool Trigger18 { get { return Triggers[18]; } set { Triggers[18] = value; } }
            public bool Trigger19 { get { return Triggers[19]; } set { Triggers[19] = value; } }
            public bool Trigger20 { get { return Triggers[20]; } set { Triggers[20] = value; } }
            public bool Trigger21 { get { return Triggers[21]; } set { Triggers[21] = value; } }
            public bool Trigger22 { get { return Triggers[22]; } set { Triggers[22] = value; } }
            public bool Trigger23 { get { return Triggers[23]; } set { Triggers[23] = value; } }
            public bool Trigger24 { get { return Triggers[24]; } set { Triggers[24] = value; } }
            public bool Trigger25 { get { return Triggers[25]; } set { Triggers[25] = value; } }
            public bool Trigger26 { get { return Triggers[26]; } set { Triggers[26] = value; } }
            public bool Trigger27 { get { return Triggers[27]; } set { Triggers[27] = value; } }
            public bool Trigger28 { get { return Triggers[28]; } set { Triggers[28] = value; } }
            public bool Trigger29 { get { return Triggers[29]; } set { Triggers[29] = value; } }
            public bool Trigger30 { get { return Triggers[30]; } set { Triggers[30] = value; } }
            public bool Trigger31 { get { return Triggers[31]; } set { Triggers[31] = value; } }

            public TriggerIDList(TriggerIDList other) {
                if (other == null)
                    return;
                for (uint i = 0; i < 32; ++i)
                    Triggers[i] = other.Triggers[i];
            }
        }
        private TriggerIDList startOffTriggers;
        [DescriptionAttribute("These Triggers will always start out as Off for clients who join servers, regardless of their current status for the host."),
        CategoryAttribute("Triggers")]
        public TriggerIDList StartOffTriggers { get { return startOffTriggers; } set { startOffTriggers = value; } }

        public PlusPropertyList(PlusPropertyList? other) : this()
        {
            if (other.HasValue)
            {
                this = other.Value;

                startOffTriggers = new TriggerIDList(other.Value.startOffTriggers);
            }
        }
    }
}
