namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet, float score, string dt)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Dt = dt;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public string Link { get; set; }

    public float Score { get; private set; }

    public string  Dt { get; private set; }
}
