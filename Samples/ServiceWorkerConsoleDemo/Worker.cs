using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleCAD.Core;
using SimpleCAD.Tracers;
using SimpleCAD.Core.Types;
using System.IO;
using Svg;
using static SimpleCAD.Tracers.AkkersTracer;
using System.Drawing.Imaging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;



namespace ServiceWorkerConsoleDemo
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var inputScheme = new TracingScheme()
            {
                Graph = new(10, 10),
            };
            inputScheme.Wires = new()
            {
                new() { inputScheme.Graph.GetCell(6, 3), inputScheme.Graph.GetCell(0, 9) },
                new() { inputScheme.Graph.GetCell(9, 9), inputScheme.Graph.GetCell(2, 3) }
            };
            inputScheme.Graph.GetCell(5, 3).Element = new ChipElement();
            inputScheme.Graph.GetCell(5, 4).Element = new ChipElement();
            inputScheme.Graph.GetCell(1, 1).Element = new ChipElement();
            inputScheme.Graph.GetCell(1, 2).Element = new ChipElement();
            inputScheme.Graph.GetCell(2, 1).Element = new ChipElement();
            inputScheme.Graph.GetCell(2, 2).Element = new ChipElement();

            inputScheme.Graph.GetCell(2, 3).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Left } }
            };
            inputScheme.Graph.GetCell(6, 3).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Top } }
            };
            inputScheme.Graph.GetCell(0, 9).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Right } }
            };
            inputScheme.Graph.GetCell(9, 9).Element = new ConductorElement()
            {
                ConductorWires = new()
                { new() { ConductorElement.ConductorSide.Right } }
            };
            // inputScheme.Graph.GetCell(4, 3).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Top, ConductorElement.ConductorSide.Bottom } }
            // };
            // inputScheme.Graph.GetCell(4, 4).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Bottom, ConductorElement.ConductorSide.Top } }
            // };


            // inputScheme.Graph.GetCell(4, 6).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Left, ConductorElement.ConductorSide.Top } }
            // };
            // inputScheme.Graph.GetCell(4, 7).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Top, ConductorElement.ConductorSide.Right } }
            // };
            // inputScheme.Graph.GetCell(5, 6).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Left, ConductorElement.ConductorSide.Bottom } }
            // };
            // inputScheme.Graph.GetCell(5, 7).Element = new ConductorElement()
            // {
            //     ConductorWires = new()
            //     { new() { ConductorElement.ConductorSide.Bottom, ConductorElement.ConductorSide.Right } }
            // };


            // inputScheme.Graph.GetCell(9, 1).StuffElement = new NumberElement() { Number = 1 };
            // inputScheme.Graph.GetCell(9, 2).StuffElement = new NumberElement() { Number = 2 };
            // inputScheme.Graph.GetCell(9, 3).StuffElement = new NumberElement() { Number = 3 };
            // inputScheme.Graph.GetCell(9, 4).StuffElement = new NumberElement() { Number = 4 };
            // inputScheme.Graph.GetCell(9, 5).StuffElement = new NumberElement() { Number = 5 };
            // inputScheme.Graph.GetCell(9, 6).StuffElement = new NumberElement() { Number = 6 };


            var cad = new SimpleCADWorker();


            //var stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../ss.svg"), FileMode.Create);

            //var emptyCellEl = new SvgImage();
            //svgScheme.Children[0].Children.Add(emptyCellEl);


            var tracer = cad.Tracers.Akkers(inputScheme);
            //tracer.TraceAllSteps();

            var i = 0;
            while (tracer.CurrentState != TracingStateEnum.Finished && tracer.CurrentState
                != TracingStateEnum.CannotTraceWire)
            {
                var sch = tracer.TraceStep();//.Clone();
                var svgScheme = cad.Converters.TracingSchemeToSvgConverter(sch).Convert();
                var stream = new FileStream(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"../../../svgs/sn{i}.svg"),
                    FileMode.Create);
                svgScheme.Write(stream);
                stream.Close();

                //var x = svgScheme.Draw(
                var memStream = new MemoryStream();
                var reader = new StreamReader(memStream);
                svgScheme.Write(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                string str = reader.ReadToEnd();
                //str = reader.ReadToEnd();
                Console.WriteLine(str);
                memStream.Seek(0, SeekOrigin.Begin);

                var sampleDoc = SvgDocument.Open<SvgDocument>((Stream)memStream, null);

                // var sampleDoc = SvgDocument.Open(
                //     Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"../../../svgs/sn{i}.svg"));
                sampleDoc.Draw().Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"../../../pngs/sn{i}.png"));
                i++;
            }

            // var i = 0;
            // foreach (var s in tracer.Snapshots)
            // {
            //     var svgScheme = cad.Converters.TracingSchemeToSvgConverter(s).Convert();
            //     var stream = new FileStream(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @$"../../../sn{i}.svg"), FileMode.Create);
            //     svgScheme.Write(stream);
            //     stream.Close();
            //     i++;
            // }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
