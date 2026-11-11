import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

/** Documentação .md gerada pelo backend. Sem HTML cru (react-markdown não renderiza HTML por padrão). */
export function Markdown({ text }: { text: string }) {
  return (
    <div className="prose-md text-sm">
      <ReactMarkdown remarkPlugins={[remarkGfm]}>{text}</ReactMarkdown>
    </div>
  );
}
