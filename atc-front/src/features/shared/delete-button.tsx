import { Trash2 } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "./api-error-alert";

/** Lixeira com confirmação. Se o backend recusar (registro em uso), a mensagem dele aparece no diálogo. */
export function DeleteButton({
  name,
  description,
  onDelete,
}: {
  name: string;
  /** Consequência da exclusão, quando não é óbvia. */
  description?: string;
  onDelete: () => Promise<unknown>;
}) {
  const { t } = useTranslation();
  const toast = useToast();
  const [open, setOpen] = useState(false);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const confirm = async () => {
    setPending(true);
    setError(null);
    try {
      await onDelete();
      toast({ tone: "success", title: t("common.deleted") });
      setOpen(false);
    } catch (e) {
      setError(e);
    } finally {
      setPending(false);
    }
  };
  return (
    <Dialog
      open={open}
      onOpenChange={(o) => {
        setOpen(o);
        setError(null);
      }}
    >
      <Button variant="ghost" size="icon" aria-label={t("actions.deleteItem", { name })} onClick={() => setOpen(true)}>
        <Trash2 />
      </Button>
      {open && (
        <DialogContent title={t("actions.deleteTitle", { name })} description={description}>
          <ApiErrorAlert error={error} />
          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>
              {t("actions.cancel")}
            </Button>
            <Button variant="destructive" disabled={pending} onClick={() => void confirm()}>
              {t("actions.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      )}
    </Dialog>
  );
}
