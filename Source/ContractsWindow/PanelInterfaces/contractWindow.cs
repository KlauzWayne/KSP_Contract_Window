#region license
/*The MIT License (MIT)
Contract Assembly - Monobehaviour To Check For Other Addons And Their Methods

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ContractParser;
using ProgressParser;
using Contracts;
using Contracts.Parameters;
using ContractsWindow.Toolbar;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity.Unity;
using KSP.UI;
using KSP.Localization;
using UnityEngine.Rendering;

namespace ContractsWindow.PanelInterfaces {
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class ContractWindow : MonoBehaviour, ICW_Window {
        private const string controlLock = "CWInputLock";

        private bool _isVisible;
        private bool _inputLock;
        private bool windowGenerated;
        private bool windowGenerating;
        private bool positionSet;
        private bool progressLoaded, contractsLoaded;
        private int sceneInt;
        private ContractMission currentMission;
        private ProgressUIPanel progressPanel;
        private CW_Window UIWindow;
        private Rect windowPos;

        private List<Guid> cList = new List<Guid>();
        private List<Guid> pinnedList = new List<Guid>();

        private List<ContractUIObject> sortList = new List<ContractUIObject>();

        private Coroutine _repeatingWorker;

        private static ContractWindow instance;

        public static ContractWindow Instance {
            get {
                return instance;
            }
        }

        public bool TooltipsOn {
            get {
                if(ContractLoader.Settings == null)
                    return true;

                return ContractLoader.Settings.tooltips;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.tooltips = value;

                ContractLoader.ToggleTooltips(value);
            }
        }

        public bool IgnoreScale {
            get {
                if(ContractLoader.Settings == null)
                    return false;

                return ContractLoader.Settings.ignoreKSPScale;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.ignoreKSPScale = value;
            }
        }

        public bool IsVisible {
            get {
                return _isVisible;
            }
        }

        public bool PixelPerfect {
            get {
                if(ContractLoader.Settings == null)
                    return false;

                return ContractLoader.Settings.pixelPerfect;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.pixelPerfect = value;
            }
        }

        public bool LargeFont {
            get {
                if(ContractLoader.Settings == null)
                    return false;

                return ContractLoader.Settings.largeFont;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.largeFont = value;

                ContractLoader.UpdateFontSize(value ? 1 : -1);
            }
        }

        public float MasterScale {
            get {
                return GameSettings.UI_SCALE;
            }
        }

        public float Scale {
            get {
                if(ContractLoader.Settings == null)
                    return 1;

                return ContractLoader.Settings.windowScale;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.windowScale = value;
            }
        }

        public bool BlizzyAvailable {
            get {
                return ToolbarManager.ToolbarAvailable;
            }
        }

        public bool ReplaceToolbar {
            get {
                if(ContractLoader.Settings == null)
                    return false;

                return ContractLoader.Settings.replaceStockApp;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.replaceStockApp = value;

                if(value && ContractStockToolbar.Instance != null)
                    ContractStockToolbar.Instance.replaceStockApp();
            }
        }

        public bool StockToolbar {
            get {
                if(ContractLoader.Settings == null)
                    return true;

                return ContractLoader.Settings.useStockToolbar;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.useStockToolbar = value;

                ContractScenario.Instance.toggleToolbars();
            }
        }

        public bool StockUIStyle {
            get {
                if(ContractLoader.Settings == null)
                    return false;

                return ContractLoader.Settings.stockUIStyle;
            }
            set {
                if(ContractLoader.Settings != null)
                    ContractLoader.Settings.stockUIStyle = value;

                ContractLoader.ResetUIStyle();

                if(_isVisible) {
                    StartCoroutine(WaitForRebuild());
                    //if (UIWindow != null)
                    //{
                    //    UIWindow.gameObject.SetActive(false);

                    //    Destroy(UIWindow.gameObject);

                    //    contractUtils.LogFormatted("Destroying window");
                    //}

                    //windowGenerated = false;
                    //windowGenerating = false;

                    //Open();
                }
            }
        }

        public bool LockInput {
            get {
                return _inputLock;
            }
            set {
                _inputLock = value;

                if(_inputLock)
                    InputLockManager.SetControlLock(controlLock);
                else
                    InputLockManager.RemoveControlLock(controlLock);
            }
        }

        public string AllMissionTitle {
            get {
                return Localizer.Format("#autoLOC_textAllMissionTitle");
            }
        }

        public string ProgressTitle {
            get {
                return Localizer.Format("#autoLOC_textProgressTitle");
            }
        }

        public string Version {
            get {
                return ContractScenario.Instance.InfoVersion;
            }
        }

        public Canvas TooltipCanvas {
            get {
                return UIMasterController.Instance.tooltipCanvas;
            }
        }

        public IList<IMissionSection> GetMissions {
            get {
                return new List<IMissionSection>(ContractScenario.Instance.getAllMissions().ToArray());
            }
        }

        public IMissionSection GetCurrentMission {
            get {
                return currentMission;
            }
        }

        public IProgressPanel GetProgressPanel {
            get {
                return progressPanel;
            }
        }

        public IList<IContractSection> GetAllContracts {
            get {
                return ContractScenario.Instance.MasterMission.GetContracts;
            }
        }

        public Transform ContractStorageContainer {
            get {
                return transform;
            }
        }

        private void Awake() {
            if(instance != null && instance != this) {
                Destroy(gameObject);
                return;
            }

            instance = this;

            GameEvents.OnGameSettingsApplied.Add(onSettingsApplied);
            contractParser.onContractStateChange.Add(contractStateChange);
            contractParser.onContractsParsed.Add(onContractsLoaded);
            progressParser.onProgressParsed.Add(onProgressLoaded);
        }

        private void Start() {
            sceneInt = ContractUtils.currentScene(HighLogic.LoadedScene);

            ContractLoader.UpdateFontSize(LargeFont ? 1 : 0);

            StartCoroutine(waitForContentLoad());
        }

        private void OnDestroy() {
            if(instance != this)
                return;

            instance = null;

            if(UIWindow != null) {
                UIWindow.gameObject.SetActive(false);

                Destroy(UIWindow.gameObject);
            }

            if(_repeatingWorker != null) {
                StopCoroutine(_repeatingWorker);
                _repeatingWorker = null;
            }

            GameEvents.OnGameSettingsApplied.Remove(onSettingsApplied);
            contractParser.onContractStateChange.Remove(contractStateChange);
            contractParser.onContractsParsed.Remove(onContractsLoaded);
            progressParser.onProgressParsed.Remove(onProgressLoaded);
        }

        private void onSettingsApplied() {
            if(!windowGenerated || windowGenerating)
                return;

            if(UIWindow != null) {
                windowPos = ContractScenario.Instance.windowRects[sceneInt];

                UIWindow.setScale();
                UIWindow.SetPosition(windowPos);
            }
        }

        private IEnumerator RepeatingWorker(float seconds) {
            WaitForSeconds wait = new WaitForSeconds(seconds);

            yield return wait;

            while(true) {
                if(UIWindow != null) {
                    if(UIWindow.ShowingContracts) {
                        if(cList.Count > 0)
                            refreshContracts(cList, false);
                    }
                    else
                        UIWindow.RefreshProgress();
                }

                yield return wait;
            }
        }

        public void NewMission(string title, Guid id) {
            if(string.IsNullOrEmpty(title))
                return;

            if(!ContractScenario.Instance.addMissionList(title))
                return;

            ContractMission cM = ContractScenario.Instance.getMissionList(title);

            if(cM == null)
                return;

            contractContainer c = contractParser.getActiveContract(id);

            if(c == null)
                return;

            cM.addContract(c, true, true);
        }

        public void Rebuild() {
            ContractScenario.Instance.addFullMissionList();

            currentMission = ContractScenario.Instance.MasterMission;
            foreach(contractContainer contractContainer in ContractSystem.Instance.Contracts
                .Where(contract => contract?.ContractState == Contract.State.Active)
                .Select(activeContract => contractParser.getActiveContract(activeContract.ContractGuid))
                .Where(contractContainer => contractContainer != null)) {
                currentMission.addContract(contractContainer, true, true);
            }

            UIWindow.SelectMission(currentMission);
        }

        public void SetAppState(bool on) {
            if(!StockToolbar && !ReplaceToolbar)
                return;

            if(on)
                ContractStockToolbar.Instance?.Button?.SetTrue(false);
            else
                ContractStockToolbar.Instance?.Button?.SetFalse(false);
        }

        public void SetWindowPosition(Rect r) {
            windowPos = r;

            ContractScenario.Instance.windowRects[sceneInt] = windowPos;
        }

        public void setMission(ContractMission mission) {
            currentMission = mission;

            setMission();
        }

        public void RefreshContracts() {
            if(cList.Count > 0)
                refreshContracts(cList, true);
        }

        private void refreshContracts(List<Guid> list, bool sort = true) {
            List<Guid> pinnedRemoveList = new List<Guid>();

            IEnumerable<contractContainer> contracts = list.Select(id => contractParser.getActiveContract(id) ?? contractParser.getCompletedContract(id)).Where(cc => cc != null);

            foreach(contractContainer contractContainer in contracts) {
                UpdateContract(contractContainer);
            }

            list = contracts.Select(c => c.ID).ToList();
            pinnedList = pinnedList.Where(id => contractParser.getActiveContract(id) != null).ToList();

            if(sort) {
                list = sortContracts(list, currentMission.OrderMode, currentMission.DescendingOrder);
                UIWindow?.SortMissionChildren(list);
            }
            UIWindow?.UpdateMissionChildren();
        }

        private static void UpdateContract(contractContainer cC) {
            if(cC.Root.ContractState != Contract.State.Active) {
                cC.Duration = 0;
                cC.DaysToExpire = "----";
            }
            else if(cC.Root.DateDeadline <= 0) {//Update contract timers 
                cC.Duration = double.MaxValue;
                cC.DaysToExpire = "----";
            }
            else {
                cC.Duration = cC.Root.DateDeadline - Planetarium.GetUniversalTime();
                //Calculate time in day values using Kerbin or Earth days
                cC.DaysToExpire = cC.timeInDays(cC.Duration);
            }

            cC.Title = cC.Root.Title;
            cC.Notes = cC.Root.Notes;

            foreach(parameterContainer pC in cC.AllParamList) {
                pC.Title = pC.CParam.Title;
                pC.setNotes(pC.CParam.Notes);
            }
        }

        private List<Guid> sortContracts(List<Guid> list, ContractSortClass sortClass, bool dsc) {
            sortList = list.Select(id => currentMission.getContract(id)).Where(cC => cC != null && cC.Order == null).ToList(); //order != null resembles pinned contracts

            switch(sortClass) {
                case ContractSortClass.Expiration:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Duration.CompareTo(b.Container.Duration), a.Container.Title.CompareTo(b.Container.Title)));
                    break;
                case ContractSortClass.Acceptance:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.DateAccepted.CompareTo(b.Container.Root.DateAccepted), a.Container.Title.CompareTo(b.Container.Title)));
                    break;
                case ContractSortClass.Reward:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.TotalReward.CompareTo(b.Container.TotalReward), a.Container.Title.CompareTo(b.Container.Title)));

                    break;
                case ContractSortClass.Difficulty:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.Prestige.CompareTo(b.Container.Root.Prestige), a.Container.Title.CompareTo(b.Container.Title)));

                    break;
                case ContractSortClass.Planet:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.TargetPlanet.CompareTo(b.Container.TargetPlanet), a.Container.Title.CompareTo(b.Container.Title)));

                    break;
                case ContractSortClass.Type:
                    sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.GetType().Name.CompareTo(b.Container.Root.GetType().Name), a.Container.Title.CompareTo(b.Container.Title)));
                    sortList = typeSort(sortList, !dsc);
                    break;
            }

            list.Clear();

            if(pinnedList.Count > 0)
                list.AddRange(pinnedList);

            foreach(ContractUIObject c in sortList) {
                list.Add(c.ID);
            }

            return list;
        }

        private List<ContractUIObject> typeSort(List<ContractUIObject> cL, bool B) {
            List<int> position = new List<int>();
            List<ContractUIObject> altList = new List<ContractUIObject>();
            for(int i = 0; i < cL.Count; i++) {
                foreach(ContractParameter cP in cL[i].Container.Root.AllParameters) {
                    if(cP.GetType() == typeof(ReachAltitudeEnvelope)) {
                        altList.Add(cL[i]);
                        position.Add(i);
                        break;
                    }
                }
            }
            if(altList.Count > 1) {
                //TODO: replace sorcery
                altList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachAltitudeEnvelope) a.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude.CompareTo(((ReachAltitudeEnvelope) b.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude), a.Container.Title.CompareTo(b.Container.Title)));
                for(int j = 0; j < position.Count; j++) {
                    cL[position[j]] = altList[j];
                }
            }

            return cL;
        }

        public void SetPinState(Guid id) {
            pinnedList.Add(id);
        }

        public void UnPin(Guid id) {
            ContractScenario.ListRemove(pinnedList, id);
        }

        public int GetNextPin() {
            return pinnedList.Count;
        }

        private IEnumerator WaitForRebuild() {
            if(UIWindow != null) {
                UIWindow.gameObject.SetActive(false);

                Destroy(UIWindow.gameObject);
            }

            yield return null;

            UIWindow = null;

            windowGenerated = false;
            windowGenerating = false;

            positionSet = false;

            if(!windowGenerated && !windowGenerating)
                yield return StartCoroutine(GenerateWindow());

            Open();
        }

        public void Open() {
            if(!windowGenerated && !windowGenerating)
                StartCoroutine(GenerateWindow());

            StartCoroutine(WaitForOpen());
        }

        private IEnumerator WaitForOpen() {
            while(!windowGenerated)
                yield return null;

            if(UIWindow == null)
                yield break;

            if(!positionSet)
                SetPosition();

            if(_repeatingWorker != null)
                StopCoroutine(_repeatingWorker);

            _repeatingWorker = StartCoroutine(RepeatingWorker(5));

            _isVisible = true;

            UIWindow.Open();
        }

        public void Close() {
            if(UIWindow == null)
                return;

            if(_repeatingWorker != null) {
                StopCoroutine(_repeatingWorker);
                _repeatingWorker = null;
            }

            _isVisible = false;

            UIWindow.Close();
        }

        private IEnumerator GenerateWindow() {
            if(ContractLoader.WindowPrefab == null)
                yield break;

            if(UIWindow != null)
                yield break;

            windowGenerating = true;

            UIWindow = Instantiate(ContractLoader.WindowPrefab, DialogCanvasUtil.DialogCanvasRect, false).GetComponent<CW_Window>();

            yield return StartCoroutine(UIWindow.setWindow(this));

            Close();

            windowGenerated = true;
            windowGenerating = false;
        }

        private void SetPosition() {
            positionSet = true;

            windowPos = ContractScenario.Instance.windowRects[sceneInt];

            UIWindow.SetPosition(windowPos);
        }

        private void onContractsLoaded() {
            StartCoroutine(loadContracts());
        }

        private IEnumerator loadContracts() {
            while(!contractParser.Loaded)
                yield return null;

            while(ContractScenario.Instance == null || !ContractScenario.Instance.Loaded)
                yield return null;

            loadLists();

            contractsLoaded = true;
        }

        private void loadLists() {
            ContractUtils.LogFormatted("Loading All Contract Lists...");

            generateList();

            //Load ordering lists and contract settings after primary contract dictionary has been loaded

            if(currentMission != null) {
                if(currentMission.ShowActiveMissions)
                    cList = currentMission.ActiveMissionList;
                else
                    cList = currentMission.HiddenMissionList;

                pinnedList = currentMission.loadPinnedContracts(cList);
            }
        }

        private void generateList() {
            ContractScenario.Instance.loadAllMissionLists();

            if(HighLogic.LoadedSceneIsFlight)
                currentMission = ContractScenario.Instance.setLoadedMission(FlightGlobals.ActiveVessel);
            else
                currentMission = ContractScenario.Instance.MasterMission;
        }

        private void setMission() {
            if(currentMission == null)
                return;

            if(currentMission.ShowActiveMissions)
                cList = currentMission.ActiveMissionList;
            else
                cList = currentMission.HiddenMissionList;

            pinnedList = currentMission.loadPinnedContracts(cList);

            if(UIWindow != null)
                UIWindow.SelectMission(currentMission);

            refreshContracts(cList);
        }

        public void switchLists(bool showHidden) {
            if(showHidden)
                cList = currentMission.HiddenMissionList;
            else
                cList = currentMission.ActiveMissionList;
        }

        private void onProgressLoaded() {
            StartCoroutine(loadProgressNodes());
        }

        private IEnumerator loadProgressNodes() {
            while(!progressParser.Loaded)
                yield return null;

            loadProgressLists();

            progressLoaded = true;
        }

        private void loadProgressLists() {
            progressPanel = new ProgressUIPanel();
        }

        private IEnumerator waitForContentLoad() {
            while(!progressLoaded || !contractsLoaded)
                yield return null;

            //if (_firstLoad)
            //{
            //    yield return new WaitForSeconds(3);
            //    _firstLoad = false;
            //}

            if(!windowGenerated && !windowGenerating)
                yield return StartCoroutine(GenerateWindow());

            if(ContractScenario.Instance.windowVisible[sceneInt]) {
                Open();

                if(StockToolbar || ReplaceToolbar)
                    SetAppState(true);
            }
            else {
                Close();
            }
        }

        private void contractStateChange(Contract c) {
            if(c == null)
                return;

            if(c.ContractState == Contract.State.Active) {
                contractContainer cC = contractParser.getActiveContract(c.ContractGuid);

                if(cC != null && currentMission != null) {
                    currentMission.addContract(cC, true, true, true);

                    if(currentMission.ShowActiveMissions)
                        refreshContracts(cList);

                    if(!currentMission.MasterMission)
                        ContractScenario.Instance.MasterMission.addContract(cC, true, true, true);
                }
            }
            else if(c.ContractState == Contract.State.Completed) {
                contractContainer cC = contractParser.getCompletedContract(c.ContractGuid);

                if(cC != null && currentMission != null) {
                    if(currentMission.ContractContained(cC.ID))
                        currentMission.RefreshContract(cC.ID);
                }
            }
            else if(c.ContractState == Contract.State.Declined && currentMission != null) {
                if(currentMission.ContractContained(c.ContractGuid))
                    currentMission.RefreshContract(c.ContractGuid);
            }
            else if(c.ContractState == Contract.State.Cancelled && currentMission != null) {
                if(currentMission.ContractContained(c.ContractGuid))
                    currentMission.RefreshContract(c.ContractGuid);
            }
        }
    }
}
