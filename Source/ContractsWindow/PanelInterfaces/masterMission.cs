using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractsWindow.PanelInterfaces {
    class masterMission : contractMission {
        private const string masterMissionTitle = "MasterMission";

        internal masterMission() : base(masterMissionTitle) {
            _masterMission = true;
        }

        internal masterMission(string active, string hidden, string vessels, bool asc, bool showActive, int sMode) : base(masterMissionTitle, active, hidden, vessels, asc, showActive, sMode, true) {
        }
    }
}
