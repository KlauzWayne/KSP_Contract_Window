using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractsWindow.PanelInterfaces {
    class MasterMission : ContractMission {
        private const string masterMissionTitle = "MasterMission";

        internal MasterMission() : base(masterMissionTitle) {
            _masterMission = true;
        }

        internal MasterMission(string active, string hidden, string vessels, bool asc, bool showActive, int sMode) : base(masterMissionTitle, active, hidden, vessels, asc, showActive, sMode) {
            _masterMission = true;
        }
    }
}
