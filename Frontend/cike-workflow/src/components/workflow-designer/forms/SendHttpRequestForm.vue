<script setup lang="ts">
import { computed } from "vue"
import { Input as UiInput } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import type { WorkflowDesignerState } from "@/composables/useWorkflowDesigner"
import type { ExpressionLike } from "@/core/designer/expression"
import { coerceLiteralValue } from "@/core/designer/form"
import ExpressionEditor from "../ExpressionEditor.vue"

const props = defineProps<{ activity: unknown; designer: WorkflowDesignerState }>()

const METHODS = ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"]
const JSON_FIELDS = ["content", "requestHeaders", "responseErrorCodes", "suspendOnStatusCodes"]

const activity = computed(() => props.activity as unknown as Record<string, { expression: ExpressionLike }>)

function expr(key: string): ExpressionLike {
  return activity.value[key].expression
}

function text(value: unknown): string {
  return value == null ? "" : typeof value === "object" ? JSON.stringify(value) : String(value)
}

/** Same coercion the form used before: comma-splitting for numeric/string code
 *  arrays, otherwise shape-preserving literal coercion. */
function coerce(key: string, from: unknown, raw: string): unknown {
  if (JSON_FIELDS.includes(key) && Array.isArray(from) && !raw.startsWith("[") && raw !== "") {
    return raw
      .split(",")
      .map((entry) => entry.trim())
      .filter(Boolean)
      .map((entry) => (from.length > 0 && typeof from[0] === "number" ? Number(entry) : entry))
  }
  return coerceLiteralValue(from, raw)
}
</script>

<template>
  <div class="space-y-2">
    <div class="flex items-start gap-2">
      <div class="w-36 shrink-0">
        <ExpressionEditor :expression="expr('method')" :designer="designer" label="方法" literal-default="GET">
          <template #default="{ value, commit }">
            <Select :model-value="text(value)" @update:model-value="(v) => commit(String(v))">
              <SelectTrigger size="sm" class="w-full text-xs">
                <SelectValue class="block! min-w-0 truncate" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="method in METHODS" :key="method" :value="method">{{ method }}</SelectItem>
              </SelectContent>
            </Select>
          </template>
        </ExpressionEditor>
      </div>
      <div class="min-w-0 flex-1">
        <ExpressionEditor :expression="expr('url')" :designer="designer" label="URL" literal-default="">
          <template #default="{ value, commit }">
            <UiInput
              :model-value="text(value)"
              placeholder="https://"
              @change="(e: Event) => { const to = coerce('url', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
            />
          </template>
        </ExpressionEditor>
      </div>
    </div>

    <ExpressionEditor :expression="expr('content')" :designer="designer" label="内容（JSON）" :literal-default="{}">
      <template #default="{ value, commit }">
        <Textarea
          class="font-mono text-xs"
          rows="4"
          :model-value="text(value)"
          @change="(e: Event) => { const to = coerce('content', value, (e.target as HTMLTextAreaElement).value); if (to !== undefined) commit(to) }"
        />
      </template>
    </ExpressionEditor>

    <ExpressionEditor :expression="expr('requestHeaders')" :designer="designer" label="请求头（JSON）" :literal-default="{}">
      <template #default="{ value, commit }">
        <Textarea
          class="font-mono text-xs"
          rows="3"
          :model-value="text(value)"
          @change="(e: Event) => { const to = coerce('requestHeaders', value, (e.target as HTMLTextAreaElement).value); if (to !== undefined) commit(to) }"
        />
      </template>
    </ExpressionEditor>

    <div class="grid grid-cols-2 gap-2">
      <ExpressionEditor :expression="expr('authorization')" :designer="designer" label="认证" literal-default="">
        <template #default="{ value, commit }">
          <UiInput
            :model-value="text(value)"
            @change="(e: Event) => { const to = coerce('authorization', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
          />
        </template>
      </ExpressionEditor>

      <ExpressionEditor :expression="expr('contentType')" :designer="designer" label="Content-Type" literal-default="">
        <template #default="{ value, commit }">
          <UiInput
            :model-value="text(value)"
            @change="(e: Event) => { const to = coerce('contentType', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
          />
        </template>
      </ExpressionEditor>

      <ExpressionEditor :expression="expr('timeoutInterval')" :designer="designer" label="超时（秒）" :literal-default="0">
        <template #default="{ value, commit }">
          <UiInput
            type="number"
            :model-value="text(value)"
            @change="(e: Event) => { const to = coerce('timeoutInterval', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
          />
        </template>
      </ExpressionEditor>

      <ExpressionEditor :expression="expr('responseErrorCodes')" :designer="designer" label="错误状态码" :literal-default="[]">
        <template #default="{ value, commit }">
          <UiInput
            :model-value="text(value)"
            placeholder="400, 401, 500"
            @change="(e: Event) => { const to = coerce('responseErrorCodes', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
          />
        </template>
      </ExpressionEditor>

      <ExpressionEditor :expression="expr('suspendOnStatusCodes')" :designer="designer" label="挂起状态码" :literal-default="[]">
        <template #default="{ value, commit }">
          <UiInput
            :model-value="text(value)"
            placeholder="202"
            @change="(e: Event) => { const to = coerce('suspendOnStatusCodes', value, (e.target as HTMLInputElement).value); if (to !== undefined) commit(to) }"
          />
        </template>
      </ExpressionEditor>

      <ExpressionEditor :expression="expr('waitForCompletion')" :designer="designer" label="等待完成" :literal-default="false">
        <template #default="{ value, commit }">
          <Switch :model-value="value === true" @update:model-value="(v: boolean) => commit(v)" />
        </template>
      </ExpressionEditor>
    </div>
  </div>
</template>
