using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLLE
{
    public struct PlusPropertyList
    {
        private bool teamTriggerBlue;
        public bool TeamTriggerOnForBlue
        {
            get { return teamTriggerBlue; }
            set { teamTriggerBlue = value; }
        }
    }
}
