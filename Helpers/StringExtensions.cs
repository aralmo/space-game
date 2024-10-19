using System.Text;

public static class StringExtensions{
 public static string BreakUpLines(this string text, int fontSize, float maxWidth)
    {
        StringBuilder result = new StringBuilder();
        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            string[] words = line.Split(' ');
            StringBuilder currentLine = new StringBuilder();

            foreach (string word in words)
            {
                string testLine = currentLine.ToString() + word + " ";
                int textWidth = MeasureText(testLine, fontSize);

                if (textWidth > maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        result.AppendLine(currentLine.ToString().TrimEnd());
                        currentLine.Clear();
                    }
                }

                currentLine.Append(word + " ");
            }

            if (currentLine.Length > 0)
            {
                result.AppendLine(currentLine.ToString().TrimEnd());
            }
        }

        return new string(result.ToString().Where(c => c != '\r').ToArray()).TrimEnd();
    }
}