import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ApplySection } from "./ApplySection";
import { type ProposeResult, useApplyContext, useInvalidateSession } from "./hooks";
import { ProposeSection } from "./ProposeSection";
import { ReviewSection } from "./ReviewSection";

/**
 * Aba "Propor, revisar e aplicar" da sessão (fluxo híbrido): o consultor decide em cada etapa antes de qualquer
 * gravação. 1) propor (run Propose) · 2) revisar e decidir · 3) simular, confirmar e aplicar no SAP.
 */
export function SessionApplyTab({ sessionId }: { sessionId: string }) {
  const { t } = useTranslation("remediation");
  const [step, setStep] = useState("propose");
  const [candidates, setCandidates] = useState<ProposeResult["factCandidates"]>([]);
  const invalidate = useInvalidateSession(sessionId);
  const { session } = useApplyContext(sessionId);
  return (
    <Tabs value={step} onValueChange={setStep} className="space-y-4">
      <TabsList>
        <TabsTrigger value="propose">{t("apply.step.propose")}</TabsTrigger>
        <TabsTrigger value="review">{t("apply.step.review")}</TabsTrigger>
        <TabsTrigger value="apply">{t("apply.step.apply")}</TabsTrigger>
      </TabsList>
      <TabsContent value="propose">
        <ProposeSection
          sessionId={sessionId}
          autoModeSession={session?.autoMode ?? false}
          onResult={(r) => setCandidates(r.factCandidates)}
          onDone={invalidate}
        />
      </TabsContent>
      <TabsContent value="review">
        <ReviewSection sessionId={sessionId} candidates={candidates} onChanged={invalidate} />
      </TabsContent>
      <TabsContent value="apply">
        <ApplySection sessionId={sessionId} onChanged={invalidate} />
      </TabsContent>
    </Tabs>
  );
}
