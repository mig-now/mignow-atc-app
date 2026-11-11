import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Button } from "@/components/ui/button";
import { DialogFooter } from "@/components/ui/dialog";
import { Field } from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/toast";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { attempt } from "@/features/shared/form";
import { useChangeSapPassword } from "../hooks/useSapSystems";

/** A senha vai para o cofre e nunca volta pela API (só `hasPassword`). */
export function PasswordForm({ systemId, onDone }: { systemId: string; onDone: () => void }) {
  const { t } = useTranslation("config");
  const toast = useToast();
  const change = useChangeSapPassword();
  const [password, setPassword] = useState("");
  return (
    <form
      className="space-y-4"
      onSubmit={(e) => {
        e.preventDefault();
        void attempt(async () => {
          await change.mutateAsync({ id: systemId, password });
          toast({ tone: "success", title: t("sapSystems.passwordChanged") });
          onDone();
        });
      }}
    >
      <ApiErrorAlert error={change.error} />
      <Field id="new-password" label={t("sapSystems.newPassword")}>
        <Input
          id="new-password"
          type="password"
          autoComplete="new-password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
        />
      </Field>
      <DialogFooter>
        <Button type="button" variant="outline" onClick={onDone}>
          {t("common:actions.cancel")}
        </Button>
        <Button type="submit" disabled={!password || change.isPending}>
          {t("common:actions.save")}
        </Button>
      </DialogFooter>
    </form>
  );
}
