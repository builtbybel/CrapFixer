using System;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// A simple logger class to log messages to a RichTextBox.
/// </summary>
public static class Logger
{
    /// <summary>
    /// The RichTextBox control to which log messages are written.
    /// </summary>
    public static RichTextBox OutputBox;

    private static readonly Font DefaultFont = new Font("Tahoma", 8.25f, FontStyle.Regular);

    /// <summary>
    /// Writes a message with an optional log level and custom font.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="level">The log level (e.g., Info, Warning, Error).</param>
    /// <param name="customFont">An optional font for the message.</param>
    public static void Log(string message, LogLevel level = LogLevel.Info, Font customFont = null)
    {
        if (OutputBox == null) return;

        if (OutputBox.InvokeRequired)
        {
            OutputBox.Invoke(new Action(() => LogInternal(message, level, customFont)));
        }
        else
        {
            LogInternal(message, level, customFont);
        }
    }

    /// <summary>
    /// Internal method to append text to the RichTextBox with formatting.
    /// </summary>
    private static void LogInternal(string message, LogLevel level, Font customFont = null)
    {
        message = CFixer.Helpers.Localization.TranslateRuntimeText(message);

        // string prefix = $"[{DateTime.Now:HH:mm:ss}] [{level}] ";
        string fullMessage = message + Environment.NewLine;

        // Set color based on log level
        Color color;
        switch (level)
        {
            case LogLevel.Warning:
                color = Color.OrangeRed;
                break;

            case LogLevel.Error:
                color = Color.Red;
                break;

            case LogLevel.Custom:
                color = Color.Magenta; // for plugins and other custom messages
                break;

            default:
                color = Color.Black;
                break;
        }

        // Append formatted message
        OutputBox.SelectionStart = OutputBox.TextLength;
        OutputBox.SelectionLength = 0;
        OutputBox.SelectionColor = color;
        OutputBox.SelectionFont = customFont ?? DefaultFont; // use custom font if provided, otherwise use default
        OutputBox.AppendText(fullMessage);

        // Reset selection to default
        OutputBox.SelectionColor = OutputBox.ForeColor; // reset color
        OutputBox.SelectionFont = DefaultFont; // reset font
        OutputBox.ScrollToCaret();   // scroll to the end
    }

    /// <summary>
    /// Clears the log output.
    /// </summary>
    public static void Clear()
    {
        if (OutputBox == null || OutputBox.IsDisposed)
            return;

        if (OutputBox.InvokeRequired)
        {
            OutputBox.Invoke(new Action(Clear));
            return;
        }

        OutputBox.Clear();
        OutputBox.SelectionColor = Color.Black;
    }
}

/// <summary>
/// Log level types used to define message severity.
/// </summary>
public enum LogLevel
{
    Info,
    Warning,
    Error,
    Custom
}
