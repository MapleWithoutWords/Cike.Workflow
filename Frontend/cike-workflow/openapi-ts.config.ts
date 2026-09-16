import { defineConfig } from '@hey-api/openapi-ts'

export default defineConfig({
  input: './openapi/swagger.json',
  output: {
    path: 'src/api/generated',
    clean: true,
  },
  plugins: [
    {
      name: '@hey-api/client-axios',
    },
    {
      name: '@hey-api/typescript',
      fileName: 'types.ts',
      $resolvers: {
        number(ctx) {
          // 后端全局注册 LongToStringConverter：long(Int64) 在 JSON 中序列化为字符串，
          // TS 侧必须映射为 string（雪花 id 超出 JS Number 安全范围）
          if (ctx.schema.format === 'int64') {
            return ctx.$.type('string')
          }
          return ctx.nodes.const(ctx) ?? ctx.nodes.base(ctx)
        },
      },
    },
    {
      name: '@hey-api/sdk',
      fileName: 'sdk.ts',
    },
  ],
})
