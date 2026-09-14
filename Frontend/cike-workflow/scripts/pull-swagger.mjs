/**
 * 拉取后端 Swagger JSON 并入库到 openapi/swagger.json，供 openapi-ts 生成代码。
 *
 * 用法：
 *   node scripts/pull-swagger.mjs
 *   SWAGGER_BASE_URL=http://host:port node scripts/pull-swagger.mjs
 */
import { mkdirSync, writeFileSync } from 'node:fs'
import path from 'node:path'

const baseUrl = process.env.SWAGGER_BASE_URL ?? 'http://localhost:5046'
const url = `${baseUrl.replace(/\/+$/, '')}/swagger/v1/swagger.json`
const target = path.resolve('openapi/swagger.json')

const response = await fetch(url)
if (!response.ok) {
  console.error(`[pull-swagger] ${url} -> HTTP ${response.status}`)
  process.exit(1)
}

const text = await response.text()
const doc = JSON.parse(text) // 校验是合法 JSON，坏内容不落盘
mkdirSync(path.dirname(target), { recursive: true })
writeFileSync(target, `${JSON.stringify(doc, null, 2)}\n`, 'utf8')
console.log(`[pull-swagger] ${Object.keys(doc.paths ?? {}).length} paths -> openapi/swagger.json`)
