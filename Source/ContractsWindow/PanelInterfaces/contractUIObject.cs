﻿#region license
/*The MIT License (MIT)
Contract UI Object - Object used for contracts in different mission lists

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
using UnityEngine;
using ContractParser;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity;

namespace ContractsWindow.PanelInterfaces {
    /// <summary>
    /// This is the object actually used by the contract window
    /// It stores data for each contract about ordering and whether its parameters are shown
    /// Different mission lists are able to store contracts in different states using this object
    /// </summary>
    public class ContractUIObject : IContractSection {
        private contractContainer container;
        private bool _showParams;
        private bool _hidden;
        private int? _pinId;
        private Texture _agencyLogo;
        private string _agencyName;
        private Guid _id;
        private int _difficulty;
        private ContractMission mission;
        private List<ParameterUIObject> paramList = new List<ParameterUIObject>();

        public ContractUIObject(contractContainer c, ContractMission m) {
            container = c;
            mission = m;
            _showParams = true;
            _pinId = null;

            _agencyLogo = container.RootAgent.Logo;
            _agencyName = container.RootAgent.Name;

            _difficulty = (int) container.Root.Prestige;
            _id = container.ID;

            for(int i = 0; i < c.FirstLevelParameterCount; i++) {
                parameterContainer p = c.getParameterLevelOne(i);

                if(p == null)
                    continue;

                if(string.IsNullOrEmpty(p.Title))
                    continue;

                paramList.Add(new ParameterUIObject(p));
            }
        }

        public void AddParameter() {
            if(container == null)
                return;

            paramList.Clear();

            for(int i = 0; i < container.FirstLevelParameterCount; i++) {
                parameterContainer pC = container.getParameterLevelOne(i);

                if(pC != null)
                    paramList.Add(new ParameterUIObject(pC));
            }

            UpdateContractUI();
        }

        private void UpdateContractUI() {
            mission?.RefreshContract(this);
        }

        public Texture AgencyLogo {
            get {
                return _agencyLogo;
            }
        }

        public string AgencyName {
            get {
                return _agencyName;
            }
        }

        public ContractState ContractState {
            get {
                if(container == null || container.Root == null)
                    return Unity.ContractState.Active;

                switch(container.Root.ContractState) {
                    case Contracts.Contract.State.Active:
                    case Contracts.Contract.State.Generated:
                    case Contracts.Contract.State.Offered:
                        return Unity.ContractState.Active;
                    case Contracts.Contract.State.Cancelled:
                    case Contracts.Contract.State.DeadlineExpired:
                    case Contracts.Contract.State.Declined:
                    case Contracts.Contract.State.Failed:
                    case Contracts.Contract.State.OfferExpired:
                    case Contracts.Contract.State.Withdrawn:
                        return Unity.ContractState.Fail;
                    case Contracts.Contract.State.Completed:
                        return Unity.ContractState.Complete;
                    default:
                        return Unity.ContractState.Fail;
                }
            }
        }

        public string ContractTitle {
            get {
                return container?.Title ?? "Null...";
            }
        }

        public Guid ID {
            get {
                return _id;
            }
        }

        public int Difficulty {
            get {
                return _difficulty;
            }
        }

        public void SetHidden(bool isHidden) {
            _hidden = isHidden;
        }

        public bool IsHidden {
            get {
                return _hidden;
            }
            set {
                _hidden = value;

                if(mission == null)
                    return;

                if(value) {
                    ContractScenario.ListRemove(mission.ActiveMissionList, _id);

                    mission.HiddenMissionList.Add(_id);

                    _showParams = false;

                    _pinId = null;
                }
                else {
                    ContractScenario.ListRemove(mission.HiddenMissionList, _id);

                    mission.ActiveMissionList.Add(_id);

                    _showParams = true;
                }
            }
        }

        public bool ShowParams {
            get {
                return _showParams;
            }
            set {
                _showParams = value;
            }
        }

        public bool IsPinned {
            get {
                return _pinId != null;
            }
            set {
                if(ContractWindow.Instance == null)
                    return;

                if(value) {
                    _pinId = ContractWindow.Instance.GetNextPin();

                    ContractWindow.Instance.SetPinState(_id);
                }
                else {
                    _pinId = null;

                    ContractWindow.Instance.UnPin(_id);
                }

                ContractWindow.Instance.RefreshContracts();
            }
        }

        public int? Order {
            get {
                return _pinId;
            }
            set {
                _pinId = value;
            }
        }

        private string coloredText(string s, string sprite, string color) {
            if(string.IsNullOrEmpty(s))
                return "";

            return string.Format("<color={0}>{1}{2}</color>  ", color, sprite, s);
        }

        public string RewardText {
            get {
                if(container == null)
                    return "";

                return string.Format("{0}{1}{2}", coloredText(container.FundsRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>", "#69D84FFF"), coloredText(container.SciRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Science\" tint=1>", "#02D8E9FF"), coloredText(container.RepRewString, "<sprite=\"CurrencySpriteAsset\" name=\"Reputation\" tint=1>", "#C9B003FF"));
            }
        }

        public string PenaltyText {
            get {
                if(container == null)
                    return "";

                return string.Format("{0}{1}", coloredText(container.FundsPenString, "<sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1>", "#FA4224FF"), coloredText(container.RepPenString, "<sprite=\"CurrencySpriteAsset\" name=\"Reputation\" tint=1>", "#FA4224FF"));
            }
        }

        public string GetNote {
            get {
                if(container == null)
                    return "";

                return container.Notes;
            }
        }

        public string TimeRemaining {
            get {
                if(container == null)
                    return "";

                return container.DaysToExpire;
            }
        }

        public int TimeState {
            get {
                if(container == null || container.Root == null)
                    return 2;

                if(container.Duration >= 2160000)
                    return 0;
                if(container.Duration > 0)
                    return 1;
                if(container.Root.ContractState == Contracts.Contract.State.Completed)
                    return 0;

                return 2;
            }
        }

        public IList<IParameterSection> GetParameters {
            get {
                return new List<IParameterSection>(paramList.ToArray());
            }
        }

        public void RemoveContractFromAll() {
            for(int i = ContractScenario.Instance.getAllMissions().Count - 1; i >= 0; i--) {
                ContractMission m = ContractScenario.Instance.getAllMissions()[i];

                if(m == null)
                    return;

                m.RemoveContract(this);
            }
        }

        public contractContainer Container {
            get {
                return container;
            }
        }
    }
}
