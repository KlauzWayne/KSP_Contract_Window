#region license
/*The MIT License (MIT)
progressUIPanel - Storage class for information about the main progress node panel

Copyright (c) 2016 DMagic

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

using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces {
    public class ProgressUIPanel : IProgressPanel {
        private bool _intervalVisible;
        private bool _poiVisible;
        private bool _standardVisible;
        private bool _bodyVisible;
        private readonly Dictionary<string, List<IStandardNode>> bodies = new Dictionary<string, List<IStandardNode>>();
        private readonly List<IIntervalNode> intervals = new List<IIntervalNode>();
        private readonly List<IStandardNode> pois = new List<IStandardNode>();
        private readonly List<IStandardNode> standards = new List<IStandardNode>();

        public ProgressUIPanel() {
            LoadIntervals(progressParser.getAllIntervalNodes);
            LoadStandards(progressParser.getAllStandardNodes);
            LoadPOIs(progressParser.getAllPOINodes);
            LoadBodies(progressParser.getAllBodyNodes);
        }

        private void LoadIntervals(List<progressInterval> nodes) {
            foreach (progressInterval progressInterval in nodes) {
                IntervalNodeUI node = new IntervalNodeUI(progressInterval);

                if (node != null)
                    intervals.Add(node);
            }
        }

        private void LoadStandards(List<progressStandard> nodes) {
            foreach (progressStandard node in nodes) {
                StandardNodeUI UInode = new StandardNodeUI(node);
                if (UInode != null)
                    standards.Add(UInode);
            }
        }

        private void LoadPOIs(List<progressStandard> nodes) {
            foreach (progressStandard node in nodes) {
                StandardNodeUI UInode = new StandardNodeUI(node);
                if (node != null)
                    pois.Add(UInode);
            }
        }

        private void LoadBodies(List<progressBodyCollection> nodes) {
            foreach (progressBodyCollection body in nodes) {
                if (body != null && !bodies.ContainsKey(body.Body.displayName.LocalizeBodyName())) {
                    List<IStandardNode> newNodes = new List<IStandardNode>();

                    foreach (progressStandard subnode in body.getAllNodes) {
                        StandardNodeUI node = new StandardNodeUI(subnode);

                        if (node != null)
                            newNodes.Add(node);
                    }

                    bodies.Add(body.Body.displayName.LocalizeBodyName(), newNodes);
                }
            }
        }

        public bool IntervalVisible {
            get { return _intervalVisible; }
            set { _intervalVisible = value; }
        }

        public bool POIVisible {
            get { return _poiVisible; }
            set { _poiVisible = value; }
        }

        public bool StandardVisible {
            get { return _standardVisible; }
            set { _standardVisible = value; }
        }

        public bool BodyVisible {
            get { return _bodyVisible; }
            set { _bodyVisible = value; }
        }

        public bool AnyInterval {
            get { return progressParser.AnyInterval; }
        }

        public bool AnyPOI {
            get { return progressParser.AnyPOI; }
        }

        public bool AnyStandard {
            get { return progressParser.AnyStandard; }
        }

        public bool AnyBody {
            get { return progressParser.AnyBody; }
        }

        public bool AnyBodyNode(string s) {
            progressBodyCollection body = progressParser.getProgressBody(s);
            return body?.IsReached ?? false;
        }

        public Dictionary<string, List<IStandardNode>> GetBodies {
            get { return bodies; }
        }

        public IList<IIntervalNode> GetIntervalNodes {
            get { return new List<IIntervalNode>(intervals.ToArray()); }
        }

        public IList<IStandardNode> GetPOINodes {
            get { return new List<IStandardNode>(pois.ToArray()); }
        }

        public IList<IStandardNode> GetStandardNodes {
            get { return new List<IStandardNode>(standards.ToArray()); }
        }
    }
}
