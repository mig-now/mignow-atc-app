import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ApiError } from "@/api/client";
import type { SettingsResponse, UpdateSettingsCommand } from "@/api/generated/atc/model";
import { getSettingsGetQueryKey, settingsUpdate, useSettingsGet } from "@/api/generated/atc/settings/settings";
import { payload, unwrapped } from "@/api/unwrapped";

export function useSettings() {
  return unwrapped(useSettingsGet());
}

export function useSaveSettings() {
  const queryClient = useQueryClient();
  return useMutation<SettingsResponse, ApiError, UpdateSettingsCommand>({
    mutationFn: (input) => payload(settingsUpdate(input)),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: getSettingsGetQueryKey() }),
  });
}
