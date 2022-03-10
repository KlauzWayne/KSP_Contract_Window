#region license
/*The MIT License (MIT)
Contract Scenario - Scenario Module To Store Save Specific Info

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Contracts;
using ContractsWindow.Toolbar;
using ContractsWindow.PanelInterfaces;
using ContractParser;

namespace ContractsWindow {
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames
        , GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class ContractScenario : ScenarioModule {
        #region Scenario Setup

        public static ContractScenario Instance {
            get {
                return instance;
            }
        }

        private static ContractScenario instance;

        //Primary mission storage
        private DictionaryValueList<string, ContractMission> missionList = new DictionaryValueList<string, ContractMission>();

        //A specific contractMission is assigned to hold all contracts; contracts can't be removed from this
        private ContractMission masterMission = new MasterMission();

        //The currently active mission
        private ContractMission currentMission;

        public ContractMission MasterMission {
            get {
                return masterMission;
            }
        }

        //initialize data for each gamescene
        internal bool[] windowVisible = new bool[4];
        internal Rect[] windowRects = new Rect[4] { new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300) };
        private int[] windowPos = new int[16] { 50, -80, 250, 300, 50, -80, 250, 300, 50, -80, 250, 300, 50, -80, 250, 300 };

        internal contractStockToolbar appLauncherButton = null;
        internal contractToolbar blizzyToolbarButton = null;

        private bool _loaded;

        private string infoVersion;

        public string InfoVersion {
            get {
                return infoVersion;
            }
        }

        public bool Loaded {
            get {
                return _loaded;
            }
        }

        //Convert all of our saved strings into the appropriate arrays for each game scene
        public override void OnLoad(ConfigNode node) {
            instance = this;

            try {
                ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
                if(scenes != null) {
                    ContractUtils.LogFormatted("Loading Contracts Window + Data");
                    windowPos = ContractUtils.stringSplit(scenes.GetValue("WindowPosition"));
                    windowVisible = ContractUtils.stringSplitBool(scenes.GetValue("WindowVisible"));
                    int[] winPos = new int[4]
                    {
                        windowPos[4 * ContractUtils.currentScene(HighLogic.LoadedScene)]
                        , windowPos[(4 * ContractUtils.currentScene(HighLogic.LoadedScene)) + 1]
                        , windowPos[(4 * ContractUtils.currentScene(HighLogic.LoadedScene)) + 2]
                        , windowPos[(4 * ContractUtils.currentScene(HighLogic.LoadedScene)) + 3]
                    };

                    //All saved contract missions are loaded here
                    //Each mission has a separate contract list
                    foreach(ConfigNode m in scenes.GetNodes("Contracts_Window_Mission")) {
                        if(m == null)
                            continue;

                        string name;
                        string activeString = "";
                        string hiddenString = "";
                        string vesselString = "";
                        bool master = false;

                        if(!m.HasValue("MissionName"))
                            continue;

                        name = m.GetValue("MissionName");

                        ContractUtils.LogFormatted("Loading Contract Mission: {0}", name);

                        if(name == "MasterMission")
                            master = true;

                        if(m.HasValue("ActiveListID"))
                            activeString = m.GetValue("ActiveListID");
                        if(m.HasValue("HiddenListID"))
                            hiddenString = m.GetValue("HiddenListID");
                        if(m.HasValue("VesselIDs"))
                            vesselString = m.GetValue("VesselIDs");

                        if (!bool.TryParse(m.GetValue("AscendingSort"), out bool ascending))
                            ascending = true;
                        if (!bool.TryParse(m.GetValue("ShowActiveList"), out bool showActive))
                            showActive = true;
                        if (!int.TryParse(m.GetValue("SortMode"), out int sortMode))
                            sortMode = 0;

                        ContractMission mission;

                        if(master) {
                            mission = new MasterMission(activeString, hiddenString, vesselString, ascending, showActive, sortMode);
                            masterMission = mission;
                        }
                        else {
                            mission = new ContractMission(name, activeString, hiddenString, vesselString, ascending, showActive, sortMode);
                        }

                        if(!missionList.Contains(name))
                            missionList.Add(name, mission);
                    }

                    loadWindow(winPos);
                }
            }
            catch(Exception e) {
                ContractUtils.LogFormatted("Contracts Window Settings Cannot Be Loaded: {0}", e);
            }

            _loaded = true;
        }

        public override void OnSave(ConfigNode node) {
            try {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.Save();

                saveWindow(windowRects[ContractUtils.currentScene(HighLogic.LoadedScene)]);

                ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");

                //Scene settings
                scenes.AddValue("WindowPosition", ContractUtils.stringConcat(windowPos));
                scenes.AddValue("WindowVisible", ContractUtils.stringConcat(windowVisible));

                for(int i = missionList.Count - 1; i >= 0; i--) {
                    ContractMission m = missionList.At(i);

                    if(m == null)
                        continue;

                    ConfigNode missionNode = new ConfigNode("Contracts_Window_Mission");

                    missionNode.AddValue("MissionName", m.InternalName);
                    missionNode.AddValue("ActiveListID", m.stringConcat(m.ActiveMissionList));
                    missionNode.AddValue("HiddenListID", m.stringConcat(m.HiddenMissionList));
                    missionNode.AddValue("VesselIDs", m.vesselConcat(currentMission));
                    missionNode.AddValue("AscendingSort", m.AscendingOrder);
                    missionNode.AddValue("ShowActiveList", m.ShowActiveMissions);
                    missionNode.AddValue("SortMode", (int) m.OrderMode);

                    scenes.AddNode(missionNode);
                }

                node.AddNode(scenes);
            }
            catch(Exception e) {
                ContractUtils.LogFormatted("Contracts Window Settings Cannot Be Saved: {0}", e);
            }
        }

        private void Start() {
            Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
            var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            infoVersion = ainfoV?.InformationalVersion ?? "";

            bool stockToolbar = true;

            if(ContractLoader.Settings != null)
                stockToolbar = ContractLoader.Settings.useStockToolbar;

            if(stockToolbar || !ToolbarManager.ToolbarAvailable || ContractLoader.Settings.replaceStockApp) {
                appLauncherButton = gameObject.AddComponent<contractStockToolbar>();

                if(blizzyToolbarButton != null) {
                    Destroy(blizzyToolbarButton);
                    blizzyToolbarButton = null;
                }
            }
            else if(ToolbarManager.ToolbarAvailable && !stockToolbar) {
                blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();

                if(appLauncherButton != null) {
                    Destroy(appLauncherButton);
                    appLauncherButton = null;
                }
            }

            contractParser.onParameterAdded.Add(onParameterAdded);
        }

        //Remove our contract window object
        private void OnDestroy() {
            contractParser.onParameterAdded.Remove(onParameterAdded);

            if(appLauncherButton != null)
                Destroy(appLauncherButton);

            if(blizzyToolbarButton != null)
                Destroy(blizzyToolbarButton);
        }

        #endregion

        #region contract Events

        private void onParameterAdded(Contract c, ContractParameter cP) {
            contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

            if(cc == null)
                return;

            var missions = getMissionsContaining(cc.ID);

            for(int i = missions.Count - 1; i >= 0; i--) {
                ContractMission m = missions[i];

                if(m == null)
                    continue;

                ContractUIObject cUI = m.getContract(cc.ID);

                if(cUI == null)
                    continue;

                cUI.AddParameter();
            }
        }

        //Used by external assemblies to update parameter values for the UI
        internal void paramChanged(Type t) {
            foreach(contractContainer cC in contractParser.getActiveContracts) {
                cC.updateParameterInfo(t);
            }
        }

        //Used by external assemblies to update contract values for the UI
        internal void contractChanged(Type t) {
            foreach(contractContainer cC in contractParser.getActiveContracts) {
                if(cC.Root.GetType() == t)
                    cC.updateContractInfo();
            }
        }

        #endregion

        #region internal methods

        internal void toggleToolbars() {
            bool stockToolbar = true;

            if(ContractLoader.Settings != null)
                stockToolbar = ContractLoader.Settings.useStockToolbar;

            if(stockToolbar || !ToolbarManager.ToolbarAvailable) {
                if(blizzyToolbarButton != null) {
                    Destroy(blizzyToolbarButton);
                    blizzyToolbarButton = null;
                }

                if(appLauncherButton == null)
                    appLauncherButton = gameObject.AddComponent<contractStockToolbar>();
            }
            else if(ToolbarManager.ToolbarAvailable && !stockToolbar) {
                if(appLauncherButton != null) {
                    Destroy(appLauncherButton);
                    appLauncherButton = null;
                }

                if(blizzyToolbarButton == null)
                    blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();
            }
        }

        internal bool addMissionList(string name) {
            if(!missionList.Contains(name)) {
                ContractMission mission = new ContractMission(name);
                missionList.Add(name, mission);
                return true;
            }
            else
                ContractUtils.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

            return false;
        }

        internal bool addMissionList(ContractMission mission) {
            if(!missionList.Contains(mission.MissionTitle)) {
                missionList.Add(mission.MissionTitle, mission);
                return true;
            }
            else
                ContractUtils.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

            return false;
        }

        //Used to add the master contract mission list; usually when something has gone wrong
        internal void addFullMissionList() {
            string masterMissionTitle = "MasterMission";

            if(missionList.Contains(masterMissionTitle))
                removeMissionList(masterMissionTitle);

            if(addMissionList(masterMissionTitle)) {
                missionList[masterMissionTitle].MasterMission = true;
                addAllContractsToMaster();
                masterMission = missionList[masterMissionTitle];
            }
        }

        //Adds all contracts to the master mission
        private void addAllContractsToMaster() {
            ContractMission Master = missionList.Values.First(x => x?.MasterMission == true);

            if(Master != null) {
                List<contractContainer> active = contractParser.getActiveContracts;
                foreach(contractContainer c in active.Where(x=>x!=null)) {
                    Master.addContract(c, true, true);
                }
            }
        }

        internal void removeMissionList(string name)
        {
            if (missionList.Contains(name))
            {
                missionList.Remove(name);
            }
            else
                ContractUtils.LogFormatted("No Mission List Of Name: [{0}] Found", name);
        }

        internal void resetMissionsList() {
            missionList.Clear();
        }

        internal ContractMission getMissionList(string name, bool warn = false) {
            if(missionList.Contains(name))
                return missionList[name];
            else if(warn)
                ContractUtils.LogFormatted("No Mission Of Name [{0}] Found In Primary Mission List", name);

            return null;
        }

        internal void setCurrentMission(string s) {
            ContractMission m = getMissionList(s, true);

            if(m != null)
                currentMission = m;
            else
                currentMission = masterMission;


            ContractWindow.Instance.setMission(currentMission);
        }

        internal ContractMission setLoadedMission(Vessel v) {
            if(v == null)
                return masterMission;

            for(int i = 0; i < missionList.Count; i++) {
                ContractMission m = missionList.At(i);

                if(m == null)
                    continue;

                if(m.ContainsVessel(v))
                    return m;
            }

            return masterMission;
        }

        internal List<ContractMission> getMissionsContaining(Guid id) {
            return missionList.Values.Where(m => m.ContractContained(id)).ToList();
        }

        //Returns an ordered list of missions for the main window; the master mission is always first
        internal List<ContractMission> getAllMissions() {
            List<ContractMission> mList = new List<ContractMission>();
            List<ContractMission> tempList = new List<ContractMission>();

            for(int i = 0; i < missionList.Count; i++) {
                ContractMission m = missionList.At(i);

                if(m == null)
                    continue;

                if(m.MasterMission)
                    mList.Add(m);
                else
                    tempList.Add(m);
            }

            if(mList.Count == 0) {
                if(addMissionList("MasterMission"))
                    mList.Add(getMissionList("MasterMission"));
            }

            tempList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(false, a.ActiveContracts.CompareTo(b.ActiveContracts), a.MissionTitle.CompareTo(b.MissionTitle)));

            if(tempList.Count > 0)
                mList.AddRange(tempList);

            return mList;
        }

        //Initializes all missions that were added during the loading process
        internal void loadAllMissionLists() {
            if(missionList.Count <= 0) {
                ContractUtils.LogFormatted("No Mission Lists Detected; Regenerating Master List");
                addFullMissionList();
            }
            else {
                for(int i = 0; i < missionList.Count; i++) {
                    ContractMission m = missionList.At(i);

                    if(m == null)
                        continue;

                    if(m.MasterMission) {
                        m.buildMissionList();

                        List<contractContainer> active = contractParser.getActiveContracts;

                        for(int j = 0; j < active.Count; j++) {
                            contractContainer c = active[j];

                            if(c == null)
                                continue;

                            m.addContract(c, true, false);
                        }

                        masterMission = m;
                    }
                    else
                        m.buildMissionList();
                }
            }
        }

        internal static bool ListRemove(List<Guid> list, Guid id) {
            if(list.Contains(id)) {
                list.Remove(id);
                return true;
            }

            return false;
        }

        #endregion

        #region save/load methods

        //Save and load the window rectangle position
        private void saveWindow(Rect source) {
            int i = ContractUtils.currentScene(HighLogic.LoadedScene);
            windowPos[i * 4] = (int) source.x;
            windowPos[(i * 4) + 1] = (int) source.y;
            windowPos[(i * 4) + 2] = (int) source.width;
            windowPos[(i * 4) + 3] = (int) source.height;
        }

        private void loadWindow(int[] window) {
            int i = ContractUtils.currentScene(HighLogic.LoadedScene);
            windowRects[i] = new Rect(window[0], window[1], window[2], window[3]);
        }

        #endregion

    }
}
