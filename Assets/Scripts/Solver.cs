using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;
using TMPro;

public class Solver : MonoBehaviour
{
    List<string> dictionary = new List<string>();
    public GameObject wordObject;
    TextMeshProUGUI wordText;
    int wordIndex = 0;
    Dictionary<string, List<int>> found = new Dictionary<string, List<int>>();
    List<string> words = new List<string>();
    List<List<int>> paths = new List<List<int>>();
    Game gameScript;

    void Start()
    {
        gameScript = gameObject.GetComponent<Game>();
        dictionary = gameScript.GetDictionary();
        var trie = Trie.BuildTrie(dictionary);
        var board = Board.GetBoardArray(gameScript.GetBoard());
        var solver = new Solverobj(board, trie);

        found = solver.FindWordsWithPath();
        words = new List<string>(found.Keys);
        paths = new List<List<int>>(found.Values);

        // var scores = new List<int>(found.Keys.Select(i => (int)Mathf.Pow(2, i.Length - 2)));
        // Debug.Log(score.Sum());

        wordText = wordObject.GetComponent<TMPro.TextMeshProUGUI>();
        UpdateWordAndPath();
    }

    public void NextWord()
    {
        if (wordIndex < words.Count - 1)
        {
            wordIndex++;
        }
        UpdateWordAndPath();
    }

    public void PrevWord()
    {
        if (wordIndex > 0)
        {
            wordIndex--;
        }
        UpdateWordAndPath();
    }

    bool WordGot()
    {
        List<string> playedWords = gameScript.GetPlayedWords();
        return playedWords.Contains(words[wordIndex]);
    }

    void UpdateWordAndPath()
    {
        wordText.color = Color.black;
        wordText.text = words[wordIndex].ToLower();
        gameScript.HighlightPath(paths[wordIndex], WordGot());
    }
}

public class Solverobj
{
    public readonly char[,] _board;
    public readonly Trie _trie;

    public Solverobj(char[,] board, Trie trie)
    {
        _board = board;
        _trie = trie;
    }

    private readonly List<Point> _neighboursDelta = new List<Point>
        {
            new Point(-1, -1),  new Point(-1, 0),   new Point(-1, 1),
            new Point(0,  -1),  /*   origin   */    new Point(0,  1),
            new Point(1,  -1),  new Point(1,  0),   new Point(1,  1),
        };

    private List<Point> GetNeighbours(Point point)
    {
        var neighbours = new List<Point>();
        foreach (Point delta in _neighboursDelta)
        {
            Point neigh = new Point(
                delta.row + point.row,
                delta.col + point.col
            );
            if (neigh.row < 4 && neigh.col < 4 && neigh.row >= 0 && neigh.col >= 0)
            {
                neighbours.Add(neigh);
            }
        }
        return neighbours;
    }

    private void DFS(Point point, List<int> visited, Dictionary<string, List<int>> found, string prefix)
    {
        visited.Add(Board.PointToInt(point));
        prefix += _board[point.row, point.col];
        if (_trie.ContainsPrefix(prefix))
        {
            if (_trie.ContainsWord(prefix) && prefix.Length > 2)
            {
                found[prefix] = visited.Take(visited.Count).ToList();
            }
            List<Point> neighbours = GetNeighbours(point);
            foreach (Point neigh in neighbours)
            {
                if (!visited.Contains(Board.PointToInt(neigh)))
                {
                    DFS(neigh, visited, found, prefix);
                }
            }
        }
        visited.Remove(Board.PointToInt(point));
    }

    public Dictionary<string, List<int>> FindWordsWithPath()
    {
        var found = new Dictionary<string, List<int>>();
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                DFS(new Point(row, col), new List<int>(), found, "");
            }
        }
        var orderedFound = found.OrderByDescending(i => i.Key.Length);
        return orderedFound.ToDictionary(i => i.Key, i => i.Value);
    }
}

public class Point
{
    public readonly int row;
    public readonly int col;

    public Point(int r, int c)
    {
        row = r;
        col = c;
    }

    public static void PrintPoints(List<Point> points)
    {
        foreach (Point point in points)
        {
            Debug.Log("(" + point.row + ", " + point.col + ")");
        }
    }
}

public class Trie
{
    private readonly Dictionary<char, Trie> _node;

    private Trie()
    {
        _node = new Dictionary<char, Trie>();
    }

    public static Trie BuildTrie(IEnumerable<string> words)
    {
        var trie = new Trie();
        foreach (string word in words)
        {
            Trie currentNode = trie;
            foreach (char letter in word)
            {
                if (!currentNode._node.ContainsKey(letter))
                {
                    currentNode._node[letter] = new Trie();
                }
                currentNode = currentNode._node[letter];
            }
            currentNode._node['*'] = null;
        }
        return trie;
    }

    public bool ContainsWord(string word)
    {
        Trie finalNode = GetFinalNode(word);
        return finalNode != null && finalNode._node.ContainsKey('*');
    }

    public bool ContainsPrefix(string prefix)
    {
        Trie finalNode = GetFinalNode(prefix);
        return finalNode != null;
    }

    private Trie GetFinalNode(string word)
    {
        Trie currentNode = this;
        foreach (char letter in word)
        {
            if (!currentNode._node.TryGetValue(letter, out currentNode))
            {
                return null;
            }
        }
        return currentNode;
    }
}

public class Board
{
    public static char[,] GetBoardArray(char[] board)
    {
        char[,] array = new char[4, 4];
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                int boardIndex = PointToInt(new Point(row, col));
                array[row, col] = board[boardIndex];
            }
        }
        return array;
    }

    public static int PointToInt(Point point)
    {
        return point.row * 4 + point.col;
    }

    public static void Print2DArray<T>(T[,] array)
    {
        for (int row = 0; row < array.GetLength(0); row++)
        {
            string output = "";
            for (int col = 0; col < array.GetLength(1); col++)
            {
                output += array[row, col] + ". ";
            }
            Debug.Log(output);
        }
    }
}