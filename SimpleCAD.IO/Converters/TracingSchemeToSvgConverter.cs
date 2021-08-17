using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SimpleCAD.Core;
using SimpleCAD.Core.Interfaces;
using SimpleCAD.Core.Types;
using SimpleCAD.IO.Converters;
using Svg;
using Svg.Transforms;

namespace SimpleCAD.IO.Converters
{
    public class TracingSchemeToSvgConverter : ITracingSchemeConverter<SvgDocument>
    {
        public TracingScheme Scheme { get; set; }
        public TracingSchemeToSvgConverter(TracingScheme scheme)
        {
            Scheme = scheme;
        }
        public void SaveAsSvg(string path)
        {

        }
        public SvgDocument Convert()
        {
            var svg = new SvgDocument()
            {
                Height = Scheme.Graph.Height * 100,
                Width = Scheme.Graph.Width * 100,
            };
            svg.ViewBox = new SvgViewBox(0, 0, svg.Height, svg.Width);
            var group = svg;
            //svg.Children.Add(group);
            //             <defs>
            //   <pattern id="pattern1"
            //            x="0" y="0" width="100" height="100"
            //            patternUnits="userSpaceOnUse" >
            // 			<rect width="100" height="100" stroke-width="3" style="fill:none;stroke:black;" />


            //   </pattern>
            // </defs>
            //   <rect x="0" y="0" width="1000" height="1000"
            //     stroke-width="6" style="stroke: #000000; fill: url(#pattern1);" />       
            var defs = new SvgDefinitionList();

            var pattern = new SvgPatternServer()
            {
                ID = "coverPattern",
                Width = 100,
                Height = 100,
                PatternUnits = SvgCoordinateUnits.UserSpaceOnUse,
                Fill = SvgPaintServer.None,
            };
            pattern.Children.Add(SvgElementsTemplate.SvgEmptyCellElement());
            defs.Children.Add(pattern);
            group.Children.Add(new SvgRectangle()
            {
                X = 0,
                Y = 0,
                Width = svg.Width,
                Height = svg.Height,
                StrokeWidth = 6,
                Fill = pattern,
            });
            svg.Children.Add(defs);

            for (uint i = 0; i < Scheme.Graph.Height; i++)
                for (uint j = 0; j < Scheme.Graph.Width; j++)
                {
                    //var emptyCellEl = SvgElementsTemplate.SvgEmptyCellElement();

                    //emptyCellEl.CustomAttributes.Add("y", (i * 100).ToString());
                    //emptyCellEl.CustomAttributes.Add("x", (j * 100).ToString());

                    //group.Children.Add(emptyCellEl);

                    var cell = Scheme.Graph.GetCell(i, j);

                    var svgEl = GetSvgElementByCellElement(cell.Element);
                    if (svgEl != null)
                    {
                        svgEl.CustomAttributes.Add("y", (i * 100).ToString());
                        svgEl.CustomAttributes.Add("x", (j * 100).ToString());
                        group.Children.Add(svgEl);
                    }
                    svgEl = GetSvgElementByStuffElement(cell.StuffElement);
                    if (svgEl != null)
                    {
                        svgEl.CustomAttributes.Add("y", (i * 100).ToString());
                        svgEl.CustomAttributes.Add("x", (j * 100).ToString());
                        group.Children.Add(svgEl);
                    }
                }

            return svg;
        }
        protected SvgElement GetSvgElementByCellElement(Element element)
        {
            if (element is ChipElement)
            {
                return SvgElementsTemplate.SvgChipElement();
            }
            else if (element is ConductorElement conductor)
            {
                //TODO
                SvgElement svgConductor = null;
                if (conductor.ConductorWires[0].Count == 1)
                {
                    svgConductor = SvgElementsTemplate.SvgHalfConductorElement(90 * (int)conductor.ConductorWires[0][0]);
                }
                if (conductor.ConductorWires[0].Count == 2)
                {
                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Right ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Left)
                        svgConductor = SvgElementsTemplate.SvgDirectConductorElement(90);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Bottom ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Top)
                        svgConductor = SvgElementsTemplate.SvgDirectConductorElement();

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Top ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Left)
                        svgConductor = SvgElementsTemplate.SvgAngledConductorElement(180);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Right ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Top)
                        svgConductor = SvgElementsTemplate.SvgAngledConductorElement(270);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Bottom ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Right)
                        svgConductor = SvgElementsTemplate.SvgAngledConductorElement();

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Left ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][1] == ConductorElement.ConductorSide.Bottom)
                        svgConductor = SvgElementsTemplate.SvgAngledConductorElement(90);
                }

                if (conductor.ConductorWires[0].Count == 3){
                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Right ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Left)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(270);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Bottom ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Top)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(0);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Top ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Left)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(0);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Top &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Right ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Top)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(-90);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Right &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Bottom ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Right)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(180);

                    if (conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Bottom &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Left ||
                        conductor.ConductorWires[0][0] == ConductorElement.ConductorSide.Left &&
                        conductor.ConductorWires[0][2] == ConductorElement.ConductorSide.Bottom)
                        svgConductor = SvgElementsTemplate.SvgTripleConductorElement(90);
                }

                return svgConductor;

            }
            else return null;

        }
        protected SvgElement GetSvgElementByStuffElement(StuffElement element)
        {
            if (element is NumberElement numEl)
            {
                if ((int)numEl.Number > 99)
                    return SvgElementsTemplate.SvgColorRectElement(
                        Color.FromArgb((int)numEl.Number * 10 / int.MaxValue * 255,
                                       (int)numEl.Number * 10 / int.MaxValue * 255,
                                       (int)numEl.Number * 10 / int.MaxValue * 255));
                else
                    return SvgElementsTemplate.SvgTextCellElement(((int)numEl.Number).ToString());
            }
            else if (element is ColorStuffElement colEl)
            {
                return SvgElementsTemplate.SvgColorRectElement(colEl.Color);
            }
            return null;
        }
    }


    public static class SvgElementsTemplate

    {
        public static SvgElement SvgEmptyCellElement()
        {

            //var el = new SvgImage();
            //el.CustomAttributes.Add("xlink:href", "data:img/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAChklEQVR4nO2cy4rqQBRFqzQKgpOAOPFH/P+R/+HYaRAVbY5NQt1QettJu2zWAkliPZK4PY86pjvnnG+32y0Jg5xSUg0QTX8pk8kkLZfLdL1e03w+T9PpNOWc76+woNgG4/2g79PTt5f9a3OMxz8bV/b9ybl/em3jPvE5xGdQO3c5x/h6e2Ls+XxOp9Pp3h7HMeejueL4eDwO7zd9p7Zt0263S6vVKjVNUz3ZJ1G7+f/xbMwr84UI8XpEOU/XdWm73ab9fv8tSN+wXq/TZrNJi8Xio4X4NMIbhQX1DHshRLgp+X1KixkEMdNiMAhSmo28j0GFZ0FIfg8tBECZ0aoCgGpQ12UxGAT59IXgX0GXBUNBYOiyYGghAEx7YVjLAmMMgaHLgqHLAlAN6gryPqpB3WovA1WAYbUXhmkvDIM6DGMIDF0WDAWBocuCoSAAqqUT1yEMLJ3A0EIAVIuLZlkMXKnD0EJgKAgMXRYAn8uCUc2yhIGCANBlwXBhCEYLgWEtC4YuC4YuC4aCwNBlAfDpdzC6LADWsmBYOoFh6QSMK3UYPpcFw7QXhoLAMKjDUBAYZlkAqgtDs6z3YekEjFkWDAWBYZYFwxgCQwsB4O8hMPyvpGC0EBgKAsAYAsPSCRgFAeCTi2AsvwMwhoAYV0j8xfDNjEOFtSwYBnUYxhAYuiwYuiwA/jkCDIuLYHRZMHRZMLQQGFoIDKu9AMo6osVFAFVBdFnvIWJ3KUjT71wul9R13X37F4kvHDFxic+8FCTnnG9xobPZLLVt+4+l1Kymv6m+LY7Lmx3vj8eVfcbttffLtkd9a2NqfWvzvTLv+B5fOeej+UKMw+Fw30ZbtJrvgjC1IpFS+gIQZ3lQg2OEIwAAAABJRU5ErkJggg==");
            var el = new SvgRectangle()
            {
                Fill = SvgPaintServer.None,
                Stroke = new SvgColourServer(Color.Black),
                StrokeWidth = 3,
                Width = 100,
                Height = 100,
            };
            //<rect fill="none" stroke="black" stroke-width="3" width="100" height="100"/>
            return el;

        }
        
        public static SvgElement SvgTripleConductorElement(float rotateAngle = 0, Color color = default)
        {
            if (color == default)
                color = Color.Black;

            var el = new SvgFragment()
            {
                Width = 100,
                Height = 100
            };
            var line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 50,
                StartY = 0,
                EndY = 99,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 100,
                StartY = 50,
                EndY = 50,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            var circle = new SvgCircle()
            {
                Fill = new SvgColourServer(color),
                CenterX = 50,
                CenterY = 50,
                Radius = 10,
            };
            el.Children.Add(circle);          
            return el;
        }
        public static SvgElement SvgDirectConductorElement(float rotateAngle = 0, Color color = default)
        {
            if (color == default)
                color = Color.Black;

            //<line class="fil0 str0" x1="1749.7" y1="170.27" x2="1749.7" y2= "3371.75" />
            var el = new SvgFragment()
            {
                Width = 100,
                Height = 100
            };
            var line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 50,
                StartY = 0,
                EndY = 99,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            //img.CustomAttributes.Add("xlink:href", "data:img/png;base64,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");
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            return el;

        }
        public static SvgElement SvgHalfConductorElement(float rotateAngle = 0, Color color = default)
        {
            if (color == default)
                color = Color.Black;

            //<line class="fil0 str0" x1="1749.7" y1="170.27" x2="1749.7" y2= "3371.75" />
            var el = new SvgFragment()
            {
                Width = 100,
                Height = 100
            };
            var line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 50,
                StartY = 0,
                EndY = 50,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            //img.CustomAttributes.Add("xlink:href", "data:img/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAG0klEQVR4nO1dSUsrQRDucd8XFBdcEMSjNw/e/ele/AMKgngw7lvcd83jy3s19GuqOpoMM/NJPgjqdBJrprrWrq5OkiSp1Wo110Y5kDjn2twoEbqElI6ODjc0NOS+vr5cT0+P6+zsdEmS1F+QIPwEwt8BeY9Axv33a98Rfj72ufC91WrVvb+/q0+yq6vLjY2N1e+pEW0h/fgMnoH2v/3vCOkV4LOg6+3trT6Ov/Gd1nfh75eXl/R6l7xpfHzcbW1tucnJyfoNaf+sLLi4uHAbGxtuf39fpQjM2NzcdAsLCz+iWHtg3xkLASbgZcH/nqenJ7e+vu4qlcpfhsjA1NSUm5ubc/39/WXkwX84Ozur34gGPLjPz8/6AxkcHCwLySagjSBBgvQ3MAJqigF9fX3u8fFRpRQMubu7q6sBFvgSkzKEydPq7e016YVkYHINDAzkTlcWSBnii03ZAXX18fFhUgmVdX5+TnM/PlIuxIxQ2XB9fV2nSLzAEPBwWNRviP/cXhbAPYfK0tQWGASVBilhgT+peLjgATGIJQFg0vPzs3t4eCiIup9DNepMKgtxEtTSb0TKkDIHghrgRVlqViJ1RlCqLElPSIojBMZubm6KJrMpUDIEkMDPikdmZ2dzpigbUKoseFBack/+hso6PT0tiLrWQCkhYAgeegiRFqiy19fXYon8AejdXuSyXIN0D1MqiD6XhbSJFfhhtkF6VldXc6crC1DakNHR0VRKQmBigWHb29tFktg0KFUW0uuxwBBMGR4ezpWmrECpsiwvS4AxrHyyQDXqTAwRD8tKLiLPtbOzUwBlzUE16kzZXizNxnJvYMra2lquNGUFShuC9LtUxYQQo45CCEZQZnuRNunu7lbLc9y/xCP9iiGT24vEobZaKAzCEi+qaBhB62VBLVk0I0Zp57JyBJghRQ6aZItbzAhKleWX+GgPHushWFdnBCVDYCOs1ImgvWKYI1DyikIGaxKNjIxQeY0+KBkCgw231wIk6OTkpGgyvw01dcI0o1CpHzPcyDrQL+EypU5QuWhF6riGe2EqtvZBKSEw6GCIJiG4BulhclLU5CLbDcjOJA0YwyIWIygjdddglxKuX11d5U5TFqCUEMQYsa1njswm+khraRiTiyEk4YgFrHZyMUfAPmheltgWpE6sLW9lBH1d1v39vVooJ7D2jpQVqpfFBMQYiMath46drahMYQQlQ2ZmZswtz7ItemVlJXe6mgW9ysJaiLVDSron7O7u5k5Xs6APDNE4AGsiVmsLuLysbi8l1UtLS9ElXKis+fn53OnKApS5rKOjo/R3TUoY2oNYoFRZ/lY2TUrghe3t7RVAWeugVFmS0Y2NT09P50pTVqBkSMxgy1i76iRH+I3HfIjLKx2BWEBf/S4PXuvOJjfHtMfQB6XKQh7LKnIQpjDZEPpc1uLiojkmS7vtbG+OgFtrNZ+B9wXDHisTKhvoUyeXl5f1QjkLsC+szWkoI3VIgRWNy8RibWBGWZcl2xE0iFFnamDmg9LthX3Ay1KzkHb6QjkmhmDNHK9GlSeMoG1g1mgXLisoGYI1c9fA7rGm4Cm9LHFprY5yuMYUh6iBIZOXhbSIeFmhHYG7Cw+LqbUGfeokth1B1tQPDg4Koa1VpNVmbF6WVQwn12OtyMsM2up3F3FEkA1G7RYjKL0sqKtYqQ9syPHxce50ZQHaQjkrPSITq90mNmdoNkLuAS4vk4TQr4egS4OmrsSgQ3KYsr30XUkbZXIxznD+lAbaRsqxSFza/DGCtpFyLNUD6WCrohFQMgSzP7ZEi60KTIEhfeoEM8o6hQ1jyPTSB4ZMQOLQKoSTo0xvb29p7oi+chEPO5adRupkYmIiV5qyAmX63QVd5XxgtlmHFpcV9DbERSJ13BxcYpba3jBDQluXpTWfkZkGCWE5ejU0FbRHHsFOWJPoVxx5xLYeEjs4UlpsMIL22DzpHKcBkTr9ljYmlYU4BDbCkmq03Tg8PMydrixAqbLQnAz7P6zzQyBB9Ol3tiVc6+huAdP9/IpeJ7HKRNbUu2M+8shqrYFrKDVdXl4uhLZWQVvbi2Vcq4wUUXqlUimEtlZBKSFYC7HyVSIhrBXwtI0DYqcjsB2b54My24tDwbTGAe7fxMJ6CFMnBz+rQJlcBCMwgaxT2mLb3coIlSGM56lbdo9tj7rPkLT6HW4kUg4MxQHSHEDWP0LgHhCL4H4E1nuLBmj0GZIkSVITMce5HL6kWCrBH5NtyP51rVuP/zDkPeG4dt0fE+CBV6vV6BHeWMJFvBLSF35fjAaNRot+jd7v3LP0qZeEKUY5G0v9UtAu4f5KOOf+AKPaOcDBqaYlAAAAAElFTkSuQmCC");
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            var circle = new SvgCircle()
            {
                Fill = new SvgColourServer(color),
                CenterX = 50,
                CenterY = 50,
                Radius = 10,
            };
            el.Children.Add(circle);
            return el;
        }
        public static SvgElement SvgAngledConductorElement(float rotateAngle = 0, Color color = default)
        {

            if (color == default)
                color = Color.Black;

            //<line class="fil0 str0" x1="1749.7" y1="170.27" x2="1749.7" y2= "3371.75" />
            var el = new SvgFragment()
            {
                Width = 100,
                Height = 100
            };
            var line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 50,
                StartY = 45,
                EndY = 99,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            //img.CustomAttributes.Add("xlink:href", "data:img/png;base64,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");
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            line = new SvgLine()
            {
                Fill = SvgPaintServer.None,
                StartX = 50,
                EndX = 99,
                StartY = 50,
                EndY = 50,
                StrokeWidth = 10,
                Stroke = new SvgColourServer(color),
            };
            //img.CustomAttributes.Add("xlink:href", "data:img/png;base64,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");
            line.Transforms = new() { new SvgRotate(rotateAngle, 50, 50) };
            el.Children.Add(line);
            return el;

        }
        public static SvgElement SvgChipElement()
        {

            var el = new SvgFragment();
            el.Children.Add(new SvgPath());
            el.Children.Last().CustomAttributes.Add("d", "M0,27.5 L0,57.5 57.5,0 27.5,0 Z");
            el.Children.Add(new SvgPath());
            el.Children.Last().CustomAttributes.Add("d", "M0,84 L0,99 15,99 99,15 99,0 84,0 Z");
            el.Children.Add(new SvgPath());
            el.Children.Last().CustomAttributes.Add("d", "M99,71.5 L99,41.5 41.5,99 71.5,99 Z");
            return el;

        }
        public static SvgElement SvgTextCellElement(string text)
        {

            var el = new SvgFragment
            {
                Width = 100,
                Height = 100
            };
            var textEl = new SvgText();
            textEl.CustomAttributes.Add("x", "50");
            textEl.CustomAttributes.Add("y", "80");

            textEl.CustomAttributes.Add("font-family", "sans-serif");
            textEl.CustomAttributes.Add("font-weight", "550");
            textEl.CustomAttributes.Add("font-size", "80px");
            textEl.CustomAttributes.Add("letter-spacing", "-5");
            textEl.CustomAttributes.Add("text-anchor", "middle");

            textEl.Text = text;

            el.Children.Add(textEl);
            return el;
        }
        public static SvgElement SvgColorRectElement(Color color)
        {

            var el = new SvgRectangle
            {
                Width = 100,
                Height = 100,
                Fill = new SvgColourServer(color),
            };
            //el.CustomAttributes.Add("fill-opacity","1");
            return el;
        }
    }
}



namespace SimpleCAD.Core
{

    public static class TracingSchemeToSvgConverterExtensions
    {
        public static TracingSchemeToSvgConverter TracingSchemeToSvgConverter(this ConvertersCatalog catalog,
                                                                              TracingScheme scheme)
        {
            return new TracingSchemeToSvgConverter(scheme);
        }
    }
}