using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

public static class DictionaryHelper
{

    public static Dictionary<(int startIndex, int endIndex), string> ToLines(string originalContent)
    {

        string[] linesArray = originalContent.Split(Environment.NewLine);
        Dictionary<(int startIndex, int endIndex), string> lines = new();
        for (int i = 0, currentLineIndex = 0; i < linesArray.Length; currentLineIndex += linesArray[i].Length - 1, ++i)
        {
            int startIndex = currentLineIndex;
            int endIndex = (linesArray[i].Length == 0) ? currentLineIndex : currentLineIndex + linesArray[i].Length - 1;
            lines[(startIndex, endIndex)] = linesArray[i];
            if (i < linesArray.Length - 1)
            {
                currentLineIndex += Environment.NewLine.Length + 1;
            }
            //asd\r\n
            //def
        }
        return lines;
    }
    public static (int startIndex, int endIndex) GetStartAndEndIndexOfLine(int index, Dictionary<(int startIndex, int endIndex), string> lines)
    {
        //Duyệt nhị phân (lower_bound) tìm range của index, giảm time complexity xuống O(log n)
        int left = 0;
        int right = lines.Keys.Count() - 1;
        int mid = (left + right) / 2;

        while (left < right)
        {
            if (lines.Keys.ElementAt(mid).endIndex >= index)
            {
                right = mid;
            }
            else if (lines.Keys.ElementAt(mid).endIndex < index)
            {
                left = mid + 1;
            }
            mid = (left + right) / 2;
        }

        return (lines.Keys.ElementAt(mid).startIndex, lines.Keys.ElementAt(mid).endIndex);
    }
    public static int GetLineIndex(int index, Dictionary<(int startIndex, int endIndex), string> lines)
    {
        //Duyệt nhị phân (lower_bound) tìm range của index, giảm time complexity xuống O(log n)
        int left = 0;
        int right = lines.Keys.Count() - 1;
        int mid = (left + right) / 2;
        while (left < right)
        {
            if (lines.Keys.ElementAt(mid).endIndex >= index)
            {
                right = mid;
            }
            else if (lines.Keys.ElementAt(mid).endIndex < index)
            {
                left = mid + 1;
            }
            mid = (left + right) / 2;
        }

        return mid;
    }
    /// <summary>
    /// Lấy scope của VnrControl hiện tại
    /// <para> [div] FieldTitle </para>
    ///   <para> VnrLabel</para>
    /// <para>[/div] </para>
    /// <para >[div] FieldValue </para>
    ///  <para>   new BuilderInfo{properties...} </para>
    ///   <para>  VnrControl</para>
    /// <para>[/div] </para>
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentVnrControlScope(int controlIndex, string[] lines, Dictionary<(int startIndex, int endIndex), string> linesDictionary, int maxLevel = 2)
    {
        int index = GetLineIndex(controlIndex, linesDictionary);
        //Từ control ta sẽ tìm tối đa 2 parent div 
        Stack<int> s = new Stack<int>();
        int up = index;
        int down = index;
        int curLevel = 0;
        string openingDivPattern = @"<div\b[^>]*>";
        string closingDivPattern = @"</div\b[^>]*>";

        //Duyệt chiều đi lên, gặp </div> thì push vào stack
        //Khi Gặp <div> =>
        //TH1: Đỉnh stack rỗng hoặc đỉnh stack là 1 hoặc nhiều <div> => + 1 level
        //TH2: Đỉnh stack là </div> thì pop nó ra ngoài => Quét xong 1 block
        //Khi gặp </div> => 
        //Đưa nó vào đỉnh stack
        while (curLevel < maxLevel && up >= 0)
        {
            --up;
            if (up >= 0 && Regex.Match(lines[up], closingDivPattern).Success)
            {
                s.Push(0);
            }
            if (up >= 0 && Regex.Match(lines[up], openingDivPattern).Success)
            {
                if (s.Count == 0 || s.Peek() == 1)
                {
                    s.Push(1);
                    ++curLevel;
                }
                else if (s.Count != 0 && s.Peek() == 0)
                {
                    s.TryPop(out int p);
                }
            }
        }
        curLevel = 0;
        while (curLevel < maxLevel && down < lines.Length)
        {
            ++down;
            if (down < lines.Length && Regex.Match(lines[down], openingDivPattern).Success)
            {
                s.Push(0);
            }
            if (down < lines.Length && Regex.Match(lines[down], closingDivPattern).Success)
            {
                if (s.Count == 0 || s.Peek() == 1)
                {
                    s.Push(1);
                    ++curLevel;

                }
                else if (s.Count != 0 && s.Peek() == 0)
                {
                    s.TryPop(out int p);
                }
            }
        }
        string result = string.Empty;
        // Console.ForegroundColor = ConsoleColor.Green;
        // Console.WriteLine("===================================================================================");

        // Console.ForegroundColor = ConsoleColor.DarkGreen;
        // Console.WriteLine("Line: {0} -> {1}", up, down);

        for (int i = Math.Max(up, 0); i <= Math.Min(down, lines.Length - 1); ++i)
        {
            result += lines[i];
            if (i != down)
            {
                result += Environment.NewLine;
            }
        }
        // Console.ForegroundColor = ConsoleColor.Yellow;
        // Console.WriteLine(result);

        return result;

    }
}