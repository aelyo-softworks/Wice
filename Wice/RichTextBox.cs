using System;
using System.IO;
using System.Threading;
using DirectN;

namespace Wice
{
    public class RichTextBox : RenderVisual, IDisposable
    {
        private TextHost _host;
        private bool _disposedValue = false;

#if DEBUG
        public static ILogger Logger { get => TextHost.Logger; set => TextHost.Logger = value; }
#endif

        public RichTextBox(TextServicesGenerator generator = TextServicesGenerator.Default)
        {
            BackgroundColor = _D3DCOLORVALUE.Transparent;
            _host = new TextHost(generator);
            _host.TextColor = 0;
            //_host.Options |= TextHostOptions.Vertical;
            //_host.Text = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql{\f2 {\lang1036\ltrch azeaze }{\lang1036\b\ltrch ceci }{\lang1036\ltrch est en gras}\li0\ri0\sa0\sb0\fi0\ql\par}
            //}
            //}";
            //_host.RtfText = File.ReadAllText(@"d:\temp\wice.rtf");
            //_host.Text = File.ReadAllText(@"d:\temp\wice.htm");
            _host.Text = "héllo\nworld";

            //_host.HtmlText = @"<html><head><style>body{font-family:Arial,sans-serif;font-size:10pt;}</style><style>.cf0{font-family:Calibri;font-size:9.7pt;background-color:#FFFFFF;}</style></head><body><p>h&#xE9;llo</p><p>world</p></body></html>";
            //_host.HtmlText = "hello";
            //var html = @"<html><head><style>body{font-family:Arial,sans-serif;font-size:10pt;}</style><style>.cf0{font-family:Calibri;font-size:9.7pt;background-color:#FFFFFF;}</style></head><body><p>héllo</p><p>world</p></body></html>";
            //var rtf = @"{\rtf1\ansi\ansicpg1252\uc1\htmautsp\deff2{\fonttbl{\f0\fcharset0 Times New Roman;}{\f2\fcharset0 Segoe UI;}}{\colortbl\red0\green0\blue0;\red255\green255\blue255;}\loch\hich\dbch\pard\plain\ltrpar\itap0{\lang1033\fs18\f2\cf0 \cf0\ql{\f2 {\lang1036\ltrch azeaze }{\lang1036\b\ltrch ceci }{\lang1036\ltrch est en gras}\li0\ri0\sa0\sb0\fi0\ql\par}
            //}
            //}";
            //var doc = _host.Document;
            //doc.Open(new ManagedIStream("héllo" + Environment.NewLine + "😀world!"), 0, 1200);

            //_host.Text = "<html><body>this is a text</body></html>";

            //_host.Text = @"{\rtf1\ansi\deff0
            //{\colortbl;\red0\green0\blue0;\red255\green0\blue0;}
            //This line is the default color\line
            //\cf2
            //This héllo 😱 is red\line
            //\cf1
            //This line is the default color
            //}";
        }

        //protected override D2D_SIZE_F MeasureCore(D2D_SIZE_F constraint) => new D2D_SIZE_F(40, 40);

        protected override void ArrangeCore(D2D_RECT_F finalRect)
        {
            base.ArrangeCore(finalRect);
            _host.Activate(finalRect.TotagRECT());
        }

        protected internal override void RenderCore(RenderContext context)
        {
            base.RenderCore(context);
            _host.Draw(context.DeviceContext.Object, ArrangedRect.TotagRECT());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects)
                    Interlocked.Exchange(ref _host, null)?.Dispose();
                }

                // free unmanaged resources (unmanaged objects) and override finalizer
                // set large fields to null
                _disposedValue = true;
            }
        }

        ~RichTextBox() { Dispose(disposing: false); }
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }
    }
}
