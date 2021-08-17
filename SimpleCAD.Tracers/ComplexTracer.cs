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
    public class ComplexTracer : LeeTracer
    {
        public ComplexTracer(TracingScheme scheme) : base(scheme)
        {
        }

        protected List<Cell> BuildedWire = new();
        public override TracingScheme TraceStep()
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
                InProcessWires = new(Scheme.Wires);
                return Scheme;
            }

            if (CurrentState == TracingStateEnum.PathBuildFinished)
            {
                BuildedWire.Clear();
                Path.Clear();
                NewPartPath.Clear();
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
        protected override void MakeStepWavePropagation()
        {
            if (CurrentWire == null)
            {
                WavePropagationIndex = 0;
                CurrentWire = InProcessWires.First();
                Path.Add(CurrentWire[0]);
                BuildedWire.Add(CurrentWire[0]);
                CurrentWire[0].StuffElement = new ColorStuffElement() { Color = Color.FromArgb(150, Color.GreenYellow) };
                CurrentWire
                    .Where(x => CurrentWire.IndexOf(x) != 0).ToList()
                    .ForEach(x => x.StuffElement = new ColorStuffElement() { Color = Color.FromArgb(150, Color.IndianRed) });
                Front = CurrentWire.First().Neightbors.Where(c => c != null).ToList();
                return;
            }

            if (Front.Count == 0)
            {
                CurrentState = TracingStateEnum.CannotTraceWire;
                return;
            }
            if (CurrentWire
                .Where(c => !BuildedWire.Contains(c))
                .Any(c => Front.Contains(c)))
            {
                LastPathCell = CurrentWire
                    .Where(c => !BuildedWire.Contains(c))
                    .Where(c => Front.Contains(c)).FirstOrDefault();

                BuildedWire.Add(LastPathCell);
                CurrentState = TracingStateEnum.PathBuilding;
                return;
            }
            Front = Front.Where(c => c.IsFree).ToList();

            Front.Where(c => c.StuffElement == null).ToList().ForEach(c => c.StuffElement = new NumberElement()
            { Number = NormalizedWavePropagationIndex });

            WavePropagationIndex++;
            Front = Front.SelectMany(
                c => c.Neightbors.Where(c => c != null)).Distinct().ToList();

        }
        protected override void MakeStepPathBuilding()
        {
            WavePropagationIndex--;
            NewPartPath.Add(LastPathCell);
            Cell newPathCell;
            if (Path.Any(c => LastPathCell.Neightbors.Contains(c)))
            {
                newPathCell = Path.Where(c => LastPathCell.Neightbors.Contains(c)).FirstOrDefault();

                if (BuildedWire.Count != CurrentWire.Count)
                {
                    WavePropagationIndex = 0;
                    Path.AddRange(NewPartPath);
                    Scheme.ClearStuff();
                    NewPartPath.Clear();

                    Path.ForEach(
                        c => c.StuffElement = new ColorStuffElement()
                        { Color = Color.FromArgb(150, Color.GreenYellow) });
                    CurrentWire
                        .Where(x => !BuildedWire.Contains(x)).ToList()
                        .ForEach(x => x.StuffElement = new ColorStuffElement() { Color = Color.FromArgb(150, Color.IndianRed) });

                    CurrentState = TracingStateEnum.WavePropagation;
                    Front = Path.SelectMany(c => c.Neightbors.Where(c => c != null)).Distinct().ToList();
                }
                else
                {
                    CurrentState = TracingStateEnum.PathBuildFinished;
                }
            }
            else
            {
                var possiblePathCells = LastPathCell.Neightbors.Where(
                    c => c?.StuffElement != null && (c.StuffElement as NumberElement)?.Number == NormalizedWavePropagationIndex).ToList();
                newPathCell = possiblePathCells.First();
            }

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
        protected List<Cell> Path = new();
        protected List<Cell> NewPartPath = new();

    }
    public static class ComplexTracerExtension
    {
        public static ComplexTracer Complex(this TracersCatalog catalog, TracingScheme scheme)
        {
            return new ComplexTracer(scheme);
        }
    }
}
