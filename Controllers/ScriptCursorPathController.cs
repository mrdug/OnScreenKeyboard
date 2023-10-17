using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ScriptCursorPathController : ControllerBase
{
    private static string KeyboardString = "ABCDEF\nGHIJKL\nMNOPQR\nSTUVWX\nYZ1234\n567890";
    /*  ABCDEF
        GHIJKL
        MNOPQR
        STUVWX
        YZ1234
        567890 */
    
    private static char[][]? myKeyboardLayout = CreateKeyboardLayout(KeyboardString);
    private static char[][] KeyboardLayout = myKeyboardLayout;

    private static int CurrentRow;
    private static int CurrentCol;

    [HttpPost]
    public async Task<IActionResult> Whatever(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is missing or empty.");

        try
        {
            // Little test: Display myKeyboardLayout
            //DisplayKeyboardLayout(KeyboardLayout);

            var AllPaths = new StringBuilder();
            using var reader = new StreamReader(file.OpenReadStream());

            string? InputLine;
            int lineCount = 0;

            while ((InputLine = await reader.ReadLineAsync()) != null)
            {
                ResetCurrentPosition();
                lineCount++;
                var pathForLine = ProcessInputLine(InputLine);
                AllPaths.AppendLine(pathForLine);
            }

            return Ok($"{AllPaths}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private void ResetCurrentPosition()
    {
        CurrentCol = 0;
        CurrentRow = 0;
    }

    private string ProcessInputLine(string inputLine)
    {
        var pathForLine = new StringBuilder();
        foreach (char character in inputLine)
        {
            pathForLine.Append(PathToChar(character));
        }
        RemoveTrailingComma(pathForLine);
        return pathForLine.ToString();
    }

    private void RemoveTrailingComma(StringBuilder path)
    {
        if (path.Length > 0 && path[path.Length - 1] == ',')
        {
            path.Length--;
        }
    }

    private string PathToChar(char character)
    {
        int RowDiff;
        int ColDiff;
        for (int row = 0; row < KeyboardLayout.Length; row++)
        {
            for (int col = 0; col < KeyboardLayout[row].Length; col++)
            {
                if (KeyboardLayout[row][col] == char.ToUpper(character))
                {
                    RowDiff = row - CurrentRow;
                    ColDiff = col - CurrentCol;
                    CurrentRow = row;
                    CurrentCol = col;

                    var upOrDown = GetRepeatedDirection("U", "D", RowDiff);
                    var leftOrRight = GetRepeatedDirection("L", "R", ColDiff);

                    var path = new StringBuilder();

                    if (upOrDown.Length > 0)
                    {
                        path.Append(upOrDown);
                        path.Append(",");
                    }

                    if (leftOrRight.Length > 0)
                    {
                        path.Append(leftOrRight);
                        path.Append(",");
                    }

                    path.Append("#,");
                    return path.ToString();
                }
                else if (character == ' ')
                {
                    return "S,";
                }
            }
        }
        return character + ",";  //This should never be reached
    }

    private string GetRepeatedDirection(string negativeDirection, string positiveDirection, int diff)
    {
        if (diff == 0)
            return string.Empty;

        string direction = diff > 0 ? positiveDirection : negativeDirection;
        return string.Join(",", Enumerable.Repeat(direction, Math.Abs(diff)));
    }

    private static char[][] CreateKeyboardLayout(string keyboardString)
    {
        string[] keyboardLines = keyboardString.Split('\n');
        char[][] layout = new char[keyboardLines.Length][];

        for (int i = 0; i < keyboardLines.Length; i++)
        {
            layout[i] = keyboardLines[i].ToCharArray();
        }

        return layout;
    }

    // Method to display the keyboard layout
    private static void DisplayKeyboardLayout(char[][] layout)
    {
        foreach (var row in layout)
        {
            Console.WriteLine(new string(row));
        }
    }
}
