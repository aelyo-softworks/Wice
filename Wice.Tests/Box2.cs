﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectN;

namespace Wice.Tests
{
    public class Box2 : RenderBox
    {
        protected override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);
            var rc = RelativeRenderRect;
            rc = rc.Deflate(2);
            Application.Trace(this + " rc:" + rc);
            context.DeviceContext.Object.DrawRectangle(ref rc, context.CreateSolidColorBrush(_D3DCOLORVALUE.Black).Object, 5, null);
        }
    }
}
