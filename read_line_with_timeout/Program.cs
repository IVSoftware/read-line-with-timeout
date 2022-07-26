using System.Runtime.InteropServices;

var timeout = TimeSpan.FromSeconds(5);
Console.WriteLine($"Timeout is set to {timeout}");
Console.WriteLine("Cancel: CONTROL + C");
Console.WriteLine("===================");
SetForegroundWindow(GetConsoleWindow());

while (true)
{
    Console.WriteLine();
    Console.WriteLine(DateTime.Now);
    try
    {
        Console.WriteLine(await ReadLineWithTimeout(timeout) + "(ECHO)");
    }
    catch
    {
        Console.WriteLine("Cancelled");
    }
    Console.WriteLine(DateTime.Now);
}

async Task<string> ReadLineWithTimeout(TimeSpan timeout)
{
    List<char> currentLine = new List<char>();
    var wdtStartOrRestart = DateTime.Now;

    while (true)
    {
        while (Console.KeyAvailable)
        {
            wdtStartOrRestart = DateTime.Now;
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    goto breakFromSwitch;
                case ConsoleKey.Backspace:
                    Console.Write("\b \b");
                    var removeIndex = currentLine.Count - 1;
                    if(removeIndex != -1)
                    {
                        currentLine.RemoveAt(removeIndex);
                    }
                    break;
                case ConsoleKey.LeftArrow:
                case ConsoleKey.RightArrow:
                    // Handling these is more than we're taking on right now.
                    break;
                default:
                    Console.Write(keyInfo.KeyChar);
                    currentLine.Add(keyInfo.KeyChar);
                    break;
            }
        }
        await Task.Delay(1);
        if(DateTime.Now.Subtract(wdtStartOrRestart) > timeout)
        {
            throw new TaskCanceledException();
        }
    }
    breakFromSwitch:
        return string.Join(String.Empty, currentLine);
}

// This is just for bringing the Console window to the front in VS debugger.
[DllImport("kernel32.dll", ExactSpelling = true)]
static extern IntPtr GetConsoleWindow();

[DllImport("user32.dll")]
[return: MarshalAs(UnmanagedType.Bool)]
static extern bool SetForegroundWindow(IntPtr hWnd);