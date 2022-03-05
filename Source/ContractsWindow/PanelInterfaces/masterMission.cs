using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContractsWindow.PanelInterfaces {
    class masterMission : contractMission {
        private const string masterMissionTitle = "MasterMission";
        public new bool MasterMission {
            get {
                return true;
            }
            set {
            }
        }

        internal masterMission() : base(masterMissionTitle) {
        }

        internal masterMission(string active, string hidden, string vessels, bool asc, bool showActive, int sMode) : base(masterMissionTitle, active, hidden, vessels, asc, showActive, sMode, true) {
        }
    }
}
