import { diffLines } from "diff";
import { useTranslation } from "react-i18next";
import { cn } from "@/lib/cn";

type Row = {
  left?: { n: number; text: string; kind: "same" | "removed" };
  right?: { n: number; text: string; kind: "same" | "added" };
};

function rows(before: string, after: string): Row[] {
  const out: Row[] = [];
  let l = 1;
  let r = 1;
  const split = (value: string) => value.replace(/\n$/, "").split("\n");
  const parts = diffLines(before.replace(/\r\n/g, "\n"), after.replace(/\r\n/g, "\n"));
  for (let i = 0; i < parts.length; i++) {
    const p = parts[i];
    if (!p) continue;
    const lines = split(p.value);
    const next = parts[i + 1];
    if (!p.added && !p.removed) {
      for (const text of lines)
        out.push({ left: { n: l++, text, kind: "same" }, right: { n: r++, text, kind: "same" } });
    } else if (p.removed && next?.added) {
      const added = split(next.value);
      for (let k = 0; k < Math.max(lines.length, added.length); k++) {
        const left = lines[k];
        const right = added[k];
        out.push({
          left: left !== undefined ? { n: l++, text: left, kind: "removed" } : undefined,
          right: right !== undefined ? { n: r++, text: right, kind: "added" } : undefined,
        });
      }
      i++;
    } else if (p.removed) {
      for (const text of lines) out.push({ left: { n: l++, text, kind: "removed" } });
    } else {
      for (const text of lines) out.push({ right: { n: r++, text, kind: "added" } });
    }
  }
  return out;
}

function Cell({ line }: { line?: { n: number; text: string; kind: string } }) {
  return (
    <>
      <td className="w-10 border-r border-border px-2 text-right text-muted-foreground select-none">{line?.n ?? ""}</td>
      <td
        className={cn(
          "px-2 whitespace-pre",
          line?.kind === "removed" && "bg-destructive/15",
          line?.kind === "added" && "bg-success/15",
          !line && "bg-muted/40",
        )}
      >
        {line?.text ?? ""}
      </td>
    </>
  );
}

/** Antes e depois lado a lado (o fonte inteiro, com as linhas alteradas destacadas). */
export function SourceDiff({ before, after }: { before: string; after: string }) {
  const { t } = useTranslation();
  return (
    <div className="max-h-[60dvh] overflow-auto rounded-md border border-border">
      <table className="w-full border-collapse font-mono text-xs">
        <thead className="sticky top-0 bg-muted">
          <tr>
            <th colSpan={2} className="px-2 py-1.5 text-left font-sans font-semibold">
              {t("diff.before")}
            </th>
            <th colSpan={2} className="border-l border-border px-2 py-1.5 text-left font-sans font-semibold">
              {t("diff.after")}
            </th>
          </tr>
        </thead>
        <tbody>
          {rows(before, after).map((row, i) => (
            // biome-ignore lint/suspicious/noArrayIndexKey: linhas do diff não têm id estável
            <tr key={i} className="align-top">
              <Cell line={row.left} />
              <Cell line={row.right} />
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
