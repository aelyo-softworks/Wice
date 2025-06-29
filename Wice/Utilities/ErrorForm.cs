using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace Wice.Utilities;

internal class ErrorForm : Form
{
    private readonly TableLayoutPanel _table = new();
    private readonly TableLayoutPanel _titlePanel = new();
    private readonly System.Windows.Forms.Button _quit = new();
    private readonly System.Windows.Forms.Button _details = new();

    public ErrorForm(IList<Exception> errors)
    {
        if (errors.IsEmpty())
            throw new ArgumentException(nameof(errors));

        try
        {
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
        }
        catch
        {
            // do nothing
        }

        const int margin = 10;
        const int iconSize = 64;
        MinimizeBox = false;
        MaximizeBox = false;
        Size = new System.Drawing.Size(600, iconSize + 4 * margin);
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowOnly;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        _quit.Margin = new Padding(margin - 2);
        _quit.Anchor = AnchorStyles.Right;
        _quit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        _quit.Text = "&Quit";

        _details.Margin = _quit.Margin;
        _details.Anchor = AnchorStyles.Left;
        _details.DialogResult = System.Windows.Forms.DialogResult.None;
        _details.Text = "&Details";
        _details.Click += (s, e) =>
        {
            _details.Enabled = false;
            MinimumSize = Size;
            _table.RowCount = 3;
            var errorTextBox = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9)
            };

            string errorText;
            if (errors.Count == 1)
            {
                errorText = errors[0].ToString().Trim();
            }
            else
            {
                var i = 1;
                var sb = new StringBuilder();
                foreach (var error in errors)
                {
                    if (i > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }

                    sb.AppendLine("Error #" + i);
                    i++;

                    sb.Append(error);
                }
                errorText = sb.ToString().Trim();
            }

            if (NativeWindow.IsKeyPressed(VIRTUAL_KEY.VK_SHIFT))
            {
                errorText += Environment.NewLine + Environment.NewLine + "----------" + Environment.NewLine + Environment.NewLine + DiagnosticsInformation.Serialize(Assembly.GetEntryAssembly());
            }

            var measure = TextRenderer.MeasureText(errorText, errorTextBox.Font);
            var area = Screen.FromControl(this).WorkingArea;
            var minSize = new Size();
            if ((measure.Width + 100) < area.Width)
            {
                minSize.Width = measure.Width;
            }
            else
            {
                minSize.Width = Width - 2 * margin;
            }

            if ((measure.Height + 100) < area.Height)
            {
                minSize.Height = measure.Height + SystemInformation.VerticalScrollBarArrowHeight;
            }
            else
            {
                minSize.Height = 400;
            }

            errorTextBox.MinimumSize = minSize;
            errorTextBox.AppendText(errorText);
            _table.Controls.Add(errorTextBox, 0, 2);
            _table.SetColumnSpan(errorTextBox, 2);
            _table.RowStyles[0] = new RowStyle(SizeType.AutoSize);
            _table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            NativeWindow.FromHandle(Handle).Center();
        };

        _titlePanel.Dock = DockStyle.Fill;
        _titlePanel.AutoSize = true;
        _titlePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _titlePanel.RowCount = 1;
        _titlePanel.ColumnCount = 2;

        var picture = new PictureBox
        {
            Size = new Size(iconSize, iconSize),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Image = SystemIcons.Error.ToBitmap()
        };
        _titlePanel.Controls.Add(picture, 0, 0);

        var errorLabel = new Label
        {
            Margin = new Padding(margin),
            Dock = DockStyle.Fill
        };
        var errText = " occurred in a component of your application. If you press Details, you can view more details that could help us diagnose the problem." +
            Environment.NewLine +
            "If you press Quit, the application will terminate.";
        if (errors.Count == 1)
        {
            errorLabel.Text = "An unhandled error" + errText;
        }
        else
        {
            errorLabel.Text = "Unhandled errors have " + errText;
        }

        _titlePanel.Controls.Add(errorLabel, 1, 0);

        _table.ColumnCount = 2;
        _table.RowCount = 2;
        _table.AutoSize = true;
        _table.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _table.Dock = DockStyle.Fill;
        _table.Controls.Add(_titlePanel, 0, 0);
        _table.SetColumnSpan(_titlePanel, 2);
        _table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        _table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _table.Controls.Add(_quit, 1, 1);
        _table.Controls.Add(_details, 0, 1);
        Controls.Add(_table);

        AcceptButton = _quit;
        CancelButton = _quit;
    }

#if DEBUG
    protected override void OnLoad(EventArgs e)
    {
        _details.PerformClick();
        base.OnLoad(e);
    }
#endif
}
