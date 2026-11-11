import { Download, Plus } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import type { ExtractionResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Input, NativeSelect } from "@/components/ui/input";
import { newSessionPath } from "@/features/remediation";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { useCreateSession, useDownloadExtractionFile } from "../hooks/useExtraction";

const PRIORITIES = ["1", "1,2", "1,2,3"] as const;
type Priorities = (typeof PRIORITIES)[number];

/** Escopo de prioridades da remediação: baixa o CSV no layout da atc-remediacao ou cria a sessão e abre `/remediacao/:id`. */
export function RemediationDialog({
  extraction,
  defaultRequest,
  onClose,
}: {
  extraction: ExtractionResponse | null;
  defaultRequest?: string | null;
  onClose: () => void;
}) {
  const { t } = useTranslation("extraction");
  const navigate = useNavigate();
  const [priorities, setPriorities] = useState<Priorities>("1");
  const [request, setRequest] = useState<string | null>(null);
  const create = useCreateSession();
  const download = useDownloadExtractionFile();
  const requestValue = request ?? defaultRequest ?? "";

  return (
    <Dialog open={!!extraction} onOpenChange={(o) => !o && onClose()}>
      <DialogContent
        title={t("remediation.title", { id: extraction?.displayId ?? "" })}
        description={t("remediation.description")}
      >
        {extraction && (
          <div className="space-y-4">
            <Field id="rem-priorities" label={t("remediation.priorities")} hint={t("remediation.prioritiesHint")}>
              <NativeSelect
                id="rem-priorities"
                value={priorities}
                onChange={(e) => setPriorities(e.target.value as Priorities)}
              >
                {PRIORITIES.map((p) => (
                  <option key={p} value={p}>
                    {t(`remediation.scope.${p}`)}
                  </option>
                ))}
              </NativeSelect>
            </Field>
            <Field id="rem-request" label={t("remediation.request")} hint={t("remediation.requestHint")}>
              <Input id="rem-request" value={requestValue} onChange={(e) => setRequest(e.target.value)} />
            </Field>
            <ApiErrorAlert error={create.error ?? download.error} />
            <DialogFooter>
              <Button
                variant="outline"
                disabled={download.isPending}
                onClick={() =>
                  download.mutate({
                    id: extraction.id,
                    kind: "remediacao",
                    priorities,
                    fallbackName: extraction.fileBaseName,
                  })
                }
              >
                <Download />
                {t("remediation.download")}
              </Button>
              <Button
                disabled={create.isPending}
                onClick={() =>
                  create.mutate(
                    { extractionId: extraction.id, priorities, request: requestValue.trim() || null },
                    { onSuccess: (s) => navigate(newSessionPath(s)) },
                  )
                }
              >
                <Plus />
                {t("remediation.createSession")}
              </Button>
            </DialogFooter>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
