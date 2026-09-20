<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { makeEditPropertyCommand } from "@/core/designer/commands"
import { coerceLiteralValue } from "@/core/designer/form"

const props = defineProps<{ activity: unknown; designer: { executeCommand: (command: unknown) => void } }>()

const METHODS = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"]
const JSON_FIELDS = ["content", "requestHeaders", "responseErrorCodes", "suspendOnStatusCodes"]

const activity = computed(() => props.activity as unknown as Record<string, { expression: { value?: unknown } }>)

function field(key: string) {
  return activity.value[key].expression as unknown as Record<string, unknown>
}

function display(key: string): string {
  const value = field(key)["value"]
  return value == null ? "" : typeof value === "object" ? JSON.stringify(value) : String(value)
}

function commit(key: string, raw: string): void {
  const from = field(key)["value"]
  const to = JSON_FIELDS.includes(key)
    ? (Array.isArray(from) && !raw.startsWith("[") && raw !== ""
        ? raw.split(",").map((entry) => entry.trim()).filter(Boolean).map((entry) => (from.length > 0 && typeof from[0] === "number" ? Number(entry) : entry))
        : coerceLiteralValue(from, raw))
    : coerceLiteralValue(from, raw)
  if (to === undefined) return
  props.designer.executeCommand(makeEditPropertyCommand(field(key), "value", from, to))
}

function commitNumber(key: string, event: Event): void {
  commit(key, (event.target as HTMLInputElement).value)
}

function commitBoolean(key: string, value: boolean): void {
  props.designer.executeCommand(makeEditPropertyCommand(field(key), "value", field(key)["value"], value))
}

function commitMethod(method: string): void {
  props.designer.executeCommand(makeEditPropertyCommand(field("method"), "value", field("method")["value"], method))
}
</script>

<template>
  <div class="space-y-2">
    <div class="flex items-end gap-2">
      <div class="w-32 space-y-1">
        <Label class="text-xs">方法</Label>
        <Select :model-value="display('method')" @update:model-value="(value) => commitMethod(String(value))">
          <SelectTrigger class="h-8 text-xs"><SelectValue /></SelectTrigger>
          <SelectContent>
            <SelectItem v-for="method in METHODS" :key="method" :value="method">{{ method }}</SelectItem>
          </SelectContent>
        </Select>
      </div>
      <div class="flex-1 space-y-1">
        <Label class="text-xs">URL</Label>
        <UiInput :model-value="display('url')" placeholder="https://" @change="commit('url', ($event.target as HTMLInputElement).value)" />
      </div>
    </div>
    <div class="space-y-1">
      <Label class="text-xs">内容（JSON）</Label>
      <Textarea class="font-mono text-xs" rows="4" :model-value="display('content')" @change="commit('content', ($event.target as HTMLTextAreaElement).value)" />
    </div>
    <div class="space-y-1">
      <Label class="text-xs">请求头（JSON）</Label>
      <Textarea class="font-mono text-xs" rows="3" :model-value="display('requestHeaders')" @change="commit('requestHeaders', ($event.target as HTMLTextAreaElement).value)" />
    </div>
    <div class="grid grid-cols-2 gap-2">
      <div class="space-y-1">
        <Label class="text-xs">认证</Label>
        <UiInput :model-value="display('authorization')" @change="commit('authorization', ($event.target as HTMLInputElement).value)" />
      </div>
      <div class="space-y-1">
        <Label class="text-xs">Content-Type</Label>
        <UiInput :model-value="display('contentType')" @change="commit('contentType', ($event.target as HTMLInputElement).value)" />
      </div>
      <div class="space-y-1">
        <Label class="text-xs">超时（秒）</Label>
        <UiInput type="number" :model-value="display('timeoutInterval')" @change="commitNumber('timeoutInterval', $event)" />
      </div>
      <div class="space-y-1">
        <Label class="text-xs">错误状态码</Label>
        <UiInput :model-value="display('responseErrorCodes')" placeholder="400, 401, 500" @change="commit('responseErrorCodes', ($event.target as HTMLInputElement).value)" />
      </div>
      <div class="space-y-1">
        <Label class="text-xs">挂起状态码</Label>
        <UiInput :model-value="display('suspendOnStatusCodes')" placeholder="202" @change="commit('suspendOnStatusCodes', ($event.target as HTMLInputElement).value)" />
      </div>
      <div class="flex items-end gap-2 pb-1">
        <Switch :model-value="field('waitForCompletion')['value'] === true" @update:model-value="(value: boolean) => commitBoolean('waitForCompletion', value)" />
        <Label class="text-xs">等待完成</Label>
      </div>
    </div>
  </div>
</template>
