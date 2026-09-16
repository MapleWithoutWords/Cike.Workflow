import type { InternalAxiosRequestConfig } from 'axios'
import { client } from './generated/client.gen'

client.setConfig({
  baseURL: import.meta.env.VITE_API_BASE_URL,
})

// 请求拦截器：注入 Bearer token
client.instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`)
  }
  return config
})

export { client }
