using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MLLE
{
    class PlusPropertyList
    {
        private bool teamTriggerBlue = false;
        public bool TeamTriggerOnForBlue
        {
            get { return teamTriggerBlue; }
            set { teamTriggerBlue = value; }
        }
    }
}
