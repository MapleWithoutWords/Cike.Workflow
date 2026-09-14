<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { Moon, Sun } from '@lucide/vue'
import { useTheme } from '@/composables/useTheme'

const { isDark, toggle } = useTheme()
</script>

<template>
  <Tooltip>
    <TooltipTrigger as-child>
      <Button
        variant="ghost"
        size="icon"
        :aria-label="isDark ? '切换到浅色模式' : '切换到深色模式'"
        :aria-pressed="isDark"
        @click="toggle"
      >
        <Sun v-if="!isDark" class="size-4" />
        <Moon v-else class="size-4" />
        <span class="sr-only">{{ isDark ? '切换到浅色模式' : '切换到深色模式' }}</span>
      </Button>
    </TooltipTrigger>
    <TooltipContent side="bottom">
      {{ isDark ? '浅色模式' : '深色模式' }}
    </TooltipContent>
  </Tooltip>
</template>
