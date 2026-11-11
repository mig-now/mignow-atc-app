import { useState } from "react";
import { useTranslation } from "react-i18next";
import type { ItemResponse } from "@/api/generated/atc/model";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ApiErrorAlert } from "@/features/shared";

/** Confirmação explícita antes de gravar no SAP: ambiente, request e objetos. Bloqueada em produção. */
export function ConfirmApplyDialog({
  systemName,
  request,
  items,
  production,
  pending,
  error,
  onConfirm,
  onClose,
}: {
  systemName: string;
  request: string;
  items: ItemResponse[];
  production: boolean;
  pending: boolean;
  error: unknown;
  onConfirm: () => void;
  onClose: () => void;
}) {
  const { t } = useTranslation("remediation");
  const [ack, setAck] = useState(false);
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent
        title={t("apply.confirm.title")}
        description={t("apply.confirm.description", { system: systemName, request })}
      >
        <div className="space-y-4">
          {production && (
            <Alert tone="danger" title={t("apply.confirm.productionTitle")}>
              {t("apply.confirm.production")}
            </Alert>
          )}
          <p className="text-sm font-medium">{t("apply.confirm.objects", { count: items.length })}</p>
          <ul className="max-h-60 list-disc overflow-auto pl-5 text-sm">
            {items.map((i) => (
              <li key={i.id}>
                #{i.n} {i.objectType} {i.objectName}
              </li>
            ))}
          </ul>
          <div className="flex items-start gap-2">
            <Checkbox
              id="apply-confirm-ack"
              checked={ack}
              disabled={production}
              onChange={(e) => setAck(e.target.checked)}
            />
            <Label htmlFor="apply-confirm-ack">{t("apply.confirm.ack", { request })}</Label>
          </div>
          <ApiErrorAlert error={error} />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            {t("apply.decision.back")}
          </Button>
          <Button variant="destructive" disabled={production || !ack || pending} onClick={onConfirm}>
            {t("apply.confirm.submit")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
