<script setup lang="ts">
import { computed } from "vue"
import { ChevronDown, ChevronUp, CircleAlert, CircleCheck, ListChecks, LoaderCircle, Pin, PinOff } from "@lucide/vue"
import type { ValidationProblem } from "@/composables/useWorkflowDesigner"
import { useDockState } from "@/composables/useDockState"

/**
 * Canvas-bottom "problem list" dock — the single outlet for canvas validation
 * problems (ADR 0002). Collapsible IDE-Problems style: the header always shows
 * the problem count / validating / pass state; the body lists each problem and
 * emits `reveal` for locatable rows so the designer can drill to the node.
 */
const props = defineProps<{
  problems: ValidationProblem[]
  validating: boolean
  validationError: string | null
}>()

const emit = defineEmits<{
  reveal: [problem: ValidationProblem]
}>()

const dock = useDockState()
/** Open/pin state lives in the shared dock layer (ADR 0006), not in props. */
const open = dock.open.problems
const pinned = dock.pinned.problems

const count = computed(() => props.problems.length)

/** A problem is locatable when it carries a NodeId chain or an activity id. */
function isLocatable(problem: ValidationProblem): boolean {
  return Boolean(problem.nodeId || problem.activityId)
}

function onRowClick(problem: ValidationProblem): void {
  if (isLocatable(problem)) emit("reveal", problem)
}
</script>

<template>
  <div class="flex shrink-0 flex-col border-t bg-background">
    <div class="flex h-8 shrink-0 items-center">
      <button
        type="button"
        class="flex h-full min-w-0 flex-1 items-center gap-2 px-3 text-left text-xs hover:bg-muted/50"
        :aria-expanded="open"
        @click="dock.toggleOpen('problems')"
      >
        <LoaderCircle v-if="validating" :size="14" class="shrink-0 animate-spin text-muted-foreground" />
        <ListChecks v-else :size="14" class="shrink-0 text-muted-foreground" />
        <span v-if="count > 0" class="font-medium text-destructive">{{ count }} 个问题</span>
        <span v-else-if="validationError" class="font-medium text-warning">校验失败</span>
        <span v-else class="font-medium text-muted-foreground">校验通过</span>
        <span class="ml-auto shrink-0 text-muted-foreground">{{ open ? "收起" : "展开" }}</span>
        <component :is="open ? ChevronDown : ChevronUp" :size="14" class="shrink-0 text-muted-foreground" />
      </button>
      <button
        type="button"
        class="mr-2 shrink-0 rounded p-0.5 hover:bg-muted hover:text-foreground"
        :class="pinned && 'text-foreground'"
        :title="pinned ? '取消固定问题清单' : '固定问题清单'"
        @click="dock.togglePin('problems')"
      >
        <component :is="pinned ? Pin : PinOff" :size="14" />
      </button>
    </div>

    <div v-if="open" class="max-h-56 overflow-y-auto border-t">
      <p v-if="validationError" class="px-3 py-2 text-xs text-warning">
        无法完成校验（{{ validationError }}）。这不是"画布没问题"，请检查后端连接后重试。
      </p>
      <div
        v-else-if="count === 0"
        class="flex items-center gap-2 px-3 py-3 text-xs text-muted-foreground"
      >
        <CircleCheck :size="14" class="shrink-0 text-success" />
        画布校验通过，没有发现问题。
      </div>
      <ul v-else>
        <li v-for="(problem, index) in problems" :key="`${problem.nodeId ?? problem.activityId ?? 'wf'}-${index}`">
          <button
            type="button"
            class="flex w-full items-start gap-2 px-3 py-1.5 text-left text-xs hover:bg-muted/50 disabled:cursor-default disabled:hover:bg-transparent"
            :disabled="!isLocatable(problem)"
            @click="onRowClick(problem)"
          >
            <CircleAlert :size="14" class="mt-0.5 shrink-0 text-destructive" />
            <span class="min-w-0 flex-1">
              <span class="text-foreground">{{ problem.message || "（无消息）" }}</span>
              <span v-if="problem.name" class="ml-2 text-muted-foreground">· {{ problem.name }}</span>
              <span v-if="!isLocatable(problem)" class="ml-2 text-muted-foreground">（工作流级，不可跳转）</span>
            </span>
          </button>
        </li>
      </ul>
    </div>
  </div>
</template>
