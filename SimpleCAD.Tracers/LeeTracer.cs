using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SimpleCAD.Core;
using SimpleCAD.Core.Interfaces;
using SimpleCAD.Core.Types;
using Svg;

namespace SimpleCAD.Tracers
{
    public class LeeTracer : ITracer
    {
        public LeeTracer(TracingScheme scheme)
        {
            StartScheme = scheme;
            methodsDict = new()
            {
                { TracingStateEnum.WavePropagation, () => MakeStepWavePropagation() },
                { TracingStateEnum.PathBuilding, () => MakeStepPathBuilding() }
            };

        }
        public List<TracingScheme> Snapshots { get; protected set; } = new();
        public TracingScheme Result { get; protected set; }
        public TracingScheme StartScheme { get; set; }
        public TracingStateEnum CurrentState { get; protected set; } = TracingStateEnum.NotStarted;
        public TracingScheme TraceAllSteps()
        {
            while (CurrentState != TracingStateEnum.Finished && CurrentState != TracingStateEnum.CannotTraceWire)
            {
                var sch = (TracingScheme)TraceStep();//.Clone();
                Snapshots.Add(sch);
            }

            return Scheme;
        }
        public virtual TracingScheme TraceStep()
        {
            if (CurrentState == TracingStateEnum.Finished)
                throw new Exception("Tracing already finished");

            if (CurrentState == TracingStateEnum.NotStarted && StartScheme.Wires.Count != 0)
            {
                //Scheme = (TracingScheme)StartScheme.Clone();
                Scheme = StartScheme;
                Snapshots = new();
                CurrentState = TracingStateEnum.WavePropagation;
                ProcessedWires = new();
                InProcessWires = new (Scheme.Wires);
                return Scheme;
            }

            if (CurrentState == TracingStateEnum.PathBuildFinished)
            {
                InProcessWires.Remove(CurrentWire);
                ProcessedWires.Add(CurrentWire);
                Scheme.Graph.ClearStuff();
                CurrentWire = null;
                if (InProcessWires.Count == 0)
                    CurrentState = TracingStateEnum.Finished;
                else
                {
                    CurrentState = TracingStateEnum.WavePropagation;
                }
                return Scheme;
            }

            methodsDict.GetValueOrDefault(CurrentState)?.Invoke();
            return Scheme;
        }
        protected virtual void MakeStepWavePropagation()
        {
            if (CurrentWire == null)
            {
                WavePropagationIndex = 0;
                CurrentWire = InProcessWires.First();
                CurrentWire[0].StuffElement = new ColorStuffElement() { Color = Color.FromArgb(150, Color.GreenYellow) };
                CurrentWire[1].StuffElement = new ColorStuffElement() { Color = Color.FromArgb(150, Color.IndianRed) };
                Front = CurrentWire.First().Neightbors.Where(c => (c != null && c.IsFree) || c == CurrentWire[1]).ToList();
                return;
            }
            Front.Where(c => c.StuffElement == null).ToList().ForEach(c => c.StuffElement = new NumberElement()
            { Number = NormalizedWavePropagationIndex });

            if (Front.Contains(CurrentWire[1]) || CurrentWire.First().Neightbors.Contains(CurrentWire[1]))
            {
                LastPathCell = null;
                CurrentState = TracingStateEnum.PathBuilding;
                return;
            }
            if (Front.Count == 0)
            {
                CurrentState = TracingStateEnum.CannotTraceWire;
                return;
            }
            WavePropagationIndex++;
            Front = Front.SelectMany(c => c.Neightbors.Where(c => c != null && (c.IsFree || c == CurrentWire[1]))).Distinct().ToList();

        }
        protected virtual void MakeStepPathBuilding()
        {
            if (LastPathCell == null)
            {
                LastPathCell = CurrentWire[1];
            }
            WavePropagationIndex--;
            var possiblePathCells = LastPathCell.Neightbors.Where(
                c => c?.StuffElement != null && (c.StuffElement as NumberElement)?.Number == NormalizedWavePropagationIndex).ToList();

            Cell newPathCell;
            if (possiblePathCells.Contains(CurrentWire[0]) || LastPathCell.Neightbors.Contains(CurrentWire[0]))
            {
                CurrentState = TracingStateEnum.PathBuildFinished;
                newPathCell = CurrentWire[0];
            }
            else
                newPathCell = possiblePathCells.First();


            ConductorElement.ConductorSide sideForLast;
            ConductorElement.ConductorSide sideForNew;
            if (newPathCell.LeftNeightbor == LastPathCell)
            {
                sideForLast = ConductorElement.ConductorSide.Right;
                sideForNew = ConductorElement.ConductorSide.Left;
            }
            else if (newPathCell.RightNeightbor == LastPathCell)
            {
                sideForLast = ConductorElement.ConductorSide.Left;
                sideForNew = ConductorElement.ConductorSide.Right;
            }
            else if (newPathCell.TopNeightbor == LastPathCell)
            {
                sideForLast = ConductorElement.ConductorSide.Bottom;
                sideForNew = ConductorElement.ConductorSide.Top;
            }
            else if (newPathCell.BottomNeightbor == LastPathCell)
            {
                sideForLast = ConductorElement.ConductorSide.Top;
                sideForNew = ConductorElement.ConductorSide.Bottom;
            }
            else
            {
                throw new Exception("Error with sides");
            }
            if (LastPathCell.Element == null)
                LastPathCell.Element = new ConductorElement() { ConductorWires = new() { new() } };
           (LastPathCell.Element as ConductorElement)?.ConductorWires[0].Add(sideForLast);

            if (newPathCell.Element != null)
            {
                (newPathCell.Element as ConductorElement).ConductorWires[0].Add(sideForNew);
            }
            else
            {
                var condEl = new ConductorElement();
                condEl.ConductorWires.Add(new() { sideForNew });
                newPathCell.Element = condEl;
            }

            LastPathCell = newPathCell;
        }
        protected Cell LastPathCell;
        protected virtual ulong NormalizedWavePropagationIndex
        {
            get
            {
                return WavePropagationIndex + 1;
            }
        }
        protected ulong WavePropagationIndex = 0;
        protected TracingScheme Scheme { get; set; }
        protected List<Cell> CurrentWire;
        protected List<List<Cell>> InProcessWires;
        protected List<List<Cell>> ProcessedWires;
        protected List<Cell> Front;

        public enum TracingStateEnum
        {
            NotStarted,
            WavePropagation,
            PathBuilding,
            PathBuildFinished,
            CannotTraceWire,
            Finished
        }
        protected Dictionary<TracingStateEnum, Action> methodsDict;
    }
    public static class LiTracerExtension
    {
        public static LeeTracer Lee(this TracersCatalog catalog, TracingScheme scheme)
        {
            return new LeeTracer(scheme);
        }
    }
}
