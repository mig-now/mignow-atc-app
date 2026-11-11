namespace Mignow.Atc.Application.Modules.Remediation.Apply;

/// <summary>
/// Porte do <c>difflib.SequenceMatcher</c> do Python (sem <c>isjunk</c>, com <c>autojunk</c>): mesmos blocos e opcodes, para o diff
/// do <c>preparar</c> sair igual ao da skill (inclusive o tratamento dos elementos "populares" em sequências com 200+ linhas).
/// </summary>
internal sealed class SequenceMatcher
{
    public readonly record struct Block(int A, int B, int Size);

    public readonly record struct Opcode(string Tag, int I1, int I2, int J1, int J2);

    private readonly IReadOnlyList<string> _a;
    private readonly IReadOnlyList<string> _b;
    private readonly Dictionary<string, List<int>> _b2j = [];

    public SequenceMatcher(IReadOnlyList<string> a, IReadOnlyList<string> b)
    {
        _a = a;
        _b = b;
        for (var i = 0; i < b.Count; i++)
        {
            if (!_b2j.TryGetValue(b[i], out var indices))
                _b2j[b[i]] = indices = [];
            indices.Add(i);
        }
        var n = b.Count;
        if (n >= 200)
        {
            var ntest = (n / 100) + 1;
            foreach (var popular in _b2j.Where(kv => kv.Value.Count > ntest).Select(kv => kv.Key).ToList())
                _b2j.Remove(popular);
        }
    }

    private Block FindLongestMatch(int alo, int ahi, int blo, int bhi)
    {
        int besti = alo, bestj = blo, bestsize = 0;
        var j2len = new Dictionary<int, int>();
        for (var i = alo; i < ahi; i++)
        {
            var newj2len = new Dictionary<int, int>();
            if (_b2j.TryGetValue(_a[i], out var js))
                foreach (var j in js)
                {
                    if (j < blo)
                        continue;
                    if (j >= bhi)
                        break;
                    var k = (j2len.TryGetValue(j - 1, out var prev) ? prev : 0) + 1;
                    newj2len[j] = k;
                    if (k > bestsize)
                    {
                        besti = i - k + 1;
                        bestj = j - k + 1;
                        bestsize = k;
                    }
                }
            j2len = newj2len;
        }
        while (besti > alo && bestj > blo && _a[besti - 1] == _b[bestj - 1])
        {
            besti--;
            bestj--;
            bestsize++;
        }
        while (besti + bestsize < ahi && bestj + bestsize < bhi && _a[besti + bestsize] == _b[bestj + bestsize])
            bestsize++;
        return new Block(besti, bestj, bestsize);
    }

    public List<Block> GetMatchingBlocks()
    {
        int la = _a.Count, lb = _b.Count;
        var queue = new Stack<(int Alo, int Ahi, int Blo, int Bhi)>();
        queue.Push((0, la, 0, lb));
        List<Block> matching = [];
        while (queue.Count > 0)
        {
            var (alo, ahi, blo, bhi) = queue.Pop();
            var x = FindLongestMatch(alo, ahi, blo, bhi);
            if (x.Size == 0)
                continue;
            matching.Add(x);
            if (alo < x.A && blo < x.B)
                queue.Push((alo, x.A, blo, x.B));
            if (x.A + x.Size < ahi && x.B + x.Size < bhi)
                queue.Push((x.A + x.Size, ahi, x.B + x.Size, bhi));
        }
        matching = [.. matching.OrderBy(m => m.A).ThenBy(m => m.B).ThenBy(m => m.Size)];
        int i1 = 0, j1 = 0, k1 = 0;
        List<Block> nonAdjacent = [];
        foreach (var (i2, j2, k2) in matching.Select(m => (m.A, m.B, m.Size)))
        {
            if (i1 + k1 == i2 && j1 + k1 == j2)
                k1 += k2;
            else
            {
                if (k1 > 0)
                    nonAdjacent.Add(new Block(i1, j1, k1));
                i1 = i2;
                j1 = j2;
                k1 = k2;
            }
        }
        if (k1 > 0)
            nonAdjacent.Add(new Block(i1, j1, k1));
        nonAdjacent.Add(new Block(la, lb, 0));
        return nonAdjacent;
    }

    public List<Opcode> GetOpcodes()
    {
        int i = 0, j = 0;
        List<Opcode> answer = [];
        foreach (var (ai, bj, size) in GetMatchingBlocks().Select(m => (m.A, m.B, m.Size)))
        {
            var tag = string.Empty;
            if (i < ai && j < bj)
                tag = "replace";
            else if (i < ai)
                tag = "delete";
            else if (j < bj)
                tag = "insert";
            if (tag.Length > 0)
                answer.Add(new Opcode(tag, i, ai, j, bj));
            i = ai + size;
            j = bj + size;
            if (size > 0)
                answer.Add(new Opcode("equal", i - size, i, j - size, j));
        }
        return answer;
    }

    public List<List<Opcode>> GetGroupedOpcodes(int n)
    {
        var codes = GetOpcodes();
        if (codes.Count == 0)
            codes.Add(new Opcode("equal", 0, 1, 0, 1));
        if (codes[0].Tag == "equal")
        {
            var c = codes[0];
            codes[0] = new Opcode("equal", Math.Max(c.I1, c.I2 - n), c.I2, Math.Max(c.J1, c.J2 - n), c.J2);
        }
        if (codes[^1].Tag == "equal")
        {
            var c = codes[^1];
            codes[^1] = new Opcode("equal", c.I1, Math.Min(c.I2, c.I1 + n), c.J1, Math.Min(c.J2, c.J1 + n));
        }
        var nn = n + n;
        List<List<Opcode>> groups = [];
        List<Opcode> group = [];
        foreach (var code in codes)
        {
            var (tag, i1, i2, j1, j2) = (code.Tag, code.I1, code.I2, code.J1, code.J2);
            if (tag == "equal" && i2 - i1 > nn)
            {
                group.Add(new Opcode(tag, i1, Math.Min(i2, i1 + n), j1, Math.Min(j2, j1 + n)));
                groups.Add(group);
                group = [];
                i1 = Math.Max(i1, i2 - n);
                j1 = Math.Max(j1, j2 - n);
            }
            group.Add(new Opcode(tag, i1, i2, j1, j2));
        }
        if (group.Count > 0 && !(group.Count == 1 && group[0].Tag == "equal"))
            groups.Add(group);
        return groups;
    }
}
