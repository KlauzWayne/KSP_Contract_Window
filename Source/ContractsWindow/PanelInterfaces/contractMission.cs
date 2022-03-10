#region license
/*The MIT License (MIT)
Contract Mission - Object to hold info about a mission list

Copyright (c) 2014 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using ContractParser;
using ContractsWindow.Unity.Unity;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.PanelInterfaces {
    /// <summary>
    /// A list of contracts; each with its own sort order and separate lists for active and hidden contracts
    /// A special master mission list is used to store all contracts and is used as the source for all other missions
    /// </summary>
    public class ContractMission : IMissionSection {
        private bool _isVisible;
        private string _missionTitle;
        private string activeString;
        private string hiddenString;
        private string vesselIDString = "";
        private readonly Dictionary<Guid, Vessel> currentVessels = new Dictionary<Guid, Vessel>();
        private readonly Dictionary<Guid, ContractUIObject> contractList = new Dictionary<Guid, ContractUIObject>();
        private readonly List<Guid> activeMissionList = new List<Guid>();
        private readonly List<Guid> hiddenMissionList = new List<Guid>();
        private bool ascendingOrder = true;
        private bool showActiveMissions = true;
        private ContractSortClass orderMode = ContractSortClass.Difficulty;
        internal bool _masterMission = false;
        private CW_MissionSection UIParent;

        public string ContractNumber {
            get {
                return contractList.Count.ToString();
            }
        }

        public bool IsVisible {
            get {
                return _isVisible;
            }
            set {
                _isVisible = value;
            }
        }

        public bool MasterMission {
            get {
                return _masterMission;
            }
            set {
                _masterMission = value;
            }
        }

        public bool ShowHidden {
            get {
                return !showActiveMissions;
            }
            set {
                showActiveMissions = !value;

                if (ContractWindow.Instance == null)
                    return;

                ContractWindow.Instance.switchLists(value);

                ContractWindow.Instance.RefreshContracts();
            }
        }

        public bool DescendingOrder {
            get {
                return !ascendingOrder;
            }
            set {
                ascendingOrder = !value;

                if (ContractWindow.Instance == null)
                    return;

                ContractWindow.Instance.RefreshContracts();
            }
        }

        public string MissionTitle {
            get {
                if (_masterMission)
                    return "All Contracts";

                return _missionTitle;
            }
            set {
                if (_masterMission)
                    return;

                string old = _missionTitle;

                ContractScenario.Instance.removeMissionList(old);

                _missionTitle = value;

                if (!ContractScenario.Instance.addMissionList(_missionTitle))
                    _missionTitle = old;
            }
        }

        public void AddContract(IContractSection contract) {
            if (UIParent != null)
                UIParent.AddContract(contract);

            addContract(((ContractUIObject)contract).Container, !contract.IsHidden, false);
        }

        public bool ContractContained(Guid contract) {
            return contractList.ContainsKey(contract);
        }

        public IList<IContractSection> GetContracts {
            get {
                return new List<IContractSection>(contractList.Values.ToArray());
            }
        }

        public void SetSort(int i) {
            orderMode = (ContractSortClass)i;

            if (ContractWindow.Instance == null)
                return;

            ContractWindow.Instance.RefreshContracts();
        }

        public void RemoveContract(IContractSection contract) {
            if (UIParent != null)
                UIParent.RemoveContract(contract.ID);

            removeContract(((ContractUIObject)contract).Container);
        }

        public void RemoveMission() {
            ContractScenario.Instance.removeMissionList(_missionTitle);

            ContractScenario.Instance.setCurrentMission(ContractScenario.Instance.MasterMission.MissionTitle);
        }

        public void SetMission() {
            ContractScenario.Instance.setCurrentMission(_missionTitle);
        }

        public void SetParent(CW_MissionSection m) {
            UIParent = m;
        }

        public string Name {
            get {
                if (_masterMission)
                    return "All Contracts";

                return _missionTitle;
            }
            internal set {
                _missionTitle = value;
            }
        }

        public string InternalName {
            get {
                return _missionTitle;
            }
        }

        public string VesselIDs {
            get {
                return vesselIDString;
            }
        }

        public int ActiveContracts {
            get {
                return contractList.Count;
            }
        }

        public List<Guid> ActiveMissionList {
            get {
                return activeMissionList;
            }
        }

        public List<Guid> HiddenMissionList {
            get {
                return hiddenMissionList;
            }
        }

        public bool AscendingOrder {
            get {
                return ascendingOrder;
            }
            internal set {
                ascendingOrder = value;
            }
        }

        public bool ShowActiveMissions {
            get {
                return showActiveMissions;
            }
            internal set {
                showActiveMissions = value;
            }
        }

        public ContractSortClass OrderMode {
            get {
                return orderMode;
            }
            internal set {
                orderMode = value;
            }
        }

        public bool ContainsVessel(Vessel v) {
            return v != null && currentVessels.ContainsKey(v.id);
        }

        public void RefreshContract(Guid id) {
            IContractSection contract = getContract(id);

            if (contract != null)
                RefreshContract(contract);
        }

        public void RefreshContract(IContractSection contract) {
            if (contract != null)
                UIParent?.RefreshContract(contract);
        }

        internal ContractMission(string n, string active, string hidden, string vessels, bool asc, bool showActive, int sMode) {
            _missionTitle = n;
            activeString = active;
            hiddenString = hidden;
            vesselIDString = vessels;
            ascendingOrder = asc;
            showActiveMissions = showActive;
            orderMode = (ContractSortClass)sMode;
        }

        internal ContractMission(string n) {
            _missionTitle = n;
        }

        internal ContractUIObject getContract(Guid id) {
            ContractUIObject c = contractList[id];

            if (c?.Container != null)
                return c;

            return null;
        }

        internal void buildMissionList() {
            contractList.Clear();
            activeMissionList.Clear();
            hiddenMissionList.Clear();
            buildMissionList(activeString, true);
            buildMissionList(hiddenString, false);
            buildVesselList(vesselIDString);
            ContractUtils.LogFormatted("Processing Mission: {0}\nActive Contracts: {1} - Hidden Contracts: {2}", _missionTitle, activeMissionList.Count, hiddenMissionList.Count);
        }

        private void buildMissionList(string s, bool Active) {
            if (!string.IsNullOrEmpty(s)) {
                string[] sA = s.Split(',');

                foreach (string part in sA) {
                    try {
                        string[] sB = part.Split('|');
                        Guid g = new Guid(sB[0]);

                        if (g != null) {
                            contractContainer c = contractParser.getActiveContract(g);

                            if (c != null) {
                                addContract(c, Active, true);

                                ContractUIObject cUI = getContract(g);

                                if (cUI != null) {
                                    cUI.SetHidden(!Active);
                                    cUI.Order = stringIntParse(sB[1]);
                                    cUI.ShowParams = stringBoolParse(sB[2]);
                                }
                            }
                        }
                    } catch (Exception e) {
                        ContractUtils.LogFormatted("Guid invalid: {0}", e);
                    }
                }
            }
        }

        internal List<Guid> loadPinnedContracts(List<Guid> gID) {
            List<ContractUIObject> pinned = gID.Select(id => getContract(id)).Where(contract => contract?.Order != null).ToList();
            List<Guid> pinnedIds = new List<Guid>();

            if (pinned.Count > 0) {
                pinned.Sort((a, b) => {
                    return Comparer<int?>.Default.Compare(a.Order, b.Order);
                });
                pinnedIds = pinned.Select(x => x.Container.Root.ContractGuid).ToList();
            }

            return pinnedIds;
        }

        private bool stringBoolParse(string source) {
            if (bool.TryParse(source, out bool b))
                return b;
            return true;
        }

        private int? stringIntParse(string s) {
            if (int.TryParse(s, out int i))
                return i;
            return null;
        }

        internal void addContract(contractContainer c, bool active, bool warn, bool addToUI = false) {
            if (!activeMissionList.Contains(c.Root.ContractGuid) && !hiddenMissionList.Contains(c.Root.ContractGuid)) {
                if (addToMasterList(c, addToUI)) {
                    if (active)
                        activeMissionList.Add(c.Root.ContractGuid);
                    else
                        hiddenMissionList.Add(c.Root.ContractGuid);
                }
            } else if (warn)
                ContractUtils.LogFormatted("Mission List Already Contains Contract: {0}", c.Title);
        }

        private bool addToMasterList(contractContainer c, bool add) {
            if (!contractList.ContainsKey(c.Root.ContractGuid)) {
                ContractUIObject cUI = new ContractUIObject(c, this);

                contractList.Add(c.Root.ContractGuid, cUI);

                if (add)
                    UIParent?.AddContract(cUI);

                return true;
            } else
                ContractUtils.LogFormatted("Master Mission List For: [{0}] Already Contains Contract: [{1}]", _missionTitle, c.Title);

            return false;
        }

        internal void removeContract(contractContainer c) {
            if (ContractScenario.ListRemove(activeMissionList, c.Root.ContractGuid) || ContractScenario.ListRemove(hiddenMissionList, c.Root.ContractGuid))
                removeFromMasterList(c);
        }

        private void removeFromMasterList(contractContainer c) {
            if (contractList.ContainsKey(c.Root.ContractGuid))
                contractList.Remove(c.Root.ContractGuid);
        }

        private void addToVessels(Vessel v, bool warn = true) {
            if (!currentVessels.ContainsKey(v.id))
                currentVessels.Add(v.id, v);
            else if (warn)
                ContractUtils.LogFormatted("Mission [{0}] Vessel List Already Contains A Vessel With ID: [{1}] And Title [{2}]", _missionTitle, v.id, v.vesselName);
        }

        private void removeFromVessels(Vessel v, bool warn = true) {
            if (currentVessels.ContainsKey(v.id))
                currentVessels.Remove(v.id);
            else if (warn)
                ContractUtils.LogFormatted("Mission [{0}] Vessel List Does Not Contain A Vessel With ID: [{1}] And Title [{2}]", _missionTitle, v.id, v.vesselName);
        }

        internal string stringConcat(List<Guid> source) {
            if (source.Count == 0)
                return "";

            List<string> s = new List<string>();

            foreach (Guid guid in source) {
                ContractUIObject c = getContract(guid);

                if (c == null)
                    continue;

                string i = c.Order?.ToString() ?? "N";

                string id = string.Format("{0}|{1}|{2}", guid, i, c.ShowParams);

                s.Add(id);
            }

            return string.Join(",", s.ToArray());
        }

        internal string vesselConcat(ContractMission m) {
            if (m == null || !HighLogic.LoadedSceneIsFlight)
                return vesselIDString;

            Vessel vessel = FlightGlobals.ActiveVessel;

            if (vessel == null)
                return vesselIDString;

            bool withVessel = ContainsVessel(vessel);
            bool currentMission = m == this;

            if (withVessel && !currentMission)
                removeFromVessels(vessel, false);
            else if (!withVessel && currentMission)
                addToVessels(vessel, false);

            List<Vessel> source = currentVessels.Values.ToList();

            return vesselConcat(source);
        }

        private string vesselConcat(List<Vessel> vesselList) {
            if (vesselList.Count <= 0)
                return "";

            string[] vesselIdList = vesselList.Select(vessel => vessel?.id.ToString()).ToArray();

            return string.Join(",", vesselIdList);
        }

        private void buildVesselList(string s) {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (string.IsNullOrEmpty(s))
                return;

            string[] a = s.Split(',');

            foreach (string part in a) {
                try {
                    Guid g = new Guid(part);
                    Vessel v = FlightGlobals.Vessels.FirstOrDefault(V => V.id == g);

                    if (v != null)
                        addToVessels(v);
                } catch (Exception e) {
                    ContractUtils.LogFormatted("Guid invalid: {0} for mission [{1}]", e, _missionTitle);
                }
            }

            List<Vessel> source = currentVessels.Values.ToList();

            vesselIDString = vesselConcat(source);
        }
    }
}
