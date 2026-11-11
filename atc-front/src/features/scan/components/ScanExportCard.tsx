import { Download } from "lucide-react";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import type { ScanResponse } from "@/api/generated/atc/model";
import { getScansP1Url, getScansSummaryUrl } from "@/api/generated/atc/scans/scans";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogFooter } from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/input";
import { newSessionPath } from "@/features/remediation";
import { ApiErrorAlert } from "@/features/shared/api-error-alert";
import { summaryOf, useImportScan } from "../hooks/useScans";
import { downloadFile } from "../lib/download";

/** Downloads (CSV P1, resumo.md) e importação na remediação, com confirmação explícita (arquiva a sessão atual). */
export function ScanExportCard({ scan }: { scan: ScanResponse }) {
  const { t } = useTranslation("scan");
  const navigate = useNavigate();
  const sum = summaryOf(scan);
  const [includeOtherLine, setIncludeOtherLine] = useState(false);
  const [open, setOpen] = useState(false);
  const [downloadError, setDownloadError] = useState(false);
  const importScan = useImportScan(scan.id);
  const ready = sum != null;

  const download = (url: string, name: string) => {
    setDownloadError(false);
    downloadFile(url, name).catch(() => setDownloadError(true));
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("export.title")}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-3">
        {!ready ? <p className="text-sm text-muted-foreground">{t("export.notReady")}</p> : null}
        <label className="flex items-center gap-2 text-sm">
          <Checkbox checked={includeOtherLine} onChange={(e) => setIncludeOtherLine(e.target.checked)} />
          {t("export.includeOtherLine")}
        </label>
        <div className="flex flex-wrap gap-2">
          <Button
            variant="outline"
            disabled={!ready}
            onClick={() =>
              download(
                getScansP1Url(scan.id, { includeOtherLine }),
                sum?.csv_remediacao?.arquivo ?? `p1_${scan.id}.csv`,
              )
            }
          >
            <Download aria-hidden /> {t("export.csv")}
          </Button>
          <Button
            variant="outline"
            disabled={!ready}
            onClick={() => download(getScansSummaryUrl(scan.id), "resumo.md")}
          >
            <Download aria-hidden /> {t("export.summary")}
          </Button>
          <Button disabled={!ready} onClick={() => setOpen(true)}>
            {t("export.import")}
          </Button>
        </div>
        {downloadError ? <Alert tone="danger">{t("export.downloadFailed")}</Alert> : null}
        <Dialog
          open={open}
          onOpenChange={(o) => {
            setOpen(o);
            importScan.reset();
          }}
        >
          {open ? (
            <DialogContent title={t("import.title")} description={t("import.description")}>
              <Alert tone="warning">{t("import.warning")}</Alert>
              {importScan.error ? (
                <div className="mt-3">
                  <ApiErrorAlert error={importScan.error} />
                </div>
              ) : null}
              <DialogFooter>
                <Button variant="outline" onClick={() => setOpen(false)}>
                  {t("import.cancel")}
                </Button>
                <Button
                  disabled={importScan.isPending}
                  onClick={() => importScan.mutate(includeOtherLine, { onSuccess: (s) => navigate(newSessionPath(s)) })}
                >
                  {t("import.confirm")}
                </Button>
              </DialogFooter>
            </DialogContent>
          ) : null}
        </Dialog>
      </CardContent>
    </Card>
  );
}
