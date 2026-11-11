import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router";
import type { SessionResponse } from "@/api/generated/atc/model";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Field } from "@/components/ui/field";
import { Input, NativeSelect } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { PageHeader } from "@/features/shared/page-header";
import { ImportSummaryCard } from "../components/ImportSummaryCard";
import { useCreateSession, useExtractionOptions, useImportCsv, useSapSystemOptions } from "../hooks";
import { newSessionPath } from "../session-path";

const PRIORITIES = ["1", "1,2", "1,2,3"] as const;

function SystemSelect({
  id,
  value,
  onChange,
}: {
  id: string;
  value: string;
  onChange: (id: string, defaultRequest: string) => void;
}) {
  const { t } = useTranslation("remediation");
  const systems = useSapSystemOptions({ page: 1, pageSize: 200, active: true });
  return (
    <Field id={id} label={t("new.system")}>
      <NativeSelect
        id={id}
        value={value}
        onChange={(e) => {
          const s = systems.data?.items.find((x) => x.id === e.target.value);
          onChange(e.target.value, s?.defaultRequest ?? "");
        }}
      >
        <option value="">{t("new.chooseSystem")}</option>
        {systems.data?.items.map((s) => (
          <option key={s.id} value={s.id}>
            {s.displayName}
          </option>
        ))}
      </NativeSelect>
    </Field>
  );
}

function PrioritySelect({ id, value, onChange }: { id: string; value: string; onChange: (v: string) => void }) {
  const { t } = useTranslation("remediation");
  return (
    <Field id={id} label={t("new.priorities")}>
      <NativeSelect id={id} value={value} onChange={(e) => onChange(e.target.value)}>
        {PRIORITIES.map((p) => (
          <option key={p} value={p}>
            {t(`new.priorities.${p}`)}
          </option>
        ))}
      </NativeSelect>
    </Field>
  );
}

function FromExtraction() {
  const { t, i18n } = useTranslation("remediation");
  const navigate = useNavigate();
  const create = useCreateSession();
  const [systemId, setSystemId] = useState("");
  const [extractionId, setExtractionId] = useState("");
  const [priorities, setPriorities] = useState<string>("1");
  const [request, setRequest] = useState("");
  const extractions = useExtractionOptions(systemId);

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    create.mutate(
      { extractionId, priorities, request: request.trim() || null },
      { onSuccess: (s) => navigate(newSessionPath(s)) },
    );
  };

  return (
    <form onSubmit={submit} className="max-w-xl space-y-4">
      <SystemSelect
        id="ex-system"
        value={systemId}
        onChange={(id, req) => {
          setSystemId(id);
          setExtractionId("");
          setRequest(req);
        }}
      />
      <Field id="ex-extraction" label={t("new.extraction")}>
        <NativeSelect
          id="ex-extraction"
          value={extractionId}
          disabled={!systemId}
          onChange={(e) => setExtractionId(e.target.value)}
        >
          <option value="">{t("new.chooseExtraction")}</option>
          {extractions.data?.items.map((x) => (
            <option key={x.id} value={x.id}>
              {t("new.extractionOption", {
                id: x.displayId,
                date: new Intl.DateTimeFormat(i18n.language, { dateStyle: "short" }).format(new Date(x.extractedAt)),
                count: x.newCount,
              })}
            </option>
          ))}
        </NativeSelect>
      </Field>
      <PrioritySelect id="ex-priorities" value={priorities} onChange={setPriorities} />
      <Field id="ex-request" label={t("new.request")} hint={t("new.requestHint")}>
        <Input id="ex-request" value={request} onChange={(e) => setRequest(e.target.value)} />
      </Field>
      <ApiErrorAlert error={create.error} />
      <Button type="submit" disabled={!extractionId || create.isPending}>
        {t("new.create")}
      </Button>
    </form>
  );
}

function FromCsv() {
  const { t } = useTranslation("remediation");
  const importCsv = useImportCsv();
  const [systemId, setSystemId] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [priorities, setPriorities] = useState<string>("1");
  const [request, setRequest] = useState("");
  const [created, setCreated] = useState<SessionResponse | null>(null);

  const submit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) return;
    importCsv.mutate(
      { sapSystemId: systemId, file, priorities, request: request.trim() || undefined },
      { onSuccess: setCreated },
    );
  };

  return (
    <div className="space-y-4">
      <form onSubmit={submit} className="max-w-xl space-y-4">
        <SystemSelect
          id="csv-system"
          value={systemId}
          onChange={(id, req) => {
            setSystemId(id);
            setRequest(req);
          }}
        />
        <Field id="csv-file" label={t("new.file")} hint={t("new.fileHint")}>
          <Input
            id="csv-file"
            type="file"
            accept=".csv,text/csv"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
        </Field>
        <PrioritySelect id="csv-priorities" value={priorities} onChange={setPriorities} />
        <Field id="csv-request" label={t("new.request")} hint={t("new.requestHint")}>
          <Input id="csv-request" value={request} onChange={(e) => setRequest(e.target.value)} />
        </Field>
        <ApiErrorAlert error={importCsv.error} />
        <Button type="submit" disabled={!systemId || !file || importCsv.isPending}>
          {t("new.import")}
        </Button>
      </form>
      {created?.import && (
        <div className="max-w-3xl space-y-3">
          <ImportSummaryCard summary={created.import} />
          <Button asChild>
            <Link to={newSessionPath(created)}>{t("new.openSession")}</Link>
          </Button>
        </div>
      )}
    </div>
  );
}

/** `/remediacao/nova`: sessão a partir de uma extração ou de um CSV do ATC. */
export function NewRemediationPage() {
  const { t } = useTranslation("remediation");
  return (
    <>
      <PageHeader title={t("new.title")} description={t("new.description")} />
      <Card>
        <CardContent>
          <Tabs defaultValue="extraction">
            <TabsList>
              <TabsTrigger value="extraction">{t("new.fromExtraction")}</TabsTrigger>
              <TabsTrigger value="csv">{t("new.fromCsv")}</TabsTrigger>
            </TabsList>
            <TabsContent value="extraction">
              <FromExtraction />
            </TabsContent>
            <TabsContent value="csv">
              <FromCsv />
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>
    </>
  );
}
