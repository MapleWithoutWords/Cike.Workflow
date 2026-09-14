import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    /** 页面标题（面包屑/文档标题） */
    title?: string
    /** 空间内功能区，用于 L2 高亮与面包屑 */
    section?: 'overview' | 'definitions' | 'instances'
    /**
     * 布局名称。默认 'app'（带 L1+L2 外壳）；
     * 'blank' 用于设计器等全屏场景。
     */
    layout?: 'app' | 'blank'
  }
}

interface ImportMetaEnv {
  /** 后端 API 根地址（不含 /api 前缀），如 http://localhost:5046 */
  readonly VITE_API_BASE_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}

export {}
