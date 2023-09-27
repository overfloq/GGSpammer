using PastelExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGSpammer.Objects;
internal class RealtimeUpdater
{
    readonly ManagedInputParameters _parameters;
    private RealtimeUpdater(ManagedInputParameters parameters)
    {
        _parameters = parameters;
    }

    public static RealtimeUpdater Init(ManagedInputParameters parameters)
    {
        PastelEx.EraseLine();
        return new(parameters);
    }

    int oldLength = 0;
    public void UpdateText(ReadOnlySpan<char> text)
    {
        lock (Lock)
        {
            Console.CursorLeft = _parameters.ConsoleOffset;
            Console.Out.Write(text);

            if (text.Length > oldLength)
            {
                // Overwrite the length
                int need = text.Length - oldLength;
                Console.Write(new string(' ', need));
            }

            oldLength = text.Length;
        }
    }
}
