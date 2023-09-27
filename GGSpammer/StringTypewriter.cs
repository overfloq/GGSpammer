using System.Text;

namespace GGSpammer;
internal class StringTypewriter
{
    internal static void Write(in ReadOnlySpan<char> input)
    {
        StringBuilder sb = default!;
        for (var i = 0; i < input.Length; i++)
        {
            if (input[i] == '\u001b')
            {
                if (sb == default)
                    sb = new StringBuilder();
                else
                    sb.Clear();

                for (var j = i; j < input.Length; j++)
                {
                    sb.Append(input[j]);

                    if (input[j] == 'm')
                        break;
                }

                Console.Write(sb.ToString());
                i += sb.Length - 1;

                continue;
            }

            if (i > 0)
                Thread.Sleep(20);

            Console.Write(input[i]);
        }
    }

    internal static void WriteLine(in ReadOnlySpan<char> input) => Write($"{input}{Environment.NewLine}");
}