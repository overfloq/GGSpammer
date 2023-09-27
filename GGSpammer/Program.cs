using GGSpammer.Objects;
using GGSpammer.Pages;
using GGSpammer.Properties;
using PastelExtended;
using System.Collections.Immutable;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

if (Console.IsOutputRedirected)
{
    Console.Error.WriteLine("Redirected output is not supported. Please, run this tool without redirecting it's output.");
    return;
}

const string GUID = "eb46b40a-76ac-4231-94c2-0b9487c9e0a6";
using var mutex = new Mutex(true, $"Global/{GUID}", out bool mCreated);

PastelEx.Settings
    .Palette = ColorPalette.Color;

Console.SetWindowSize(90, 24);

#if !DEBUG
Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
#endif

//PastelEx.Clear();
using var titleToken = new CancellationTokenSource();

const string defaultTitle = "GG Spammer";
var titles = new[]{
    "Coded by overflow",
    "Extremly fast",
    "Many options and modes",
    "Education pursposes only"
}.ToImmutableArray();

var titleTask = Task.Factory.StartNew(() =>
{
    StringBuilder builder = new();
    void TypewriteTitle(string title)
    {
        foreach (var character in title)
        {
            if (titleToken.IsCancellationRequested)
                return;

            builder.Append(character);
            Console.Title = $"{defaultTitle}    {builder}_";
            Thread.Sleep(100);
        }

        for (int i = 0; i < 4 && !titleToken.IsCancellationRequested; i++)
        {
            Console.Title = $"{defaultTitle}    {builder}{(i % 2 == 1 ? '_' : default)}";
            Thread.Sleep(500);
        }

        for (int i = builder.Length - 1; i >= 0 && !titleToken.IsCancellationRequested; i--)
        {
            // Remove last char.
            builder.Remove(i, 1);
            Console.Title = $"{defaultTitle}    {builder}{(builder.Length == 0 ? default : '_')}";
            Thread.Sleep(100);
        }

        Thread.Sleep(500);
    }

    while (!titleToken.IsCancellationRequested)
    {
        foreach (var title in titles)
        {
            builder.Clear();
            TypewriteTitle(title);
        }
    }
}, TaskCreationOptions.LongRunning);

if (!mCreated)
{
    RedrawScreen();

    WriteLine("An instance of GGSpammer is already running!".Fg(SecondaryColor));

    WriteLine("Once you close your previous instance, we'll let you inside. If");
    WriteLine("there wouldn't be such a check, it could lead to problems, like");
    WriteLine("database corruption by writing from multiple processes!");

    try
    {
        mutex.WaitOne();
    }
    catch (AbandonedMutexException) { }
}

const string UserAgreementFile = "./User Agreement.txt";
string configFile = Path.Join(GetCoreDirectory().FullName, "config.json");

JsonConfig jsonConfig;

if (!File.Exists(configFile))
{
    jsonConfig = new();
    File.WriteAllText(configFile,
        JsonSerializer.Serialize(jsonConfig, JsonConfigContext.Default.JsonConfig));
}
else
{
    jsonConfig = JsonSerializer.Deserialize(
        File.ReadAllText(configFile), JsonConfigContext.Default.JsonConfig) ?? new();
}

if (jsonConfig.DisableColors)
{
    PastelEx.Settings
        .Enabled = false;
}
else
{
    PastelEx.Background = Color.Black;
    PastelEx.Foreground = Color.FromArgb(0xAAAAAA);
}

if (!jsonConfig.TosAccepted)
{
    RedrawScreen();

    if (!File.Exists(UserAgreementFile))
        File.WriteAllText(UserAgreementFile, Resources.UserAgreement);

    WriteLine($"You must agree to our {"User Agreement".Fg(SecondaryColor)} located in {"User Agreement.txt".Fg(Color.LightGray)}!");
    WriteLine("Press enter twice to agree and save your decision . . .");

    DateTime pressedTime = default;
    while (true)
    {
        while (Console.ReadKey(true).Key != ConsoleKey.Enter) { }
        if (pressedTime != default && pressedTime > DateTime.Now.AddSeconds(-0.5))
            break;

        pressedTime = DateTime.Now;
    }

    // Save decision to Yes
    jsonConfig.TosAccepted = true;
    File.WriteAllText(configFile,
        JsonSerializer.Serialize(jsonConfig, JsonConfigContext.Default.JsonConfig));
}

ShowLoadingScreen(() =>
{
    // Simulate work, like update checker.
    Thread.Sleep(500);
});

// Convert data from log-file into a database file.
Database.Checkpoint();

while (true)
{
    new ManagedPage(new MainMenuPage())
        .OpenAndWait();
}